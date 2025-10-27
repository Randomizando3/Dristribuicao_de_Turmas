using System;
using System.Collections.Generic;
using System.Linq;
using DistribuicaoTurmas.Models;

namespace DistribuicaoTurmas.Services
{
    public class DistributionService
    {
        private static readonly Lazy<DistributionService> _lazy = new Lazy<DistributionService>(() => new DistributionService());
        public static DistributionService Instance => _lazy.Value;

        public (bool ok, AllocationResult result, string error) Generate()
        {
            var storage = StorageService.Instance;
            var rooms = storage.Rooms.ToList();
            var classes = storage.Classes.ToList();

            var result = new AllocationResult();
            var issues = new List<string>();

            if (!rooms.Any())
                return (false, null, "Não há salas cadastradas.");

            // -------- VALIDAÇÕES PRÉ-GERAÇÃO --------
            // 1) turmas sem horário
            var noSlot = classes.Where(c => c.Slot1 == null && c.Slot2 == null)
                                .Select(c => c.Name).ToList();
            if (noSlot.Any())
                issues.Add($"Há turmas sem horário definido: {string.Join(", ", noSlot)}.");

            // 2) capacidade insuficiente p/ alguma turma
            var maxCap = rooms.Any() ? rooms.Max(r => r.Capacity) : 0;
            var overCap = classes.Where(c => c.Enrolled > maxCap).ToList();
            foreach (var c in overCap)
                issues.Add($"A turma \"{c.Name}\" tem {c.Enrolled} alunos e excede a maior sala disponível (capacidade {maxCap}).");

            // 3) necessidade de computadores sem sala compatível
            var compRooms = rooms.Where(r => r.HasComputers).ToList();
            var lackCompRoom = classes.Where(c => c.NeedsComputers &&
                                                  !compRooms.Any(r => r.Capacity >= c.Enrolled))
                                      .ToList();
            foreach (var c in lackCompRoom)
                issues.Add($"A turma \"{c.Name}\" exige computadores, mas não há sala com computadores e capacidade ≥ {c.Enrolled}.");

            // 4) excesso de turmas por horário (com análise de computadores)
            //    — conta por slot: quantas com/sem computadores e compara com #salas com/sem comp.
            var totalRooms = rooms.Count;
            var totalComp = compRooms.Count;
            var totalNoComp = totalRooms - totalComp;

            IEnumerable<(ClassSection cls, ScheduleSlot slot, bool needsComp)> EnumerateSlots()
            {
                foreach (var c in classes)
                {
                    if (c.Slot1 != null) yield return (c, c.Slot1.Value, c.NeedsComputers);
                    if (c.Slot2 != null) yield return (c, c.Slot2.Value, c.NeedsComputers);
                }
            }

            var bySlot = EnumerateSlots().GroupBy(x => x.slot);
            foreach (var g in bySlot)
            {
                var needCompCount = g.Count(x => x.needsComp);
                var noCompCount = g.Count(x => !x.needsComp);

                if (needCompCount > totalComp)
                    issues.Add($"No horário {g.Key.ToString().Replace('_', ' ')} existem {needCompCount} turmas que exigem computadores, mas só {totalComp} sala(s) com computadores.");

                var roomsLeftForNoComp =
                    (needCompCount <= totalComp)
                        ? totalRooms - needCompCount   // as turmas com computadores ocupam salas com comp
                        : totalRooms - totalComp;      // todas as salas com comp já seriam insuficientes

                if (noCompCount > roomsLeftForNoComp)
                    issues.Add($"No horário {g.Key.ToString().Replace('_', ' ')} há {noCompCount} turmas sem exigência de computadores para {roomsLeftForNoComp} sala(s) disponível(eis).");
            }

            if (issues.Any())
                return (false, null, "Não foi possível gerar a distribuição:\n• " + string.Join("\n• ", issues));

            // -------- ALOCAÇÃO (gananciosa com prioridades) --------
            var occupied = new Dictionary<ScheduleSlot, HashSet<string>>();
            foreach (ScheduleSlot slot in Enum.GetValues(typeof(ScheduleSlot)))
                occupied[slot] = new HashSet<string>();

            var itemsToPlace = new List<(ClassSection cls, ScheduleSlot slot)>();
            foreach (var c in classes)
            {
                if (c.Slot1 != null) itemsToPlace.Add((c, c.Slot1.Value));
                if (c.Slot2 != null) itemsToPlace.Add((c, c.Slot2.Value));
            }

            // prioridade: precisa de computador > mais alunos > nome (estável)
            itemsToPlace = itemsToPlace
                .OrderByDescending(i => i.cls.NeedsComputers)
                .ThenByDescending(i => i.cls.Enrolled)
                .ThenBy(i => i.cls.Name)
                .ToList();

            foreach (var item in itemsToPlace)
            {
                var cls = item.cls;
                var slot = item.slot;

                var candidates = rooms
                    .Where(r => r.Capacity >= cls.Enrolled)
                    .Where(r => !occupied[slot].Contains(r.Number))
                    .Where(r => !cls.NeedsComputers || r.HasComputers)
                    .OrderBy(r => r.Capacity) // menor sala possível
                    .ToList();

                if (!candidates.Any())
                {
                    // Isso não deveria acontecer após as validações, mas ainda assim descrevemos causa provável:
                    var fullAtSlot = rooms.All(r => occupied[slot].Contains(r.Number));
                    if (fullAtSlot)
                        issues.Add($"No horário {slot.ToString().Replace('_', ' ')} todas as salas já estão ocupadas.");
                    else if (!rooms.Any(r => r.Capacity >= cls.Enrolled))
                        issues.Add($"A turma \"{cls.Name}\" tem {cls.Enrolled} alunos e excede todas as salas.");
                    else if (cls.NeedsComputers && !rooms.Any(r => r.HasComputers && r.Capacity >= cls.Enrolled))
                        issues.Add($"A turma \"{cls.Name}\" exige computadores e não há sala compatível livre.");
                    else
                        issues.Add($"Não foi possível alocar \"{cls.Name}\" em {slot.ToString().Replace('_', ' ')} devido a restrições de capacidade/ocupação.");

                    return (false, result, "Não foi possível gerar a distribuição:\n• " + string.Join("\n• ", issues));
                }

                var chosen = candidates.First();
                occupied[slot].Add(chosen.Number);

                result.Assignments.Add(new Assignment
                {
                    ClassName = cls.Name,
                    Teacher = cls.Teacher,
                    Slot = slot,
                    RoomNumber = chosen.Number,
                    RoomCapacity = chosen.Capacity
                });
            }

            return (true, result, null);
        }
    }
}

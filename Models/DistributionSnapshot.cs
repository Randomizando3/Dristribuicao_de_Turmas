using System;

namespace DistribuicaoTurmas.Models
{
    public class DistributionSnapshot
    {
        public string Name { get; set; }          // ex.: Distribuição001_31/10/2025
        public DateTime CreatedAt { get; set; }   // para ordenação/mostra
        public AllocationResult Result { get; set; } = new AllocationResult();

        // Conveniências para UI
        public int TotalAtribuicoes => Result?.Assignments?.Count ?? 0;
        public string CreatedAtBr => CreatedAt.ToString("dd/MM/yyyy");
    }
}

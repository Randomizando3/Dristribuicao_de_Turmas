using System.Collections.Generic;
using DistribuicaoTurmas.Models;

namespace DistribuicaoTurmas.Helpers
{
    public static class SlotHelper
    {
        public static List<ScheduleSlot> AllSlots = new List<ScheduleSlot>
        {
            ScheduleSlot.Segunda_M1, ScheduleSlot.Segunda_M2, ScheduleSlot.Segunda_N1, ScheduleSlot.Segunda_N2,
            ScheduleSlot.Terca_M1,   ScheduleSlot.Terca_M2,   ScheduleSlot.Terca_N1,   ScheduleSlot.Terca_N2,
            ScheduleSlot.Quarta_M1,  ScheduleSlot.Quarta_M2,  ScheduleSlot.Quarta_N1,  ScheduleSlot.Quarta_N2,
            ScheduleSlot.Quinta_M1,  ScheduleSlot.Quinta_M2,  ScheduleSlot.Quinta_N1,  ScheduleSlot.Quinta_N2,
            ScheduleSlot.Sexta_M1,   ScheduleSlot.Sexta_M2,   ScheduleSlot.Sexta_N1,   ScheduleSlot.Sexta_N2
        };
    }
}

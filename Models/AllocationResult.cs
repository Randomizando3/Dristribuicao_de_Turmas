using System.Collections.ObjectModel;

namespace DistribuicaoTurmas.Models
{
    public class AllocationResult
    {
        public ObservableCollection<Assignment> Assignments { get; set; } = new ObservableCollection<Assignment>();
    }

    public class Assignment
    {
        public string ClassName { get; set; }
        public string Teacher { get; set; }
        public ScheduleSlot Slot { get; set; }
        public string RoomNumber { get; set; }
        public int RoomCapacity { get; set; }

        public string DisplaySlot => Slot.ToString().Replace('_', ' ');
        public string DisplayRoom => $"Sala {RoomNumber} (cap. {RoomCapacity})";
    }
}

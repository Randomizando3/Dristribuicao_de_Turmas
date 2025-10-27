using System.ComponentModel;

namespace DistribuicaoTurmas.Models
{
    public class ClassSection : INotifyPropertyChanged
    {
        private string _name;
        private int _enrolled;
        private string _teacher;
        private bool _needsComputers;
        private ScheduleSlot? _slot1;
        private ScheduleSlot? _slot2;

        public string Name { get => _name; set { _name = value; OnChanged(nameof(Name)); } }
        public int Enrolled { get => _enrolled; set { _enrolled = value; OnChanged(nameof(Enrolled)); } }
        public string Teacher { get => _teacher; set { _teacher = value; OnChanged(nameof(Teacher)); } }
        public bool NeedsComputers { get => _needsComputers; set { _needsComputers = value; OnChanged(nameof(NeedsComputers)); OnChanged(nameof(NeedsComputersDisplay)); } }

        public string NeedsComputersDisplay
        {
            get => NeedsComputers ? "S" : "N";
            set => NeedsComputers = (value == "S");
        }

        public ScheduleSlot? Slot1 { get => _slot1; set { _slot1 = value; OnChanged(nameof(Slot1)); } }
        public ScheduleSlot? Slot2 { get => _slot2; set { _slot2 = value; OnChanged(nameof(Slot2)); } }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnChanged(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}

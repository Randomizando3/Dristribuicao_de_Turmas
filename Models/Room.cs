using System.ComponentModel;

namespace DistribuicaoTurmas.Models
{
    public class Room : INotifyPropertyChanged
    {
        private string _number;
        private int _capacity;
        private bool _hasComputers;

        public string Number
        {
            get => _number;
            set { if (_number != value) { _number = value; OnChanged(nameof(Number)); } }
        }

        public int Capacity
        {
            get => _capacity;
            set { if (_capacity != value) { _capacity = value; OnChanged(nameof(Capacity)); } }
        }

        /// <summary>
        /// Indica se a sala possui computadores. Editável direto no DataGrid (CheckBox).
        /// </summary>
        public bool HasComputers
        {
            get => _hasComputers;
            set
            {
                if (_hasComputers != value)
                {
                    _hasComputers = value;
                    OnChanged(nameof(HasComputers));
                    OnChanged(nameof(HasComputersDisplay)); // útil para exibições S/N em outros lugares
                }
            }
        }

        /// <summary>
        /// Apenas para exibição (S/N) onde for conveniente.
        /// </summary>
        public string HasComputersDisplay => HasComputers ? "S" : "N";

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

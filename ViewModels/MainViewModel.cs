using System.Windows.Controls;
using DistribuicaoTurmas.Views;

namespace DistribuicaoTurmas.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private UserControl _currentView;
        public UserControl CurrentView
        {
            get => _currentView;
            set { _currentView = value; Raise(); }
        }

        // Flags para marcar o botão ativo na sidebar
        private bool _isRoomsActive;
        public bool IsRoomsActive { get => _isRoomsActive; set { _isRoomsActive = value; Raise(); } }

        private bool _isClassesActive;
        public bool IsClassesActive { get => _isClassesActive; set { _isClassesActive = value; Raise(); } }

        private bool _isDistributionsActive;
        public bool IsDistributionsActive { get => _isDistributionsActive; set { _isDistributionsActive = value; Raise(); } }

        public RelayCommand GoRoomsCommand { get; }
        public RelayCommand GoClassesCommand { get; }
        public RelayCommand GoDistributionCommand { get; }

        public MainViewModel()
        {
            GoRoomsCommand = new RelayCommand(() =>
            {
                CurrentView = new RoomsPage();
                SetActive("rooms");
            });

            GoClassesCommand = new RelayCommand(() =>
            {
                CurrentView = new ClassesPage();
                SetActive("classes");
            });

            GoDistributionCommand = new RelayCommand(() =>
            {
                CurrentView = new DistributionPage();
                SetActive("dists");
            });

            // Tela inicial
            CurrentView = new UserControl
            {
                Content = new TextBlock
                {
                    Text = "Bem-vindo(a)! Use o menu à esquerda para começar.",
                    Margin = new System.Windows.Thickness(24),
                    FontSize = 18
                }
            };
            SetActive("none");
        }

        private void SetActive(string which)
        {
            IsRoomsActive = which == "rooms";
            IsClassesActive = which == "classes";
            IsDistributionsActive = which == "dists";
        }
    }
}

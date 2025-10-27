using System.Windows;
using DistribuicaoTurmas.ViewModels;

namespace DistribuicaoTurmas.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}

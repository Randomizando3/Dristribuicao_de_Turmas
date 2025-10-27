using System.Windows.Controls;
using DistribuicaoTurmas.ViewModels;

namespace DistribuicaoTurmas.Views
{
    public partial class ClassesPage : UserControl
    {
        public ClassesPage()
        {
            InitializeComponent();
            DataContext = new ClassesViewModel();
        }
    }
}

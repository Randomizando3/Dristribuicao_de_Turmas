using System.Windows.Controls;
using DistribuicaoTurmas.ViewModels;

namespace DistribuicaoTurmas.Views
{
    public partial class RoomsPage : UserControl
    {
        public RoomsPage()
        {
            InitializeComponent();
            DataContext = new RoomsViewModel();
        }
    }
}

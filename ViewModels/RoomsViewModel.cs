using System.Collections.ObjectModel;
using DistribuicaoTurmas.Models;
using DistribuicaoTurmas.Services;

namespace DistribuicaoTurmas.ViewModels
{
    public class RoomsViewModel : BaseViewModel
    {
        public ObservableCollection<Room> Rooms => StorageService.Instance.Rooms;
        public Room SelectedRoom { get; set; }
        public string[] SNOptions { get; } = new[] { "S", "N" };

        public RelayCommand AddRoomCommand { get; }
        public RelayCommand RemoveSelectedCommand { get; }

        public RoomsViewModel()
        {
            AddRoomCommand = new RelayCommand(() =>
            {
                Rooms.Add(new Room { Number = "Sala 101", Capacity = 40, HasComputers = false });
            });

            RemoveSelectedCommand = new RelayCommand(() =>
            {
                if (SelectedRoom != null)
                    Rooms.Remove(SelectedRoom);
            });
        }
    }
}

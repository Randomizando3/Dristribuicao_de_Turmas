using System.Collections.ObjectModel;
using DistribuicaoTurmas.Models;
using DistribuicaoTurmas.Services;
using DistribuicaoTurmas.Helpers;
using System.Collections.Generic;

namespace DistribuicaoTurmas.ViewModels
{
    public class ClassesViewModel : BaseViewModel
    {
        public ObservableCollection<ClassSection> Classes => StorageService.Instance.Classes;
        public ClassSection SelectedClass { get; set; }
        public string[] SNOptions { get; } = new[] { "S", "N" };
        public List<ScheduleSlot> AllSlots { get; } = SlotHelper.AllSlots;

        public RelayCommand AddClassCommand { get; }
        public RelayCommand RemoveSelectedCommand { get; }

        public ClassesViewModel()
        {
            AddClassCommand = new RelayCommand(() =>
            {
                Classes.Add(new ClassSection
                {
                    Name = "Cálculo I",
                    Enrolled = 40,
                    Teacher = "João",
                    NeedsComputers = false,
                    Slot1 = ScheduleSlot.Segunda_M1
                });
            });

            RemoveSelectedCommand = new RelayCommand(() =>
            {
                if (SelectedClass != null)
                    Classes.Remove(SelectedClass);
            });
        }
    }
}

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using DistribuicaoTurmas.Helpers;
using DistribuicaoTurmas.Models;
using Newtonsoft.Json;

namespace DistribuicaoTurmas.Services
{
    public class StorageService
    {
        private static readonly Lazy<StorageService> _lazy = new Lazy<StorageService>(() => new StorageService());
        public static StorageService Instance => _lazy.Value;

        public ObservableCollection<Room> Rooms { get; private set; } = new ObservableCollection<Room>();
        public ObservableCollection<ClassSection> Classes { get; private set; } = new ObservableCollection<ClassSection>();
        public ObservableCollection<DistributionSnapshot> Distributions { get; private set; } = new ObservableCollection<DistributionSnapshot>();

        private readonly string _appDir;
        private readonly string _roomsPath;
        private readonly string _classesPath;
        private readonly string _distsPath;

        // Debounce para reduzir escritas em disco
        private readonly Debouncer _roomsDebounce = new Debouncer(400);
        private readonly Debouncer _classesDebounce = new Debouncer(400);

        private StorageService()
        {
            _appDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DistribuicaoTurmas");
            Directory.CreateDirectory(_appDir);
            _roomsPath = Path.Combine(_appDir, "salas.json");
            _classesPath = Path.Combine(_appDir, "turmas.json");
            _distsPath = Path.Combine(_appDir, "distribuicoes.json");

            LoadRooms();
            LoadClasses();
            LoadDistributions();
        }

        #region Rooms
        public void LoadRooms()
        {
            UnwireRoomsHandlers();
            Rooms.Clear();

            if (File.Exists(_roomsPath))
            {
                var json = File.ReadAllText(_roomsPath);
                var list = JsonConvert.DeserializeObject<ObservableCollection<Room>>(json) ?? new ObservableCollection<Room>();
                foreach (var r in list) Rooms.Add(r);
            }
            else
            {
                Rooms.Add(new Room { Number = "101", Capacity = 40, HasComputers = true });
                Rooms.Add(new Room { Number = "201", Capacity = 30, HasComputers = false });
                SaveRooms();
            }

            WireRoomsHandlers();
        }

        public void SaveRooms()
        {
            var json = JsonConvert.SerializeObject(Rooms, Formatting.Indented);
            File.WriteAllText(_roomsPath, json);
        }

        private void WireRoomsHandlers()
        {
            Rooms.CollectionChanged += Rooms_CollectionChanged;
            foreach (var r in Rooms) r.PropertyChanged += Room_PropertyChanged;
        }

        private void UnwireRoomsHandlers()
        {
            Rooms.CollectionChanged -= Rooms_CollectionChanged;
            foreach (var r in Rooms) r.PropertyChanged -= Room_PropertyChanged;
        }

        private void Rooms_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (Room r in e.NewItems) r.PropertyChanged += Room_PropertyChanged;

            if (e.OldItems != null)
                foreach (Room r in e.OldItems) r.PropertyChanged -= Room_PropertyChanged;

            _roomsDebounce.Run(SaveRooms);
        }

        private void Room_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _roomsDebounce.Run(SaveRooms);
        }
        #endregion

        #region Classes
        public void LoadClasses()
        {
            UnwireClassesHandlers();
            Classes.Clear();

            if (File.Exists(_classesPath))
            {
                var json = File.ReadAllText(_classesPath);
                var list = JsonConvert.DeserializeObject<ObservableCollection<ClassSection>>(json) ?? new ObservableCollection<ClassSection>();
                foreach (var c in list) Classes.Add(c);
            }
            else
            {
                Classes.Add(new ClassSection { Name = "Física I", Enrolled = 35, Teacher = "Ana", NeedsComputers = false });
                SaveClasses();
            }

            WireClassesHandlers();
        }

        public void SaveClasses()
        {
            var json = JsonConvert.SerializeObject(Classes, Formatting.Indented);
            File.WriteAllText(_classesPath, json);
        }

        private void WireClassesHandlers()
        {
            Classes.CollectionChanged += Classes_CollectionChanged;
            foreach (var c in Classes) c.PropertyChanged += Class_PropertyChanged;
        }

        private void UnwireClassesHandlers()
        {
            Classes.CollectionChanged -= Classes_CollectionChanged;
            foreach (var c in Classes) c.PropertyChanged -= Class_PropertyChanged;
        }

        private void Classes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (ClassSection c in e.NewItems) c.PropertyChanged += Class_PropertyChanged;

            if (e.OldItems != null)
                foreach (ClassSection c in e.OldItems) c.PropertyChanged -= Class_PropertyChanged;

            _classesDebounce.Run(SaveClasses);
        }

        private void Class_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _classesDebounce.Run(SaveClasses);
        }
        #endregion

        #region Distributions
        public void LoadDistributions()
        {
            Distributions.Clear();
            if (File.Exists(_distsPath))
            {
                var json = File.ReadAllText(_distsPath);
                var list = JsonConvert.DeserializeObject<ObservableCollection<DistributionSnapshot>>(json) ?? new ObservableCollection<DistributionSnapshot>();
                foreach (var d in list.OrderByDescending(x => x.CreatedAt)) Distributions.Add(d);
            }
            else
            {
                SaveDistributions();
            }
        }

        public void SaveDistributions()
        {
            var json = JsonConvert.SerializeObject(Distributions, Formatting.Indented);
            File.WriteAllText(_distsPath, json);
        }

        public DistributionSnapshot AddSnapshot(AllocationResult result)
        {
            var next = NextSequenceNumber();
            var name = $"Distribuição{next:000}_{DateTime.Now:dd/MM/yyyy}";
            var snap = new DistributionSnapshot
            {
                Name = name,
                CreatedAt = DateTime.Now,
                Result = result
            };
            Distributions.Insert(0, snap);
            SaveDistributions();
            return snap;
        }

        public void DeleteSnapshot(DistributionSnapshot snap)
        {
            if (snap == null) return;
            Distributions.Remove(snap);
            SaveDistributions();
        }

        private int NextSequenceNumber()
        {
            int max = 0;
            foreach (var d in Distributions)
            {
                var name = d.Name ?? "";
                var under = name.IndexOf('_');
                if (name.StartsWith("Distribuição") && under > "Distribuição".Length)
                {
                    var numPart = name.Substring("Distribuição".Length, under - "Distribuição".Length);
                    if (int.TryParse(numPart, out int n) && n > max) max = n;
                }
            }
            return max + 1;
        }
        #endregion
    }
}

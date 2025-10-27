using System.Collections.ObjectModel;
using System.Windows; 
using DistribuicaoTurmas.Models;
using DistribuicaoTurmas.Services;

namespace DistribuicaoTurmas.ViewModels
{
    public class DistributionViewModel : BaseViewModel
    {
        private readonly StorageService _storage = StorageService.Instance;

        public ObservableCollection<DistributionSnapshot> Distributions => _storage.Distributions;

        private DistributionSnapshot _selectedSnapshot;
        public DistributionSnapshot SelectedSnapshot
        {
            get => _selectedSnapshot;
            set
            {
                _selectedSnapshot = value;
                Raise();
                if (value != null)
                {
                    Result = value.Result ?? new AllocationResult();
                    IsViewing = true; // entra no modo visualização
                    ErrorMessage = null;
                }
            }
        }

        private AllocationResult _result = new AllocationResult();
        public AllocationResult Result
        {
            get => _result;
            set { _result = value; Raise(); }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; Raise(); }
        }

        private bool _isViewing;
        public bool IsViewing
        {
            get => _isViewing;
            set { _isViewing = value; Raise(); }
        }

        public RelayCommand GenerateCommand { get; }
        public RelayCommand DeleteSelectedCommand { get; }
        public RelayCommand BackToListCommand { get; }

        public DistributionViewModel()
        {
            // ===== Gerar Distribuição =====
            GenerateCommand = new RelayCommand(() =>
            {
                var (ok, res, err) = DistributionService.Instance.Generate();
                Result = res ?? new AllocationResult();
                ErrorMessage = ok ? null : err;

                // Falhou → mostra mensagem detalhada
                if (!ok)
                {
                    MessageBox.Show(
                        err,
                        "Não foi possível gerar a distribuição",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }

                // Sucesso → salvar snapshot e exibir
                if (res != null)
                {
                    var snap = _storage.AddSnapshot(res);
                    SelectedSnapshot = snap;
                    IsViewing = true;
                }
            });

            // ===== Excluir Distribuição Selecionada =====
            DeleteSelectedCommand = new RelayCommand(() =>
            {
                if (SelectedSnapshot != null)
                {
                    _storage.DeleteSnapshot(SelectedSnapshot);
                    SelectedSnapshot = null;
                    Result = new AllocationResult();
                    IsViewing = false;
                }
            });

            // ===== Voltar à Lista =====
            BackToListCommand = new RelayCommand(() =>
            {
                SelectedSnapshot = null;
                IsViewing = false;
            });
        }
    }
}

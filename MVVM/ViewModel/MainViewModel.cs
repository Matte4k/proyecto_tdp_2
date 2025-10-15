using proyecto_tdp_2.MVVM.View;
using System.ComponentModel;
using System.Windows.Input;

namespace proyecto_tdp_2.MVVM.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {


        private static MainViewModel _instance = null!;
        private object _currentView = null!;

        public event PropertyChangedEventHandler? PropertyChanged;

        public static MainViewModel Instance => _instance ??= new MainViewModel();

        public object CurrentView
        {

            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged(nameof(CurrentView));
            }
        }

        public ICommand ShowDashboardCommand { get; } = null!;
        public ICommand ShowReclamosCommand { get; } = null!;


        public MainViewModel()
        {
            CurrentView = new ClaimView();

        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

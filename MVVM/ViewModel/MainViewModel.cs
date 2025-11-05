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
            ShowDashboardCommand = new RelayCommand(o => CurrentView = new DashboardView(Session.UserId, Session.Rol));
            ShowReclamosCommand = new RelayCommand(o => CurrentView = new MisReclamosView());
            CurrentView = new DashboardView(Session.UserId, Session.Rol);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

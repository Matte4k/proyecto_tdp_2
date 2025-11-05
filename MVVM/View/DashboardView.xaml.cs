using proyecto_tdp_2.MVVM.ViewModel;
using System.Windows.Controls;

namespace proyecto_tdp_2.MVVM.View
{

    public partial class DashboardView : UserControl
    {
        public DashboardView(int idOperador, string rol)
        {
            InitializeComponent();
            DataContext = new DashboardViewModel(idOperador, rol);
        }
    }
}

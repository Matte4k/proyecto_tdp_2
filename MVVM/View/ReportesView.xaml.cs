using proyecto_tdp_2.MVVM.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace proyecto_tdp_2.MVVM.View
{
    public partial class ReportesView : UserControl
    {
        public ReportesView()
        {
            InitializeComponent();
            DataContext = new ReportesViewModel();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ReportesViewModel;
            vm.GenerarReportePDF();
        }
    }

}

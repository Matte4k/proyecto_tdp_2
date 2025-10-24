using proyecto_tdp_2.Helpers;
using System.Windows;
using System.Windows.Controls;

namespace proyecto_tdp_2.MVVM.View
{
    public partial class DetalleReclamo : UserControl
    {

        public string NombreUsuario { get; set; } = string.Empty;
        public string RolUsuario { get; set; } = string.Empty;
        public DetalleReclamo()
        {
            InitializeComponent();

        }


        private void RadioButton_Checked_2(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Navigator.NavigateTo(new MisReclamosView());
        }
    }
}

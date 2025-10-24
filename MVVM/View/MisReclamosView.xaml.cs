using proyecto_tdp_2.Helpers;
using System.Windows;
using System.Windows.Controls;

namespace proyecto_tdp_2.MVVM.View
{
    public partial class MisReclamosView : UserControl
    {
        public string NombreUsuario { get; set; } = string.Empty;
        public string RolUsuario { get; set; } = string.Empty;
        public MisReclamosView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Navigator.NavigateTo(new DetalleReclamo());
        }

    }
}

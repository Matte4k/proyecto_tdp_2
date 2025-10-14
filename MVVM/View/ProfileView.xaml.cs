using System.Windows;
using System.Windows.Controls;

namespace proyecto_tdp_2.MVVM.View
{
    public partial class ProfileView : UserControl
    {
        public ProfileView()
        {
            InitializeComponent();

            txtNombre.Text = $"{Session.Nombre} {Session.Apellido}";
            txtRol.Text = Session.Rol;
            txtCorreo.Text = Session.Correo;
            txtTelefono.Text = Session.Telefono;
            txtServicio.Text = Session.Servicio;
            txtProvincia.Text = Session.Provincia;
            txtDni.Text = Session.Dni;
            txtCuit.Text = Session.Cuit;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            EditProfileView editar = new EditProfileView();
            editar.Show();
        }
    }
}

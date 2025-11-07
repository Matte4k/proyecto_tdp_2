using System.Configuration;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace proyecto_tdp_2.MVVM.View
{
    public partial class UserManagementView : UserControl
    {
        string connectionString = ConfigurationManager.ConnectionStrings["MiReclamoDB"].ConnectionString;
        int idSesion;
        string rolSesion;

        public UserManagementView(int idUsuarioSesion, string rol)
        {
            InitializeComponent();
            idSesion = idUsuarioSesion;
            rolSesion = rol;
            CargarUsuarios();
        }

        private void CargarUsuarios()
        {
            List<UsuarioItem> lista = new List<UsuarioItem>();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string query = rolSesion == "SuperAdmin"
                    ? @"SELECT U.id_usuario, U.nombre, U.apellido, R.nombre AS rol, U.estado
                        FROM Usuario U
                        JOIN Roles R ON R.id_rol = U.id_rol"
                    : @"SELECT U.id_usuario, U.nombre, U.apellido, R.
                        nombre AS rol, U.estado
                        FROM Usuario U
                        JOIN Roles R ON R.id_rol = U.id_rol
                        WHERE U.id_usuario IN (
                            SELECT id_operador FROM SupervisorOperador WHERE id_supervisor = @idSupervisor
                        )";

                SqlCommand cmd = new SqlCommand(query, con);
                if (rolSesion != "SuperAdmin")
                    cmd.Parameters.AddWithValue("@idSupervisor", idSesion);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lista.Add(new UsuarioItem
                    {
                        Id = reader.GetInt32(0),
                        Nombre = reader.GetString(1),
                        Apellido = reader.GetString(2),
                        Rol = reader.GetString(3),
                        Estado = reader.GetBoolean(4)
                    });
                }
            }

            UsuariosList.ItemsSource = lista;
            txtCantidad.Text = $"Total usuarios: {lista.Count}";
        }

        private void BtnDesactivar_Click(object sender, RoutedEventArgs e)
        {
            var seleccionado = (UsuarioItem)UsuariosList.SelectedItem;
            if (seleccionado == null)
            {
                MessageBox.Show("Seleccione un usuario para dar de baja.", "Atención");
                return;
            }

            CambiarEstado(seleccionado.Id, false);
        }

        private void BtnActivar_Click(object sender, RoutedEventArgs e)
        {
            var seleccionado = (UsuarioItem)UsuariosList.SelectedItem;
            if (seleccionado == null)
            {
                MessageBox.Show("Seleccione un usuario para activar.", "Atención");
                return;
            }

            CambiarEstado(seleccionado.Id, true);
        }

        private void CambiarEstado(int idUsuario, bool nuevoEstado)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("UPDATE Usuario SET estado = @estado WHERE id_usuario = @id", con);
                cmd.Parameters.AddWithValue("@estado", nuevoEstado);
                cmd.Parameters.AddWithValue("@id", idUsuario);
                cmd.ExecuteNonQuery();
            }

            CargarUsuarios();
        }
    }

    public class UsuarioItem
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Rol { get; set; }
        public bool Estado { get; set; }

        public string EstadoTexto => Estado ? "Activo" : "Inactivo";
        public Brush EstadoColor => Estado ? Brushes.Green : Brushes.Red;
    }
}

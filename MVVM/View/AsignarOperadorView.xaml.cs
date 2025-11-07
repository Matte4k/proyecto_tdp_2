using System.Configuration;
using System.Data.SqlClient;
using System.Windows;

namespace proyecto_tdp_2.MVVM.View
{
    public partial class AsignarOperadorViewe : Window
    {
        public int? OperadorSeleccionadoId { get; private set; }

        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MiReclamoDB"].ConnectionString;

        public AsignarOperadorViewe(int idReclamo, string rolUsuario, int idSupervisor)
        {
            InitializeComponent();
            CargarOperadores(rolUsuario, idSupervisor);
        }

        private void CargarOperadores(string rolUsuario, int idSupervisor)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "";

                    if (rolUsuario == "SuperAdmin")
                    {
                        query = "SELECT id_usuario, nombre + ' ' + apellido AS nombre FROM Usuario WHERE id_rol = 2";
                    }
                    else if (rolUsuario == "Supervisor")
                    {
                        query = @"
                        SELECT u.id_usuario, u.nombre + ' ' + u.apellido AS nombre
                        FROM Usuario u
                        INNER JOIN SupervisorOperador so ON u.id_usuario = so.id_operador
                        WHERE so.id_supervisor = @idSupervisor";
                    }

                    SqlCommand cmd = new SqlCommand(query, conn);
                    if (rolUsuario == "Supervisor")
                        cmd.Parameters.AddWithValue("@idSupervisor", idSupervisor);

                    SqlDataReader reader = cmd.ExecuteReader();
                    var operadores = new List<(int id, string nombre)>();
                    while (reader.Read())
                    {
                        operadores.Add((reader.GetInt32(0), reader.GetString(1)));
                    }

                    comboOperadores.ItemsSource = operadores;
                    comboOperadores.DisplayMemberPath = "nombre";
                    comboOperadores.SelectedValuePath = "id";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar operadores: {ex.Message}");
            }
        }
        private void BtnAsignar_Click(object sender, RoutedEventArgs e)
        {
            if (comboOperadores.SelectedValue != null)
            {
                OperadorSeleccionadoId = (int)comboOperadores.SelectedValue;
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Debes seleccionar un operador.");
            }
        }
        private void TopBar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                this.DragMove();
        }

    }

}

using System.Configuration;
using System.Data.SqlClient;
using System.Windows;

namespace proyecto_tdp_2.MVVM.View
{
    public partial class AsignarOperadorView : Window
    {
        public int? OperadorSeleccionadoId { get; private set; }

        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MiReclamoDB"].ConnectionString;
        private readonly int idReclamo;
        private readonly string rolUsuario;
        private readonly int idSupervisor;

        public AsignarOperadorView(int idReclamo, string rolUsuario, int idSupervisor)
        {
            InitializeComponent();

            this.idReclamo = idReclamo;
            this.rolUsuario = rolUsuario;
            this.idSupervisor = idSupervisor;

            CargarOperadores();
        }

        private class Operador
        {
            public int Id { get; set; }
            public string Nombre { get; set; }
        }

        private void CargarOperadores()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = "";

                    if (rolUsuario == "SuperAdmin")
                    {
                        query = @"
                            SELECT id_usuario, nombre + ' ' + apellido AS nombre
                            FROM Usuario
                            WHERE id_rol = (SELECT id_rol FROM Roles WHERE nombre = 'Operador')
                              AND estado = 1
                            ORDER BY nombre";
                    }
                    else if (rolUsuario == "Supervisor")
                    {
                        query = @"
                            SELECT u.id_usuario, u.nombre + ' ' + u.apellido AS nombre
                            FROM Usuario u
                            INNER JOIN SupervisorOperador so ON u.id_usuario = so.id_operador
                            WHERE so.id_supervisor = @idSupervisor
                              AND u.estado = 1
                            ORDER BY u.nombre";
                    }

                    SqlCommand cmd = new SqlCommand(query, conn);
                    if (rolUsuario == "Supervisor")
                        cmd.Parameters.AddWithValue("@idSupervisor", idSupervisor);

                    SqlDataReader reader = cmd.ExecuteReader();
                    var operadores = new List<Operador>();

                    while (reader.Read())
                    {
                        operadores.Add(new Operador
                        {
                            Id = reader.GetInt32(0),
                            Nombre = reader.GetString(1)
                        });
                    }

                    reader.Close();

                    if (operadores.Count == 0)
                    {
                        MessageBox.Show("No se encontraron operadores disponibles para asignar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    comboOperadores.ItemsSource = operadores;
                    comboOperadores.DisplayMemberPath = "Nombre";
                    comboOperadores.SelectedValuePath = "Id";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar operadores: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAsignar_Click(object sender, RoutedEventArgs e)
        {
            if (comboOperadores.SelectedValue == null)
            {
                MessageBox.Show("Debes seleccionar un operador.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int operadorSeleccionado = (int)comboOperadores.SelectedValue;

            if (rolUsuario == "Supervisor" && !EsOperadorDelSupervisor(operadorSeleccionado))
            {
                MessageBox.Show("No puedes asignar reclamos a un operador que no esté bajo tu supervisión.", "Acceso denegado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (AsignarReclamoAOperador(idReclamo, operadorSeleccionado))
            {
                OperadorSeleccionadoId = operadorSeleccionado;
                MessageBox.Show("Reclamo asignado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
        }

        private bool EsOperadorDelSupervisor(int idOperador)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT COUNT(*) FROM SupervisorOperador WHERE id_supervisor = @idSupervisor AND id_operador = @idOperador";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@idSupervisor", idSupervisor);
                cmd.Parameters.AddWithValue("@idOperador", idOperador);

                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        private bool AsignarReclamoAOperador(int idReclamo, int idOperador)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string checkQuery = "SELECT COUNT(*) FROM AsignacionReclamo WHERE reclamo_asignado = @idReclamo";
                    SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                    checkCmd.Parameters.AddWithValue("@idReclamo", idReclamo);
                    int existe = (int)checkCmd.ExecuteScalar();

                    string query;
                    if (existe > 0)
                    {
                        query = @"
                            UPDATE AsignacionReclamo
                            SET usuario_asignado = @idOperador
                            WHERE reclamo_asignado = @idReclamo";
                    }
                    else
                    {
                        query = @"
                            INSERT INTO AsignacionReclamo (reclamo_asignado, usuario_asignado)
                            VALUES (@idReclamo, @idOperador)";
                    }

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@idReclamo", idReclamo);
                    cmd.Parameters.AddWithValue("@idOperador", idOperador);

                    int rows = cmd.ExecuteNonQuery();
                    return rows > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al asignar reclamo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void TopBar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                this.DragMove();
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

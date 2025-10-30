using proyecto_tdp_2.Helpers;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace proyecto_tdp_2.MVVM.View
{
    public partial class DetalleReclamo : UserControl
    {

        public string NombreUsuario { get; set; } = string.Empty;
        public string RolUsuario { get; set; } = string.Empty;

        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MiReclamoDB"].ConnectionString;
        public DetalleReclamo()
        {
            InitializeComponent();

            Loaded += DetalleReclamo_Loaded;
        }

        private void DetalleReclamo_Loaded(object sender, RoutedEventArgs e)
        {
            if (Tag is int idReclamo)
                CargarReclamo(idReclamo);
        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            Navigator.NavigateTo(new MisReclamosView());
        }

        private void CargarReclamo(int idReclamo)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"
                        SELECT r.id_reclamo, r.descripcion, r.fecha_creacion, 
                               tr.nombre AS tipo, u.direccion AS ubicacion, 
                               e.nombre AS estado, c.nombre + ' ' + c.apellido AS cliente, 
                               c.email, c.telefono, p.nombre AS prioridad,
                               he.fecha_cambio, he.comentario,
                               us.nombre + ' ' + us.apellido AS usuario,
                               ur.nombre + ' ' + ur.apellido AS usuario_responsable
                        FROM Reclamos r
                        INNER JOIN TipoReclamo tr ON r.tipo_reclamo = tr.id_tipo
                        INNER JOIN Prioridades p ON r.prioridad = p.id_prioridad
                        INNER JOIN Ubicacion u ON r.id_zona = u.id_zona
                        INNER JOIN Estados e ON r.id_estado = e.id_estado
                        INNER JOIN Clientes c ON r.cliente_reclamo = c.id_cliente
                        LEFT JOIN (
                            SELECT h1.id_reclamo, h1.fecha_cambio, h1.comentario, h1.id_operador
                            FROM HistorialEstado h1
                            WHERE h1.fecha_cambio = (
                                SELECT MAX(h2.fecha_cambio)
                                FROM HistorialEstado h2
                                WHERE h2.id_reclamo = h1.id_reclamo
                            )
                        ) he ON r.id_reclamo = he.id_reclamo
                        LEFT JOIN AsignacionReclamo ar ON r.id_reclamo = ar.reclamo_asignado
                        LEFT JOIN Usuario ur ON ar.usuario_asignado = ur.id_usuario
                        LEFT JOIN Usuario us ON he.id_operador = us.id_usuario
                        WHERE r.id_reclamo = @id;";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", idReclamo);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtTitulo.Text = $"Detalles del Reclamo #{reader["id_reclamo"]}";
                                txtIdReclamo.Text = $"#{reader["id_reclamo"]}";
                                txtDescripcion.Text = reader["descripcion"].ToString();
                                txtTipo.Text = reader["tipo"].ToString();
                                txtZona.Text = reader["ubicacion"].ToString();
                                txtPrioridad.Text = reader["prioridad"].ToString();
                                txtEstado.Text = reader["estado"].ToString();
                                txtCliente.Text = reader["cliente"].ToString();
                                txtEmail.Text = reader["email"].ToString();
                                txtTelefono.Text = reader["telefono"].ToString();

                                txtFechaCreacion.Text = Convert.ToDateTime(reader["fecha_creacion"]).ToString("dd/MM/yyyy HH:mm");

                                if (reader["fecha_cambio"] != DBNull.Value)
                                {
                                    txtUltimaActualizacion.Text = Convert.ToDateTime(reader["fecha_cambio"]).ToString("dd/MM/yyyy HH:mm");
                                    txtComentarioHistorial.Text = reader["comentario"]?.ToString();
                                    txtOperador.Text = reader["usuario_responsable"]?.ToString();
                                }
                                else
                                {
                                    txtUltimaActualizacion.Text = "Sin modificaciones";
                                    txtComentarioHistorial.Text = "";
                                    txtOperador.Text = reader["usuario_responsable"]?.ToString();
                                }
                            }
                            else
                            {
                                MessageBox.Show("No se encontró el reclamo solicitado.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los detalles del reclamo: {ex.Message}");
            }
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

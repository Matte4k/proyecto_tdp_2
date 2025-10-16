using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace proyecto_tdp_2.MVVM.View
{
    public partial class NewType : UserControl
    {
        string connectionString = ConfigurationManager.ConnectionStrings["MiReclamoDB"].ConnectionString;
        public NewType()
        {
            InitializeComponent();
            if (Session.Rol != "SuperAdmin")
            {
                MessageBox.Show("No tiene permisos para crear tipos de reclamos.", "Acceso denegado",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                this.IsEnabled = false;
                return;
            }
        }

        private void CargarTiposPadre()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT id_tipo, nombre FROM TipoReclamo WHERE subtipo_reclamo IS NULL";
                    SqlCommand cmd = new SqlCommand(query, conn);

                    DataTable dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());

                    TypeCombo.ItemsSource = dt.DefaultView;
                    TypeCombo.DisplayMemberPath = "nombre";
                    TypeCombo.SelectedValuePath = "id_tipo";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar tipos: {ex.Message}");
            }
        }

        private void CbIsSubType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbIsSubType.SelectedItem is ComboBoxItem item)
            {
                Type.Visibility = item.Content.ToString() == "Si" ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void BtnCrear_Click(object sender, RoutedEventArgs e)
        {
            string nombre = NombreReclamo.Text.Trim();
            string descripcion = DescReclamo.Text.Trim();
            bool esSubtipo = ((ComboBoxItem)cbIsSubType.SelectedItem).Content.ToString() == "Si";
            int? idPadre = null;

            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(descripcion))
            {
                MessageBox.Show("Debe completar todos los campos obligatorios.", "Validación",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (esSubtipo && TypeCombo.SelectedValue != null)
                idPadre = Convert.ToInt32(TypeCombo.SelectedValue);

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string nombre = NombreReclamo.Text.Trim();
                    string descripcion = DescReclamo.Text.Trim();

                    conn.Open();
                    SqlCommand cmd = new SqlCommand("sp_InsertarTipoReclamo", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@id_usuario", Session.UserId);
                    cmd.Parameters.AddWithValue("@nombre", nombre);
                    cmd.Parameters.AddWithValue("@descripcion", descripcion);
                    cmd.Parameters.Add("@subtipo_reclamo", SqlDbType.Int).Value = (object?)idPadre ?? DBNull.Value;

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Tipo de reclamo creado correctamente.", "Éxito",
                                    MessageBoxButton.OK, MessageBoxImage.Information);

                    LimpiarCampos();
                    CargarTiposPadre();
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error SQL: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnBorrarTodo_Click(object sender, RoutedEventArgs e)
        {
            LimpiarCampos();
        }

        private void LimpiarCampos()
        {
            NombreReclamo.Text = string.Empty;
            DescReclamo.Text = string.Empty;
            cbIsSubType.SelectedIndex = 0;
            TypeCombo.SelectedIndex = -1;
            Type.Visibility = Visibility.Collapsed;
        }
    }
}

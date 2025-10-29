using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace proyecto_tdp_2.MVVM.View
{

    public partial class ClientesView : UserControl
    {
        string connectionString = ConfigurationManager.ConnectionStrings["MiReclamoDB"].ConnectionString;
        private DataTable _clientesDT = new DataTable();
        public ClientesView()
        {
            InitializeComponent();
            CargarClientes();
        }

        int paginaActual = 1;
        int registrosPorPagina = 5;

        private void CargarClientes()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT id_cliente, nombre, apellido, telefono, cuit, dni, email, estado 
                             FROM Clientes
                             WHERE estado = 1";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    _clientesDT.Clear();
                    _clientesDT.Load(cmd.ExecuteReader());
                    dgClientes.ItemsSource = _clientesDT.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar clientes: {ex.Message}");
            }
        }

        private void MostrarPagina(int numeroPagina)
        {
            int totalRegistros = _clientesDT.Rows.Count;
            int inicio = (numeroPagina - 1) * registrosPorPagina;

            if (inicio >= totalRegistros)
            {
                return;
            }

            DataTable paginaDT = _clientesDT.AsEnumerable()
                .Skip(inicio)
                .Take(registrosPorPagina)
                .CopyToDataTable();

            dgClientes.ItemsSource = paginaDT.DefaultView;

            txtPagina.Text = $"Página {paginaActual}";
        }


        private void BtnAnterior_Click(object sender, RoutedEventArgs e)
        {
            if (paginaActual > 1)
            {
                paginaActual--;
                MostrarPagina(paginaActual);
            }
        }

        private void BtnSiguiente_Click(object sender, RoutedEventArgs e)
        {
            int totalPaginas = (int)Math.Ceiling((double)_clientesDT.Rows.Count / registrosPorPagina);
            if (paginaActual < totalPaginas)
            {
                paginaActual++;
                MostrarPagina(paginaActual);
            }
        }

        private void BtnBuscar_Click(object sender, RoutedEventArgs e)
        {
            string dni = txtBuscarDni.Text.Trim();

            if (string.IsNullOrEmpty(dni))
            {
                dgClientes.ItemsSource = _clientesDT.DefaultView;
                return;
            }

            DataView dv = new DataView(_clientesDT);
            dv.RowFilter = $"Convert(dni, 'System.String') LIKE '%{dni}%'";
            dgClientes.ItemsSource = dv;
        }

        private void BtnCrearNuevoCliente_Click(object sender, RoutedEventArgs e)
        {
            RegisterClientView CrearCliente = new RegisterClientView();
            CrearCliente.Show();
        }

        private void BtnBorrar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btn && btn.DataContext is DataRowView rowView)
                {
                    int idCliente = Convert.ToInt32(rowView["id_cliente"]);
                    bool estado = Convert.ToBoolean(rowView["estado"]);

                    string mensaje = estado
                        ? "¿Está seguro que desea dar de baja este cliente?"
                        : "¿Desea reactivar (dar de alta) este cliente?";

                    string titulo = estado ? "Dar de baja cliente" : "Dar de alta cliente";

                    MessageBoxResult result = MessageBox.Show(mensaje, titulo, MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        using (SqlConnection conn = new SqlConnection(connectionString))
                        {
                            conn.Open();
                            string query = "UPDATE Clientes SET estado = @nuevoEstado WHERE id_cliente = @id";
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@nuevoEstado", !estado);
                                cmd.Parameters.AddWithValue("@id", idCliente);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        string accion = estado ? "baja" : "alta";
                        MessageBox.Show($"El cliente ha sido dado de {accion} correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                        CargarClientes();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cambiar el estado del cliente: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is DataRowView rowView)
            {
                int idCliente = Convert.ToInt32(rowView["id_cliente"]);

                RegisterClientView editWindow = new RegisterClientView();

                // Pre-carga de datos
                editWindow.tbFullName.Text = rowView["nombre"].ToString();
                editWindow.tbSurName.Text = rowView["apellido"].ToString();
                editWindow.tbPhone.Text = rowView["telefono"].ToString();
                editWindow.tbCUIT.Text = rowView["cuit"].ToString();
                editWindow.tbCUIT.IsEnabled = false; // no editable
                editWindow.tbDNI.Text = rowView["dni"].ToString();
                editWindow.tbDNI.IsEnabled = false; // no editable
                editWindow.tbEmail.Text = rowView["email"].ToString();

                editWindow.Title = "Editar Cliente";
                editWindow.ShowDialog();

                // si la ventana se cerró con cambios, recargamos la lista
                if (editWindow.DialogResult == true)
                {
                    CargarClientes();
                }
            }
        }

        private void FiltroChanged(object sender, RoutedEventArgs e)
        {
            CargarClientesInactivos();
        }

        private void CargarClientesInactivos()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT id_cliente, nombre, apellido, telefono, cuit, dni, email, estado FROM Clientes";

                    if (chkIncluirInactivos.IsChecked == false)
                        query += " WHERE estado = 1";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    _clientesDT.Clear();
                    _clientesDT.Load(cmd.ExecuteReader());
                    dgClientes.ItemsSource = _clientesDT.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar clientes: {ex.Message}");
            }
        }

    }


}

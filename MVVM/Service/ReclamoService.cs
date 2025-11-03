using proyecto_tdp_2.MVVM.Model;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace proyecto_tdp_2.Service
{
    public class ReclamoService
    {
        string connectionString = ConfigurationManager.ConnectionStrings["MiReclamoDB"].ConnectionString;

        public List<Reclamo> ObtenerReclamos(string dni = "", string tipo = "Todos", string estado = "Todos", string prioridad = "Todos")
        {
            List<Reclamo> reclamos = new();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = @"
        SELECT 
            r.id_reclamo,
            r.descripcion,
            U.direccion,
            P.nombre AS prioridad,
            T.nombre AS tipo_reclamo,
            E.nombre AS estado,
            r.fecha_creacion,
            C.dni AS dni_cliente,
            C.nombre + ' ' + C.apellido AS cliente
        FROM Reclamos r
        INNER JOIN Clientes C ON r.cliente_reclamo = C.id_cliente
        INNER JOIN Prioridades P ON r.prioridad = P.id_prioridad
        INNER JOIN TipoReclamo T ON r.tipo_reclamo = T.id_tipo
        INNER JOIN Estados E ON r.id_estado = E.id_estado
        INNER JOIN Ubicacion U ON r.id_zona = U.id_zona
        WHERE (@dni IS NULL OR C.dni = @dni)
          AND (@tipo IS NULL OR T.nombre = @tipo)
          AND (@prioridad IS NULL OR P.nombre = @prioridad)
        ORDER BY r.fecha_creacion DESC
        OFFSET 0 ROWS; -- opcional: quité TOP 100 para que pagines en otro lado; si querés TOP 100, agregalo aquí
    ";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    // DNI: si viene vacío o null -> DbNull (no filtrar). Si viene valor -> intentar parsear a long.
                    if (string.IsNullOrWhiteSpace(dni) || !long.TryParse(dni, out long dniParsed))
                    {
                        cmd.Parameters.Add("@dni", SqlDbType.BigInt).Value = DBNull.Value;
                    }
                    else
                    {
                        cmd.Parameters.Add("@dni", SqlDbType.BigInt).Value = dniParsed;
                    }

                    if (string.IsNullOrWhiteSpace(tipo) || tipo.Equals("Todos", StringComparison.OrdinalIgnoreCase))
                        cmd.Parameters.AddWithValue("@tipo", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("@tipo", tipo);

                    if (string.IsNullOrWhiteSpace(prioridad) || prioridad.Equals("Todos", StringComparison.OrdinalIgnoreCase))
                        cmd.Parameters.AddWithValue("@prioridad", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("@prioridad", prioridad);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var reclamo = new Reclamo();


                            if (!reader.IsDBNull(0)) reclamo.IdReclamo = reader.GetInt32(0);
                            if (!reader.IsDBNull(1)) reclamo.Descripcion = reader.GetString(1);
                            if (!reader.IsDBNull(2)) reclamo.Direccion = reader.GetString(2);
                            if (!reader.IsDBNull(3)) reclamo.Prioridad = reader.GetString(3);
                            if (!reader.IsDBNull(4)) reclamo.Tipo = reader.GetString(4);
                            if (!reader.IsDBNull(5)) reclamo.Estado = reader.GetString(5);
                            if (!reader.IsDBNull(6)) reclamo.FechaCreacion = reader.GetDateTime(6);
                            if (!reader.IsDBNull(7)) reclamo.DniCliente = reader.GetInt64(7).ToString();
                            if (!reader.IsDBNull(8)) _ = reader.GetString(8);

                            reclamos.Add(reclamo);
                        }
                    }
                }
            }


            return reclamos;
        }
    }
}

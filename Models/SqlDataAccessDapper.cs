using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System.Data;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Text;
namespace ApiPractice.Models {
    public class SqlDataAccessDapper {

        public static string cnxn { set; get; } = "";

        public static IConfiguration Settings;
        
        private static string resuelvecadenapassword(string mac, string cadenaresBase64) {
            string cadenares = "";
            byte[] bytes = Convert.FromBase64String(cadenaresBase64);
            cadenares = Encoding.UTF8.GetString(bytes);
            cadenares = cadenares.Length >= 8
            ? cadenares.Substring(cadenares.Length - 4) +
              cadenares.Substring(4, cadenares.Length - 8) +
              cadenares.Substring(0, 4)
            : cadenares;
            var e01 = new encripta();
            string llave01 = KeyGenerator.GenerarClave(mac);
            cadenares = e01.DecryptString128Bit(cadenares, llave01);
            return cadenares;
        }

        /// <Desarrollo>TekLink</Desarrollo>
        /// <revision>2026-07-13</revision>
        /// <summary>
        /// return the connection string  choosing between production and test
        /// </summary>
        /// <param name="cnx"></param>
        /// <returns>Conection String</returns>
        public static string GetConnection(int cnx) {
            if (cnxn.Length > 0)
                return cnxn;

            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).AddEnvironmentVariables();

            Settings = builder.Build();
            //return cnxn = resuelvecadenapassword(  Settings.GetConnectionString("ProdConnection"), "PassConnString#2358");
            return cnxn = Settings.GetConnectionString("ProdConnection");

        }

        /// <Desarrollo>TekLink</Desarrollo>
        /// <revision>2026-07-13</revision>
        /// <summary>
        /// Runs a SQL command and returns the result as a JSON string
        /// </summary>
        /// <param name="Command"></param>
        /// <returns>Data as a Json String</returns>
        public async static Task<string> ExecuteCommandJson(string Command) {
            try {
                string ConText = GetConnection(0);
                using (SqlConnection conn = new SqlConnection(ConText)) {
                    using (SqlCommand cmd = new SqlCommand(Command, conn)) {
                        conn.Open();
                        using (SqlDataReader rdr = await cmd.ExecuteReaderAsync()) {
                            DataTable dataTable = new DataTable();
                            dataTable.Load(rdr);
                            string json = JsonConvert.SerializeObject(dataTable);
                            return json;
                        }
                    }
                }

            } catch {
                throw;
            }
        }

        /// <Desarrollo>TekLink</Desarrollo>
        /// <revision>2026-07-13</revision>
        /// <summary>
        /// This Function Returns "" if the query is executed successfully, otherwise it throws the exception  as a string
        /// </summary>
        /// <param name="query"></param>
        /// <returns>1 o el error relacionado a la consulta</returns>
        public async static Task<string> ExecuteQuery(string query) {
            try {
                string ConText = GetConnection(0);
                using (SqlConnection conn = new SqlConnection(ConText)) {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    using (SqlCommand cmd = new SqlCommand(query, conn)) {
                        cmd.Transaction = transaction;
                        try {
                            var ans = await cmd.ExecuteNonQueryAsync();
                            transaction.Commit();
                            return "";
                        } catch {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            } catch {
                throw;
            }

        }

        /// <Desarrollo>TekLink</Desarrollo>
        /// <revision>2026-07-13</revision>
        /// <summary>
        /// Ejecuta una consulta y se extrae sun informacion como datatable
        /// </summary>
        /// <param name="command"></param>
        /// <returns>Resultado de la consulta en formato de un DataTable/returns>
        public static async Task<DataTable> ExtractDataTable(string command) {
            try {
                Debug.WriteLine(command);
                string ConText = GetConnection(0);
                using (SqlConnection conn = new SqlConnection(ConText)) {
                    using (SqlCommand cmd = new SqlCommand(command, conn)) {
                        conn.Open();
                        using (SqlDataReader rdr = await cmd.ExecuteReaderAsync()) {
                            DataTable dataTable = new DataTable();
                            dataTable.Load(rdr);
                            return dataTable;
                        }
                    }
                }
            } catch {
                throw;
            }
        }

        /// <Desarrollo>TekLink</Desarrollo>
        /// <revision>2026-07-13</revision>
        /// <summary>
        /// Ejecuta una consulta y convierte el resultado a una lista de Clase T 
        /// </summary>
        /// <typeparam name="T">tipos de clase relacionada a la consulta</typeparam>
        /// <param name="command">Consulta a ejecutar en la BD</param>
        /// <returns>una lista de cualquier clase en </returns>
        public async static Task<List<T>> ExecuteCommandClassList<T>(string command) where T : new() {
            try {
                Debug.WriteLine(command);
                string ConText = GetConnection(0);
                DataTable dataTable = new DataTable();

                using (SqlConnection conn = new SqlConnection(ConText)) {
                    using (SqlCommand cmd = new SqlCommand(command, conn)) {
                        conn.Open();
                        using (SqlDataReader rdr = await cmd.ExecuteReaderAsync()) {
                            dataTable.Load(rdr);

                        }
                    }
                }

                List<T> List = new List<T>();
                foreach (DataRow row in dataTable.Rows) {
                    T item = new T();
                    foreach (DataColumn col in dataTable.Columns) {
                        var property = typeof(T).GetProperty(col.ColumnName);
                        if (property != null && row[col] != DBNull.Value) {
                            property.SetValue(item, row[col]);
                        }
                    }
                    List.Add(item);
                }
                return List;

            } catch (Exception ex) {
                Debug.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        /// <Desarrollo>TekLink</Desarrollo>
        /// <revision>2026-07-13</revision>
        /// <summary>
        /// Ejecuta  un sp Con los parametros enviados en un diccionario
        /// </summary>
        /// <param name="StoreProcedure"></param>
        /// <param name="Parameters"></param>
        /// <returns>Datatable </returns>
        public static async Task<DataTable> SpParametersCatchDataTable(string StoreProcedure, Dictionary<string, object> Parameters) {
            try {

                Debug.WriteLine("Store Procedure: " + StoreProcedure);
                string ConText = GetConnection(0);
                DataTable dataTable = new DataTable();
                ShowExecCallSP(StoreProcedure, Parameters);
                using (SqlConnection conn = new SqlConnection(ConText)) {
                    using (SqlCommand cmd = new SqlCommand(StoreProcedure, conn)) {
                        cmd.CommandType = CommandType.StoredProcedure;
                        conn.Open();
                        foreach (var param in Parameters) {

                            cmd.Parameters.AddWithValue(param.Key, param.Value);
                        }

                        using (SqlDataReader rdr = await cmd.ExecuteReaderAsync()) {
                            dataTable.Load(rdr);
                        }
                    }
                }
                    
                return dataTable; 
            } catch {
                return new DataTable();
                
            }
        }

        /// <Desarrollo>TekLink</Desarrollo>
        /// <revision>2026-07-13</revision>
        /// <summary>
        /// muestra el texto de la llamada de un sp para fines de Debug
        /// </summary>
        /// <param name="storeProcedure"></param>
        /// <param name="parameters"></param>
        private static void ShowExecCallSP(string storeProcedure, Dictionary<string, object> parameters) {
            StringBuilder sb = new StringBuilder();
            sb.AppendJoin(" ","EXEC ", storeProcedure);
            foreach (var param in parameters){
                sb.Append($" {param.Key} = '{param.Value}',");
            }
            Console.WriteLine(sb.ToString().TrimEnd(','));  

        }
        
        /// <Desarrollo>TekLink</Desarrollo>
        /// <revision>2026-07-13</revision>
        /// <summary>
        /// Envia y carga la informacion de una tabla a partir de su nombre y un DataTable
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public async static Task SendTable(DataTable dt, string TableName) {
            string ConText = GetConnection(0);
            using (SqlConnection conn = new SqlConnection( ConText )) {
                try {
                    await conn.OpenAsync();
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn)) {
                        bulkCopy.DestinationTableName = TableName;
                        // Write from the source to the destination
                        await bulkCopy.WriteToServerAsync(dt);
                    }
                    return;
                } catch {
                    throw;
                }
            }
        }

    }
}

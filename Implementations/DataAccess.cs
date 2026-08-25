using ApiPractice.Interfaces;
using ApiPractice.Models;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace ApiPractice.Implementations {
    public class DataAccess : IDataAccess {
        public async Task<string> ExtractDataParts(string select, string from, string where) {
            try {
                string Command = @$"select {select} from {from} {where}";
                string json = await SqlDataAccessDapper.ExecuteCommandJson(Command);
                return json;
            } catch {
                throw;
            }
        }

        public async Task<int> GetAccessUser(LoginData user) {
            try {

                if (user.vchHeaderUser.CompareTo(user.vchUser) != 0 || user.vchHeaderMacAddress.CompareTo(user.vchMacAddress) != 0)
                    throw new Exception("Los Datos no se relacionan con el Token");

                string Command = @$"select ide from  vwckadmusuarios with(nolock) where usuario = '{user.vchUser}' and activo =1";
                var dt2 = await SqlDataAccessDapper.ExtractDataTable(Command);

                return dt2.Rows.Count >= 1 ? Convert.ToInt32(dt2.Rows[0]["ide"]) : -1;

            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        
        public async Task<List<TableDefinition>> GetTableDefinition(string TableName) {
            try {
                List<TableDefinition> ans = new List<TableDefinition>();
                string Command = @$"select campo,titulo,tagname,esllave,escatalogo,fkckadmtipovalor from pkadmextraccion where tabla ='{TableName}'";
                ans = await SqlDataAccessDapper.ExecuteCommandClassList<TableDefinition>(Command);

                return ans;
            } catch {
                throw;
            }
        }

        /// <Desarrollo>TekLink</Desarrollo>
        /// <revision>2026-07-13</revision>
        /// <summary>
        /// Ejecuta la Consulta enviada
        /// </summary>
        /// <param name="query"></param>
        /// <returns>1 si todo fue ejecutado con exito caso contrario regresa el mensaje de la excepcion</returns>
        public async Task<string> RunQuery(string query) {
            try {
                string json = await SqlDataAccessDapper.ExecuteQuery(query);
                return json;
            } catch {
                throw;
            }
        }

        public async Task<List<SpParameters>> ReadStoreProcedure(string StoreProcedure) {
            try {
                string Command = @$"SELECT PARAMETER_NAME, DATA_TYPE, ORDINAL_POSITION FROM INFORMATION_SCHEMA.PARAMETERS 
                    WHERE SPECIFIC_NAME = '{StoreProcedure}';";
                var ans = await SqlDataAccessDapper.ExecuteCommandClassList<SpParameters>(Command);
                return ans;
            } catch {
                throw;
            }
        }

        public async Task<int> ExtractIde(string commandText) {
            try {                
                var result = await SqlDataAccessDapper.ExtractDataTable(commandText);
                if( result.Rows.Count>=1 )
                    return Convert.ToInt32(result.Rows[0][0]);  
                return -1;
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        
    }
}

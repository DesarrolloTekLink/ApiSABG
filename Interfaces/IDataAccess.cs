    using ApiPractice.Models;
using System.Data;

namespace ApiPractice.Interfaces {
    public interface IDataAccess {


        /// <Desarrollo>TekLink</Desarrollo>
        /// <revision>2026-07-13</revision>
        /// <summary>
        /// ejecuta la query construida con los parametros
        /// </summary>
        /// <param name="select"></param>
        /// <param name="from"></param>
        /// <param name="where"></param>
        /// <returns> la ejecucion de la consulta en formato JSON</returns>
        Task<string> ExtractDataParts(string select, string from, string where);
        
        /// <Desarrollo>TekLink</Desarrollo>
        /// <revision>2026-07-13</revision>
        /// <summary>
        /// genera la validacion del equipo a traves de la extraccio del ide del equipo
        /// </summary>
        /// <param name="user"></param>
        /// <returns>ide del equipo si no esta regisgtrada regresa -1</returns>
        Task<int> GetAccessUser(LoginData user);

        /// <Desarrollo>TekLink</Desarrollo>
        /// <revision>2026-07-13</revision>
        /// <summary>
        /// Extraemos la definicion del contrato desde la Base de Datos 
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns>Lista de los componentes y relacion de los elementos al json</returns>
        Task<List<TableDefinition>> GetTableDefinition(string tableName);
        
        Task<string> RunQuery(string query);

        /// <Desarrollo>TekLink</Desarrollo>
        /// <revision>2026-07-13</revision>
        /// <summary>
        /// Leemos la condiciones de los parametros para registrar los datos del contrato
        /// </summary>
        /// <param name="StoreProcedure"></param>
        /// <returns>Lista de parametros requeridos del contrato</returns>
        Task<List<SpParameters>> ReadStoreProcedure(string StoreProcedure);
        

        /// <summary>
        /// busca el elemento de la tabla regresando su llave principal  
        /// </summary>
        /// <param name="commandText"></param>
        /// <returns></returns>
        Task<int> ExtractIde(string commandText);
        
    }
}

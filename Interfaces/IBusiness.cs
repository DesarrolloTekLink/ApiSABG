using ApiPractice.Models;
using System.Net.Http.Headers;

namespace ApiPractice.Interfaces {
    public interface IBusiness {

        /// <summary>
        /// Prepara la Carga del Contrato a partir de los datos relacionados al json procesado previamente
        /// </summary>
        /// <param name="spDictionary"></param>
        /// <param name="inputList"></param>
        /// <param name="tableColumnsList"></param>
        /// <param name="sPStructure"></param>
        /// <param name="clientIp"></param>
        /// <param name="user"></param>
        /// <returns>agrega los datos faltantae al Diccionario spDictionary</returns>
        Task BuiltStoreProcedure(Dictionary<string, object> spDictionary, List<InputInfo> inputList, List<TableDefinition> tableColumnsList, List<SpParameters> sPStructure, string clientIp, int user );

        /// <Desarrollo>TekLink</Desarrollo>
        /// <revision>2026-07-13</revision>
        /// <summary>
        /// Limpia todas las tablas relacionadas al contrato
        /// </summary>
        /// <param name="mainide"></param>
        /// <param name="Table"></param>
        /// <returns></returns>
        Task<string> DeleteAllTables(int mainide, String Table);

        /// <Desarrollo>TekLink</Desarrollo>
        /// <revision>2026-07-13</revision>
        /// <summary>
        /// Extrae la informacion relacionada a las tablas de los contratos, general el formato y lo almacena en una clase
        /// </summary>
        /// <param name="jsonObj"></param>
        /// <returns>Lista de los contratos con ya toda la informacion sintetizada </returns>
        List<TablasExtraidas> ExtractTablesList(dynamic? jsonObj);

        /// <summary>
        /// Crea el Contrato o Actualiza la Informacion si el contrato ya existe
        /// </summary>
        /// <param name="inputList"></param>
        /// <param name="tableColumnsList"></param>
        /// <param name="tableName"></param>
        /// <returns>Ide del Contrato Principal</returns>
        Task<int>  FindKeyTable(List<InputInfo> inputList, List<TableDefinition> tableColumnsList, string tableName);
       
        Task GetandSendDataCatalogues(Dictionary<string, object> spDictionary, List<InputInfo> inputList, List<TableDefinition> tableColumnsList, List<SpParameters> sPStructure);
        
        
       /// <summary>
       /// Inserta y guarda la tabla relacionada al contrato 
       /// </summary>
       /// <param name="table"></param>
       /// <param name="mainide"></param>
       /// <param name="tableName"></param>
       /// <param name="clientIp"></param>
       /// <param name="ideUser"></param>
       /// <returns></returns>
        Task SendTable(TablasExtraidas table, int mainide, string tableName, string clientIp, int ideUser);
    }
}

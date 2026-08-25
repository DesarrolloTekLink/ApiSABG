using ApiPractice.Implementations;
using ApiPractice.Interfaces;
using ApiPractice.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Diagnostics;
using System.Text.Json;


namespace ApiPractice.Controllers {
    [Route("DataController")]
    [ApiController]
    public class WriterDataController : ControllerBase {
        IDataAccess FuncData = new DataAccess();
        IBusiness FuncBusiness = new Business(new DataAccess(), new ExtraFunctions(new DataAccess()));
        IExtraFunctions FuncExtra = new ExtraFunctions(new DataAccess());

        [HttpPost("ExtractData")]
        public async Task<IActionResult> ExtractData([FromBody] SelectRequest q) {

            try {
                // Task 1: Login         
                LoginData User = FuncExtra.ExtractData(Request.Headers);
                if (await FuncData.GetAccessUser(User) == -1)
                    return Ok("Error:Usuario no autorizado");

                // Task 2: Get Data
                string json = await FuncData.ExtractDataParts(q.select, q.from, q.where);
                // Task 3: Match Json/Class
                Debug.WriteLine(json);
                return Ok(json);
            } catch (Exception ex) {
                Debug.WriteLine(ex.Message);
                return Ok(ex.Message);
            }
        }

        [HttpPost("UpdateQuery")]
        public async Task<IActionResult> UpdateQuery([FromBody] QueryRequest tmp) {
            try {
                // Task 1: Login         
                LoginData User = FuncExtra.ExtractData(Request.Headers);
                if (await FuncData.GetAccessUser(User) == -1)
                    return Ok("Usuario no autorizado");

                // Task 2: Get Data
                string json = await FuncData.RunQuery(tmp.query);
                // Task 3: Match Json/Class
                return Ok(json);
            } catch (Exception ex) {
                Debug.WriteLine(ex.Message);
                return Ok(ex.Message);
            }
        }

        public class InsertDataRequest
        {
            public string TableName { get; set; }
            public string Ancla { get; set; }
        }

        [HttpPost("InsertData")]
        public async Task<IActionResult> InsertData(string TableName, string Ancla) {
            try {
                var ini = DateTime.Now;
                
                // Task 1: Login         
                string rawJsonString = await new StreamReader(Request.Body).ReadToEndAsync();
                LoginData User = FuncExtra.ExtractData(Request.Headers);
                int ideUser = await FuncData.GetAccessUser(User);
                if (ideUser == -1)
                    return Ok("Usuario no autorizado");


                string clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();

                // Task 2: Get Data AND Json to Class
                dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(rawJsonString);
                List<InputInfo> InputList = FuncExtra.ExtractInputInfo(jsonObj);

                Debug.WriteLine(JsonSerializer.Serialize(InputList));

                                
                #region  ProcessData ListaInputsInfo

                    // Task 3: Extract Columns Definitions From the Main Table 
                    List<TableDefinition> TableColumnsList = await FuncData.GetTableDefinition(TableName);
                    FuncExtra.ShowClassDebug<TableDefinition>(TableColumnsList);

                    // Task 4: Get Store Procedure Structure
                    List<SpParameters> SPStructure = await FuncData.ReadStoreProcedure($"Sp{TableName}");
                    FuncExtra.ShowClassDebug<SpParameters>(SPStructure);

                    //Extra Step: Built Dictionary to define the Key and Value for the SP   

                    // Task 5: Define Type of Operation (Insert or Update) and Get the Key Values
                    int Mainide = await FuncBusiness.FindKeyTable(InputList, TableColumnsList, TableName);
#if DEBUG
                Console.WriteLine("Mainide: " + Mainide);
#endif

                //Task 7: Built Store Procedure with the SPStructure and the Key Values                    
                Dictionary<string, object> SpLogApi = FuncExtra.BuiltNewDefaultParameters(clientIp, ideUser, -1);                   
                    Dictionary<string, object> SpDictionary = FuncExtra.BuiltNewDefaultParameters(clientIp, ideUser, Mainide);
                
                    await FuncBusiness.BuiltStoreProcedure(SpDictionary, InputList, TableColumnsList, SPStructure, clientIp, ideUser);

                    Debug.WriteLine("Dictionary with Catalogues:" + JsonSerializer.Serialize(SpDictionary));
                    //Task 8: Run Store Procedure with the data already define in the Dictionary 
                    DataTable dt = await SqlDataAccessDapper.SpParametersCatchDataTable("Sp" + TableName, SpDictionary);
                    if( Mainide<=0 )
                        Mainide = int.Parse(dt.Rows[0][0].ToString());

                #endregion

                #region Process TablasExtraidas
                //Task 1: Extract Tablas List
                List<TablasExtraidas> TablasList = FuncBusiness.ExtractTablesList(jsonObj);
                FuncExtra.ShowClassDebug(TablasList);
                //Task 2: Delete Whole Data
                var answer = await FuncBusiness.DeleteAllTables(Mainide,TableName);
                Debug.WriteLine("Delete All Tables Answer: " + answer);
                //Task 3: Send each single table 
                foreach (var Table in TablasList) 
                    await FuncBusiness.SendTable(Table, Mainide, TableName,clientIp,ideUser);
                #endregion

                #region LogRegister

                SpLogApi.Add("@tablaPrincipal", TableName);
                SpLogApi.Add("@tiempoInicio", ini);
                SpLogApi.Add("@tablaide", Mainide);
                SpLogApi.Add("@totalTablas", TablasList.Count);
                var fin = DateTime.Now;
                SpLogApi.Add("@tiempoFinal", fin);
                SpLogApi.Add("@tiempoSegundos", (fin-ini).Seconds);
                await SqlDataAccessDapper.SpParametersCatchDataTable("SpLogApi", SpLogApi);

                #endregion
                
                return Ok(1);
            } catch (Exception ex) {
                Debug.WriteLine(ex.Message);
                return Ok(ex.Message);
            }
        }


    }
}

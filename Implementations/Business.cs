using ApiPractice.Interfaces;
using ApiPractice.Models;
using Microsoft.AspNetCore.Mvc.Razor.Infrastructure;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ApiPractice.Implementations {
    public class Business : IBusiness {

        private readonly IDataAccess _dataAccess;
        private readonly IExtraFunctions _ExtraFunctions;

        // Inject DataAccess through constructor
        public Business(IDataAccess dataAccess, IExtraFunctions Extra) {
            _dataAccess = dataAccess;
            _ExtraFunctions = Extra;
        }

        public Task GetandSendDataCatalogues(Dictionary<string, object> spDictionary, List<InputInfo> inputList, List<TableDefinition> tableColumnsList, List<SpParameters> sPStructure) {
            throw new NotImplementedException();
        }


        public async Task BuiltStoreProcedure(Dictionary<string, object> spDictionary, List<InputInfo> inputList, List<TableDefinition> tableColumnsList, List<SpParameters> sPStructure, string clientIp, int user = -1) {
            try {

                foreach (var StoreValue in sPStructure) {
                    Debug.WriteLine("Validation:" + spDictionary.ContainsKey(StoreValue.PARAMETER_NAME) + " Key:" + StoreValue.PARAMETER_NAME);
                    if (spDictionary.ContainsKey(StoreValue.PARAMETER_NAME))
                        continue;

                    string SpParameterName = StoreValue.PARAMETER_NAME.Replace("@", "");
                    TableDefinition Columna = tableColumnsList.Where(Col => Col.campo.ToLower().CompareTo(SpParameterName) == 0).FirstOrDefault();
                    //InputInfo Element = inputList.Where(input => input.Nombre.CompareTo(Columna.tagname) == 0 && input.Panel.CompareTo(Columna.titulo) == 0).FirstOrDefault();
                    InputInfo Element = inputList.Where(input => input.Nombre.CompareTo(Columna.tagname) == 0 ).FirstOrDefault();
                    if (Element == null) {
#if DEBUG
                        Console.WriteLine("ELEMENT NULL: " + JsonSerializer.Serialize(Columna));
#endif
                        Element = new InputInfo(Columna.escatalogo, (TipoValor)Columna.fkckadmtipovalor);                        
                    } else
                        Debug.WriteLine("ELEMENT: " + JsonSerializer.Serialize(Element));


                    if (Columna.escatalogo) {
                        int llaveCat = await _ExtraFunctions.GetCatalogueKey(Columna.campo.Remove(0, 2), Element.ValorOriginal, user);
                        spDictionary[StoreValue.PARAMETER_NAME] = llaveCat;
                    } else
                        spDictionary[StoreValue.PARAMETER_NAME] = ConverterType.StringtoType((TipoValor)Columna.fkckadmtipovalor, Element.ValorOriginal);

                    Debug.WriteLine("Val:" + StoreValue.PARAMETER_NAME + " , " + spDictionary[StoreValue.PARAMETER_NAME]);
                }


            } catch (Exception ex) {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }

        public async Task<int> FindKeyTable(List<InputInfo> inputList, List<TableDefinition> tableColumnsList, string tableName) {
            try {
                //Find IDE 
                int IDE = -1;
                var ideColumn = tableColumnsList.FirstOrDefault(col => col.esllave == true);
                Debug.WriteLine($"IDE Column: {JsonSerializer.Serialize(ideColumn)}");

                //InputInfo InputKey = inputList.FirstOrDefault(input => input.Nombre.CompareTo(ideColumn.tagname) == 0 && input.Panel.CompareTo(ideColumn.titulo) == 0);
                InputInfo InputKey = inputList.FirstOrDefault(input => input.Nombre.CompareTo(ideColumn.tagname) == 0 );
                if (InputKey == null)
                    throw new Exception("No se encontró el campo clave en la lista de entradas.");
                Debug.WriteLine($"IDE Column: {ideColumn.tagname}, InputKey: {JsonSerializer.Serialize(InputKey)}");

                // Find the value of IDE in the database using DataAccess
                string CommandText = $"Select ide from vw{tableName} where {ideColumn.campo}='{InputKey.ValorOriginal}'";

                IDE = await _dataAccess.ExtractIde(CommandText);
                return IDE;
            } catch (Exception ex) {
                // Log the exception or handle it as needed
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }

        }

        public List<TablasExtraidas> ExtractTablesList(dynamic? jsonObj) {
            try {
                List<TablasExtraidas> TablasList = new List<TablasExtraidas>();
                foreach (var TableJson in (JArray)jsonObj.TablasExtraidas) {
                    TablasExtraidas Tabla = new TablasExtraidas { Titulo = TableJson["Titulo"].ToString() };
                    Debug.WriteLine($"Titulo type : {TableJson.GetType()}");
                    JArray filasArray = (JArray)TableJson["Filas"];

                    bool InsertedColumns = false;
                    int mxColSize = 0;
                    foreach (var Fila in filasArray) {

                        if (Fila is JObject filaObj) {
                            List<CellData> RowData = new List<CellData>();
                            Debug.WriteLine($"Fila Object: {JsonSerializer.Serialize(filaObj)}");

                            foreach (var col in ((JObject)filaObj).Properties()) {
                                CellData Block;
                                if (InsertedColumns == false) {
                                    foreach (var prop in filaObj.Properties()) {
                                        Debug.WriteLine("PropName:" + prop.Name);
                                        Tabla.Columns.Add(prop.Name);
                                    }
                                    InsertedColumns = true;
                                }

                                Debug.WriteLine("col:" + JsonSerializer.Serialize(col));
                                string key = col.Name;
                                mxColSize= Math.Max(mxColSize, col.Value.ToString().Length);

                                TipoValor tipo = ConverterType.FindTipeValue(col.Value.ToString());
                                object val = ConverterType.StringtoType(tipo, col.Value.ToString());
                                Block = new CellData(col.Name, val, tipo);
                                RowData.Add(Block);
                                _ExtraFunctions.ShowSingleClassDebug<CellData>(Block);
                            }
                            Tabla.Matrix.Add(RowData);
                        }
                    }

                    Debug.WriteLine("Tabla:" + JsonSerializer.Serialize(Tabla));

                    if (Tabla.Matrix.Count > 0  && mxColSize>0)                         
                        TablasList.Add(Tabla);
                    
                }

                return TablasList;
            } catch (Exception e) {
                Debug.WriteLine("Error: " + e);
                throw;
            }
        }

        public async Task<string> DeleteAllTables(int IDE, String Table){
            try {
                string Command = $"EXEC spDeleteTables{Table} {Table.Replace("pk", "@ide")} ={IDE}";
                /*
                   string Command = "";
                   switch (Table) {
                    case "pkadmBESA":
                        Command = @$"EXEC spDeleteTablespkadmBESA @ideadmBESA={IDE}";
                        break;
                    case "pkadmMFIJ":
                        Command = @$"EXEC spDeleteTablespkadmMFIJ @ideadmMFIJ={IDE}";
                        break;
                    case "pkadmContratos":
                        Command = @$"EXEC spDeleteTablespkadmContratos @ideadmContratos={IDE}";
                        break;
                }*/
                var DeleteMessagge = await _dataAccess.RunQuery(Command);
                if (DeleteMessagge.Length > 0)
                    throw new Exception("Deleting Data Error:" + DeleteMessagge);
                return "";
            } catch {
                throw;
            }

        }

        
         public async Task SendTable(TablasExtraidas TableContent, int mainide, string tableName, string clientIp, int ideUser) {
            try {
                //Step 0: See If Data Exists 
                if (TableContent.Matrix.Count == 0)
                    return;

                //Step 1: Extract WordType Table 
                TableTypes TableStructure = new TableTypes(tableName);
                _ExtraFunctions.ShowSingleClassDebug(TableStructure);
                //Step 2: Insert Fila
                int Filaide = await InsertTitulo(TableStructure.TableTitle, mainide, TableContent.Titulo, _ExtraFunctions.BuiltNewDefaultParameters(clientIp, ideUser));
                Debug.WriteLine($"Inserted Fila IDE: {Filaide}");
                //Step 3: Send Whole DataTable
                int NumberRow = 1;
                foreach (var Row in TableContent.Matrix) {
                    int Rowide = await InsertRow(TableStructure, Filaide, NumberRow, _ExtraFunctions.BuiltNewDefaultParameters(clientIp, ideUser));
                    int NumberCell = 1;
                    foreach (var Cell in Row) {
                        int Cellide = await InsertCell(TableStructure, Rowide, NumberCell, Cell, _ExtraFunctions.BuiltNewDefaultParameters(clientIp, ideUser));
                        Debug.WriteLine($"Inserted Cell IDE: {Cellide} for Column: {Cell.ColumnName} with Value: {Cell.Value}");
                        NumberCell++;
                    }
                    NumberRow++;
                }

            } catch (Exception ex) {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }

        /* New Version SendTable
         * public async Task SendTable(TablasExtraidas TableContent, int mainide, string tableName, string clientIp, int ideUser) {
            try {
                //Step 0: See If Data Exists 
                if (TableContent.Matrix.Count == 0)
                    return;

                //Step 1: Extract WordType Table 
                TableTypes TableStructure = new TableTypes(tableName);
                _ExtraFunctions.ShowSingleClassDebug(TableStructure);

                //Step 2: Insert Fila
                int Filaide = await InsertTitulo(TableStructure.TableTitle, mainide, TableContent.Titulo, _ExtraFunctions.BuiltNewDefaultParameters(clientIp, ideUser));
                Debug.WriteLine($"Inserted Fila IDE: {Filaide}");
                //Step 3: Send Whole DataTable
                int NumberRow = 1;
                DataTable RowDt = new DataTable();
                
                foreach (var Row in TableContent.Matrix) {
                    int Rowide = await InsertRow(TableStructure, Filaide, NumberRow, _ExtraFunctions.BuiltNewDefaultParameters(clientIp, ideUser));
                    int NumberCell = 1;
                    foreach (var Cell in Row) {
                        int Cellide = await InsertCell(TableStructure, Rowide, NumberCell, Cell, _ExtraFunctions.BuiltNewDefaultParameters(clientIp, ideUser));
                        Debug.WriteLine($"Inserted Cell IDE: {Cellide} for Column: {Cell.ColumnName} with Value: {Cell.Value}");
                        NumberCell++;
                    }
                    NumberRow++;
                }

            } catch (Exception ex) {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }
        */


        /// <Desarrollo>TekLink</Desarrollo>
        /// <revision>2026-07-13</revision>
        /// <summary>
        /// inserta un dato en la tabla relacionado a la fila registrada previamente
        /// </summary>
        /// <param name="tableStructure"></param>
        /// <param name="Rowide"></param>
        /// <param name="NumberCell"></param>
        /// <param name="cell"></param>
        /// <param name="Parameters"></param>
        /// <returns>ide del valor insertado</returns>
        private async Task<int> InsertCell(TableTypes tableStructure,int Rowide,int NumberCell, CellData cell, Dictionary<string, object> Parameters) {
            try {
                Parameters["fk" + tableStructure.TableFilas.ToLower()] = Rowide;
                Parameters["@clavecampo"] = cell.ColumnName;
                Parameters["@valorcampo"] = cell.Value;
                Parameters["@ordencampo"] = NumberCell;
                Parameters["@fkckadmtipovalor"] = (int)cell.TypeValue;
                Debug.WriteLine($"Inserting Cell with Parameters: \n{JsonSerializer.Serialize(Parameters)}");
                var dt = await SqlDataAccessDapper.SpParametersCatchDataTable("sp" + tableStructure.TableCampos, Parameters);
                return int.TryParse(dt.Rows[0][0].ToString(), out int ide) ? ide : -1;
            } catch (Exception ex) {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }

        /// <Desarrollo>TekLink</Desarrollo>
        /// <revision>2026-07-13</revision>
        /// <summary>
        /// Construye una fila relacionada a la fila registrada previamente 
        /// </summary>
        /// <param name="TableStructure"></param>
        /// <param name="filaide"></param>
        /// <param name="numberRow"></param>
        /// <param name="Parameters"></param>
        /// <returns>ide de la fila</returns>
        private async Task<int> InsertRow(TableTypes TableStructure, int filaide, int numberRow, Dictionary<string, object> Parameters) {
            try {
                Parameters["@orden"] = numberRow;
                Parameters["fkskadm" + TableStructure.KeyWord] = filaide;
                var dt = await SqlDataAccessDapper.SpParametersCatchDataTable("sp" + TableStructure.TableFilas, Parameters);
                return int.TryParse(dt.Rows[0][0].ToString(), out int ide) ? ide : -1;
            } catch (Exception ex) {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }

        /* New Version InsertRow
         * private async Task<int> InsertRow(TableTypes TableStructure, int filaide, int numberRow, Dictionary<string, object> Parameters) {
            try {
                Parameters["@orden"] = numberRow;
                Parameters["fkskadm" + TableStructure.KeyWord] = filaide ;
                var dt = await SqlDataAccessDapper.SpParametersCatchDataTable("sp" + TableStructure.TableFilas, Parameters);
                return int.TryParse(dt.Rows[0][0].ToString(), out int ide) ? ide : -1;
            } catch (Exception ex) {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }*/


        /// <Desarrollo>TekLink</Desarrollo>
        /// <revision>2026-07-13</revision>
        /// <summary>
        /// Inserta el Titulo de la Tabla ligandolo al contrato principal
        /// </summary>
        /// <param name="Table"></param>
        /// <param name="Mainide"></param>
        /// <param name="titulo"></param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        private async Task<int> InsertTitulo(string Table, int Mainide, string titulo , Dictionary<string, object> Parameters) {
            try {
                Parameters["@titulo"] = titulo;
                Parameters[Table.Replace("sk", "@fkpk")] = Mainide;
                Console.WriteLine(JsonSerializer.Serialize(Parameters));

                var dt = await SqlDataAccessDapper.SpParametersCatchDataTable("sp" + Table, Parameters);
                return int.TryParse(dt.Rows[0][0].ToString(), out int ide) ? ide : -1;
            } catch (Exception ex) {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }
    }
}

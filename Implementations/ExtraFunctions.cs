using ApiPractice.Interfaces;
using ApiPractice.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace ApiPractice.Implementations {
    public class    ExtraFunctions : IExtraFunctions {

        private readonly IDataAccess _dataAccess;
        private readonly IExtraFunctions _ExtraFunctions;

        public ExtraFunctions(IDataAccess dataAccess) {
            _dataAccess = dataAccess;
        }

        public string resuelvecadenapassword(string mac, string cadenaresBase64) {
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

        public string gencadenapassword(string mac, string userapi) {
            string cadenares = "";
            string cadenaori = mac + "|" + userapi + "|" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string llave01 = KeyGenerator.GenerarClave(mac);
            var e01 = new encripta();
            cadenares = e01.EncryptString128Bit(cadenaori, llave01);
            cadenares = cadenares.Length >= 8
            ? cadenares.Substring(cadenares.Length - 4) +
              cadenares.Substring(4, cadenares.Length - 8) +
              cadenares.Substring(0, 4)
            : cadenares;
            string cadenaresBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(cadenares));
            return cadenaresBase64;
        }


        public LoginData ExtractData(IHeaderDictionary headers) {
            LoginData data = new LoginData();
            try {

                // CREATING MY OWN PASWORD FOR TESTING PURPOSES, IN PRODUCTION THIS SHOULD NOT BE HARDCODED
                string testMac = "548EB7295711";
                string pass = gencadenapassword(testMac, "adminComprasMX@puebla.gob.mx");

                string OriginalPass = resuelvecadenapassword(testMac, pass);
#if DEBUG
                Console.WriteLine($"password for testing: \n {pass}");
                Console.WriteLine($"Original : \n {OriginalPass}");
#endif


                //TODO: Validar que los headers existan y no sean nulos antes de acceder a ellos
                data.vchHeaderUser = headers["User"].FirstOrDefault();
                data.vchHeaderPassword = headers["Password"].FirstOrDefault();
                data.vchHeaderMacAddress = headers["MacAddress"].FirstOrDefault();

                string RealString = resuelvecadenapassword(data.vchHeaderMacAddress, data.vchHeaderPassword);
                Debug.WriteLine($"Decrypted string: {RealString}");


                var wrds = RealString.Split('|');
                if (wrds.Length != 3)
                    throw new Exception("Error:Token Erroneo");

                data.vchMacAddress = wrds[0];
                data.vchUser = wrds[1];
                data.ConvertDateTime(wrds[2]);

                DateTime QueryTime = DateTime.Now; // Current time
                TimeSpan difference = QueryTime - data.dtTime;
                int limit = 1;
#if DEBUG
                limit = 90; // For testing purposes, you can set this to a   longer duration
#endif
                //TODO: Test this time difference logic, it should be less than 1 minute, but for testing purposes, you can set it to a longer duration
                if (difference.TotalMinutes > limit)
                    throw new Exception("Error: Token Expirado");

                return data;
            } catch (Exception ex) {
                Console.WriteLine($"Error: {ex.Message}");
                throw; // Rethrow the exception to be handled by the caller
            }
        }

        public List<InputInfo> ExtractInputInfo(dynamic? jsonObj) {
            try {
                List<InputInfo> inputList = new List<InputInfo>();
                foreach (var obj in (JArray)jsonObj.ListaInputsInfo) {
                    //InputInfo Extract = JsonSerializer.Deserialize<InputInfo>(obj);
                    InputInfo Extract = obj.ToObject<InputInfo>();
                    inputList.Add(Extract);
                }
                return inputList;
            } catch {
                throw;
            }
        }

        public void ShowClassDebug<T>(List<T> List) {
            Debug.WriteLine(typeof(T).FullName + ":" + List.Count);
            Debug.WriteLine(JsonSerializer.Serialize(List));
        }

        public async Task<int> GetCatalogueKey(string CatTable, string valorOriginal, int user) {
            try {
                if (valorOriginal.Length == 0)
                    return 1;

                string comm = $@"select  ide  from vw{CatTable}  where  nombre = '{valorOriginal}' ";
                int ide = await _dataAccess.ExtractIde(comm);
                if (ide > 1)
                    return ide;

                var parms = await _dataAccess.ReadStoreProcedure("sp" + CatTable);
                Dictionary<string, object> spCat = new Dictionary<string, object>();
                spCat["@tinTipMov"] = 1;
                spCat["@ide"] = -1;
                spCat["@nombre"] = valorOriginal;
                spCat["@fkusuario"] = user;
                spCat["@feccap"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                spCat["@sip"] = "0.0.0.0";
                spCat["@mareli"] = -1;


                DataTable ans = await SqlDataAccessDapper.SpParametersCatchDataTable($"sp{CatTable}", spCat);


                return int.Parse(ans.Rows[0][0].ToString());
            } catch
              (Exception ex) {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }
        
        public void ShowSingleClassDebug<T>(T block) {
            Debug.WriteLine(typeof(T).FullName + ":" + JsonSerializer.Serialize(block));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spDictionary"></param>
        /// <param name="clientIp"></param>
        /// <param name="user"></param>
        /// <param name="IDE">  1=Insert 2=Update </param>
        public Dictionary<string, object> BuiltNewDefaultParameters(string clientIp, int ideUser,int IDE = -1 ) {
            Dictionary<string, object> spDictionary = new Dictionary<string, object>();
            spDictionary["@fkusuario"] = ideUser;
            spDictionary["@feccap"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            spDictionary["@sip"] = clientIp; // Check IP On Production 
            spDictionary["@mareli"] = -1;
            spDictionary["@ide"] = IDE;
            Debug.WriteLine("ideKey:" + spDictionary["@ide"]);
            spDictionary["@tinTipMov"] = IDE > -1 ? 2 : 1; // 2 for update and 1 for insert

            return spDictionary;
        }
    }
}

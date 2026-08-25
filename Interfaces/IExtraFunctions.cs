using ApiPractice.Models;

namespace ApiPractice.Interfaces {
    public interface IExtraFunctions {

        public string resuelvecadenapassword(string mac, string cadenaresBase64);
        
        public string gencadenapassword(string mac, string userapi);
        
        /// <Desarrollo>TekLink</Desarrollo>
        /// <revision>2026-07-13</revision>
        /// <summary>
        /// Extrae la informacion y el Token desde los headers 
        /// </summary>
        /// <param name="headers"></param>
        /// <returns>Infromacion de acceso sobre el equipo</returns>
        public LoginData ExtractData(IHeaderDictionary headers);

        /// <Desarrollo>TekLink</Desarrollo>
        /// <revision>2026-07-13</revision>
        /// <summary>
        /// Convertimos la informacion recibida en un Json a una lista de la Clase InputInfo
        /// </summary>
        /// <param name="jsonObj"></param>
        /// <returns>Lista de los componentes  que definen el Contrato</returns>
        public List<InputInfo> ExtractInputInfo(dynamic? jsonObj);

        public void ShowClassDebug<T>(List<T> List);

        /// <Desarrollo>TekLink</Desarrollo>
        /// <revision>2026-07-13</revision>
        /// <summary>
        ///  Regresa el Ide del objeto relacionado al catalogo
        /// </summary>
        /// <param name="CatTable"></param>
        /// <param name="valorOriginal"></param>
        /// <param name="user"></param>
        /// <returns>ide del objeto catalogo</returns>
        Task<int> GetCatalogueKey(string CatTable, string valorOriginal, int user);
        //public void BuiltDefaultParameters(Dictionary<string, object> spDictionary, string clientIp, int user, int IDE);


        /// <Desarrollo>TekLink</Desarrollo>
        /// <revision>2026-07-13</revision>
        /// <summary>
        /// Muestra la Informacion  de la clase T para fines de Debug 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="block"></param>
        void ShowSingleClassDebug<T>(T block);

        /// <Desarrollo>TekLink</Desarrollo>
        /// <revision>2026-07-13</revision>
        /// <summary>
        /// creamos el Diccionario con los parametros estandar al llamar cualquier SP que registre algun dato
        /// </summary>
        /// <param name="clientIp"></param>
        /// <param name="ideUser"></param>
        /// <param name="IDE"></param>
        /// <returns>Diccionario con parametros fedault</returns>
        Dictionary<string, object> BuiltNewDefaultParameters(string clientIp, int ideUser,int IDE = -1);
    }
}

namespace ApiPractice.Models {

    public class LoginData {
        public string vchHeaderUser { get; set; } = "";
        public string vchHeaderPassword { get; set; } = "";
        public string vchHeaderMacAddress { get; set; } = "";

        public string vchUser { get; set; } = "";
        public string vchPassword { get; set; } = "";
        public string vchMacAddress { get; set; } = "";
     
       public DateTime dtTime { get; set; } = new DateTime();

        public void ConvertDateTime(string dateString) { 
            dtTime= DateTime.ParseExact(dateString, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture);
        }

    }
}

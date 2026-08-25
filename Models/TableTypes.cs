namespace ApiPractice.Models {
    public class TableTypes {

        public string KeyWord { get; set; } = "";
        public string TableTitle { get; set; } = "";
        public string TableFilas { get; set; } = "";
        public string TableCampos { get; set; } = "";
        public TableTypes(string WholeMainTableName) {
            string KiWord = WholeMainTableName.Replace("pkadm","");
            KeyWord = "" + KiWord;
            TableTitle = "skadm" + KiWord;
            TableFilas = "tkadm" + KiWord +"Filas";
            TableCampos = "tkadm" + KiWord + "Campos";
        }
    }
}

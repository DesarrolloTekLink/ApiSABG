namespace ApiPractice.Models {
    public class InputInfo {
        public string Panel { get; set; } = "";

        public string Nombre { get; set; } = "";
        public string ValorOriginal { get; set; } = "";

        public int TipoDato { get; set; } = 0;

        public string ValorConvertido { get; set; } = "";

        public InputInfo(bool Iscatalog, TipoValor  Tip ) {
            if (Iscatalog)
                ValorOriginal = "1";
            else ValorOriginal = ConverterType.GetDefaultValue(Tip);
        }

    }



}

namespace ApiPractice.Models {
    public class TableDefinition {
        public string campo { get; set; } = "";
        public string titulo { get; set; } = "";
        public string tagname { get; set; } = "";
        public bool esllave { get; set; } = false;
        public bool escatalogo { get; set; } = false;
        public int fkckadmtipovalor { get; set; } = 0;
    }
}

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace ApiPractice.Models {
    public class TablasExtraidas {
        [JsonPropertyName("Titulo")]
        public string Titulo { get; set; }

        public List<string> Columns { get; set; } = new List<string>();

        public List<List<CellData>> Matrix { get; set; } = new List<List<CellData>>();

    }
    public class CellData {
        
        public string ColumnName { get; set; } = "";
        public object Value { get; set; }
        public TipoValor TypeValue;
        public CellData(string Name, object Val, TipoValor type) {
            ColumnName = Name;
            Value = Val;
            TypeValue  = type;

        }
    }

}

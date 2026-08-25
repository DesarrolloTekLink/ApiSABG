using System.Diagnostics;
using System.Globalization;
using System.Reflection.Metadata.Ecma335;

namespace ApiPractice.Models {
    public static class ConverterType {

        /// <Desarrollo>TekLink</Desarrollo>
        /// <revision>2026-07-13</revision>
        /// <summary>
        /// regresa un Valor default en relacion al tipo de variable 
        /// </summary>
        /// <param name="VariableType"></param>
        /// <returns></returns>
        public static string GetDefaultValue(TipoValor  VariableType ) {
            switch (VariableType) {
                case TipoValor.Cadena:
                    return "";
                case TipoValor.Entero:
                    return "0";
                case TipoValor.Moneda:
                    return "0.0";
                case TipoValor.Booleano:
                    return "0";
                case TipoValor.Fecha:
                    return "1900-01-01 00:00:00.000";
                default:
                    return "";

            }
        }

        /// <Desarrollo>TekLink</Desarrollo>
        /// <revision>2026-07-13</revision>
        /// <summary>
        /// define que tipo de valor es una cadena de texto 
        /// </summary>
        /// <param name="Text"></param>
        /// <returns></returns>
        public static TipoValor FindTipeValue(string Text) {
            if( Text.Contains("$") )
                return TipoValor.Moneda;
            if (Text.Contains("/") || Text.Contains("-"))   
                return TipoValor.Fecha;
            if(Text.ToLower().Contains("true") || Text.ToLower().Contains("false") || Text.ToLower().Contains("no") || Text.ToLower().Contains("yes"))
                return TipoValor.Booleano;
             if (int.TryParse(Text, out _))
                return TipoValor.Entero;
            return TipoValor.Cadena;
        }

        /// <Desarrollo>TekLink</Desarrollo>
        /// <revision>2026-07-13</revision>
        /// <summary>
        ///  Transforma un string en relacion al tipo de valor correspondiente
        /// </summary>
        /// <param name="VariableType"></param>
        /// <param name="type"></param>
        /// <returns>Objeto</returns>
        public static Object StringtoType(TipoValor VariableType, string type = "") {
            try {
                type = type.Trim().ToLower();
                switch (VariableType) {

                    case TipoValor.Cadena:
                        return type.ToUpper();

                    case TipoValor.Entero:
                        return type.Length == 0 ? 0 : int.Parse(type);

                    case TipoValor.Moneda:
                        return type.Length == 0 ? 0 : decimal.Parse(type.Replace("$", "").Replace("%",""));

                    case TipoValor.Fecha: // TODO: Date Validation{  dd/MM/yyyy hh:mm:ss tt}

                        Debug.WriteLine(type);
                        type = type.Trim();
                        type = type.Replace("/", "-");
                        string time = (type.Length > 10 ? " HH:mm" : "");
                        time = (type.Length > 16 ? time + ":ss" : time + "");
                        time = type.Contains("m") == true ? time.Replace("HH", "hh") + " tt" : time;
                        try {
                            return DateTime.ParseExact(type, "dd-MM-yyyy" + time, CultureInfo.InvariantCulture, DateTimeStyles.None);
                            //return Fecha.HasValue == true ? Fecha : new DateTime(1900, 1, 1);
                        } catch {
                            return new DateTime(1900, 1, 1);
                        }
                    case TipoValor.Booleano: // fALSE/nO
                        return (type.CompareTo("false") == 0 || type.CompareTo("no") == 0 || type.Length == 0) ? false : true;
                    default:
                        return type;
                }
            } catch {
                Console.WriteLine($"Error converting value: {type} to type: {VariableType}");
            }
            return type;    
        }
    }

}


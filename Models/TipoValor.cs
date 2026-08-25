using System.ComponentModel.DataAnnotations;

namespace ApiPractice.Models {
    

    public enum TipoValor {
        [Display(Name = "Sin Especificar")]
        SinEspecificar = 1,

        [Display(Name = "Cadena")]
        Cadena = 2,

        [Display(Name = "Entero")]
        Entero = 3,

        [Display(Name = "Moneda")]
        Moneda = 4,

        [Display(Name = "Fecha")]
        Fecha = 5,
        
        [Display(Name = "Booleano")]
        Booleano = 6

    }

}

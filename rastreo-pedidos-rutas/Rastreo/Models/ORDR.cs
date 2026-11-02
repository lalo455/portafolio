using System;
using System.ComponentModel.DataAnnotations;

namespace Rastreo.Models
{
    public class ORDR
    {
        public int DocEntry { get; set; }
        [Required]
        [StringLength(8, ErrorMessage = "La longitud debe ser de 8 caracteres.cambio")]
        public string UserId { get; set; }
        [Required]
        [StringLength(15, ErrorMessage = "La longitud debe ser de 15 caracteres.")]
        public string CardCode { get; set; }
        [Required]
        public DateTime DocDate { get; set; }
        [Required]
        public DateTime DocDueDate { get; set; }

        [MaxLength(100, ErrorMessage = "La longitud no debe ser mayor a 100 caracteres.")]
        public string NumAtCard { get; set; }
 
        [MaxLength(254, ErrorMessage = "La longitud no debe ser mayor a 100 caracteres.")]
        public string Comments { get; set; }
        [Required]
        [MaxLength(1, ErrorMessage = "La longitud no debe ser mayor a 1 caracter.")]
        public string Status { get; set; }

        public string RefInternas { get; set; }
        [Required]
        [MaxLength(3, ErrorMessage = "La longitud no debe ser mayor a 3 caracter.")]
        public string Moneda { get; set; }

        [StringLength(10, ErrorMessage = "La longitud debe ser de 10 caracteres.")]
        public string Contacto { get; set; }
        [Required]
        [MaxLength(254, ErrorMessage = "La longitud no debe ser mayor a 254 caracteres.")]
        public string DireccionEntrega { get; set; }
        [Required]
        [MaxLength(254, ErrorMessage = "La longitud no debe ser mayor a 10 caracteres.")]
        public string SlpCode { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Rastreo.Models
{
    public class RDR1
    {
        public int DocEntry { get; set; }
        public int RDR1PK { get; set; }
        public string UserId { get; set; }
        public List<SelectListItem> ListaUserId { get; set; }
        [Required]
        [StringLength(20, ErrorMessage = "La longitud debe ser de 8 caracteres.cambio")]
        public string ItemCode { get; set; }

        [MaxLength(20, ErrorMessage = "La longitud no debe ser mayor a 20 caracteres.")]
        public string Subs { get; set; }
        [Required]
        public float Quantity { get; set; }
        [Required]
        [StringLength(10, ErrorMessage = "La longitud debe ser de 10 caracteres.")]
        public string Almacen { get; set; }

        [MaxLength(254, ErrorMessage = "La longitud no debe ser mayor a 254 caracteres.")]
        public string Comments { get; set; }
        [Required]
        [MaxLength(1, ErrorMessage = "La longitud no debe ser mayor a 1 caracter.")]
        public string Status { get; set; }
        [Required]
        public float Price { get; set; }
        [Required]
        public float Tax { get; set; }
        [Required]
        public float Total { get; set; }
        [Required]
        public int BaseEntry { get; set; }
        public string Espe { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public DateTime EntregaVta { get; set; }
        [Required]
        public DateTime EntregaEsti { get; set; }

        [MaxLength(100, ErrorMessage = "La longitud no debe ser mayor a 100 caracteres.")]
        public string Clave { get; set; }
        [Required]
        [MaxLength(254, ErrorMessage = "La longitud no debe ser mayor a 254 caracteres.")]
        public string DescLarga { get; set; }

        [MaxLength(30, ErrorMessage = "La longitud no debe ser mayor a 30 caracteres.")]
        public string DiasEntrega { get; set; }
    }
}
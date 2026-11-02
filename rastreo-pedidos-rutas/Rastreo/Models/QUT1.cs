using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Rastreo.Models
{
    public class QUT1
    {
        public int DocEntry             { get; set; }
        public int QUT1PK               { get; set; }
        public string   UserId          { get; set; }
        public List<SelectListItem> ListaUserId { get; set; }
        [Required]
        [StringLength(20, ErrorMessage = "La longitud debe ser de 8 caracteres.cambio")]
        public string   ItemCode        { get; set; }
        [Required]
        [MaxLength(20, ErrorMessage = "La longitud no debe ser mayor a 20 caracteres.")]
        public string   Subs            { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{0:C0}")]
        public float    Quantity        { get; set; }
        [Required]
        [StringLength(10, ErrorMessage = "La longitud debe ser de 10 caracteres.")]
        public string   Almacen         { get; set; }
  
        [MaxLength(254, ErrorMessage = "La longitud no debe ser mayor a 254 caracteres.")]
        public string   Comments        { get; set; }
        [Required]
        [MaxLength(1, ErrorMessage = "La longitud no debe ser mayor a 1 caracter.")]
        public string   Status          { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{0:C0}")]
        public float    Price           { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{0:C0}")]
        public float    Tax             { get; set; }
        [Required]
        public float    Total           { get; set; }
        [Required]
        public string   Espe            { get; set; }
        [Required]
        public string   Name            { get; set; }
        [Required]
        public DateTime EntregaVta      { get; set; }
        [Required]
        public DateTime EntregaEsti     { get; set; }
        [Required]
        [MaxLength(100, ErrorMessage = "La longitud no debe ser mayor a 100 caracteres.")]
        public string   Clave           { get; set; }
        [Required]
        [MaxLength(254, ErrorMessage = "La longitud no debe ser mayor a 254 caracteres.")]
        public string   DescLarga       { get; set; }
        [Required]
        [MaxLength(30, ErrorMessage = "La longitud no debe ser mayor a 30 caracteres.")]
        public string   DiasEntrega     { get; set; }
    }
}
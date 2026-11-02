using System.ComponentModel.DataAnnotations;

namespace Encuesta.Web.ViewModels
{
    public class SurveyViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(200, ErrorMessage = "El título no puede exceder los 200 caracteres.")]
        [Display(Name = "Título")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Descripción")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}

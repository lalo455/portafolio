using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using Encuesta.Core.Interfaces;
using Encuesta.Core.Entities;
using Encuesta.Web.ViewModels;

namespace Encuesta.Web.Controllers
{
    public class SurveyController : Controller
    {
        private readonly ISurveyService _surveyService;

        public SurveyController(ISurveyService surveyService)
        {
            _surveyService = surveyService;
        }

        public async Task<IActionResult> Index()
        {
            var surveys = await _surveyService.GetAllActiveAsync();
            var viewModels = surveys.Select(s => new SurveyViewModel
            {
                Id = s.Id,
                Title = s.Title,
                Description = s.Description,
                IsActive = s.IsActive
            });
            return View(viewModels);
        }

        public IActionResult Create()
        {
            return View(new SurveyViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SurveyViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var survey = new Survey
            {
                Title = viewModel.Title,
                Description = viewModel.Description
            };

            await _surveyService.CreateAsync(survey);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            await _surveyService.DeactivateAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}

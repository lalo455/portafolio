using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Encuesta.Core.Entities;
using Encuesta.Core.Interfaces;

namespace Encuesta.Application.Services
{
    public class SurveyService : ISurveyService
    {
        private readonly ISurveyRepository _surveyRepository;

        public SurveyService(ISurveyRepository surveyRepository)
        {
            _surveyRepository = surveyRepository;
        }

        public async Task<IEnumerable<Survey>> GetAllActiveAsync()
        {
            var surveys = await _surveyRepository.GetAllAsync();
            var activeSurveys = new List<Survey>();
            foreach (var survey in surveys)
            {
                if (survey.IsActive)
                {
                    activeSurveys.Add(survey);
                }
            }
            return activeSurveys;
        }

        public async Task<Survey?> GetByIdAsync(int id)
        {
            return await _surveyRepository.GetByIdAsync(id);
        }

        public async Task<int> CreateAsync(Survey survey)
        {
            if (string.IsNullOrWhiteSpace(survey.Title))
            {
                throw new ArgumentException("El título es obligatorio.", nameof(survey.Title));
            }
            
            survey.CreatedAt = DateTime.UtcNow;
            survey.IsActive = true;
            
            return await _surveyRepository.AddAsync(survey);
        }

        public async Task DeactivateAsync(int id)
        {
            await _surveyRepository.DeactivateAsync(id);
        }
    }
}

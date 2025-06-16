using BloodDonationAPI.DTO;
using BloodDonationAPI.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodDonationAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SurveyController : ControllerBase
    {
        private readonly ISurveyService _surveyService;

        public SurveyController(ISurveyService surveyService)
        {
            _surveyService = surveyService;
        }
        /// <summary>
        /// Dùng để lấy tất cả các câu hỏi
        /// </summary>
        
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpGet("questions")]
        public async Task<ActionResult<List<SurveyQuestionDto>>> GetAllQuestions()
        {
            var questions = await _surveyService.GetAllQuestions();
            return Ok(questions);
        }

        /// <summary>
        /// Dùng để user trả lời theo questionid và optionid
        /// </summary>
        
        /// <param name="dto"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("answers")]
        public async Task<ActionResult<string>> SubmitAnswer(SubmitAnswerDto dto)
        {
            var username = User.Identity.Name;
            var result = await _surveyService.SubmitAnswer(username, dto);
            if (result == "Question not found")
                return NotFound(result);
            return Ok(result);
        }

        /// <summary>
        /// Dùng để lấy tất cả các câu trả lời theo tên user
        /// </summary>
        
        /// <param name="dto"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("answers/{username}")]
        public async Task<ActionResult<List<UserAnswerDto>>> GetUserAnswers(string username, [FromQuery] int eventId)
        {
            var answers = await _surveyService.GetUserAnswers(username, eventId);
            return Ok(answers);
        }
    }
} 
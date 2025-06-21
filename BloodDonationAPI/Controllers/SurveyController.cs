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


        [HttpPost("submit-survey-answers")]
        public async Task<IActionResult> SubmitSurveyAnswersAsync([FromBody] SurveyAnswerDto dto)
        {
            try
            {
                await _surveyService.SubmitSurveyAnswersAsync(dto);
                return Ok(new { message = "Câu trả lời đã được lưu thành công." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi lưu câu trả lời: " + ex.Message });
            }
        }


        [HttpGet("answered/{appointmentId}")]
        public async Task<IActionResult> GetAnsweredByAppointmentIdAsync(int appointmentId)
        {
            var answered = await _surveyService.GetAnsweredByAppointmentIdAsync(appointmentId);
            if (answered == null || !answered.Any())
            {
                return NotFound(new { message = "Không tìm thấy câu trả lời cho cuộc hẹn này." });
            }
            return Ok(answered);
        }

        ///// <summary>
        ///// Dùng để user trả lời theo questionid và optionid
        ///// </summary>

        ///// <param name="dto"></param>
        ///// <returns></returns>
        //[Authorize]
        //[HttpPost("answers")]
        //public async Task<ActionResult<string>> SubmitAnswer(SubmitAnswerDto dto)
        //{
        //    var username = User.Identity.Name;
        //    var result = await _surveyService.SubmitAnswer(username, dto);
        //    if (result == "Question not found")
        //        return NotFound(result);
        //    return Ok(result);
        //}

        ///// <summary>
        ///// Dùng để lấy tất cả các câu trả lời theo tên user
        ///// </summary>

        ///// <param name="dto"></param>
        ///// <returns></returns>
        //[Authorize(Roles = "Admin,Staff")]
        //[HttpGet("answers/{username}")]
        //public async Task<ActionResult<List<UserAnswerDto>>> GetUserAnswers(string username, [FromQuery] int eventId)
        //{
        //    var answers = await _surveyService.GetUserAnswers(username, eventId);
        //    return Ok(answers);
        //}
    }
} 
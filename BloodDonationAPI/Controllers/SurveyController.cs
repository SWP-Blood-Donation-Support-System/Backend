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
        /// Api này dùng để submit câu trả lời khảo sát của người dùng
        /// </summary>
        /// <remarks>
        /// FE khi bấm nút dăng kí thì sẻ tạo 1 appointmentId và chuyển trang cho người dùng trả lời khảo sát. 
        /// 
        /// đầu tiên là khi bấm nút sẻ gọi api lấy các câu hỏi khảo sát, sau đó người dùng trả lời xong thì sẽ submit câu trả lời khảo sát này bằng cách gọi api này.
        /// 
        /// Lấy appoimentID từ cái đăng kí mới tạo ra, và gửi kèm theo các câu trả lời của người dùng.khi trả lời xong và bấm nút submit thì sẽ gọi api này để lưu câu trả lời khảo sát vào database.
        /// </remarks>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("submit-survey-answers")]
        public async Task<IActionResult> SubmitSurveyAnswersAsync([FromBody] SurveyAnswerDto dto)
        {
          var result = await _surveyService.SubmitSurveyAnswersAsync(dto);
           return Ok(new { message = result });
        }

        /// <summary>
        /// Dùng để lấy tất cả các câu trả lời đã được trả lời theo appointmentId
        /// </summary>
        /// <remarks>
        /// FE sẻ gửi appointmentId của cuộc hẹn để lấy các câu trả lời đã được trả lời cho cuộc hẹn đó.
        /// 
        /// dung de cho user xem lai cac cau tra loi da tra loi cua minh cho cuoc hen do. 
        /// 
        /// hoac staff xem lai cac cau tra loi da tra loi cua user cho cuoc hen do.
        /// 
        /// </remarks>
        /// <param name="appointmentId"></param>
        /// <returns></returns>
        [HttpGet("answered/{appointmentId}")]
        public async Task<IActionResult> GetAnsweredByAppointmentIdAsync(int appointmentId)
        {
            var answered = await _surveyService.GetAnsweredByAppointmentIdAsync(appointmentId);
            if (answered == null )
            {
                return NotFound(new { message = "Không tìm thấy câu trả lời cho cuộc hẹn này." });
            }
            return Ok(answered);
        }

        /// <summary>
        /// Api này dùng để lấy tất cả các câu trả lời đã được trả lời và có trạng thái đang chờ duyệt (pending) của cuộc hẹn.
        /// </summary>
        /// <remarks>
        /// cái này dùng để staff xem tất cả các câu trả lời đã được trả lời và có trạng thái đang chờ duyệt (pending) của cuộc hẹn. để làm them chức năng kế là duyệt trạng thái cho lịch hẹn này.
        /// </remarks>
        /// <returns></returns>

        [HttpGet("pending")]
        public async Task<IActionResult> GetAllAnsweredHaveAppointmentStatusAsync()
        {
            var answered = await _surveyService.GetAllAnsweredOfAppointmentHavePendinStatusAsync();
            if (answered == null || !answered.Any())
            {
                return NotFound(new { message = "Không tìm thấy câu trả lời cho cuộc hẹn này." });
            }
            return Ok(answered);
        }

        /// <summary>
        /// Api này dùng để cập nhật trạng thái của cuộc hẹn sau khi đã duyệt câu trả lời khảo sát.
        /// </summary>
        /// <remarks>
        /// Sau khi staff duyệt câu trả lời khảo sát thì sẽ gọi api này để cập nhật trạng thái của cuộc hẹn.
        /// 
        /// Trạng thái sẻ là "Đã đủ điều kiện", "Không đủ điều kiện" hoặc đang xét duyệt.
        /// </remarks>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPut("update-appointment-status")]
        public async Task<IActionResult> UpdateAppointmentStatus([FromBody] UpdataAppointmentStatusDto dto)
        {
            if (dto == null || dto.AppointmentId <= 0 || string.IsNullOrEmpty(dto.Status))
            {
                return BadRequest(new { message = "Thông tin cập nhật không hợp lệ." });
            }
            var result = await _surveyService.UpdateAppointmentStatus(dto);
            if (result)
            {
                return Ok(new { message = "Cập nhật trạng thái lịch hẹn thành công." });
            }
            return NotFound(new { message = "Lịch hẹn không tồn tại hoặc cập nhật thất bại." });
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
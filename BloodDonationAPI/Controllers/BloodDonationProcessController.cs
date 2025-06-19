using BloodDonationAPI.DTO;
using BloodDonationAPI.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BloodDonationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BloodDonationProcessController : ControllerBase
    {
        private readonly IBloodDonationProcessService _service;

        public BloodDonationProcessController(IBloodDonationProcessService service)
        {
            _service = service;
        }

        /// <summary>
        /// Api này dùng để lấy tất cacr các user nào đã đăng ký tham gia hiến máu của một sự kiện hiến máu
        /// </summary>
        /// <remarks>
        /// nhập vào EventID của sự kiện hiến máu để lấy danh sách người đã đăng ký tham gia sự kiện đó.
        /// 
        /// FE sẽ hiện cho staff xem các event có và khi nhan vao 1 event nào đó thì sẽ lấy eventID của event đó và gửi vào đây để lấy danh sách người đã đăng ký tham gia sự kiện đó.
        /// </remarks>
        /// <param name="AppointmentID"></param>
        /// <returns></returns>
        [HttpGet("GetRegisterListByEventID/{EventID}")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> GetRegistrationsByEventID(int EventID)
        {
            var result = await _service.GetRegistrationsByEventID(EventID);
            if (result == null || !result.Any())
            {
                return NotFound("No registrations found for this appointment ID.");
            }
            return Ok(result);
        }
        /// <summary>
        /// API này dùng để checkin người đã đăng ký tham gia hiến máu
        /// </summary>
        /// <remarks>
        /// Nhap vào AppointmentId của người đã đăng ký tham gia hiến máu để cập nhật trạng thái của họ là đã đến.
        /// 
        /// FE thi sau khi lấy danh sách người đã đăng ký tham gia hiến máu thì sẽ hiện nút checkin cho từng người, khi nhấn vào nút đó thì sẽ gửi AppointmentId của người đó vào đây để cập nhật trạng thái của họ là đã đến.
        /// </remarks>
        /// <param name="checkInDto"></param>
        /// <returns></returns>
        [HttpPut("Checkin")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> CheckIn([FromBody] CheckInDto checkInDto)
        {
            if (checkInDto == null || checkInDto.AppointmentId <= 0)
            {
                return BadRequest(new { message = "Invalid appointment history ID." });
            }

            var result = await _service.CheckInAsync(checkInDto);
            if (!result)
            {
                return NotFound(new { message = "Appointment history not found or update failed." });
            }
            return Ok(new { message = "Appointment status updated successfully." });
        }
        /// <summary>
        /// API nay dùng để ghi nhận hiến máu của người đã đăng ký tham gia hiến máu 
        /// </summary>
        /// <remarks>
        /// Nhập vào AppointmentId sau đó cập nhat trạng thái của người đã đăng ký tham gia hiến máu là đã hiến máu, đồng thời ghi nhận nhóm máu và thể tích máu đã hiến. 
        /// 
        /// Nó dùng nhóm máu của người đã đăng ký tham gia hiến máu nếu người đó đã đăng ký nhóm máu trong hồ sơ của họ, nếu không thì Staff sẻ nhập nhóm máu vào và cập nhật vào hồ sơ cho user.
        /// 
        /// Nó sẽ thêm bản ghi vào bảng BloodDetail để ghi nhận chi tiết về việc hiến máu, bao gồm nhóm máu, thể tích máu và thời gian hiến máu.
        /// 
        /// Cộng luon thể tích máu đã hiến vào tổng thể tích máu của nhóm máu đó trong bảng BloodBank, nếu nhóm máu đó không có trong bảng BloodBank thì sẽ tạo mới một bản ghi cho nhóm máu đó.
        /// </remarks>
        /// <param name="donateDto"></param>
        /// <returns></returns>
        [HttpPost("RecordDonation")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> RecordDonation([FromBody] DonateDto donateDto)
        {
            if (donateDto == null || donateDto.AppointmentId <= 0 || string.IsNullOrEmpty(donateDto.BloodType) || donateDto.Volume <= 0)
            {
                return BadRequest("Invalid donation data.");
            }
            var result = await _service.RecordDonationAsync(donateDto);
            if (!result)
            {
                return NotFound(new { message = "Không tìm thấy lịch hẹn hoặc không thể ghi nhận hiến máu" });
            }
            return Ok(new { message = "Đã ghi nhận hiến máu thành công" });
        }

        //[HttpPost("AddDonationHistory")]
        //[Authorize(Roles = "Staff")]
        //public async Task<IActionResult> AddDonationHistory([FromBody] CreateDonationHistoryDto registrationDto)
        //{
        //    if (registrationDto == null || string.IsNullOrEmpty(registrationDto.Username))
        //    {
        //        return BadRequest("Invalid donation history data.");
        //    }
        //    var result = await _service.AddDonationHistoryAsync(registrationDto);
        //    if (!result)
        //    {
        //        return NotFound("User not found or donation history could not be added.");
        //    }
        //    return Ok("Donation history added successfully.");
        //}
        //[HttpPost("AddBloodToBank")]
        //[Authorize(Roles = "Staff")]
        //public async Task<IActionResult> AddBloodToBank([FromBody] AddBloodBankDto dto)
        //{
        //    try
        //    {
        //        var result = await _service.AddBloodToBankAsync(dto);
        //        if (result == null)
        //        {
        //            return BadRequest("Failed to add blood to the bank. Please check the input data.");
        //        }
        //        else
        //        {
        //            return Ok(new
        //            {
        //                message = "Blood added to bank successfully.",
        //                bloodType = result.BloodType,
        //                bloodVolumeTotal = result.BloodVolumeTotal,
        //                bloodBankStatus = result.BloodBankStatus,
        //            });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "An error occurred while adding blood to the bank", error = ex.Message });
        //    }

        //}

        //[HttpGet("GetDonationHistoryByUserName/{username}")]
        //[Authorize(Roles = "User")]
        //public async Task<IActionResult> GetDonationHistoryByUserName(string username)
        //{
        //    if (string.IsNullOrEmpty(username))
        //    {
        //        return BadRequest("Username cannot be null or empty.");
        //    }
        //    var result = await _service.GetDonationHistoryByUserNameAsync(username);
        //    if (result == null || !result.Any())
        //    {
        //        return NotFound("No donation history found for this user.");
        //    }
        //    return Ok(result);
        //}
    }

}

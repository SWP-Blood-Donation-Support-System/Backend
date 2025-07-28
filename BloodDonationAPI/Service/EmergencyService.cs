using BloodDonationAPI.DTO;
using BloodDonationAPI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BloodDonationAPI.Service
{
    public class EmergencyService : IEmergencyService
    {
        private readonly BloodDonationSystemContext _context;
        private readonly ILogger<EmergencyService> _logger;
        private readonly INotificationService _notificationService;

        public EmergencyService(
            BloodDonationSystemContext context,
            ILogger<EmergencyService> logger,
            INotificationService notificationService)
        {
            _context = context;
            _logger = logger;
            _notificationService = notificationService;
        }

        public async Task<RegisterEmergencyResponseDto> RegisterEmergency(string username, string role, RegisterEmergencyDto dto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null)
                    return new RegisterEmergencyResponseDto 
                    { 
                        Message = "User not found.", 
                        IsSuccess = false 
                    };

                // Validate blood type
                if (!IsValidBloodType(dto.BloodType))
                    return new RegisterEmergencyResponseDto 
                    { 
                        Message = "Invalid blood type.", 
                        IsSuccess = false 
                    };

                // Validate hospital if provided
                Hospital? hospital = null;
                if (dto.HospitalId.HasValue)
                {
                    hospital = await _context.Hospitals.FindAsync(dto.HospitalId.Value);
                    if (hospital == null)
                        return new RegisterEmergencyResponseDto 
                        { 
                            Message = "Invalid hospital ID.", 
                            IsSuccess = false 
                        };
                }
                else
                {
                    return new RegisterEmergencyResponseDto 
                    { 
                        Message = "Hospital ID is required.", 
                        IsSuccess = false 
                    };
                }

                // Set emergency status based on user role
                string emergencyStatus = (role == "Staff" || role == "Admin") ? "Đã xét duyệt" : "Chờ xét duyệt";

                var emergency = new Emergency
                {
                    Username = username,
                    EmergencyDate = DateOnly.FromDateTime(DateTime.Now),
                    BloodType = dto.BloodType,
                    EmergencyStatus = emergencyStatus,
                    EmergencyNote = $"Cần {dto.RequiredUnits} đơn vị nhóm máu {dto.BloodType} tại {hospital.HospitalName}",
                    RequiredUnits = dto.RequiredUnits,
                    HospitalId = dto.HospitalId,
                    EmergencyMedical = dto.EmergencyMedical,
                    EmergencyImage = dto.EmergencyImage,
                    EndDate = dto.EndDate
                };

                _context.Emergencies.Add(emergency);
                await _context.SaveChangesAsync();

                return new RegisterEmergencyResponseDto 
                { 
                    Message = "Emergency registration successful.", 
                    EmergencyId = emergency.EmergencyId,
                    IsSuccess = true 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RegisterEmergency");
                throw;
            }
        }

        public async Task<List<Emergency>> GetEmergencies()
        {
            try
            {
                var emergencies = await _context.Emergencies
                    .OrderByDescending(e => e.EmergencyDate)
                    .ToListAsync();
                var today = DateOnly.FromDateTime(DateTime.Now);
                foreach (var e in emergencies)
                {
                    if (e.EndDate.HasValue && e.EndDate < today && e.EmergencyStatus != "Đã quá hạn" && e.EmergencyStatus != "Đã được đáp ứng")
                    {
                        e.EmergencyStatus = "Đã quá hạn";
                    }
                }
                await _context.SaveChangesAsync();
                return emergencies;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetEmergencies: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<string> UpdateEmergencyStatus(int emergencyId, string status)
        {
            try
            {
                var emergency = await _context.Emergencies.FindAsync(emergencyId);
                if (emergency == null)
                    return "Emergency not found.";

                if (status != "Đã xét duyệt" && status != "Từ chối" && status != "Lượng máu đang được chuyển đến")
                    return "Invalid status. Status must be either 'Đã xét duyệt', 'Từ chối' hoặc 'Lượng máu đang được chuyển đến'.";

                emergency.EmergencyStatus = status;
                await _context.SaveChangesAsync();

                // Nếu chuyển sang "Đã xét duyệt", kiểm tra kho máu và gửi thông báo phù hợp
                if (status == "Đã xét duyệt")
                {
                    // Kiểm tra kho máu có đủ không
                    var availableBlood = await _context.BloodDetails
                        .Where(b => b.BloodType == emergency.BloodType && 
                                   b.BloodDetailStatus == "Còn hạn" && 
                                   b.Volume > 0)
                        .SumAsync(b => b.Volume);

                    if (availableBlood < emergency.RequiredUnits)
                    {
                        // Nếu không đủ máu, gửi thông báo cho user tạo đơn
                        if (_notificationService != null)
                        {
                            await _notificationService.CreateUserNotificationApprovedButInsufficientBlood(emergencyId);
                        }
                    }
                }

                return "Emergency status updated successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating emergency status");
                throw;
            }
        }

        public async Task<BloodCompareResultDto> CompareBloodForEmergency(int emergencyId)
        {
            var emergency = await _context.Emergencies.FirstOrDefaultAsync(e => e.EmergencyId == emergencyId);
            if (emergency == null)
                throw new Exception("Emergency not found");
            if (string.IsNullOrEmpty(emergency.BloodType) || !emergency.RequiredUnits.HasValue)
                throw new Exception("Emergency missing blood type or required units");
            // Lấy các dòng máu còn hạn đúng nhóm máu
            var availableBlood = await _context.BloodDetails
                .Where(b => b.BloodType == emergency.BloodType && b.BloodDetailStatus == "Đã lưu trữ" && b.Volume > 0)
                .OrderBy(b => b.BloodDetailDate)
                .ToListAsync();
            var totalAvailable = availableBlood.Sum(b => b.Volume ?? 0);
            var result = new BloodCompareResultDto
            {
                IsEnough = totalAvailable >= emergency.RequiredUnits,
                RequiredUnits = emergency.RequiredUnits,
                AvailableUnits = totalAvailable
            };
            if (result.IsEnough)
            {
                int remaining = emergency.RequiredUnits.Value;
                result.Details = new List<BloodCompareResultDto.BloodDetailInfo>();
                foreach (var blood in availableBlood)
                {
                    if (remaining <= 0) break;
                    int use = Math.Min(blood.Volume ?? 0, remaining);
                    result.Details.Add(new BloodCompareResultDto.BloodDetailInfo
                    {
                        BloodDetailId = blood.BloodDetailId,
                        BloodType = blood.BloodType,
                        Volume = use,
                        BloodDetailDate = blood.BloodDetailDate
                    });
                    remaining -= use;
                }
            }
            return result;
        }

        public async Task<string> UpdateEmergency(int emergencyId, string username, string role, RegisterEmergencyDto dto)
        {
            var emergency = await _context.Emergencies.FindAsync(emergencyId);
            if (emergency == null)
                return "Emergency not found.";

            // Chỉ cho phép user tạo hoặc Admin/Staff sửa
            if (emergency.Username != username && role != "Admin" && role != "Staff")
                return "You are not authorized to update this emergency.";

            // Cập nhật thông tin
            if (!string.IsNullOrEmpty(dto.BloodType))
                emergency.BloodType = dto.BloodType;
            if (dto.RequiredUnits.HasValue)
                emergency.RequiredUnits = dto.RequiredUnits;
            if (dto.HospitalId.HasValue)
                emergency.HospitalId = dto.HospitalId;
            if (!string.IsNullOrEmpty(dto.EmergencyMedical))
                emergency.EmergencyMedical = dto.EmergencyMedical;
            if (!string.IsNullOrEmpty(dto.EmergencyImage))
                emergency.EmergencyImage = dto.EmergencyImage;
            if (dto.EndDate.HasValue)
                emergency.EndDate = dto.EndDate;
            // Có thể cập nhật thêm các trường khác nếu cần

            await _context.SaveChangesAsync();
            return "Emergency updated successfully.";
        }

        public async Task<string> DeleteEmergency(int emergencyId, string username, string role)
        {
            var emergency = await _context.Emergencies.FindAsync(emergencyId);
            if (emergency == null)
                return "Emergency not found.";

            // Chỉ cho phép user tạo hoặc Admin/Staff xóa
            if (emergency.Username != username && role != "Admin" && role != "Staff")
                return "You are not authorized to delete this emergency.";

            _context.Emergencies.Remove(emergency);
            await _context.SaveChangesAsync();
            return "Emergency deleted successfully.";
        }

        public async Task<List<Emergency>> GetEmergenciesByUsername(string username)
        {
            var emergencies = await _context.Emergencies
                .Where(e => e.Username == username)
                .OrderByDescending(e => e.EmergencyDate)
                .ToListAsync();
            var today = DateOnly.FromDateTime(DateTime.Now);
            bool changed = false;
            foreach (var e in emergencies)
            {
                if (e.EndDate.HasValue && e.EndDate < today && e.EmergencyStatus != "Đã quá hạn" && e.EmergencyStatus != "Đã được đáp ứng")
                {
                    e.EmergencyStatus = "Đã quá hạn";
                    changed = true;
                }
            }
            if (changed) await _context.SaveChangesAsync();
            return emergencies;
        }

        public async Task<string> MarkEmergencyAsFulfilled(int emergencyId, string username)
        {
            var emergency = await _context.Emergencies.FindAsync(emergencyId);
            if (emergency == null)
                return "Emergency not found.";
            if (emergency.Username != username)
                return "You are not authorized to update this emergency.";
            if (emergency.EmergencyStatus == "Từ chối" || emergency.EmergencyStatus == "Đợi xét duyệt")
                return "Cannot mark as fulfilled when status is 'Từ chối' or 'Đợi xét duyệt'.";
            if (emergency.EmergencyStatus != "Đã xét duyệt" && emergency.EmergencyStatus != "Lượng máu đang được chuyển đến")
                return "Only emergencies with status 'Đã xét duyệt' or 'Lượng máu đang được chuyển đến' can be marked as fulfilled.";
            emergency.EmergencyStatus = "Đã được đáp ứng";
            await _context.SaveChangesAsync();
            return "Emergency marked as fulfilled.";
        }

        public async Task<string> SetEmergencyStatusToTransferring(int emergencyId)
        {
            var emergency = await _context.Emergencies.FindAsync(emergencyId);
            if (emergency == null)
                return "Emergency not found.";
            emergency.EmergencyStatus = "Lượng máu đang được chuyển đến";
            await _context.SaveChangesAsync();

            // Gửi thông báo cho user tạo đơn
            if (_notificationService != null)
            {
                await _notificationService.CreateUserNotificationBloodTransferring(emergencyId);
            }

            return "Emergency status set to 'Lượng máu đang được chuyển đến'.";
        }

        private bool IsValidBloodType(string bloodType)
        {
            var validTypes = new[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };
            return validTypes.Contains(bloodType);
        }
    }
} 
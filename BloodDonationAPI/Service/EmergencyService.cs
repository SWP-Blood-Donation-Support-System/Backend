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

        public EmergencyService(BloodDonationSystemContext context, ILogger<EmergencyService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string> RegisterEmergency(string username, RegisterEmergencyDto dto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null)
                    return "User not found.";

                if (user.ProfileStatus != "Active")
                    return "User profile is not active.";

                // Validate blood type
                if (!IsValidBloodType(dto.BloodType))
                    return "Invalid blood type.";

                // Validate hospital if provided
                Hospital? hospital = null;
                if (dto.HospitalId.HasValue)
                {
                    hospital = await _context.Hospitals.FindAsync(dto.HospitalId.Value);
                    if (hospital == null)
                        return "Invalid hospital ID.";
                }
                else
                {
                    return "Hospital ID is required.";
                }

                var emergency = new Emergency
                {
                    Username = username,
                    EmergencyDate = DateOnly.FromDateTime(DateTime.Now),
                    BloodType = dto.BloodType,
                    EmergencyStatus = "Chờ xét duyệt",
                    EmergencyNote = $"Cần {dto.RequiredUnits} đơn vị nhóm máu {dto.BloodType} tại {hospital.HospitalName}",
                    RequiredUnits = dto.RequiredUnits,
                    HospitalId = dto.HospitalId
                };

                _context.Emergencies.Add(emergency);
                await _context.SaveChangesAsync();

                return "Emergency registration successful.";
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
                    .Select(e => new Emergency
                    {
                        EmergencyId = e.EmergencyId,
                        Username = e.Username,
                        EmergencyDate = e.EmergencyDate,
                        BloodType = e.BloodType,
                        EmergencyStatus = e.EmergencyStatus,
                        EmergencyNote = e.EmergencyNote,
                        RequiredUnits = e.RequiredUnits,
                        HospitalId = e.HospitalId,
                        Hospital = e.HospitalId.HasValue ? new Hospital
                        {
                            HospitalId = e.Hospital.HospitalId,
                            HospitalName = e.Hospital.HospitalName,
                            HospitalAddress = e.Hospital.HospitalAddress,
                            HospitalPhone = e.Hospital.HospitalPhone
                        } : null,
                        UsernameNavigation = new User
                        {
                            Username = e.UsernameNavigation.Username,
                            FullName = e.UsernameNavigation.FullName,
                            Phone = e.UsernameNavigation.Phone,
                            Email = e.UsernameNavigation.Email
                        }
                    })
                    .OrderByDescending(e => e.EmergencyDate)
                    .ToListAsync();

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

                if (status != "Đã xét duyệt" && status != "Từ chối")
                    return "Invalid status. Status must be either 'Đã xét duyệt' or 'Từ chối'.";

                emergency.EmergencyStatus = status;
                await _context.SaveChangesAsync();

                return "Emergency status updated successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateEmergencyStatus");
                throw;
            }
        }

        private bool IsValidBloodType(string bloodType)
        {
            var validTypes = new[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };
            return validTypes.Contains(bloodType);
        }
    }
} 
using BloodDonationAPI.DTO;
using BloodDonationAPI.Entities;

namespace BloodDonationAPI.Service
{
    public interface IEmergencyService
    {
        Task<RegisterEmergencyResponseDto> RegisterEmergency(string username, string role, RegisterEmergencyDto dto);
        Task<List<Emergency>> GetEmergencies();
        Task<string> UpdateEmergencyStatus(int emergencyId, string status);
        Task<BloodCompareResultDto> CompareBloodForEmergency(int emergencyId);
        Task<string> UpdateEmergency(int emergencyId, string username, string role, RegisterEmergencyDto dto);
        Task<string> DeleteEmergency(int emergencyId, string username, string role);
        Task<List<Emergency>> GetEmergenciesByUsername(string username);
        Task<string> MarkEmergencyAsFulfilled(int emergencyId, string username);
    }
}
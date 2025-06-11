using BloodDonationAPI.DTO;
using BloodDonationAPI.Entities;

namespace BloodDonationAPI.Service
{
    public interface IBloodDonationProcessService
    {
      Task<List<AppointmentRegistrationDto>> GetRegistrationsByAppointmentID(int AppointmentID);

        Task<bool> UpdateAppointmentStatusAsync(UpdateAppointmentStatusDto updateDto);

        Task<bool> AddDonationHistoryAsync(CreateDonationHistoryDto registrationDto);

        Task<BloodBank> AddBloodToBankAsync(AddBloodBankDto dto);

        Task<List<DonationHistoryDto>> GetDonationHistoryByUserNameAsync(string username);
    }
}

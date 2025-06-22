using BloodDonationAPI.DTO;
using BloodDonationAPI.Entities;

namespace BloodDonationAPI.Service
{
    public interface IBloodDonationProcessService
    {
      Task<List<AppointmentRegistrationDto>> GetRegistrationsByEventID(int EventID);

        Task<bool> CheckInAsync(CheckInDto checkInDto);

        Task<bool> RecordDonationAsync(DonateDto donateDto);

        Task UpdateEligibleUsersAsync ();

        //Task<BloodBank> AddBloodToBankAsync(AddBloodBankDto dto);

        //Task<List<DonationHistoryDto>> GetDonationHistoryByUserNameAsync(string username);
    }
}

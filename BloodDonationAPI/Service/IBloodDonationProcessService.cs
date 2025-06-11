using BloodDonationAPI.DTO;

namespace BloodDonationAPI.Service
{
    public interface IBloodDonationProcessService
    {
      Task<List<AppointmentRegistrationDto>> GetRegistrationsByAppointmentID(int AppointmentID);

        Task<bool> UpdateAppointmentStatusAsync(UpdateAppointmentStatusDto updateDto);
    }
}

using BloodDonationAPI.DTO;
using BloodDonationAPI.Entities;

namespace BloodDonationAPI.Service
{
    public interface IAppointmentServiece
    {
        Task<List<AppointmentList>> GetEventsLists();
        Task<string> RegisterAppointment( string userName , RegisterAppointmentDto Dto);

        Task<List<AppointmentHistoryDto>> GetByUsernameAsync(string username);

        Task<bool> CancelAppointmentAsync(int appointmentId);

    }
}

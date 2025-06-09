using BloodDonationAPI.DTO;
using BloodDonationAPI.Entities;

namespace BloodDonationAPI.Service
{
    public interface IAppointmentServiece
    {
        Task<List<AppointmentList>> GetAppointmentLists();
        Task<string> RegisterAppointment( string userName , RegisterAppointmentDto Dto);
    }
}

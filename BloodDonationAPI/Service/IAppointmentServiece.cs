using BloodDonationAPI.DTO;
using BloodDonationAPI.Entities;

namespace BloodDonationAPI.Service
{
    public interface IAppointmentServiece
    {
        Task<List<EventDto>> GetEventsLists();
        Task<RegisterAppointmentResultDto> RegisterAppointment( string userName , RegisterAppointmentDto Dto);

        Task<List<AppointmentHistoryDto>> GetByUsernameAsync(string username);

        Task<bool> CancelAppointmentAsync(int appointmentRecordId);


        Task<RegisterAppointmentResultDto> RegisterAppointmentV2(string userName, RegisterAppointmentDtoV2 Dto);

    }
}

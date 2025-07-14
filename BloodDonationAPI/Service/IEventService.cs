using BloodDonationAPI.Entities;

namespace BloodDonationAPI.Service
{
    public interface IEventService
    {
        Task<List<Event>> GetAllEventsAsync();
        Task<Event?> GetEventByIdAsync(int id);
        Task AddEventAsync(Event newEvent);
        Task UpdateEventAsync(int id ,Event updatedEvent);
        Task<bool> DeleteEventAsync(int id);
    }
}

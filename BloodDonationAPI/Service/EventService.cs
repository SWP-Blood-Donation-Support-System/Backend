using BloodDonationAPI.Entities;
using BloodDonationAPI.Repositories;

namespace BloodDonationAPI.Service
{
    public class EventService : IEventService
    {
        private readonly IRepository<Event> _eventRepository;
        public EventService(IRepository<Event> eventRepository)
        {
            _eventRepository = eventRepository;
        }
        
        public async Task<List<Event>> GetAllEventsAsync()
        {
            return (await _eventRepository.GetAllAsync()).ToList();
        }
        
        public async Task<Event?> GetEventByIdAsync(int id)
        {
            return await _eventRepository.GetByIdAsync(id);
        }
       
        public async Task AddEventAsync(Event newEvent)
        {
            newEvent.EventStatus = "Public"; // Default status for new events
            await _eventRepository.AddAsync(newEvent);
            await _eventRepository.SaveChangesAsync();
        }
       
        public async Task UpdateEventAsync(int id, Event updatedEvent)
        {
            var existingEvent = await _eventRepository.GetByIdAsync(id);
            if (existingEvent != null)
            {
                existingEvent.EventTitle = updatedEvent.EventTitle;
                existingEvent.EventContent = updatedEvent.EventContent;
                existingEvent.EventDate = updatedEvent.EventDate;
                existingEvent.EventTime = updatedEvent.EventTime;
                existingEvent.Location = updatedEvent.Location;
                existingEvent.MaxParticipants = updatedEvent.MaxParticipants;
                existingEvent.BloodTypeRequired = updatedEvent.BloodTypeRequired;
                existingEvent.EventStatus = updatedEvent.EventStatus;
                _eventRepository.Update(existingEvent);
                await _eventRepository.SaveChangesAsync();
            }
        }
        public async Task<bool> DeleteEventAsync(int id)    
        {
            var existingEvent = await _eventRepository.GetByIdAsync(id);
            if (existingEvent == null) return false;

            _eventRepository.Delete(existingEvent);
            await _eventRepository.SaveChangesAsync();
            return true;

        }
    }
}

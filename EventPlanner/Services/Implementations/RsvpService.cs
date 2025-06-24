using AutoMapper;
using EventPlanner.DTOs.RSVP;
using EventPlanner.Enums;
using EventPlanner.Models;
using EventPlanner.Repositories.Implementations;
using EventPlanner.Repositories.Interfaces;
using EventPlanner.Services.Interfaces;

namespace EventPlanner.Services.Implementations
{
    public class RsvpService : IRsvpService
    {
        private readonly IRsvpRepository _rsvpRepository;
        private readonly IMapper _mapper;
        private readonly IInviteRepository _inviteRepository;
        private readonly IEventRepository _eventRepository;

        public RsvpService(IRsvpRepository rsvpRepository, IMapper mapper, IInviteRepository inviteRepository, IEventRepository eventRepository)
        {
            _rsvpRepository = rsvpRepository;
            _mapper = mapper;
            _inviteRepository=inviteRepository;
            _eventRepository = eventRepository;
        }

        public async Task<RsvpDTO> GetRsvpByIdAsync(int id)
        {
            var rsvp = await _rsvpRepository.GetRsvpByIdAsync(id);
            if (rsvp == null)
                throw new ArgumentException("RSVP not found");

            return _mapper.Map<RsvpDTO>(rsvp);
        }
        public async Task<IEnumerable<RsvpDTO>> GetAllRsvpsAsync()
        {
            var rsvps = await _rsvpRepository.GetAllRsvpsAsync();

            return rsvps.Select(r => new RsvpDTO
            {
                Id = r.Id,
                UserName = r.User?.FirstName + " " + r.User?.LastName,
                EventId = r.EventId,
                EventTitle = r.Event?.Title,
                Status = r.Status,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            });
        }


        public async Task<IEnumerable<RsvpDTO>> GetRsvpsByEventIdAsync(int eventId)
        {
            if (eventId <= 0)
                throw new ArgumentException("Invalid event ID");

            var rsvps = await _rsvpRepository.GetRsvpByEventIdAsync(eventId);
            return _mapper.Map<IEnumerable<RsvpDTO>>(rsvps);
        }

        public async Task<IEnumerable<RsvpDTO>> GetRsvpsByUserIdAsync(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("Invalid user ID");

            var rsvps = await _rsvpRepository.GetRsvpsByUserIdAsync(userId.ToString());
            return _mapper.Map<IEnumerable<RsvpDTO>>(rsvps);
        }
        public async Task<RsvpDTO> CreateRsvpAsync(RsvpCreateDTO rsvpDto)
        {
            if (rsvpDto == null)
                throw new ArgumentNullException(nameof(rsvpDto));

            if (string.IsNullOrWhiteSpace(rsvpDto.UserId) || rsvpDto.EventId <= 0)
                throw new ArgumentException("Invalid user or event ID");

            // Check if RSVP already exists
            var existingRsvps = await _rsvpRepository.GetRsvpsByUserIdAsync(rsvpDto.UserId);
            if (existingRsvps.Any(r => r.EventId == rsvpDto.EventId))
                throw new InvalidOperationException("RSVP already exists for this user and event");

            // Check if event exists
            var eventItem = await _eventRepository.GetEventByIdAsync(rsvpDto.EventId);
            if (eventItem == null)
                throw new ArgumentException("Event not found");

            // ❗ 1. Prevent RSVP for past or inactive events
            if (eventItem.Status != EventStatus.Upcoming || eventItem.StartDate <= DateTime.UtcNow)
                throw new InvalidOperationException("You cannot RSVP to a past or inactive event.");

            // ❗ 2. Enforce privacy: must be invited if event is private
            if (eventItem.IsPrivate)
            {
                var invite = await _inviteRepository.GetByInviteeAndEventAsync(rsvpDto.UserId, rsvpDto.EventId);
                if (invite == null)
                    throw new InvalidOperationException("You must be invited to RSVP to this private event.");
            }


            // ❗ 3. Enforce participant limit
            var rsvpsForEvent = await _rsvpRepository.GetRsvpByEventIdAsync(rsvpDto.EventId);
            var currentCount = rsvpsForEvent.Count(r => r.Status == RSVPStatus.Going);

            if (currentCount >= eventItem.MaxParticipants)
                throw new InvalidOperationException("This event is full.");

            // Create the RSVP
            var rsvp = _mapper.Map<RSVP>(rsvpDto);
            rsvp.CreatedAt = DateTime.UtcNow;
            rsvp.UpdatedAt = null;

            await _rsvpRepository.AddRsvpAsync(rsvp);
            return _mapper.Map<RsvpDTO>(rsvp);
        }



        public async Task<RsvpDTO> UpdateRsvpAsync(int id, RsvpUpdateDTO rsvpDto)
        {
            if (rsvpDto == null)
                throw new ArgumentNullException(nameof(rsvpDto));

            var existing = await _rsvpRepository.GetRsvpByIdAsync(id);
            if (existing == null)
                throw new ArgumentException("RSVP not found");

            if (rsvpDto.UserId.ToString() != existing.UserId || rsvpDto.EventId != existing.EventId)
                throw new InvalidOperationException("User or Event cannot be changed");

            // ❗ PROTECT: Prevent update if event has started
            var eventItem = await _eventRepository.GetEventByIdAsync(existing.EventId);
            if (eventItem == null)
                throw new ArgumentException("Associated event not found");

            if (eventItem.StartDate <= DateTime.UtcNow)
                throw new InvalidOperationException("You cannot update RSVP after the event has started.");

            _mapper.Map(rsvpDto, existing);
            existing.UpdatedAt = DateTime.UtcNow;

            await _rsvpRepository.UpdateRsvpAsync(existing);
            return _mapper.Map<RsvpDTO>(existing);
        }

        public async Task DeleteRsvpAsync(int id)
        {
            var rsvp = await _rsvpRepository.GetRsvpByIdAsync(id);
            if (rsvp == null)
                throw new ArgumentException("RSVP not found");

            await _rsvpRepository.DeleteRsvpAsync(id);
        }
        public async Task<RsvpDTO> RespondToInviteAsync(int inviteId, RSVPStatus responseStatus)
        {
            var invite = await _inviteRepository.GetInviteByIdAsync(inviteId)
                ?? throw new ArgumentException("Invite not found");

            if (invite.RespondedAt != null)
                throw new InvalidOperationException("Invite has already been responded to.");

            if (invite.ExpiresAt != null && invite.ExpiresAt <= DateTime.UtcNow)
                throw new InvalidOperationException("Invite has expired.");

            var existingRsvp = await _rsvpRepository.GetRsvpsByUserIdAsync(invite.InviteeId);
            if (existingRsvp.Any(r => r.EventId == invite.EventId))
                throw new InvalidOperationException("RSVP already exists for this event.");

            var rsvp = new RSVP
            {
                UserId = invite.InviteeId,
                EventId = invite.EventId,
                Status = responseStatus,
                CreatedAt = DateTime.UtcNow
            };

            await _rsvpRepository.AddRsvpAsync(rsvp);

            invite.Status = responseStatus switch
            {
                RSVPStatus.Accepted => InviteStatus.Accepted,
                RSVPStatus.Going => InviteStatus.Accepted,
                RSVPStatus.Declined => InviteStatus.Declined,
                RSVPStatus.Maybe => InviteStatus.Maybe,
                RSVPStatus.Pending => InviteStatus.Pending,
                _ => InviteStatus.Pending
            };
            invite.RespondedAt = DateTime.UtcNow;
            await _inviteRepository.UpdateInviteAsync(invite);

            return _mapper.Map<RsvpDTO>(rsvp);
        }


        public async Task<bool> UpdateStatusAsync(int id, RSVPStatus status)
        {
            var rsvp = await _rsvpRepository.GetRsvpByIdAsync(id);
            if (rsvp == null)
                throw new ArgumentException("RSVP not found");

            rsvp.Status = status;
            rsvp.UpdatedAt = DateTime.UtcNow;

            await _rsvpRepository.UpdateRsvpAsync(rsvp);
            return true;
        }

        public async Task<RsvpDTO> CancelRsvpAsync(int id)
        {
            var rsvp = await _rsvpRepository.GetRsvpByIdAsync(id);
            if (rsvp == null)
                throw new ArgumentException("RSVP not found");

            rsvp.Status = RSVPStatus.Declined;
            rsvp.UpdatedAt = DateTime.UtcNow;

            await _rsvpRepository.UpdateRsvpAsync(rsvp);
            return _mapper.Map<RsvpDTO>(rsvp);
        }
    }
}

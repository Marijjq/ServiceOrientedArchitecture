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

        public RsvpService(IRsvpRepository rsvpRepository, IMapper mapper, IInviteRepository inviteRepository)
        {
            _rsvpRepository = rsvpRepository;
            _mapper = mapper;
            _inviteRepository=inviteRepository;
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
            return _mapper.Map<IEnumerable<RsvpDTO>>(rsvps);
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

            var existingRsvps = await _rsvpRepository.GetRsvpsByUserIdAsync(rsvpDto.UserId.ToString());
            if (existingRsvps.Any(r => r.EventId == rsvpDto.EventId))
                throw new InvalidOperationException("RSVP already exists for this user and event");

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

            // Convert InviteeId (string) to an integer if necessary
            if (!int.TryParse(invite.InviteeId, out var inviteeId))
                throw new ArgumentException("Invitee ID must be a valid integer");

            // Prevent duplicate RSVP
            var existingRsvp = await _rsvpRepository.GetRsvpsByUserIdAsync(invite.InviteeId); // Pass InviteeId as string
            if (existingRsvp.Any(r => r.EventId == invite.EventId))
                throw new InvalidOperationException("RSVP already exists for this invite");

            var rsvp = new RSVP
            {
                UserId = inviteeId.ToString(),
                EventId = invite.EventId,
                Status = responseStatus,
                CreatedAt = DateTime.UtcNow,
            };

            await _rsvpRepository.AddRsvpAsync(rsvp);

            // Optional: Update RespondedAt on Invite
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

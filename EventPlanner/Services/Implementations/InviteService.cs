using AutoMapper;
using EventPlanner.DTOs.Invite;
using EventPlanner.Enums;
using EventPlanner.Models;
using EventPlanner.Repositories.Implementations;
using EventPlanner.Repositories.Interfaces;
using EventPlanner.Services.Interfaces;

namespace EventPlanner.Services.Implementations
{
    public class InviteService : IInviteService
    {
        private readonly IInviteRepository _inviteRepository;
        private readonly IUserRepository _userRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IMapper _mapper;

        public InviteService(IInviteRepository inviteRepository, IMapper mapper, IUserRepository userRepository, IEventRepository eventRepository)
        {
            _inviteRepository = inviteRepository;
            _mapper = mapper;
            _userRepository = userRepository;
            _eventRepository = eventRepository;
        }

        public async Task<InviteDTO> GetInviteByIdAsync(int id)
        {
            var invite = await _inviteRepository.GetInviteByIdAsync(id);
            return _mapper.Map<InviteDTO>(invite);
        }

        public async Task<IEnumerable<InviteDTO>> GetAllInvitesAsync()
        {
            var invites = await _inviteRepository.GetAllInvitesAsync();
            return _mapper.Map<IEnumerable<InviteDTO>>(invites);
        }

        public async Task<InviteDTO> SendInviteAsync(InviteCreateDTO inviteDto)
        {
            if (inviteDto.InviterId == inviteDto.InviteeId)
                throw new ArgumentException("Inviter cannot be the same as Invitee");

            // Validate users exist
            var inviter = await _userRepository.GetUserByIdAsync(inviteDto.InviterId);
            var invitee = await _userRepository.GetUserByIdAsync(inviteDto.InviteeId);

            if (inviter == null)
                throw new ArgumentException("Inviter does not exist");
            if (invitee == null)
                throw new ArgumentException("Invitee does not exist");

            // Validate event exists
            var eventEntity = await _eventRepository.GetEventByIdAsync(inviteDto.EventId);
            if (eventEntity == null)
                throw new ArgumentException("Event does not exist");

            var existingInvite = await _inviteRepository.GetByInviteeAndEventAsync(inviteDto.InviteeId, inviteDto.EventId);
            if (existingInvite != null)
                throw new InvalidOperationException("User is already invited to this event");

            var invite = _mapper.Map<Invite>(inviteDto);
            invite.Status = InviteStatus.Pending;
            invite.SentAt = DateTime.UtcNow;
            invite.RespondedAt = null;

            await _inviteRepository.AddInviteAsync(invite);
            return _mapper.Map<InviteDTO>(invite);
        }

        public async Task<InviteDTO> UpdateInviteAsync(int id, InviteUpdateDTO inviteDto)
        {
            var invite = await _inviteRepository.GetInviteByIdAsync(id)
                ?? throw new ArgumentException("Invite not found");

            if (invite.RespondedAt != null && invite.Status != inviteDto.Status)
                throw new InvalidOperationException("Cannot update status of a responded invite");

            _mapper.Map(inviteDto, invite); // update fields
            await _inviteRepository.UpdateInviteAsync(invite);
            return _mapper.Map<InviteDTO>(invite);
        }

        public async Task DeleteInviteAsync(int id)
        {
            var invite = await _inviteRepository.GetInviteByIdAsync(id)
                ?? throw new ArgumentException("Invite not found");

            if (invite.Status == InviteStatus.Accepted)
                throw new InvalidOperationException("Cannot delete an accepted invite");

            await _inviteRepository.DeleteInviteAsync(id);
        }

        public async Task<IEnumerable<InviteDTO>> GetPendingInvitesByUserIdAsync(string userId)
        {
            var invites = await _inviteRepository.GetPendingInvitesByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<InviteDTO>>(invites);
        }

        public async Task<InviteDTO?> GetByInviteeAndEventAsync(string inviteeId, int eventId)
        {
            var invite = await _inviteRepository.GetByInviteeAndEventAsync(inviteeId, eventId);
            return _mapper.Map<InviteDTO>(invite);
        }

    }
   
}

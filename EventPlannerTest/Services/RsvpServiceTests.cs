using AutoMapper;
using EventPlanner.DTOs.RSVP;
using EventPlanner.Enums;
using EventPlanner.Models;
using EventPlanner.Repositories.Interfaces;
using EventPlanner.Services.Implementations;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventPlannerTest.Services
{
    public class RsvpServiceTests
    {
        private readonly Mock<IRsvpRepository> _mockRsvpRepo;
        private readonly Mock<IInviteRepository> _mockInviteRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly RsvpService _service;

        public RsvpServiceTests()
        {
            _mockRsvpRepo = new Mock<IRsvpRepository>();
            _mockInviteRepo = new Mock<IInviteRepository>();
            _mockMapper = new Mock<IMapper>();
            _service = new RsvpService(_mockRsvpRepo.Object, _mockMapper.Object, _mockInviteRepo.Object);
        }

        [Fact]
        public async Task GetAllRsvpsAsync_ReturnsMappedDTOs()
        {
            var rsvps = new List<RSVP> { new RSVP { Id = 1 }, new RSVP { Id = 2 } };
            var rsvpDTOs = new List<RsvpDTO> { new RsvpDTO { Id = 1 }, new RsvpDTO { Id = 2 } };

            _mockRsvpRepo.Setup(repo => repo.GetAllRsvpsAsync()).ReturnsAsync(rsvps);
            _mockMapper.Setup(mapper => mapper.Map<IEnumerable<RsvpDTO>>(rsvps)).Returns(rsvpDTOs);

            var result = await _service.GetAllRsvpsAsync();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task CreateRsvpAsync_ValidData_ReturnsCreatedRsvp()
        {
            var createDto = new RsvpCreateDTO { UserId = "1", EventId = 2 };
            var newRsvp = new RSVP { UserId = "1", EventId = 2 };
            var resultDto = new RsvpDTO { UserId = "1", EventId = 2 };

            _mockRsvpRepo.Setup(repo => repo.GetRsvpsByUserIdAsync("1")).ReturnsAsync(new List<RSVP>());
            _mockMapper.Setup(m => m.Map<RSVP>(createDto)).Returns(newRsvp);
            _mockMapper.Setup(m => m.Map<RsvpDTO>(newRsvp)).Returns(resultDto);

            var result = await _service.CreateRsvpAsync(createDto);

            Assert.Equal("1", result.UserId);
            Assert.Equal(2, result.EventId);
        }

        [Fact]
        public async Task RespondToInviteAsync_ValidInvite_CreatesRsvp()
        {
            var invite = new Invite { Id = 1, EventId = 5, InviteeId = "10" };
            var newRsvp = new RSVP { EventId = 5, UserId = "10" };
            var rsvpDto = new RsvpDTO { EventId = 5, UserId = "10" };

            _mockInviteRepo.Setup(repo => repo.GetInviteByIdAsync(1)).ReturnsAsync(invite);
            _mockRsvpRepo.Setup(repo => repo.GetRsvpsByUserIdAsync("10")).ReturnsAsync(new List<RSVP>());
            _mockMapper.Setup(m => m.Map<RsvpDTO>(It.IsAny<RSVP>())).Returns(rsvpDto);

            var result = await _service.RespondToInviteAsync(1, RSVPStatus.Accepted);

            Assert.Equal("10", result.UserId);
            Assert.Equal(5, result.EventId);
        }

        [Fact]
        public async Task UpdateStatusAsync_ValidId_UpdatesStatus()
        {
            var rsvp = new RSVP { Id = 1, Status = RSVPStatus.Pending };

            _mockRsvpRepo.Setup(repo => repo.GetRsvpByIdAsync(1)).ReturnsAsync(rsvp);

            var result = await _service.UpdateStatusAsync(1, RSVPStatus.Accepted);

            Assert.True(result);
            Assert.Equal(RSVPStatus.Accepted, rsvp.Status);
            _mockRsvpRepo.Verify(repo => repo.UpdateRsvpAsync(rsvp), Times.Once);
        }

        [Fact]
        public async Task CancelRsvpAsync_ValidId_DeclinesRsvp()
        {
            var rsvp = new RSVP { Id = 1, Status = RSVPStatus.Accepted };
            var rsvpDto = new RsvpDTO { Id = 1, Status = RSVPStatus.Declined };

            _mockRsvpRepo.Setup(repo => repo.GetRsvpByIdAsync(1)).ReturnsAsync(rsvp);
            _mockMapper.Setup(m => m.Map<RsvpDTO>(rsvp)).Returns(rsvpDto);

            var result = await _service.CancelRsvpAsync(1);

            Assert.Equal(RSVPStatus.Declined, result.Status);
        }
    }
}

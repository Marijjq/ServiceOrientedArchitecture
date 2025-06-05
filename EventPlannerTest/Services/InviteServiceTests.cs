using AutoMapper;
using EventPlanner.DTOs.Invite;
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
    public class InviteServiceTests
    {
        private readonly Mock<IInviteRepository> _mockInviteRepo;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<IEventRepository> _mockEventRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly InviteService _service;

        public InviteServiceTests()
        {
            _mockInviteRepo = new Mock<IInviteRepository>();
            _mockUserRepo = new Mock<IUserRepository>();
            _mockEventRepo = new Mock<IEventRepository>();
            _mockMapper = new Mock<IMapper>();
            _service = new InviteService(_mockInviteRepo.Object, _mockMapper.Object, _mockUserRepo.Object, _mockEventRepo.Object);
        }

        [Fact]
        public async Task GetAllInvitesAsync_ReturnsMappedDTOs()
        {
            var invites = new List<Invite> { new Invite { Id = 1 }, new Invite { Id = 2 } };
            var dtoList = new List<InviteDTO> { new InviteDTO { Id = 1 }, new InviteDTO { Id = 2 } };

            _mockInviteRepo.Setup(repo => repo.GetAllInvitesAsync()).ReturnsAsync(invites);
            _mockMapper.Setup(m => m.Map<IEnumerable<InviteDTO>>(invites)).Returns(dtoList);

            var result = await _service.GetAllInvitesAsync();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task SendInviteAsync_ValidRequest_ReturnsInviteDTO()
        {
            var createDto = new InviteCreateDTO { InviterId = "1", InviteeId = "2", EventId = 10 };
            var invite = new Invite { InviterId = "1", InviteeId = "2", EventId = 10 };
            var dto = new InviteDTO { InviterId = "1", InviteeId = "2", EventId = 10 };

            _mockUserRepo.Setup(r => r.GetUserByIdAsync("1")).ReturnsAsync(new ApplicationUser());
            _mockUserRepo.Setup(r => r.GetUserByIdAsync("2")).ReturnsAsync(new ApplicationUser());
            _mockEventRepo.Setup(r => r.GetEventByIdAsync(10)).ReturnsAsync(new Event());
            _mockInviteRepo.Setup(r => r.GetByInviteeAndEventAsync("2", 10)).ReturnsAsync((Invite?)null);
            _mockMapper.Setup(m => m.Map<Invite>(createDto)).Returns(invite);
            _mockMapper.Setup(m => m.Map<InviteDTO>(invite)).Returns(dto);

            var result = await _service.SendInviteAsync(createDto);

            Assert.Equal("1", result.InviterId);
            Assert.Equal("2", result.InviteeId);
        }

        [Fact]
        public async Task UpdateInviteAsync_ValidData_UpdatesAndReturnsDTO()
        {
            var existing = new Invite { Id = 5, Status = InviteStatus.Pending, RespondedAt = null };
            var updateDto = new InviteUpdateDTO { Status = InviteStatus.Accepted };
            var resultDto = new InviteDTO { Id = 5, Status = InviteStatus.Accepted };

            _mockInviteRepo.Setup(r => r.GetInviteByIdAsync(5)).ReturnsAsync(existing);
            _mockMapper.Setup(m => m.Map(updateDto, existing));
            _mockInviteRepo.Setup(r => r.UpdateInviteAsync(existing));
            _mockMapper.Setup(m => m.Map<InviteDTO>(existing)).Returns(resultDto);

            var result = await _service.UpdateInviteAsync(5, updateDto);

            Assert.Equal(5, result.Id);
            Assert.Equal(InviteStatus.Accepted, result.Status);
        }

        [Fact]
        public async Task DeleteInviteAsync_ValidPendingInvite_DeletesSuccessfully()
        {
            var invite = new Invite { Id = 3, Status = InviteStatus.Pending };

            _mockInviteRepo.Setup(r => r.GetInviteByIdAsync(3)).ReturnsAsync(invite);

            await _service.DeleteInviteAsync(3);

            _mockInviteRepo.Verify(r => r.DeleteInviteAsync(3), Times.Once);
        }

        [Fact]
        public async Task GetPendingInvitesByUserIdAsync_ReturnsMappedResults()
        {
            var invites = new List<Invite> { new Invite { Id = 1 }, new Invite { Id = 2 } };
            var dtos = new List<InviteDTO> { new InviteDTO { Id = 1 }, new InviteDTO { Id = 2 } };

            _mockInviteRepo.Setup(r => r.GetPendingInvitesByUserIdAsync("7")).ReturnsAsync(invites);
            _mockMapper.Setup(m => m.Map<IEnumerable<InviteDTO>>(invites)).Returns(dtos);

            var result = await _service.GetPendingInvitesByUserIdAsync("7");

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByInviteeAndEventAsync_ReturnsInviteDTO()
        {
            var invite = new Invite { Id = 1 };
            var dto = new InviteDTO { Id = 1 };

            _mockInviteRepo.Setup(r => r.GetByInviteeAndEventAsync("2", 10)).ReturnsAsync(invite);
            _mockMapper.Setup(m => m.Map<InviteDTO>(invite)).Returns(dto);

            var result = await _service.GetByInviteeAndEventAsync("2", 10);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }
    }
}

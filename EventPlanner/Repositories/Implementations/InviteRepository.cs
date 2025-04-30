using EventPlanner.Data;
using EventPlanner.Repositories.Interfaces;

namespace EventPlanner.Repositories.Implementations
{
    public class InviteRepository : IInviteRepository
    {
        private readonly ApplicationDbContext _context;
        public InviteRepository(ApplicationDbContext context)
        {
            _context = context;
        }
    }
}

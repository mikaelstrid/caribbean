using System.Data.Entity;
using System.Threading.Tasks;
using Caribbean.DataContexts.Application;
using Caribbean.Models.Database;

namespace Caribbean.DataAccessLayer.Database
{
    public interface IAgentRepository
    {
        Task<Agent> GetByUserId(string userId);
    }

    public class AgentRepository : IAgentRepository
    {
        private readonly ApplicationDbContext _db;

        public AgentRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Agent> GetByUserId(string userId)
        {
            return await _db.Agents.Include(a => a.Agency).Include(a => a.Prints).SingleOrDefaultAsync(a => a.UserId == userId);
        }
    }
}
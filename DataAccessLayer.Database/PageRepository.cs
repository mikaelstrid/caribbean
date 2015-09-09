using Caribbean.DataContexts.Application;
using Caribbean.Models.Database;

namespace Caribbean.DataAccessLayer.Database
{
    public interface IPageRepository : IGenericRepository<Page>
    {
    }

    public class PageRepository : GenericRepository<Page>, IPageRepository
    {
        public PageRepository(ApplicationDbContext db) : base(db) { }
    }
}
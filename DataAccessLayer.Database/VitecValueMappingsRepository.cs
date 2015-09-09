using Caribbean.DataContexts.Application;
using Caribbean.Models.Database;

namespace Caribbean.DataAccessLayer.Database
{
    public interface IPageTemplatePlaceholderMappingRepository : IGenericRepository<PageTemplatePlaceholderMapping>
    {
    }

    public class PageTemplatePlaceholderMappingRepository : GenericRepository<PageTemplatePlaceholderMapping>, IPageTemplatePlaceholderMappingRepository
    {
        public PageTemplatePlaceholderMappingRepository(ApplicationDbContext db) : base(db) { }
    }
}
using Caribbean.DataContexts.Application;
using Caribbean.Models.Database;

namespace Caribbean.DataAccessLayer.Database
{
    public interface IPrintRepository : IGenericRepository<Print>
    {
    }

    public class PrintRepository : GenericRepository<Print>, IPrintRepository
    {
        public PrintRepository(ApplicationDbContext db) : base(db) { }
    }
}
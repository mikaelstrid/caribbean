using Caribbean.DataContexts.Application;
using Caribbean.Models.Database;

namespace Caribbean.DataAccessLayer.Database
{
    public interface IFieldValueRepository : IGenericRepository<FieldValue>
    {
    }

    public class FieldValueRepository : GenericRepository<FieldValue>, IFieldValueRepository
    {
        public FieldValueRepository(ApplicationDbContext db) : base(db)
        {
        }
    }
}
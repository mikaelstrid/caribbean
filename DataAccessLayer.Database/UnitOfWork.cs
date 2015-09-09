using System;
using Caribbean.DataContexts.Application;

namespace Caribbean.DataAccessLayer.Database
{
    public interface IUnitOfWork : IDisposable
    {
        IAgentRepository AgentRepository { get; }
        IPageTemplatePlaceholderMappingRepository PageTemplatePlaceholderMappingRepository { get; }
        IPrintRepository PrintRepository { get; }
        IPageRepository PageRepository { get; }
        IFieldValueRepository FieldValueRepository { get; }
        void Save();
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();
        private IAgentRepository _agentRepository;
        private IPageTemplatePlaceholderMappingRepository _pageTemplatePlaceholderMappingRepository;
        private IPrintRepository _printRepository;
        private IPageRepository _pageRepository;
        private IFieldValueRepository _fieldValueRepository;

        public IAgentRepository AgentRepository => _agentRepository ?? (_agentRepository = new AgentRepository(_db));
        public IPageTemplatePlaceholderMappingRepository PageTemplatePlaceholderMappingRepository => _pageTemplatePlaceholderMappingRepository ?? (_pageTemplatePlaceholderMappingRepository = new PageTemplatePlaceholderMappingRepository(_db));
        public IPrintRepository PrintRepository => _printRepository ?? (_printRepository = new PrintRepository(_db));
        public IPageRepository PageRepository => _pageRepository ?? (_pageRepository = new PageRepository(_db));
        public IFieldValueRepository FieldValueRepository => _fieldValueRepository ?? (_fieldValueRepository = new FieldValueRepository(_db));

        public void Save()
        {
            _db.SaveChanges();
        }



        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _db.Dispose();
                }
            }
            _disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
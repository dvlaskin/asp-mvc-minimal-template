using WebApp.Domain.Interfaces;
using WebApp.Domain.Models;

namespace WebApp.Db;

public class EntityStorage : IDisposable, IEntityStorage
{
    private readonly AppDbContext _dbContext;

    private bool disposed = false;

    private IRepository<SampleModel> _sampleRepo;


    public IRepository<SampleModel> SampleRepo
    {
        get
        {
            _sampleRepo ??= new DbRepository<SampleModel>(_dbContext);
            return _sampleRepo;
        }
    }

    public EntityStorage(AppDbContext dbContext)
    {
        this._dbContext = dbContext;
    }


    public int Save()
    {
        return _dbContext.SaveChanges();
    }

    public async Task<int> SaveAsync()
    {
        return await _dbContext.SaveChangesAsync();
    }


    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposed)
        {
            if (disposing)
            {
                _dbContext.Dispose();
            }
        }
        this.disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
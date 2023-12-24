using WebApp.Domain.Interfaces;
using WebApp.Domain.Models;

namespace WebApp.Db;

public class Repositories : IDisposable, IRepositories
{
    private readonly AppDbContext _dbContext;

    private bool disposed = false;

    private IRepository<SampleModel> _sampleRepo;


    public IRepository<SampleModel> SampleRepo
    {
        get
        {
            if (_sampleRepo == null)
            {
                _sampleRepo = new DbRepository<SampleModel>(_dbContext);
            }

            return _sampleRepo;
        }
    }

    public Repositories(AppDbContext dbContext)
    {
        this._dbContext = dbContext;
    }


    public void Save()
    {
        _dbContext.SaveChanges();
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
using WebApp.Domain.Models;

namespace WebApp.Domain.Interfaces;

public interface IEntityStorage
{
    IRepository<SampleModel> SampleRepo { get; }

    int Save();
    Task<int> SaveAsync();
}
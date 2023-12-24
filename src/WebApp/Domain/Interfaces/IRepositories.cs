using WebApp.Domain.Models;

namespace WebApp.Domain.Interfaces;

public interface IRepositories
{
    IRepository<SampleModel> SampleRepo { get; }

    void Save();
}
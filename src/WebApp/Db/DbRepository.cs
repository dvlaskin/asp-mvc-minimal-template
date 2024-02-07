using Microsoft.EntityFrameworkCore;
using WebApp.Domain.Interfaces;

namespace WebApp.Db;

public class DbRepository<T> : IRepository<T> where T : class
{
    internal AppDbContext dbContext;
    internal DbSet<T> dbSet;

    public DbRepository(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
        this.dbSet = this.dbContext.Set<T>();
    }

    public IQueryable<T> GetAllItems()
    {
        return dbSet.AsQueryable();
    }

    public async Task<T> GetItemAsync<K>(K id)
    {
        return await dbSet.FindAsync(id);
    }

    public async Task<T> AddItemAsync(T item)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        var result = await dbSet.AddAsync(item);
        return result.Entity;
    }

    public void UpdateItem(T item)
    {
        if (item is null)
            return;

        dbSet.Attach(item);
        dbContext.Entry(item).State = EntityState.Modified;
    }

    public void DeleteItem<K>(K id)
    {
        var dbItem = dbSet.Find(id);
        if (dbItem != null)
        {
            DeleteItem(dbItem);
        }
    }

    public void DeleteItem(T item)
    {
        if (dbContext.Entry(item).State == EntityState.Detached)
        {
            dbContext.Attach(item);
        }

        dbSet.Remove(item);
    }
}
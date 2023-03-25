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

    public T GetItem<K>(K id)
    {
        return dbSet.Find(id);
    }

    public void AddItem(T item)
    {
        if (item != null)
            dbSet.Add(item);
    }

    public void UpdateItem(T item)
    {
        if (item != null)
        {
            dbSet.Attach(item);
            dbContext.Entry(item).State = EntityState.Modified;
        }
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
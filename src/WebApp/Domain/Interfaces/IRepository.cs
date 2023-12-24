namespace WebApp.Domain.Interfaces;

public interface IRepository<T> where T : class
{
    IQueryable<T> GetAllItems();
    T GetItem<K>(K id);
    void AddItem(T item);
    void UpdateItem(T item);
    void DeleteItem<K>(K id);
    void DeleteItem(T item);
}
namespace WebApp.Domain.Interfaces;

public interface IRepository<T> where T : class
{
    IQueryable<T> GetAllItems();
    Task<T> GetItemAsync<K>(K id);
    Task<T> AddItemAsync(T item);
    void UpdateItem(T item);
    void DeleteItem<K>(K id);
    void DeleteItem(T item);
}
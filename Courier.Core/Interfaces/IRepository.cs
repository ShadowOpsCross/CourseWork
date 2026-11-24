namespace Courier.Core.Interfaces
{
    /// <summary>Елементарні операції зі сховищем об'єктів у пам'яті.</summary>
    public interface IRepository<T> where T : class
    {
        int Count { get; }
        void Add(T item);
        bool Remove(T item);
        List<T> GetAll();
        List<T> Find(Predicate<T> match);
    }
}

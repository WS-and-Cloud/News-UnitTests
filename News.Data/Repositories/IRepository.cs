namespace News.Data.Repositories
{
    using System.Linq;

    public interface IRepository<T> where T : class
    {
        IQueryable<T> All();

        void Add(T entity);

        void Delete(T entity);

        T Delete(object id);

        T Find(object id);

        void Update(T entity);

        void Detach(T entity);

        int SaveChanges();
    }
}
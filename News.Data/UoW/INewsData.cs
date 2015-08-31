namespace News.Data.UoW
{
    using News.Data.Repositories;
    using News.Models;

    public interface INewsData
    {
        IRepository<News> News { get; }

        IRepository<ApplicationUser> Users { get; }

        int SaveChanges();
    }
}
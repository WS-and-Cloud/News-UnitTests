namespace News.TestsWithMocking
{
    using News.Data;
    using News.Data.Repositories;
    using News.Data.UoW;
    using News.Models;

    public class NewsDataMock : INewsData
    {
        private readonly NewsRepositoryMock newsMock = new NewsRepositoryMock();

        public IRepository<News> News
        {
            get
            {
                return this.newsMock;
            }
        }

        public bool IsChangeSaved { get; set; }

        public IRepository<ApplicationUser> Users { get; private set; }

        public int SaveChanges()
        {
            this.IsChangeSaved = true;
            return 1;
        }
    }
}
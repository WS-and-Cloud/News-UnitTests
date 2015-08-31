namespace News.TestsWithMocking
{
    using System.Collections.Generic;
    using System.Linq;

    using News.Data.Repositories;
    using News.Models;

    public class NewsRepositoryMock : IRepository<News>
    {
        public NewsRepositoryMock()
        {
            this.Entities = new List<News>();
        }

        public bool IsSaved { get; set; }

        public IList<News> Entities { get; set; }

        public IQueryable<News> All()
        {
            return this.Entities.AsQueryable();
        }

        public void Add(News entity)
        {
            this.Entities.Add(entity);
        }

        public void Delete(News entity)
        {
            this.Entities.Remove(entity);
        }

        public News Delete(object id)
        {
            var entity = this.Find(id);
            this.Delete(entity);
            return entity;
        }

        public News Find(object id)
        {
            var entity = this.Entities.FirstOrDefault(e => e.Id == (int)id);
            return entity;
        }

        public void Update(News entity)
        {
            var newNews = new News()
                          {
                              Id = this.Entities.IndexOf(entity),
                              Content = entity.Content,
                              Title = entity.Title,
                              PublishedAt = entity.PublishedAt
                          };
            this.Delete(entity);
            this.Add(newNews);
        }

        public void Detach(News entity)
        {
            throw new System.NotImplementedException();
        }

        public int SaveChanges()
        {
            this.IsSaved = true;
            return 1;
        }
    }
}
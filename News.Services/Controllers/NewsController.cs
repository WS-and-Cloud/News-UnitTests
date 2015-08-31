namespace News.Services.Controllers
{
    using System;
    using System.Linq;
    using System.Web.Http;

    using News.Data.UoW;
    using News.Models;
    using News.Services.Models.NewsModels;

    public class NewsController : BaseApiController
    {
        public NewsController()
        {
        }
        
        public NewsController(INewsData data)
            : base(data)
        {
        }

        // GET api/news
        [HttpGet]
        public IHttpActionResult GetAllNews()
        {
            var allnews =
                this.Data.News.All()
                    .OrderByDescending(n => n.PublishedAt)
                    .Select(
                        n =>
                        new GetAllNewsViewModel()
                        {
                            Id = n.Id,
                            Title = n.Title,
                            Content = n.Content,
                            PublishedDate = n.PublishedAt
                        });

            return this.Ok(allnews);
        }

        // POST api/news
        [HttpPost]
        public IHttpActionResult CreateNew(CreateNewInputModel inputModel)
        {
            if (inputModel == null)
            {
                return this.BadRequest();
            }

            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var news = new News { Content = inputModel.Content, Title = inputModel.Title, PublishedAt = DateTime.Now };

            this.Data.News.Add(news);
            this.Data.SaveChanges();
            var newOutputModel = new GetAllNewsViewModel() { Id = news.Id, Content = news.Content, Title = news.Title, PublishedDate = news.PublishedAt};
            return this.CreatedAtRoute("DefaultApi", new { controller = "news", id = newOutputModel.Id }, newOutputModel);
        }

        // PUT api/news/5
        [HttpPut]
        public IHttpActionResult UpdateNewById(int id, UpdateNewInputModel inputModel)
        {
            if (inputModel == null)
            {
                return this.BadRequest();
            }

            var news = this.Data.News.All().FirstOrDefault(n => n.Id == id);
            if (news == null)
            {
                return this.NotFound();
            }

            if (inputModel.Title != null)
            {
                news.Title = inputModel.Title;
            }

            if (inputModel.Content != null)
            {
                news.Content = inputModel.Content;
            }

            if (inputModel.PublishedAt != null)
            {
                news.PublishedAt = Convert.ToDateTime(inputModel.PublishedAt);
            }

            this.Data.News.Update(news);

            this.Data.SaveChanges();

            var newOutputModel = new GetAllNewsViewModel() { Id = news.Id, Content = news.Content, Title = news.Title, PublishedDate = news.PublishedAt };
            return this.Ok(newOutputModel);
        }

        // DELETE api/news/5
        [HttpDelete]
        public IHttpActionResult DeleteNewById(int id)
        {
            var news = this.Data.News.All().FirstOrDefault(n => n.Id == id);
            if (news == null)
            {
                return this.NotFound();
            }

            this.Data.News.Delete(news);
            this.Data.SaveChanges();
            return this.Ok(new
                           {
                               news.Id
                           });
        }
    }
}

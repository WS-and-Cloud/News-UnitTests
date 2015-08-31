namespace News.Services.Controllers
{
    using System.Web.Http;

    using News.Data;
    using News.Data.UoW;

    public class BaseApiController : ApiController
    {
        public BaseApiController()
            : this (new NewsData(new NewsDbContext()))
        {
        }

        public BaseApiController(INewsData data)
        {
            this.Data = data;
        }

        public INewsData Data { get; protected set; }
    }
}

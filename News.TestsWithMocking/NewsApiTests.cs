namespace News.TestsWithMocking
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Web.Http;
    using System.Web.Http.Routing;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using News.Data;
    using News.Data.UoW;
    using News.Models;
    using News.Services.Controllers;
    using News.Services.Models.NewsModels;

    [TestClass]
    public class NewsApiTests
    {
        private NewsDataMock dataLayerMocked;
        [TestInitialize]
        public void Init()
        {
            this.dataLayerMocked = new NewsDataMock();
        }

        [TestMethod]
        public void GetAll_WhenValid_ShouldReturnBugsCollection()
        {
            // Arrange
            // var repo = new NewsRepositoryMock();
            var newsData = this.dataLayerMocked.News;
            var news = AddThreeNews();

            newsData.Add(new News()
                {
                    Title = "Test news #1",
                    Content = "Test content #1",
                    PublishedAt = new DateTime(2014,1,1)
                });
            newsData.Add(new News()
            {
                Title = "Test news #2",
                Content = "Test content #2",
                PublishedAt = new DateTime(2014, 1, 1)
            });
            newsData.Add(new News()
            {
                Title = "Test news #3",
                Content = "Test content #3",
                PublishedAt = new DateTime(2014, 1, 1)
            });

            var controller = new NewsController(dataLayerMocked);
            this.SetupController(controller, "news");

            // Act
            var httpResponse = controller.GetAllNews().ExecuteAsync(new CancellationToken()).Result;
            var result = httpResponse.Content.ReadAsAsync<List<News>>().Result;

            // Assert
            Assert.AreEqual(news.Count, result.Count);
            Assert.AreEqual(news[0].Title, result[0].Title);
            Assert.AreEqual(news[1].Title, result[1].Title);
            Assert.AreEqual(news[2].Content, result[2].Content);
        }

        [TestMethod]
        public void CreateNews_WithCorrectData_ShouldCreateNewsAndReturn201Created()
        {
            // Arrange
            var news = new CreateNewInputModel()
                       {
                           Title = "Test news #1",
                           Content = "Test content #1",
                       };
            this.dataLayerMocked.IsChangeSaved = false;

            var controller = new NewsController(this.dataLayerMocked);
            this.SetupController(controller, "news");    

            // Act
            var httpResponse = controller.CreateNew(news).ExecuteAsync(new CancellationToken()).Result;
            var newsFromService = httpResponse.Content.ReadAsAsync<News>().Result;
            
            // Arrange
            Assert.IsTrue(this.dataLayerMocked.IsChangeSaved);
            Assert.AreEqual(HttpStatusCode.Created, httpResponse.StatusCode);
            Assert.AreEqual(news.Title, newsFromService.Title);
            Assert.AreEqual(news.Content, newsFromService.Content);
        }

        [TestMethod]
        public void CreateNews_WithIncorrectData_ShouldReturnBadRequestWithOutAddingTheNews()
        {
            // Arrange
            var controller = new NewsController(this.dataLayerMocked);
            this.SetupController(controller, "news");

            // Act
            var httpResponse = controller.CreateNew(null).ExecuteAsync(new CancellationToken()).Result;
            var httpGetResponse = controller.GetAllNews().ExecuteAsync(new CancellationToken()).Result;
            var newsFromService = httpGetResponse.Content.ReadAsAsync<News[]>().Result;

            // Arrange
            Assert.AreEqual(HttpStatusCode.BadRequest, httpResponse.StatusCode);
            Assert.AreEqual(0, newsFromService.Count());
        }

        [TestMethod]
        public void UpdateNews_WithCorrectData_ShouldUpdateTheNewsAndReturn200Ok()
        {
            // Arrange
            var initialNew = new News() { Id = 1, Title = "Test news #1", Content = "Test content #1", };

            var controller = new NewsController(this.dataLayerMocked);
            this.SetupController(controller, "news");

            // Act
            this.dataLayerMocked.News.Add(initialNew);
            
            var httpGetResponse = controller.GetAllNews().ExecuteAsync(new CancellationToken()).Result;
            var newsFromService = httpGetResponse.Content.ReadAsAsync<List<News>>().Result;
            var updatedNew = new UpdateNewInputModel() { Title = "Updated", Content = "Tralala"};
            var newsId = newsFromService[0].Id;
            var httpPutResponse =
                controller.UpdateNewById(newsId, updatedNew).ExecuteAsync(new CancellationToken()).Result;
            
            var result = httpPutResponse.Content.ReadAsAsync<News>().Result;
            
            // Assert
            Assert.AreEqual(updatedNew.Title, result.Title);
            Assert.AreEqual(updatedNew.Content, result.Content);
        }

        [TestMethod]
        public void UpdateNews_WithIncorrectData_ShouldReturnBadRequestWithOutUpdatingTheNews()
        {
            // Arrange
            var initialNew = new News() { Id = 1, Title = "Test news #1", Content = "Test content #1", };

            var controller = new NewsController(this.dataLayerMocked);
            this.SetupController(controller, "news");

            // Act
            this.dataLayerMocked.News.Add(initialNew);

            var httpGetResponse = controller.GetAllNews().ExecuteAsync(new CancellationToken()).Result;
            var newsFromService = httpGetResponse.Content.ReadAsAsync<List<News>>().Result;
            
            var newsId = newsFromService[0].Id;
            var httpPutResponse =
                controller.UpdateNewById(newsId, null).ExecuteAsync(new CancellationToken()).Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, httpPutResponse.StatusCode);
            Assert.AreEqual(initialNew.Title, newsFromService[0].Title);
            Assert.AreEqual(initialNew.Content, newsFromService[0].Content);
        }


        [TestMethod]
        public void Delete_WithExistingNews_ShouldDeleteTheNewsAndReturn200Ok()
        {
            // Arrange
            var initialNew = new News() { Id = 1, Title = "Test news #1", Content = "Test content #1", };

            var controller = new NewsController(this.dataLayerMocked);
            this.SetupController(controller, "news");

            // Act
            this.dataLayerMocked.News.Add(initialNew);

            var httpGetResponse = controller.GetAllNews().ExecuteAsync(new CancellationToken()).Result;
            var newsFromService = httpGetResponse.Content.ReadAsAsync<List<News>>().Result;

            var newsId = newsFromService[0].Id;
            var httpDeleteResponse = controller.DeleteNewById(newsId).ExecuteAsync(new CancellationToken()).Result;

            Assert.AreEqual(0, this.dataLayerMocked.News.All().Count());
            Assert.AreEqual(HttpStatusCode.OK, httpDeleteResponse.StatusCode);
        }

        [TestMethod]
        public void Delete_WithNonExistingNews_ShouldDeleteTheNewsAndReturn404NotFound()
        {
            // Arrange
            var initialNew = new News() { Id = 1, Title = "Test news #1", Content = "Test content #1", };

            var controller = new NewsController(this.dataLayerMocked);
            this.SetupController(controller, "news");

            // Act
            this.dataLayerMocked.News.Add(initialNew);

            var httpGetResponse = controller.GetAllNews().ExecuteAsync(new CancellationToken()).Result;
            var newsFromService = httpGetResponse.Content.ReadAsAsync<List<News>>().Result;

            var newsId = 3; // Non existing news
            var httpDeleteResponse = controller.DeleteNewById(newsId).ExecuteAsync(new CancellationToken()).Result;

            Assert.AreEqual(1, this.dataLayerMocked.News.All().Count());
            Assert.AreEqual(HttpStatusCode.NotFound, httpDeleteResponse.StatusCode);
        }

        private static List<News> AddThreeNews()
        {
            var news = new List<News>()
                       {
                           new News()
                           {
                               Title = "Test news #1",
                               Content = "Test content #1",
                               PublishedAt = new DateTime(2014, 1, 1)
                           },
                           new News()
                           {
                               Title = "Test news #2",
                               Content = "Test content #2",
                               PublishedAt = new DateTime(2014, 1, 1)
                           },
                           new News()
                           {
                               Title = "Test news #3",
                               Content = "Test content #3",
                               PublishedAt = new DateTime(2014, 1, 1)
                           }
                       };
            return news;
        }

        private void SetupController(ApiController controller, string controllerName)
        {
            string serverUrl = "http://sample-url.com";

            // Setup the Request object of the controller
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(serverUrl)
            };
            controller.Request = request;

            // Setup the configuration of the controller
            var config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional });
            controller.Configuration = config;

            // Apply the routes to the controller
            controller.RequestContext.RouteData = new HttpRouteData(
                route: new HttpRoute(),
                values: new HttpRouteValueDictionary
                {
                    { "controller", controllerName }
                });
        }
    }
}

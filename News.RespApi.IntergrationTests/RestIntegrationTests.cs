namespace News.RespApi.IntergrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Runtime.CompilerServices;
    using System.Web.Http;
    using System.Web.Http.Routing;

    using EntityFramework.Extensions;

    using Microsoft.Owin.Testing;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using News.Data;
    using News.Models;
    using News.Services;
    using News.Services.Models.NewsModels;

    using Owin;

    [TestClass]
    public class NewsControllerIntegrationTests
    {
        private TestServer httpTestServer;

        private NewsDbContext dbcontext;

        private HttpClient httpClient;

        [TestInitialize]
        public void TestInit()
        {
            this.dbcontext = new NewsDbContext();
            
            // Start OWIN testing HTTP server with Web API support
            this.httpTestServer = TestServer.Create(appBuilder =>
            {
                var config = new HttpConfiguration();
                WebApiConfig.Register(config);
                appBuilder.UseWebApi(config);
            });
            this.httpClient = this.httpTestServer.HttpClient;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.httpTestServer.Dispose();
            this.dbcontext.Dispose();
        }

        [TestMethod]
        public void GetAllNews_NonEmptyDb_ShouldReturnAllNewsAnd200Ok()
        {
            // Arrange
            this.CleanDatabase();
            var newNews = new News() { Content = "Content #1", Title = "Title #1" };
            var newNews2 = new News() { Content = "Content #1", Title = "Title #1" };

            // Act
            this.dbcontext.News.Add(newNews);
            this.dbcontext.News.Add(newNews2);
            this.dbcontext.SaveChanges();
            var httpResponse = this.httpClient.GetAsync("/api/news").Result;
            var news = httpResponse.Content.ReadAsAsync<List<News>>().Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, httpResponse.StatusCode);
            Assert.AreEqual(2, news.Count);
            Assert.AreEqual(newNews.Title, news[0].Title);
            Assert.AreEqual(newNews.Content, news[0].Content);
        }

        [TestMethod]
        public void CreateNewNews_WithCorrectData_ShouldReturnTheNewsAnd201Created()
        {
            // Arrange
            this.CleanDatabase();

            var postContent = new FormUrlEncodedContent(new[] 
            {
                new KeyValuePair<string, string>("title", "Title #1"),
                new KeyValuePair<string, string>("content", "Content #1"),
            });

            // Act
            var httpPostResponse = this.httpClient.PostAsync("/api/news", postContent).Result;
            var newsFromService = httpPostResponse.Content.ReadAsAsync<News>().Result;
            
            var httpGetResponse = this.httpClient.GetAsync("/api/news").Result;
            var news = httpGetResponse.Content.ReadAsAsync<List<News>>().Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, httpPostResponse.StatusCode);
            Assert.AreEqual(1, news.Count);
            Assert.AreEqual("Title #1", newsFromService.Title);
            Assert.AreEqual("Content #1", newsFromService.Content);
        }

        [TestMethod]
        public void CreateNewNews_WithIncorrectData_ShouldReturn400BadRequestAndNotAddTheEntity()
        {
            // Arrange
            this.CleanDatabase();

            var postContent = new FormUrlEncodedContent(new[] 
            {
                new KeyValuePair<string, string>("title", null),
                new KeyValuePair<string, string>("content", null),
            });

            // Act
            var httpPostResponse = this.httpClient.PostAsync("/api/news", postContent).Result;
            var newsFromService = httpPostResponse.Content.ReadAsAsync<News>().Result;

            var httpGetResponse = this.httpClient.GetAsync("/api/news").Result;
            var news = httpGetResponse.Content.ReadAsAsync<List<News>>().Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, httpPostResponse.StatusCode);
            Assert.AreEqual(0, news.Count);
        }

        [TestMethod]
        public void UpdatExistingNews_WithCorrectData_ShouldUpdateTheNewsAndReturn200Ok()
        {
            // Arrange
            this.CleanDatabase();
            var newNews = new News() { Content = "Content #1", Title = "Title #1" };
            string title = "Title #1 Updated";
            var putContent = new FormUrlEncodedContent(new[] 
            {
                new KeyValuePair<string, string>("title", title),
                new KeyValuePair<string, string>("content", "Content #1"),
            });

            // Act
            this.dbcontext.News.Add(newNews);
            this.dbcontext.SaveChanges();

            var httpGetResponse = this.httpClient.GetAsync("api/news").Result;
            var newsFromServices = httpGetResponse.Content.ReadAsAsync<List<News>>().Result;
            var newId = newsFromServices[0].Id;

            var httpPutResponse = this.httpClient.PutAsync("api/news/" + newId, putContent).Result;
            var updatedNewResult = httpPutResponse.Content.ReadAsAsync<News>().Result;

            Assert.AreEqual(HttpStatusCode.OK, httpPutResponse.StatusCode);
            Assert.AreNotEqual(newNews.Title, updatedNewResult.Title);
            Assert.AreEqual(title, updatedNewResult.Title);
        }

        [TestMethod]
        public void UpdateExistingNews_WithIncorrectData_ShouldNotUpdateNewsAndReturns400BadRequest()
        {
            // Arrange
            this.CleanDatabase();
            var newNews = new News() { Content = "Content #1", Title = "Title #1" };
           
            // Act
            this.dbcontext.News.Add(newNews);
            this.dbcontext.SaveChanges();
            var newsInDb = this.dbcontext.News.FirstOrDefault(n => n.Title == newNews.Title);

            var httpPutResponse = this.httpClient.PutAsync("api/news/" + newsInDb.Id, null).Result;

            var httpGetResponse = this.httpClient.GetAsync("api/news").Result;
            var newsFromServices = httpGetResponse.Content.ReadAsAsync<List<News>>().Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, httpPutResponse.StatusCode);
            Assert.AreEqual(newNews.Title, newsFromServices[0].Title);
            Assert.AreEqual(newNews.Content, newsFromServices[0].Content);
        }


        [TestMethod]
        public void Delete_ExistingNews_ShouldDeleteNewsAndReturn200Ok()
        {
            // Arrange
            this.CleanDatabase();
            var newNews = new News() { Content = "Content #1", Title = "Title #1" };

            // Act
            this.dbcontext.News.Add(newNews);
            this.dbcontext.SaveChanges();
            var newsInDb = this.dbcontext.News.FirstOrDefault(n => n.Title == newNews.Title);
            var httpDeleteResponse = this.httpClient.DeleteAsync("api/news/" + newsInDb.Id).Result;
            var httpGetResponse = this.httpClient.GetAsync("api/news").Result;
            var httpGetResult = httpGetResponse.Content.ReadAsAsync<List<News>>().Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, httpDeleteResponse.StatusCode);
            Assert.AreEqual(0, httpGetResult.Count);
        }

        [TestMethod]
        public void Delete_NonExistingNews_ShouldReturn404NotFound()
        {
            // Arrange
            this.CleanDatabase();
            
            // Act
            var nonExistingId = 100;
            var httpDeleteResponse = this.httpClient.DeleteAsync("api/news/" + nonExistingId).Result;
            var httpGetResponse = this.httpClient.GetAsync("api/news").Result;
            var httpGetResult = httpGetResponse.Content.ReadAsAsync<List<News>>().Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, httpDeleteResponse.StatusCode);
            Assert.AreEqual(0, httpGetResult.Count);
        }



        private void CleanDatabase()
        {
            this.dbcontext.News.Delete();

            this.dbcontext.SaveChanges();
        }
    }
}

namespace News.RepositoriesTests
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Validation;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Transactions;

    using EntityFramework.Extensions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using News.Data;
    using News.Data.Repositories;
    using News.Models;

    [TestClass]
    public class NewsRepositoryCrud
    {
        private NewsDbContext dbcontext;

        private TransactionScope transactionScope;

        [TestInitialize]
        public void Init()
        {
            this.dbcontext = new NewsDbContext();
            this.dbcontext.Database.CreateIfNotExists();

            this.transactionScope = new TransactionScope();
        }

        [TestCleanup]
        public void CleanUp()
        {
            this.dbcontext.Dispose();
            this.transactionScope.Dispose();
        }

        [TestMethod]
        public void GetAllNews_ShouldReturnAllNews()
        {
            // Arange
            this.CleanUpDatabase();
            var listWithNews = new List<News>();
            var repo = new EFRepository<News>(this.dbcontext);
            var new1 = new News() { Content = "Content 1", Title = "Title 1", PublishedAt = DateTime.Now };
            var new2 = new News() { Content = "Content 2", Title = "Title 2", PublishedAt = DateTime.Now };
            
            // Act
            repo.Add(new1);
            repo.Add(new2);
            repo.SaveChanges();

            listWithNews.Add(new1);
            listWithNews.Add(new2);

            // Assert 
            Assert.AreEqual(2, repo.All().Count());
            CollectionAssert.AreEquivalent(listWithNews, repo.All().ToList());
        }

        [TestMethod]
        public void CreateNews_WithCorrectData_ShouldCreateNewsSuccessfully()
        {
            // Arrange
            this.CleanUpDatabase();
            var listWithNews = new List<News>();
            var repo = new EFRepository<News>(this.dbcontext);
            var new1 = new News() { Content = "Content 1", Title = "Title 1", PublishedAt = DateTime.Now };

            // Act
            repo.Add(new1);
            repo.SaveChanges();
            var newsInDb = repo.All().ToArray();
            
            // Assert
            Assert.AreEqual(1, newsInDb.Count());
            Assert.AreEqual(new1.Title, newsInDb[0].Title);
            Assert.AreEqual(new1.Content, newsInDb[0].Content);
            Assert.AreEqual(new1.PublishedAt.ToString(), newsInDb[0].PublishedAt.ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(DbEntityValidationException))]
        public void CreateNews_WithIncorrectData_ShouldThrow()
        {
            // Arrange
            this.CleanUpDatabase();
            var listWithNews = new List<News>();
            var repo = new EFRepository<News>(this.dbcontext);
            var new1 = new News() { Content = null, Title = null, PublishedAt = DateTime.Now };

            // Act
            repo.Add(new1);
            repo.SaveChanges();
        }

        [TestMethod]
        public void UpdateNews_WithCorrectData_ShouldUpdateTheNewsSuccessfully()
        {
            // Arrange
            this.CleanUpDatabase();
            var listWithNews = new List<News>();
            var repo = new EFRepository<News>(this.dbcontext);
            var new1 = new News() { Content = "Content of new", Title = "Title 1", PublishedAt = DateTime.Now };

            // Act
            repo.Add(new1);
            repo.SaveChanges();
            var newsInDb = repo.All().ToArray();
            var newTitle = "ModifiedTitle";
            newsInDb[0].Title = newTitle;
            repo.Update(newsInDb[0]);
            repo.SaveChanges();
            var latestNewsInDb = repo.All().ToArray();

            // Assert
            Assert.AreEqual(1, newsInDb.Count());
            Assert.AreEqual(newTitle, latestNewsInDb[0].Title);
            Assert.AreEqual(new1.Content, latestNewsInDb[0].Content);
        }

        [TestMethod]
        [ExpectedException(typeof(DbEntityValidationException))]
        public void UpdateNews_WithIncorrectData_ShouldThrow()
        {
            // Arrange
            this.CleanUpDatabase();
            var listWithNews = new List<News>();
            var repo = new EFRepository<News>(this.dbcontext);
            var new1 = new News() { Content = "Content of new", Title = "Title 1", PublishedAt = DateTime.Now };

            // Act
            repo.Add(new1);
            repo.SaveChanges();
            var newsInDb = repo.All().ToArray();
            string newTitle = null;
            newsInDb[0].Title = newTitle;
            repo.Update(newsInDb[0]);
            repo.SaveChanges();
            var latestNewsInDb = repo.All().ToArray();

            // Assert
            Assert.AreEqual(1, newsInDb.Count());
            Assert.AreEqual(new1.Title, latestNewsInDb[0].Title);
            Assert.AreEqual(new1.Content, latestNewsInDb[0].Content);
        }

        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void Update_NonExistingItem_ShouldThrow()
        {
            // Arrange
            this.CleanUpDatabase();
            var listWithNews = new List<News>();
            var repo = new EFRepository<News>(this.dbcontext);

            // Act
            var newsInDb = repo.All().ToArray();
            newsInDb[0].Title = "New Title";    // Non existing item at index 0.
        }

        [TestMethod]
        public void Delete_ExistingNews_ShouldDeleteTheNew()
        {
            // Arrange
            this.CleanUpDatabase();
            var listWithNews = new List<News>();
            var repo = new EFRepository<News>(this.dbcontext);
            var new1 = new News() { Content = "Content of new", Title = "Title 1", PublishedAt = DateTime.Now };

            // Act
            repo.Add(new1);
            repo.SaveChanges();
            var newsInDb = repo.All().ToArray();
            repo.Delete(newsInDb[0]);
            repo.SaveChanges();

            // Assert
            Assert.AreEqual(0, repo.All().ToList().Count());
        }

        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void Delete_NonExistingNews_ShouldThrow()
        {
            // Arrange
            this.CleanUpDatabase();
            var listWithNews = new List<News>();
            var repo = new EFRepository<News>(this.dbcontext);

            // Act
            var newsInDb = repo.All().ToArray();
            repo.Delete(newsInDb[0]); // Non existing item at index 0.
        }

        private void CleanUpDatabase()
        {
            this.dbcontext.News.Delete();
        }
    }
}

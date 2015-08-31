namespace News.Data
{
    using System.Data.Entity;

    using Microsoft.AspNet.Identity.EntityFramework;

    using News.Data.Migrations;
    using News.Models;

    public class NewsDbContext : IdentityDbContext<ApplicationUser>
    {
        public NewsDbContext()
            : base("NewsDbConnection", throwIfV1Schema: false)
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<NewsDbContext, Configuration>());
        }

        public virtual IDbSet<News> News { get; set; } 

        public static NewsDbContext Create()
        {
            return new NewsDbContext();
        }
    }
}

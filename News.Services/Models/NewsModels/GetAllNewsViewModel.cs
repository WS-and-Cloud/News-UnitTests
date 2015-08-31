namespace News.Services.Models.NewsModels
{
    using System;

    public class GetAllNewsViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public DateTime? PublishedDate { get; set; }
    }
}
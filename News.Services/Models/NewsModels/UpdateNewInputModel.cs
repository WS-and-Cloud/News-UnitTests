namespace News.Services.Models.NewsModels
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class UpdateNewInputModel
    {
        [MinLength(2)]
        public string Title { get; set; }

        [MinLength(2)]
        public string Content { get; set; }

        public DateTime? PublishedAt { get; set; }
    }
}
namespace News.Services.Models.NewsModels
{
    using System.ComponentModel.DataAnnotations;

    public class CreateNewInputModel
    {
        [Required]
        [MinLength(1)]
        public string Title { get; set; }
    
        [Required]
        [MinLength(1)]
        public string Content { get; set; }
    }
}
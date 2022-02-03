using System.ComponentModel.DataAnnotations;

namespace Skocko.Api.Models.Requests
{
    public class GuessRequest
    {
        [Required]
        [MinLength(5)]
        [MaxLength(5)]
        public string? Word { get; set; }
    }
}
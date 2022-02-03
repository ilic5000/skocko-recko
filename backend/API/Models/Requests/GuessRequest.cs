using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
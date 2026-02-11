using System.ComponentModel.DataAnnotations;

namespace DataGen_v1.Models
{
    public class ApiConfig
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string ProfileName { get; set; } = string.Empty;

        [Required]
        public string TargetUrl { get; set; } = string.Empty;

        public string Method { get; set; } = "POST";

        public string? HeaderName { get; set; }

        public string? HeaderValue { get; set; }
    }
}
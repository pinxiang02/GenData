using DataGen_v1.Models;
using System.ComponentModel.DataAnnotations;

namespace DataGen_v1.Models
{
    public class Generator
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public int IntervalMs { get; set; } = 1000;
        public bool IsApiEnabled { get; set; } = false;
        public bool IsDbEnabled { get; set; } = false;

        public int? ApiConfigId { get; set; }
        public ApiConfig? ApiConfig { get; set; }

        public int? DbConnectionConfigId { get; set; }
        public DbConnectionConfig? DbConnectionConfig { get; set; }
        public string? TableName { get; set; }
        
    }
}

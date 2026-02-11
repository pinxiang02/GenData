using System.ComponentModel.DataAnnotations;

namespace DataGen_v1.Models
{
    public class DbConnectionConfig
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string ConnectionString { get; set; } = string.Empty;

        // Simple selector: "SqlServer", "Postgres", etc.
        public string ProviderType { get; set; } = "SqlServer";
    }
}
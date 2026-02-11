using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataGen_v1.Models
{
    public enum DataTypeEnum
    {
        String,
        Integer,
        Double,
        Boolean,
        Object
    }

    public enum GenerationMode
    {
        FixedValue, 
        RandomPick, 
        Range,
        SmartData
    }

    public class NodeConfig
    {
        [Key]
        public int Id { get; set; }

        public int GeneratorId { get; set; }

        [Required]
        public string NodeName { get; set; } = string.Empty;

        [Required]
        public DataTypeEnum DataType { get; set; }

        [Required]
        public GenerationMode Mode { get; set; }

        public string? FixedValue { get; set; }

        public string? ValueList { get; set; }

        public double? MinRange { get; set; }
        public double? MaxRange { get; set; }

        public int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public NodeConfig? Parent { get; set; }

        public ICollection<NodeConfig> Children { get; set; } = new List<NodeConfig>();
        public Generator? Generator { get; set; }
    }
}
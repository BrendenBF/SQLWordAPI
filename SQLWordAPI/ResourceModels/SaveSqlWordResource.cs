using System.ComponentModel.DataAnnotations;

namespace SQLWordAPI.ResourceModels
{
    public class SaveSqlWordResource
    {
        [Required]
        [MaxLength(50)]
        public string SqlWord { get; set; } = string.Empty;
    }
}

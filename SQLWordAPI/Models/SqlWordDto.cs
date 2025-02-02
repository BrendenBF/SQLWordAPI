namespace SQLWordAPI.Models
{
    public class SqlWordDto
    {
        public string? Id { get; set; }
        public string SqlWord { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public bool IsActive { get; set; }
    }
}

namespace SQLWordAPI.ResourceModels
{
    public class SqlWordResource
    {
        public string? Id { get; set; }
        public string SqlWord { get; set; } = string.Empty;
        public string DateCreated { get; set; } = string.Empty;
        public string DateUpdated { get; set; } = string.Empty;
    }
}

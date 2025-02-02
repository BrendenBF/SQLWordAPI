namespace SQLWordAPI.ResourceModels
{
    public record ErrorResource
    {
        public static bool Success => false;
        public List<string> Messages { get; private set; }

        public ErrorResource(List<string> messages)
        {
            Messages = messages ?? [];
        }

        public ErrorResource(string message)
        {
            Messages = [];

            if (!string.IsNullOrWhiteSpace(message))
            {
                Messages.Add(message);
            }
        }
    }
}

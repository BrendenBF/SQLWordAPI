namespace SQLWordAPI.ResourceModels
{
    public record ResourceResponse<T>
    {
        public bool Success { get; init; }
        public string? Message { get; init; }
        public T? Resource { get; init; }

        public ResourceResponse(T resource)
        {
            Success = true;
            Message = null;
            Resource = resource;
        }

        public ResourceResponse(T resource, string message)
        {
            Success = true;
            Message = message;
            Resource = resource;
        }

        public ResourceResponse(string message)
        {
            Success = false;
            Message = message;
            Resource = default;
        }
    }

    public record ResourceResponse
    {
        public bool Success { get; init; }
        public string? Message { get; init; }

        public ResourceResponse()
        {
            Success = true;
            Message = null;
        }

        public ResourceResponse(string message)
        {
            Success = false;
            Message = message;
        }
    }
}

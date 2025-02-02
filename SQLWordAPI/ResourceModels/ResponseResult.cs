namespace SQLWordAPI.ResourceModels
{
    public record ResponseResult<T>
    {
        public bool Success { get; init; }
        public string? Message { get; init; }
        public T? Resource { get; init; }

        public ResponseResult(T resource)
        {
            Success = true;
            Message = null;
            Resource = resource;
        }

        public ResponseResult(T resource, string message)
        {
            Success = true;
            Message = message;
            Resource = resource;
        }

        public ResponseResult(string message)
        {
            Success = false;
            Message = message;
            Resource = default;
        }
    }

    public record ResponseResult
    {
        public bool Success { get; init; }
        public string? Message { get; init; }

        public ResponseResult()
        {
            Success = true;
            Message = null;
        }

        public ResponseResult(string message)
        {
            Success = false;
            Message = message;
        }
    }
}

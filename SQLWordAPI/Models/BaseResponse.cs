namespace SQLWordAPI.Models
{
    public abstract class BaseResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string ErrorMsg { get; set; }

        protected BaseResponse(string msg = "", string errorMsg = "")
        {
            Message = msg;
            ErrorMsg = errorMsg;

            Success = ErrorMsg == string.Empty;
        }
    }

    public class BaseResult : BaseResponse
    {
        public BaseResult(string msg = "", string errorMsg = "") : base(msg, errorMsg) { }
    }

    public class BaseResult<T> : BaseResponse
    {
        public T? Response { get; set; }
        public BaseResult(string msg = "", string errorMsg = "") : base(msg, errorMsg) { }
    }
}

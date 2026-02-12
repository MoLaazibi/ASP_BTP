namespace AP.BTP.Application.CQRS
{
    public class Result<T>
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }
        public static Result<T> Success(T data, string message = "")
        {
            return new Result<T> { Succeeded = true, Data = data, Message = message };
        }
        public static Result<T> Failure(string message = "")
        {
            return new Result<T> { Succeeded = false, Message = message };
        }
    }
}

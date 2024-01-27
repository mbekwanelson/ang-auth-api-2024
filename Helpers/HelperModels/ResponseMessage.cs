namespace ang_auth_api_2024.Helpers.HelperModels
{
    public class ResponseMessage<T>
    {
        public T data { get; set; }
        public string Message { get; set; }
        public bool success { get; set; }
        public int StatusCode { get; set; }

    }
}

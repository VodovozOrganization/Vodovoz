namespace DriverAPI.Models
{
    public class ErrorResponseModel
    {
        public string Error { get; set; }

        public ErrorResponseModel(string message)
        {
            Error = message;
        }
    }
}

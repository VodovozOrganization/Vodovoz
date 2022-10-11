namespace DriverAPI.DTOs
{
	public class ErrorResponseDto
	{
		public string Error { get; set; }

		public ErrorResponseDto(string message)
		{
			Error = message;
		}
	}
}

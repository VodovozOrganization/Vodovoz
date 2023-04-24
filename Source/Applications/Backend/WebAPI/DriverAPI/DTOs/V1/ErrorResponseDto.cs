namespace DriverAPI.DTOs.V1
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

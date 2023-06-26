namespace DriverAPI.DTOs.V3
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

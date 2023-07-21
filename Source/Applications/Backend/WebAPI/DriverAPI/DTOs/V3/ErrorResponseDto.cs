namespace DriverAPI.DTOs.V3
{
	/// <summary>
	/// Ошибка
	/// </summary>
	public class ErrorResponseDto
	{
		/// <summary>
		/// Описание ошибки
		/// </summary>
		public string Error { get; set; }

		public ErrorResponseDto(string message)
		{
			Error = message;
		}
	}
}

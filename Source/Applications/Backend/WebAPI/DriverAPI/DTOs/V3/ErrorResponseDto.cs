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

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="message">Сообщение об ошибке</param>
		public ErrorResponseDto(string message)
		{
			Error = message;
		}
	}
}

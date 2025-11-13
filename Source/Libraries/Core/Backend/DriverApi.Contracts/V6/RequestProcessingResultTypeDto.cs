using System.Text.Json.Serialization;

namespace DriverApi.Contracts.V6
{
	/// <summary>
	/// Результат выполнения запроса
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum RequestProcessingResultTypeDto
	{
		/// <summary>
		/// Успешно
		/// </summary>
		Success,
		/// <summary>
		/// Ошибка
		/// </summary>
		Error
	}
}

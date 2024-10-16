using System.Text.Json.Serialization;

namespace WarehouseApi.Contracts.Dto
{
	/// <summary>
	/// Результат выполнения запроса
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum OperationResultEnumDto
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

using System.Text.Json.Serialization;

namespace WarehouseApi.Contracts.V1.Dto
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

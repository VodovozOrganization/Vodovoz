using System.Text.Json.Serialization;

namespace WarehouseApi.Contracts.Dto.V1
{
	/// <summary>
	/// Статус выполнения операции погрузки
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum LoadOperationStateEnumDto
	{
		/// <summary>
		/// Погрузка не начата
		/// </summary>
		NotStarted,
		/// <summary>
		/// В процессе погрузки
		/// </summary>
		InProgress,
		/// <summary>
		/// Погрузка завершена
		/// </summary>
		Done
	}
}

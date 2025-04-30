using System.Text.Json.Serialization;

namespace WarehouseApi.Contracts.Dto
{
	/// <summary>
	/// Тип источника документа
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum DocumentSourceType
	{
		/// <summary>
		/// Документ погрузки автомобиля
		/// </summary>
		CarLoadDocument,
		/// <summary>
		/// Накладная
		/// </summary>
		Invoice
	}
}

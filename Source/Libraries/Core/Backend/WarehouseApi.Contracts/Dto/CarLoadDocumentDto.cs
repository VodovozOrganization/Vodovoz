using System.Text.Json.Serialization;

namespace WarehouseApi.Contracts.Dto
{
	public class CarLoadDocumentDto
	{
		/// <summary>
		/// Id талона погрузки авто
		/// </summary>
		[JsonPropertyName("id")]
		public int Id { get; set; }

		/// <summary>
		/// Водитель
		/// </summary>
		public string Driver { get; set; }

		/// <summary>
		/// Авто
		/// </summary>
		public string Car { get; set; }

		/// <summary>
		/// Приоритет погрузки
		/// </summary>
		public int LoadPriority { get; set; }

		/// <summary>
		/// Статус документа погрузки
		/// </summary>
		public CarLoadDocumentState State { get; set; }
	}
}

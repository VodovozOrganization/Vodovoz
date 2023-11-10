using System;
using System.Text.Json.Serialization;
using Vodovoz.Tools;

namespace DriverAPI.DTOs.V4
{
	/// <summary>
	/// Информация о событии и координатами сканирования
	/// </summary>
	public class DriverWarehouseEventData
	{
		/// <summary>
		/// Id события
		/// </summary>
		public int DriverWarehouseEventId { get; set; }
		/// <summary>
		/// Широта
		/// </summary>
		public decimal? Latitude { get; set; }
		/// <summary>
		/// Долгота
		/// </summary>
		public decimal? Longitude { get; set; }
		/// <summary>
		/// Тип документа, на котором размещен QR
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public DocumentType? DocumentType { get; set; }
		/// <summary>
		/// Номер документа
		/// </summary>
		public int? DocumentId { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public DateTime ActionTimeUtc { get; set; }
	}
}

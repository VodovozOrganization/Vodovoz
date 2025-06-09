using System;
using System.Text.Json.Serialization;

namespace Vodovoz.RobotMia.Contracts.Responses.V1
{
	/// <summary>
	/// Интервал доставки
	/// </summary>
	public class DeliveryIntervalDto
	{
		/// <summary>
		/// Идентификатор
		/// </summary>
		[JsonPropertyName("id")]
		public int Id { get; set; }

		/// <summary>
		/// Начало интервала
		/// </summary>
		[JsonPropertyName("from")]
		public DateTime From { get; set; }

		/// <summary>
		/// Конец интервала
		/// </summary>
		[JsonPropertyName("to")]
		public DateTime To { get; set; }
	}
}

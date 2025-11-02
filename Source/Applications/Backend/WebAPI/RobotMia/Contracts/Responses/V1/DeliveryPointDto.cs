using System.Text.Json.Serialization;

namespace Vodovoz.RobotMia.Contracts.Responses.V1
{
	/// <summary>
	/// Точка доставки
	/// </summary>
	public class DeliveryPointDto
	{
		/// <summary>
		/// Идентификатор
		/// </summary>
		[JsonPropertyName("id")]
		public int Id { get; set; }

		/// <summary>
		/// Адрес
		/// </summary>
		[JsonPropertyName("address")]
		public string Address { get; set; }
	}
}

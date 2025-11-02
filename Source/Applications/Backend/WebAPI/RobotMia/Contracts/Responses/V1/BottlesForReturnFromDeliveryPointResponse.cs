using System.Text.Json.Serialization;

namespace Vodovoz.RobotMia.Contracts.Responses.V1
{
	/// <summary>
	/// Ответ на зпрос количества бутылей, ожидаемых к возврату с адреса
	/// </summary>
	public class BottlesForReturnFromDeliveryPointResponse
	{
		/// <summary>
		/// Количества бутылей, ожидаемых к возврату с адреса
		/// </summary>
		[JsonPropertyName("bottles_at_delivery_point")]
		public int BottlesAtDeliveryPoint { get; set; }

		/// <summary>
		/// Количества бутылей, ожидаемых к возврату от клиента
		/// </summary>
		[JsonPropertyName("bottles_at_counterparty")]
		public int BottlesAtCouterparty { get; set; }
	}
}

using System.Text.Json.Serialization;

namespace BitrixNotificationsSend.Contracts.Dto
{
	/// <summary>
	/// Запрос на создание сделки в битриксе
	/// </summary>
	public class CreatePlannedOrderDealRequest
	{
		/// <summary>
		/// Поля создаваемой сделки
		/// </summary>
		[JsonPropertyName("fields")]
		public PlannedOrderDto Fields { get; set; }
	}
}

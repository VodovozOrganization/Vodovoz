using System.Text.Json.Serialization;

namespace BitrixApi.Contracts.Dto.Responses
{
	/// <summary>
	/// Dto ответа на запрос заказов по номеру телефона контрагента
	/// </summary>
	public class GetOrdersResponse
	{
		/// <summary>
		/// Идентификаторы найденных заказов через запятую
		/// </summary>
		[JsonPropertyName("orderIds")]
		public string OrderIds { get; set; }
	}
}

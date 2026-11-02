using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DeliveryRulesService.V2.DTO
{
	/// <summary>
	/// Правила доставки
	/// </summary>
	public class DeliveryRulesDTO
	{
		/// <summary>
		/// Статус ответа
		/// </summary>
		[JsonPropertyOrder(1)]
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public DeliveryRulesResponseStatus Status { get; set; }

		/// <summary>
		/// Сообщение
		/// </summary>
		[JsonPropertyOrder(0)]
		public string Message { get; set; }

		/// <summary>
		/// Список правил по дням
		/// </summary>
		[JsonPropertyOrder(2)]
		public IList<WeekDayDeliveryRuleDTO> WeekDayDeliveryRules { get; set; }
	}
}

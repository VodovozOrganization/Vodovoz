using System.Text.Json.Serialization;
using Vodovoz.Core.Domain.Orders.OnlineOrders;
using Vodovoz.Core.Domain.Sale;

namespace Vodovoz.Core.Data.Orders.V5
{
	/// <summary>
	/// Информация о шаблоне автозаказа
	/// </summary>
	public class OrderTemplateDto
	{
		/// <summary>
		/// Идентификатор шаблона
		/// </summary>
		public int OrderTemplateId { get; set; }
		/// <summary>
		/// Активен шаблон
		/// </summary>
		public bool IsActive { get; set; }
		/// <summary>
		/// День недели
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public WeekDayName Weekday { get; set; }
		/// <summary>
		/// Интервал повторов
		/// </summary>
		public RepeatOnlineOrderType RepeatOrder { get; set; }

		public static OrderTemplateDto Create(OnlineOrderTemplate template) =>
			new OrderTemplateDto
			{
				OrderTemplateId = template.Id,
				Weekday = template.Weekday,
				RepeatOrder = template.RepeatOrder,
				IsActive = template.IsActive,
			};
	}
}

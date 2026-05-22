using System.Collections.Generic;

namespace CustomerOrders.Contracts.V5.Orders.Templates
{
	/// <summary>
	/// Ответ с информацией по шаблонам автозаказов
	/// </summary>
	public class OrderTemplatesDto
	{
		/// <summary>
		/// Количество шаблонов клиента
		/// </summary>
		public int TemplatesCount { get; set; }
		/// <summary>
		/// Шаблон автозаказа
		/// </summary>
		public IEnumerable<OrderTemplateCardFromListDto> OrderTemplates { get; set; }

		public static OrderTemplatesDto Create(
			int count,
			IEnumerable<OrderTemplateCardFromListDto> orderTemplates) => new OrderTemplatesDto
		{
			TemplatesCount = count,
			OrderTemplates = orderTemplates
		};
	}
}

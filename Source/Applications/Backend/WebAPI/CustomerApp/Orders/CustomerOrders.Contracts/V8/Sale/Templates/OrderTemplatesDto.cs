using System.Collections.Generic;

namespace CustomerOrders.Contracts.V8.Sale.Templates
{
	public class OrderTemplatesDto
	{
		/// <summary>
		/// Количество шаблонов клиента
		/// </summary>
		public int TemplatesCount { get; set; }

		/// <summary>
		/// Можно создавать автозаказы
		/// </summary>
		public bool CanCreateOrderTemplate { get; set; }

		/// <summary>
		/// Краткая инфа по шаблонам автозаказов
		/// </summary>
		public IEnumerable<OrderTemplateCardDto> OrderTemplates { get; set; }

		public static OrderTemplatesDto Create(
			int count,
			IEnumerable<OrderTemplateCardDto> orderTemplates) => new OrderTemplatesDto
		{
			TemplatesCount = count,
			OrderTemplates = orderTemplates
		};
	}
}

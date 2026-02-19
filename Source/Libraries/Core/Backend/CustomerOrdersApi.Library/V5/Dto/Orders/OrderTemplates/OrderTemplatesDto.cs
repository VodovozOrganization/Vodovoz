using Vodovoz.Core.Data.Orders.V5;

namespace CustomerOrdersApi.Library.V5.Dto.Orders.OrderTemplates
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
		public OrderTemplateDto[] OrderTemplates { get; set; }

		public static OrderTemplatesDto Create(OrderTemplateDto[] orderTemplates) => new OrderTemplatesDto
		{
			TemplatesCount = orderTemplates.Length,
			OrderTemplates = orderTemplates
		};
	}
}

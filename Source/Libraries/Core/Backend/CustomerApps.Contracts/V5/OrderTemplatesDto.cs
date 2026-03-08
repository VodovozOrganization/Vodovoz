namespace CustomerApps.Contracts.V5
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
		public OrderTemplateCardFromListDto[] OrderTemplates { get; set; }

		public static OrderTemplatesDto Create(OrderTemplateCardFromListDto[] orderTemplates) => new OrderTemplatesDto
		{
			TemplatesCount = orderTemplates.Length,
			OrderTemplates = orderTemplates
		};
	}
}

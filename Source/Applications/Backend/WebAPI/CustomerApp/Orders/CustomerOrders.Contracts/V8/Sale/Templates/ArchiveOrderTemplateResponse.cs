namespace CustomerOrders.Contracts.V8.Sale.Templates
{
	/// <summary>
	/// Данные ответа по архивированию автозаказа
	/// </summary>
	public class ArchiveOrderTemplateResponse
	{
		/// <summary>
		/// Статус шаблона
		/// </summary>
		public TemplateStatusDto Status { get; set; }

		/// <summary>
		/// Идентификатор шаблона автозаказа
		/// </summary>
		public int OrderTemplateId { get; set; }

		public static ArchiveOrderTemplateResponse Create(int orderTemplateId, TemplateStatusDto status) =>
			new ArchiveOrderTemplateResponse
			{
				OrderTemplateId = orderTemplateId,
				Status = status
			};
	}
}

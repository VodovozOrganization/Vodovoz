namespace CustomerOrders.Contracts.V8.Sale.Templates
{
	/// <summary>
	/// Данные ответа по приостановке автозаказа
	/// </summary>
	public class PauseOrderTemplateResponse
	{
		/// <summary>
		/// Статус шаблона
		/// </summary>
		public TemplateStatusDto Status { get; set; }

		/// <summary>
		/// Идентификатор шаблона автозаказа
		/// </summary>
		public int OrderTemplateId { get; set; }

		public static PauseOrderTemplateResponse CreateSuccess(int orderTemplateId) =>
			new PauseOrderTemplateResponse
			{
				OrderTemplateId = orderTemplateId,
				Status = TemplateStatusDto.Paused
			};
	}
}

namespace CustomerOrders.Contracts.V8.Sale.Templates
{
	/// <summary>
	/// Информация по действиям по автозаказу
	/// </summary>
	public class OrderTemplateActionsDto
	{
		public OrderTemplateActionsDto() {}

		public OrderTemplateActionsDto(bool canPause, bool canResume, bool canArchive)
		{
			CanPause = canPause;
			CanResume = canResume;
			CanArchive = canArchive;
		}

		/// <summary>
		/// Можно приостановить(автозаказ не будет оформляться)
		/// </summary>
		public bool CanPause { get; set; }
		/// <summary>
		/// Можно возобновить(автозаказ начнет оформляться)
		/// </summary>
		public bool CanResume { get; set; }
		/// <summary>
		/// Можно архивировать(автозаказ станет недоступным для использования)
		/// </summary>
		public bool CanArchive { get; set; }
	}
}

namespace VodovozBusiness.Services.TrueMark
{
	/// <summary>
	/// Результат переноса кодов маркировки из отмененного заказа.
	/// </summary>
	public class CancelledOrderTrueMarkCodesReuseResult
	{
		/// <summary>
		/// Номер заказа, в который перенесены коды.
		/// </summary>
		public int TargetOrderId { get; set; }

		/// <summary>
		/// Номер созданной ЭДО-заявки.
		/// </summary>
		public int EdoRequestId { get; set; }

		/// <summary>
		/// Количество перенесенных кодов.
		/// </summary>
		public int ReusedCodesCount { get; set; }
	}
}

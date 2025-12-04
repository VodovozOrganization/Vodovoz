namespace Vodovoz.Application.Orders.Services.OrderCancellation
{
	/// <summary>
	/// Тип разрешения на отмену заказа
	/// </summary>
	public enum OrderCancellationPermitType
	{
		/// <summary>
		/// Проверка отмены заказа не проводилась
		/// </summary>
		None,

		/// <summary>
		/// Запрет отмены заказа и документооборота
		/// </summary>
		Deny,

		/// <summary>
		/// Разрешена только отмена документооборота
		/// </summary>
		AllowCancelDocflow,

		/// <summary>
		/// Разрешена отмена заказа
		/// Документооборот отменяется автоматически при отмене заказа
		/// </summary>
		AllowCancelOrder
	}
}

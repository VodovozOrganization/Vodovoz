namespace Vodovoz.Application.Orders.Services.OrderCancellation
{
	/// <summary>
	/// Разрешение на отмену заказа
	/// </summary>
	public class OrderCancellationPermit
	{
		/// <summary>
		/// Тип разрешения на отмену заказа
		/// </summary>
		public OrderCancellationPermitType Type { get; set; }

		/// <summary>
		/// Принято подтверждение на отмену документооборота
		/// </summary>
		public bool DocflowCancellationOfferConfirmation { get; set; }

		/// <summary>
		/// Принято подтверждение на отмену заказа
		/// </summary>
		public bool OrderCancellationConfirmation { get; set; }

		/// <summary>
		/// Id задачи ЭДО которая будет отменена
		/// </summary>
		public int? EdoTaskToCancellationId { get; set; }

		/// <summary>
		/// Не установленное по умолчанию разрешение на отмену заказа
		/// </summary>
		/// <returns></returns>
		public static OrderCancellationPermit Default()
		{
			return new OrderCancellationPermit
			{
				Type = OrderCancellationPermitType.None,
				DocflowCancellationOfferConfirmation = false,
				OrderCancellationConfirmation = false
			};
		}
	}
}

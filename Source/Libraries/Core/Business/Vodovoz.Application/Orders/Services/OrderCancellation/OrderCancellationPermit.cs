namespace Vodovoz.Application.Orders.Services.OrderCancellation
{
	public class OrderCancellationPermit
	{
		public OrderCancellationPermitType Type { get; set; }

		public bool DocflowCancellationOfferConfirmation { get; set; }
		public bool OrderCancellationConfirmation { get; set; }

		public int? EdoTaskToCancellationId { get; set; }

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

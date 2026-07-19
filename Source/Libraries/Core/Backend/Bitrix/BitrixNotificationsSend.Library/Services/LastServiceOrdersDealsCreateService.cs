using Microsoft.Extensions.Logging;
using System;
using Vodovoz.Domain.Orders;

namespace BitrixNotificationsSend.Library.Services
{
	public class LastServiceOrdersDealsCreateService
	{
		private readonly OrderStatus[] _completedOrderStatuses =
		{
			OrderStatus.Shipped,
			OrderStatus.UnloadingOnStock,
			OrderStatus.Closed
		};

		private readonly OrderStatus[] _canceledOrderStatuses =
		{
			OrderStatus.Canceled,
			OrderStatus.NotDelivered,
			OrderStatus.DeliveryCanceled
		};
		private readonly ILogger<LastServiceOrdersDealsCreateService> _logger;

		public LastServiceOrdersDealsCreateService(
			ILogger<LastServiceOrdersDealsCreateService> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}
	}
}

using System;
using Vodovoz.Core.Application.Orders.Services.OrderCancellation;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Core.Application.Orders
{
	public class UndeliveryOnOrderCloseEventArgs : EventArgs
	{
		public UndeliveredOrder UndeliveredOrder { get; private set; }
		public bool NeedClose { get; }

		public OrderCancellationPermit CancellationPermit { get; set; }

		public UndeliveryOnOrderCloseEventArgs(
			UndeliveredOrder undeliveredOrder,
			OrderCancellationPermit cancellationPermit,
			bool needClose = true)
		{
			NeedClose = needClose;
			CancellationPermit = cancellationPermit;
			UndeliveredOrder = undeliveredOrder;
		}
	}
}

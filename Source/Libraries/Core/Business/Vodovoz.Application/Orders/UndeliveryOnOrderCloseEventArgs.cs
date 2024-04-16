using System;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Application.Orders
{
	public class UndeliveryOnOrderCloseEventArgs : EventArgs
	{
		public UndeliveredOrder UndeliveredOrder { get; private set; }
		public bool NeedClose { get; }

		public UndeliveryOnOrderCloseEventArgs(UndeliveredOrder undeliveredOrder, bool needClose = true)
		{
			NeedClose = needClose;
			UndeliveredOrder = undeliveredOrder;
		}
	}
}

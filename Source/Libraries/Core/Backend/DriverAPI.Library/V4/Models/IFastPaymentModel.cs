﻿using Vodovoz.Domain.FastPayments;

namespace DriverAPI.Library.V4.Models
{
	public interface IFastPaymentModel
	{
		FastPaymentStatus? GetOrderFastPaymentStatus(int orderId, int? onlineOrder = null);
	}
}

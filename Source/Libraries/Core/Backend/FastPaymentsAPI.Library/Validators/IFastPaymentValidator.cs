﻿using Vodovoz.Domain.Orders;

namespace FastPaymentsAPI.Library.Validators
{
	public interface IFastPaymentValidator
	{
		string Validate(int orderId);
		string Validate(int onlineOrderId, string backUrl, string backUrlOk, string backUrlFail);
		string ValidateOnlineOrder(decimal onlineOrderSum);
		string Validate(int orderId, ref string phoneNumber);
		string Validate(Order order, int orderId);
	}
}

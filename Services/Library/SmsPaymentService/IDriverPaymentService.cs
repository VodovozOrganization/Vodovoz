using System;
namespace SmsPaymentService
{
	public interface IDriverPaymentService
	{
		void RefreshPaymentStatus(int orderId);
	}
}

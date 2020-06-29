using System;
namespace Android
{
	public interface IDriverNotificator
	{
		void SendOrderPaymentStatusChangedMessage(string deviceId, string sender, string message);
	}
}

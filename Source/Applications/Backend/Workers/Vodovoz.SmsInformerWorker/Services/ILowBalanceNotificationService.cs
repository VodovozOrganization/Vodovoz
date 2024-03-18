using Sms.External.Interface;

namespace Vodovoz.SmsInformerWorker.Services
{
	internal interface ILowBalanceNotificationService
	{
		void BalanceNotifierOnBalanceChange(object sender, SmsBalanceEventArgs e);
	}
}
using System;
namespace Sms.External.Interface
{
	/// <summary>
	/// Осуществляет уведомление об изменении баланса на счете
	/// </summary>
	public interface ISmsBalanceNotifier
	{
		event EventHandler<SmsBalanceEventArgs> OnBalanceChange;
	}
}

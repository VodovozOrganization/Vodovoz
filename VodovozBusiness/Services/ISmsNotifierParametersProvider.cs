using System;
namespace Vodovoz.Services
{
	public interface ISmsNotifierParametersProvider
	{
		string GetNewClientSmsTextTemplate();
		decimal GetLowBalanceLevel();
		string GetLowBalanceNotifiedPhone();
		string GetLowBalanceNotifyText();
	}
}

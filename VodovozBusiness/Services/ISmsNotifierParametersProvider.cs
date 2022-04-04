namespace Vodovoz.Services
{
	public interface ISmsNotifierParametersProvider
	{
		bool IsSmsNotificationsEnabled { get; }
		string GetNewClientSmsTextTemplate();
		decimal GetLowBalanceLevel();
		string GetLowBalanceNotifiedPhone();
		string GetLowBalanceNotifyText();
		string GetUndeliveryAutoTransferNotApprovedTextTemplate();
		string GetFastPaymentApiBaseUrl();
		string GetAvangardFastPayBaseUrl();
	}
}

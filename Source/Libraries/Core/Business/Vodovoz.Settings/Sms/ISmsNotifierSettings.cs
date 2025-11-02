namespace Vodovoz.Services
{
	public interface ISmsNotifierSettings
	{
		bool IsSmsNotificationsEnabled { get; }
		string NewClientSmsTextTemplate { get; }
		decimal LowBalanceLevel { get; }
		string LowBalanceNotifiedPhone { get; }
		string LowBalanceNotifyText { get; }
		string UndeliveryAutoTransferNotApprovedTextTemplate { get; }
	}
}

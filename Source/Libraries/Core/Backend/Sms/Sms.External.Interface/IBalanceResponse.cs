namespace Sms.External.Interface
{
	interface IBalanceResponse
	{
		BalanceResponseStatus Status { get; set; }

		BalanceType BalanceType { get; set; }

		decimal BalanceValue { get; set; }
	}
}

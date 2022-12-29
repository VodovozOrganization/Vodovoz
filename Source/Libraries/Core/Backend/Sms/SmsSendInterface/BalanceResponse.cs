namespace SmsSendInterface
{
	interface IBalanceResponse
	{
		BalanceResponseStatus Status { get; set; }

		BalanceType BalanceType { get; set; }

		decimal BalanceValue { get; set; }
	}

	public class BalanceResponse : IBalanceResponse
	{
		public BalanceResponseStatus Status { get; set; }

		public BalanceType BalanceType { get; set; }

		public decimal BalanceValue { get; set; }
	}

	public enum BalanceResponseStatus
	{
		Ok,
		Error
	}

	/// <summary>
	/// Тип баланса
	/// </summary>
	public enum BalanceType
	{
		/// <summary>
		/// Количество смс сообщений
		/// </summary>
		SmsCounts,

		/// <summary>
		/// Баланс в валюте
		/// </summary>
		CurrencyBalance
	}
}

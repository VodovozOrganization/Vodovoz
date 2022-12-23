using System;

namespace Sms.External.Interface
{
	public class SmsBalanceEventArgs : EventArgs
	{
		public BalanceType BalanceType { get; }
		public decimal Balance { get; }

		public SmsBalanceEventArgs(BalanceType balanceType, decimal balance)
		{
			BalanceType = balanceType;
			Balance = balance;
		}
	}
}

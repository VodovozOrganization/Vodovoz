using System;

namespace ModulKassa
{
	public class CashboxException : Exception
	{
		public CashboxException()
		{
		}

		public CashboxException(string message) : base(message)
		{
		}

		public CashboxException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}

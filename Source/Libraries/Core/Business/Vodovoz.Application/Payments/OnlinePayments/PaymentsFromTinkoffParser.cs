using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vodovoz.Domain.Payments;
using VodovozBusiness.Domain.Payments;

namespace Vodovoz.Application.Payments.OnlinePayments
{
	/// <summary>
	/// Парсер выписок банка Тинькоф
	/// </summary>
	public class PaymentsFromTinkoffParser : IPaymentByCardOnlineParser
	{
		public IEnumerable<PaymentByCardOnline> ParsedPayments { get; private set; }

		public void Parse(string fileName)
		{
			try
			{
				ParsedPayments = File.ReadAllLines(fileName)
					.Skip(1)
					.Select(x => x.Split(';'))
					.Select(x => new PaymentByCardOnline(
						x.Select(y => y.Trim('"')).ToArray()))
					.ToList();
			}
			catch(Exception e)
			{
				throw new ArgumentException("Неправильный формат выгрузки.");
			}
		}
	}
}

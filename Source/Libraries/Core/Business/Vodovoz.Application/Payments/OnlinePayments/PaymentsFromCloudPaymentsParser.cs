using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Vodovoz.Domain.Payments;
using VodovozBusiness.Domain.Payments;

namespace Vodovoz.Application.Payments.OnlinePayments
{
	/// <summary>
	/// Парсер выписок CloudPayments
	/// </summary>
	public class PaymentsFromCloudPaymentsParser : IPaymentByCardOnlineParser
	{
		private readonly IList<PaymentByCardOnline> _parsedPayments = new List<PaymentByCardOnline>();

		public IEnumerable<PaymentByCardOnline> ParsedPayments => _parsedPayments;

		public void Parse(string fileName)
		{
			using(var reader = new StreamReader(fileName, Encoding.GetEncoding(1251)))
			{
				string line;
				var paymentByCardFrom = PaymentByCardOnlineFrom.FromCloudPayments;

				var count = 0;
				while((line = reader.ReadLine()) != null)
				{
					count++;
					if(line == string.Empty)
					{
						continue;
					}

					var data = line.Split(new[] { '\t' });

					if(count == 1
					   && (data.Length < 2 || data[0] != "Номер" || data[1] != "Дата и время" || data[2] != "Банк"))
					{
						throw new ArgumentException("Не подходящий файл или выбран не тот тип загрузки.");
					}

					if(data.Length < 15
						|| (data[14] != "Завершена" && data[14] != "Completed")
						|| !Guid.TryParse(data[5], out _))
					{
						continue;
					}

					var payment = new PaymentByCardOnline(data, paymentByCardFrom);
					_parsedPayments.Add(payment);
				}
			}
		}
	}
}

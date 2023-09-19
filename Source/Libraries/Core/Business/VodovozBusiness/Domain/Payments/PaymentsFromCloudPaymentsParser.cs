using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Vodovoz.Domain.Payments
{
	public class PaymentsFromCloudPaymentsParser
	{
		private readonly string docPath;

		public List<PaymentByCardOnline> PaymentsFromCloudPayments { get; set; } = new List<PaymentByCardOnline>();

		public PaymentsFromCloudPaymentsParser(string docPath)
		{
			this.docPath = docPath;
		}

		public void Parse()
		{
			using(var reader = new StreamReader(docPath, Encoding.GetEncoding(1251)))
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

					if(data.Length < 15 || data[14] != "Завершена" || !Guid.TryParse(data[5], out _))
					{
						continue;
					}

					var payment = new PaymentByCardOnline(data, paymentByCardFrom);
					PaymentsFromCloudPayments.Add(payment);
				}
			}
		}
	}
}

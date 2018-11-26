using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vodovoz.Domain.Payments;

namespace Vodovoz.ServiceDialogs
{
	public class PaymentsFromTinkoffParser
	{
		public string DocumentPath { get; private set; }
		public List<PaymentFromTinkoff> PaymentsFromTinkoff { get; set; }

		public PaymentsFromTinkoffParser(string documentPath)
		{
			DocumentPath = documentPath;
		}

		public void Parse()
		{
			PaymentsFromTinkoff = File.ReadAllLines(DocumentPath)
			                          .Skip(1)
			                          .Select(x => x.Split(';'))
			                          .Select(
				                          x => new PaymentFromTinkoff(
					                          x.Select(y => y.Trim('"'))
					                          .ToArray()
					                         )
				                         )
			                          .ToList();
		}
	}
}
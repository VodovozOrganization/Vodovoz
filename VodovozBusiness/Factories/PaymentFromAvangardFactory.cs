using System;
using System.Globalization;
using Vodovoz.Domain.Payments;

namespace Vodovoz.Factories
{
	public class PaymentFromAvangardFactory : IPaymentFromAvangardFactory
	{
		private readonly CultureInfo _culture;

		public PaymentFromAvangardFactory()
		{
			_culture = CultureInfo.CreateSpecificCulture("ru-RU");
			_culture.NumberFormat.NumberDecimalSeparator = ".";
		}
		
		public PaymentFromAvangard CreateNewPaymentFromAvangard(ImportPaymentsFromAvangardSbpNode node)
		{
			var payment = new PaymentFromAvangard
			{
				PaidDate = node.PaidDate,
				OrderNum = node.OrderNum,
				OrderSum = node.OrderSum
			};

			return payment;
		}

		public ImportPaymentsFromAvangardSbpNode CreateImportPaymentsFromAvangardSbpNode(string[] data)
		{
			var node = new ImportPaymentsFromAvangardSbpNode
			{
				PaidDate = DateTime.Parse(data[0]),
				OrderNum = int.Parse(data[13]),
				OrderSum = decimal.Parse(data[8], _culture)
			};

			return node;
		}
	}
	
	public class ImportPaymentsFromAvangardSbpNode
	{
		public DateTime PaidDate { get; set; }
		public int OrderNum { get; set; }
		public decimal OrderSum { get; set; }
		public bool isCheckedForDuplicate { get; set; }
	}
}

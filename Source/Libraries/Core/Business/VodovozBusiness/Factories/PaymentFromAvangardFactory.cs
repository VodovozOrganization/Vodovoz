using System.Globalization;
using Vodovoz.Domain.Payments;
using Vodovoz.Nodes;

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
		
		public PaymentFromAvangard CreateNewPaymentFromAvangard(AvangardOperation node)
		{
			var payment = new PaymentFromAvangard
			{
				PaidDate = node.TransDate,
				OrderNum = node.OrderNumber,
				TotalSum = node.Amount
			};

			return payment;
		}
	}
}

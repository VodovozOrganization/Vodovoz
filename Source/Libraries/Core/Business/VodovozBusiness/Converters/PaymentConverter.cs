using System.Collections.Generic;
using System.Linq;
using TaxcomEdo.Contracts.Payments;
using Vodovoz.Domain.Payments;

namespace VodovozBusiness.Converters
{
	public class PaymentConverter : IPaymentConverter
	{
		public IEnumerable<PaymentInfoForEdo> ConvertPaymentToPaymentInfoForEdo(IEnumerable<Payment> payments)
		{
			return payments.Select(
				x => PaymentInfoForEdo.Create(x.PaymentNum.ToString(), x.Date.ToString("dd.MM.yyyy")))
				.ToList();
		}
	}
}

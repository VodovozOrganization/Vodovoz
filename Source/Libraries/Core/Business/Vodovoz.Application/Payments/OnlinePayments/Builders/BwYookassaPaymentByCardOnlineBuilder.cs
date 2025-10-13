using Vodovoz.Domain.Payments;
using VodovozBusiness.Domain.Payments;

namespace Vodovoz.Application.Payments.OnlinePayments.Builders
{
	/// <summary>
	/// Билдер для выписки по МН(мир напитков)
	/// </summary>
	public class BwYookassaPaymentByCardOnlineBuilder : DefaultYookassaPaymentByCardOnlineBuilder
	{
		public BwYookassaPaymentByCardOnlineBuilder((IOnlinePaymentRegisterColumns Columns, PaymentByCardOnlineFrom? PaymentFrom) registerData)
			: base(registerData)
		{
		}

		public override PaymentByCardOnline Build(string[] data)
		{
			Sum(data[RegisterData.Columns.PaymentSumColumn])
				.DateAndTime(data[RegisterData.Columns.DateAndTimeColumn])
				.PaymentNumberAndSource(data[RegisterData.Columns.PaymentNumberColumn], RegisterData.PaymentFrom.Value);
			
			Payment.PaymentStatus = PaymentStatus.CONFIRMED;
			return Payment;
		}
	}
}

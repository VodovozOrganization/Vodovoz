using Vodovoz.Domain.Payments;
using VodovozBusiness.Domain.Payments;

namespace Vodovoz.Application.Payments.OnlinePayments.Builders
{
	/// <summary>
	/// Билдер для выписки по ВВ Восток
	/// </summary>
	public class VvEastYookassaPaymentByCardOnlineBuilder : DefaultYookassaPaymentByCardOnlineBuilder
	{
		public VvEastYookassaPaymentByCardOnlineBuilder((IOnlinePaymentRegisterColumns Columns, PaymentByCardOnlineFrom? PaymentFrom) registerData)
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

using Vodovoz.Domain.Payments;
using VodovozBusiness.Domain.Payments;

namespace Vodovoz.Core.Application.Payments.OnlinePayments.Builders
{
	/// <summary>
	/// Билдер для выписки по Миру напитков
	/// </summary>
	public class MirNapitkovYookassaPaymentByCardOnlineBuilder : DefaultYookassaPaymentByCardOnlineBuilder
	{
		public MirNapitkovYookassaPaymentByCardOnlineBuilder((IOnlinePaymentRegisterColumns Columns, PaymentByCardOnlineFrom? PaymentFrom) registerData)
			: base(registerData)
		{
		}

		public override PaymentByCardOnline Build(string[] data)
		{
			NewPayment()
				.Sum(data[RegisterData.Columns.PaymentSumColumn])
				.DateAndTime(data[RegisterData.Columns.DateAndTimeColumn])
				.PaymentNumberAndSource(data[RegisterData.Columns.PaymentNumberColumn], RegisterData.PaymentFrom.Value);
			
			Payment.PaymentStatus = PaymentStatus.CONFIRMED;
			return Payment;
		}
	}
}

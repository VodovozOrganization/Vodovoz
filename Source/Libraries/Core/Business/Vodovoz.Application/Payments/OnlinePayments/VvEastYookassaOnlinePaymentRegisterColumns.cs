using VodovozBusiness.Domain.Payments;

namespace Vodovoz.Application.Payments.OnlinePayments
{
	/// <inheritdoc/>
	public class VvEastYookassaOnlinePaymentRegisterColumns : IOnlinePaymentRegisterColumns
	{
		public int PaymentSumColumn => 1;
		public int DateAndTimeColumn => 6;
		public int PaymentNumberColumn => 8;
		public int? EmailColumn => null;
		
		public static IOnlinePaymentRegisterColumns Create() => new VvEastYookassaOnlinePaymentRegisterColumns();
	}
}

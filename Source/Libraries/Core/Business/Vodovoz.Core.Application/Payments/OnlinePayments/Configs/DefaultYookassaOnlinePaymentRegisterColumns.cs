using VodovozBusiness.Domain.Payments;

namespace Vodovoz.Core.Application.Payments.OnlinePayments.Configs
{
	/// <inheritdoc/>
	public class DefaultYookassaOnlinePaymentRegisterColumns : IOnlinePaymentRegisterColumns
	{
		/// <inheritdoc/>
		public int PaymentSumColumn => 1;
		/// <inheritdoc/>
		public int DateAndTimeColumn => 4;
		/// <inheritdoc/>
		public int PaymentNumberColumn => 6;
		/// <inheritdoc/>
		public int? EmailColumn => 6;

		public static IOnlinePaymentRegisterColumns Create() => new DefaultYookassaOnlinePaymentRegisterColumns();
	}
}

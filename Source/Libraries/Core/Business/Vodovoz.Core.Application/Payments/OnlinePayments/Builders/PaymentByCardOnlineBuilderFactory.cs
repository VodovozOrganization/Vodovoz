using Vodovoz.Core.Application.Payments.OnlinePayments.Configs;
using Vodovoz.Domain.Payments;
using VodovozBusiness.Domain.Payments;

namespace Vodovoz.Core.Application.Payments.OnlinePayments.Builders
{
	/// <inheritdoc/>
	public class PaymentByCardOnlineBuilderFactory : IPaymentByCardOnlineBuilderFactory
	{
		/// <inheritdoc/>
		public IPaymentByCardOnlineBuilder Create((IOnlinePaymentRegisterColumns Columns, PaymentByCardOnlineFrom? PaymentFrom) registerData)
		{
			switch (registerData.Columns)
			{
				case MirNapitkovYookassaOnlinePaymentRegisterColumns _:
					return new MirNapitkovYookassaPaymentByCardOnlineBuilder(registerData);
				case VvEastYookassaOnlinePaymentRegisterColumns _:
					return new VvEastYookassaPaymentByCardOnlineBuilder(registerData);
				case VvSouthYookassaOnlinePaymentRegisterColumns _:
					return new VvSouthYookassaPaymentByCardOnlineBuilder(registerData);
				default:
					return new DefaultYookassaPaymentByCardOnlineBuilder(registerData);
			}
		}
	}
}

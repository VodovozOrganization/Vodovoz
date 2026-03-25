using Vodovoz.Domain.Payments;
using VodovozBusiness.Domain.Payments;

namespace Vodovoz.Application.Payments.OnlinePayments
{
	/// <summary>
	/// Колонки выписки Мира напитков, которые выгружаются в онлайн платеж <see cref="PaymentByCardOnline"/>
	/// </summary>
	public class BwYookassaOnlinePaymentRegisterColumns : IOnlinePaymentRegisterColumns
	{
		public int PaymentSumColumn => 1;
		public int DateAndTimeColumn => 5;
		public int PaymentNumberColumn => 7;
		public int? EmailColumn => null;

		public static IOnlinePaymentRegisterColumns Create() => new BwYookassaOnlinePaymentRegisterColumns();
	}
}

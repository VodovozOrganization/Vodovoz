using VodovozBusiness.Domain.Payments;

namespace Vodovoz.Application.Payments.OnlinePayments
{
	/// <summary>
	/// Колонки выписки ВВ Восток, которые выгружаются в онлайн платеж <see cref="PaymentByCardOnline"/>
	/// </summary>
	public class VvEastYookassaOnlinePaymentRegisterColumns : IOnlinePaymentRegisterColumns
	{
		public int PaymentSumColumn => 1;
		public int DateAndTimeColumn => 6;
		public int PaymentNumberColumn => 8;
		public int? EmailColumn => null;
		
		public static IOnlinePaymentRegisterColumns Create() => new VvEastYookassaOnlinePaymentRegisterColumns();
	}
}

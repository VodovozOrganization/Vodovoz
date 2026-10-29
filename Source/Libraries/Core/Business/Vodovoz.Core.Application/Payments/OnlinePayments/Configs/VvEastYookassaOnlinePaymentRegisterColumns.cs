using VodovozBusiness.Domain.Payments;

namespace Vodovoz.Core.Application.Payments.OnlinePayments.Configs
{
	/// <summary>
	/// Колонки выписки ВВ Восток, которые выгружаются в онлайн платеж <see cref="PaymentByCardOnline"/>
	/// </summary>
	public class VvEastYookassaOnlinePaymentRegisterColumns : IOnlinePaymentRegisterColumns
	{
		/// <inheritdoc/>
		public int PaymentSumColumn => 1;
		/// <inheritdoc/>
		public int DateAndTimeColumn => 6;
		/// <inheritdoc/>
		public int PaymentNumberColumn => 8;
		/// <inheritdoc/>
		public int? EmailColumn => null;
		
		public static IOnlinePaymentRegisterColumns Create() => new VvEastYookassaOnlinePaymentRegisterColumns();
	}
}

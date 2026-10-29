using VodovozBusiness.Domain.Payments;

namespace Vodovoz.Core.Application.Payments.OnlinePayments.Configs
{
	/// <summary>
	/// Колонки выписки ВВ Юг, которые выгружаются в онлайн платеж <see cref="PaymentByCardOnline"/>
	/// </summary>
	public class VvSouthYookassaOnlinePaymentRegisterColumns : IOnlinePaymentRegisterColumns
	{
		/// <inheritdoc/>
		public int PaymentSumColumn => 1;
		/// <inheritdoc/>
		public int DateAndTimeColumn => 5;
		/// <inheritdoc/>
		public int PaymentNumberColumn => 7;
		/// <inheritdoc/>
		public int? EmailColumn => null;

		public static IOnlinePaymentRegisterColumns Create() => new VvSouthYookassaOnlinePaymentRegisterColumns();
	}
}

namespace Vodovoz.Core.Domain.Common
{
	/// <summary>
	/// Типы оплат при заказе клиентом
	/// </summary>
	public enum CounterpartyOrderPaymentType
	{
		/// <summary>
		/// Безнал
		/// </summary>
		Cashless,
		/// <summary>
		/// Все остальное, кроме безнала(наличка, терминал, QR и т.д.)
		/// </summary>
		Cash
	}
}

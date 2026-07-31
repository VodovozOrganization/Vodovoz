using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;

namespace Vodovoz.Core.Domain.Orders
{
	/// <summary>
	/// Правила формирования чека по заказу
	/// </summary>
	public static class OrderReceiptRules
	{
		/// <summary>
		/// Предполагает ли форма оплаты заказа отправку чека
		/// </summary>
		/// <param name="reasonForLeaving">Цель приобретения воды клиентом по заказу</param>
		/// <param name="paymentType">Форма оплаты заказа</param>
		/// <param name="isReceiptRequiredForPaymentFrom">
		/// Требуется ли чек для места, откуда проведена оплата. Имеет значение только для оплаты онлайн
		/// </param>
		/// <returns><see langword="true"/>, если форма оплаты предполагает отправку чека</returns>
		public static bool IsSendingReceiptExpectedByPaymentType(
			ReasonForLeaving? reasonForLeaving,
			PaymentType paymentType,
			bool? isReceiptRequiredForPaymentFrom)
		{
			if(reasonForLeaving == ReasonForLeaving.Tender)
			{
				return false;
			}

			switch(paymentType)
			{
				case PaymentType.Cash:
				case PaymentType.Terminal:
				case PaymentType.DriverApplicationQR:
				case PaymentType.SmsQR:
					return true;
				case PaymentType.PaidOnline:
					return isReceiptRequiredForPaymentFrom ?? true;
				default:
					return false;
			}
		}
	}
}

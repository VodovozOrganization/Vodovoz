using System.Collections.Generic;
using Vodovoz.Domain.Payments;

namespace VodovozBusiness.Domain.Payments
{
	/// <summary>
	/// Парсер онлайн платежей
	/// </summary>
	public interface IPaymentByCardOnlineParser
	{
		/// <summary>
		/// Парсинг выписки
		/// </summary>
		/// <param name="filename">файл</param>
		void Parse(string filename);
		/// <summary>
		/// Полученные платежи из выписки
		/// </summary>
		IEnumerable<PaymentByCardOnline> ParsedPayments { get; }
	}
}

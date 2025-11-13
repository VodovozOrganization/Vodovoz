using System.Collections.Generic;
using TaxcomEdo.Contracts.Payments;
using Vodovoz.Domain.Payments;

namespace VodovozBusiness.Converters
{
	public interface IPaymentConverter
	{
		/// <summary>
		/// Конвертация коллекции платежей <see cref="Payment"/> в информацию о них для ЭДО <see cref="PaymentInfoForEdo"/>
		/// </summary>
		/// <param name="payments">Коллекция платежей</param>
		/// <returns>Список оплат для ЭДО</returns>
		IEnumerable<PaymentInfoForEdo> ConvertPaymentToPaymentInfoForEdo(IEnumerable<Payment> payments);
	}
}

using System.Collections.Generic;
using CustomerOrders.Contracts;
using CustomerOrders.Contracts.V5.Carts;

namespace CustomerOrdersApi.Library.V6.Services
{
	/// <summary>
	/// Создатель доступных методов оплат для ИПЗ
	/// </summary>
	public interface IPaymentMethodsCreator
	{
		/// <summary>
		/// Создание доступных методов оплат под конкретный источник
		/// </summary>
		/// <param name="source">Источник(ИПЗ)</param>
		/// <returns>Список доступных методов оплат</returns>
		IEnumerable<PaymentMethod> GetPaymentMethods(ExternalSource source);
	}
}

using System.Collections.Generic;
using CustomerApp.Contracts.Common;
using CustomerOrders.Abstractions;
using CustomerOrdersApi.Library.V8.Dto.Carts;

namespace CustomerOrdersApi.Library.V8.Services
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

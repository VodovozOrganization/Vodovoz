using System.Collections.Generic;
using System.Linq;
using CustomerOrders.Contracts.V5.Carts;

namespace CustomerOrdersApi.Library.V6.Factories.DeliveryConditions
{
	/// <summary>
	/// Фабрика по созданию доступных методов оплат для ИПЗ
	/// </summary>
	public abstract class PaymentMethodFactory
	{
		/// <summary>
		/// Создание необходимых методов оплат
		/// </summary>
		/// <param name="paymentTypes">Список доступных методов оплат для ИПЗ</param>
		/// <returns></returns>
		protected virtual IEnumerable<PaymentMethod> Create(IEnumerable<(PaymentMethodType Type, bool Available)> paymentTypes)
		{
			var index = 0;
			
			return paymentTypes
				.Select(x => PaymentMethod.Create(++index, x.Type, x.Available))
				.ToList();
		}
	}
}

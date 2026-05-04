using System.Collections.Generic;
using System.Linq;
using CustomerOrders.Contracts.V5.Carts;

namespace CustomerOrdersApi.Library.V5.Factories.DeliveryConditions
{
	/// <summary>
	/// Фабрика по созданию доступных методов оплат для ИПЗ
	/// </summary>
	public abstract class PaymentMethodFactory
	{
		private int _index = 0;

		/// <summary>
		/// Создание необходимых методов оплат
		/// </summary>
		/// <param name="paymentTypes">Список доступных методов оплат для ИПЗ</param>
		/// <returns></returns>
		protected virtual IEnumerable<PaymentMethod> Create(IEnumerable<(PaymentMethodType Type, bool Available)> paymentTypes)
		{
			_index = 0;
			
			return paymentTypes
				.Select(x => PaymentMethod.Create(++_index, x.Type, x.Available))
				.ToList();
		}
	}
}

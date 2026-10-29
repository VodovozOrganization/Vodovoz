using System.Collections.Generic;
using Vodovoz.Domain.Client;
using VodovozBusiness.Domain.Orders;

namespace VodovozBusiness.Domain.Sale
{
	public interface ISaleSource
	{
		/// <summary>
		/// Точка доставки
		/// </summary>
		DeliveryPoint DeliveryPoint { get; }
		/// <summary>
		/// Клиент
		/// </summary>
		Counterparty Counterparty { get; }
		/// <summary>
		/// Список позиций на продажу
		/// </summary>
		IEnumerable<ISaleItem> SaleItems { get; }
		/// <summary>
		/// Есть права на альтернативную цену
		/// </summary>
		bool HasPermissionsForAlternativePrice { get; }
	}
}

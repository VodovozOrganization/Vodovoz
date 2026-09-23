using System;
using System.Collections;
using System.Collections.Generic;
using Vodovoz.Domain.Client;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Orders.Delivery;

namespace VodovozBusiness.Domain.Sale
{
	public interface ISaleSource : IFreeDeliveryPrice
	{
		/// <summary>
		/// Клиент
		/// </summary>
		Counterparty Counterparty { get; }
		/// <summary>
		/// Вид оплаты
		/// </summary>
		PaymentType? PaymentType { get; }
		/// <summary>
		/// Дата доставки
		/// </summary>
		DateTime? DeliveryDate { get; }
		/// <summary>
		/// Загружен из 1С
		/// </summary>
		bool IsLoadedFrom1C { get; }
		/// <summary>
		/// Есть залоги
		/// </summary>
		bool HasDeposits { get; }
		/// <summary>
		/// Есть неоплаченные доставки
		/// </summary>
		bool HasNonPaidDeliveries { get; }
		/// <summary>
		/// Список позиций на продажу
		/// </summary>
		IList SaleItemsList { get; }
		/// <summary>
		/// Есть права на альтернативную цену
		/// </summary>
		bool HasPermissionsForAlternativePrice { get; }
	}
}

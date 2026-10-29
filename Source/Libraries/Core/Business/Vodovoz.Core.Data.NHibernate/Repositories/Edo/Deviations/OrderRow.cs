using System;

namespace Vodovoz.Core.Data.NHibernate.Repositories.Edo.Deviations
{
	/// <summary>
	/// Строка заказа, по которому создана задача ЭДО
	/// </summary>
	internal class OrderRow
	{
		/// <summary>
		/// Идентификатор заказа
		/// </summary>
		public int OrderId { get; set; }

		/// <summary>
		/// Дата доставки заказа. Пустая, если у заказа она не проставлена
		/// </summary>
		public DateTime? DeliveryDate { get; set; }
	}
}

using System;

namespace Vodovoz.Core.Data.NHibernate.Repositories.Edo.Deviations
{
	/// <summary>
	/// Строка заказа, по которому создана задача ЭДО.
	/// Дата доставки читается отдельно от заявки: заявка без заказа
	/// выпала бы из выборки вместе со своей задачей
	/// </summary>
	internal class OrderRow
	{
		/// <summary>
		/// Код заказа
		/// </summary>
		public int OrderId { get; set; }

		/// <summary>
		/// Дата доставки заказа. Пустая, если у заказа она не проставлена
		/// </summary>
		public DateTime? DeliveryDate { get; set; }
	}
}

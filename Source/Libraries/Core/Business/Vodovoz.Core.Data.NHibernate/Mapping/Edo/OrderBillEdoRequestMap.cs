using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	/// <summary>
	/// Маппинг неформализованной заявки счета из заказа
	/// </summary>
	public class OrderBillEdoRequestMap : SubclassMap<OrderBillEdoRequest>
	{
		/// <summary>
		/// Настраивает дискриминатор типа счета
		/// </summary>
		public OrderBillEdoRequestMap()
		{
			DiscriminatorValue(nameof(OrderDocumentType.Bill));
		}
	}
}

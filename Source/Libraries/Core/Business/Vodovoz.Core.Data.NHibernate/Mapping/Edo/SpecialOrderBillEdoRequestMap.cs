using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	/// <summary>
	/// Маппинг неформализованной заявки особого счета из заказа
	/// </summary>
	public class SpecialOrderBillEdoRequestMap : SubclassMap<SpecialOrderBillEdoRequest>
	{
		/// <summary>
		/// Настраивает дискриминатор типа счета
		/// </summary>
		public SpecialOrderBillEdoRequestMap()
		{
			DiscriminatorValue(nameof(OrderDocumentType.SpecialBill));
		}
	}
}

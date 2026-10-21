using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	/// <summary>
	/// Маппинг неформализованной заявки счета без отгрузки на предоплату
	/// </summary>
	public class BillWithoutShipmentForAdvancePaymentEdoRequestMap : SubclassMap<BillWithoutShipmentForAdvancePaymentEdoRequest>
	{
		/// <summary>
		/// Настраивает дискриминатор типа счета
		/// </summary>
		public BillWithoutShipmentForAdvancePaymentEdoRequestMap()
		{
			DynamicUpdate();
			DiscriminatorValue(nameof(OrderDocumentType.BillWSForAdvancePayment));
		}
	}
}

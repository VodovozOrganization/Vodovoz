using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	/// <summary>
	/// Маппинг неформализованной заявки счета без отгрузки на постоплату
	/// </summary>
	public class BillWithoutShipmentForPaymentEdoRequestMap : SubclassMap<BillWithoutShipmentForPaymentEdoRequest>
	{
		/// <summary>
		/// Настраивает дискриминатор типа счета
		/// </summary>
		public BillWithoutShipmentForPaymentEdoRequestMap()
		{
			DynamicUpdate();
			DiscriminatorValue(nameof(OrderDocumentType.BillWSForPayment));
		}
	}
}

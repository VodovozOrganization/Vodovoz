using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	/// <summary>
	/// Маппинг неформализованной заявки счета без отгрузки на долг
	/// </summary>
	public class BillWithoutShipmentForDebtEdoRequestMap : SubclassMap<BillWithoutShipmentForDebtEdoRequest>
	{
		/// <summary>
		/// Настраивает дискриминатор типа счета
		/// </summary>
		public BillWithoutShipmentForDebtEdoRequestMap()
		{
			DynamicUpdate();
			DiscriminatorValue(nameof(OrderDocumentType.BillWSForDebt));
		}
	}
}

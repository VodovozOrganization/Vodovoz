using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class DriversScannedTrueMarkCodeMap : ClassMap<DriversScannedTrueMarkCode>
	{
		public DriversScannedTrueMarkCodeMap()
		{
			Table("drivers_scanned_true_mark_codes");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.RawCode).Column("raw_code");
			Map(x => x.OrderItemId).Column("order_item_id");
			Map(x => x.IsDefective).Column("is_defective");
			Map(x => x.IsProcessingCompleted).Column("is_processing_completed");
		}
	}
}

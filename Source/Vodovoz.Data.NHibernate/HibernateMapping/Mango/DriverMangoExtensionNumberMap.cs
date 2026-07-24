using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Mango;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Mango
{
	public class DriverMangoExtensionNumberMap : ClassMap<DriverMangoExtensionNumber>
	{
		public DriverMangoExtensionNumberMap()
		{
			Table("driver_mango_extension_numbers");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.DriverId).Column("driver_id");
			Map(x => x.ExtensionNumber).Column("extension_number");
			Map(x => x.MangoUserId).Column("mango_user_id");
			Map(x => x.Status).Column("status");
			Map(x => x.ActivatedAt).Column("activated_at");
			Map(x => x.DeactivatedAt).Column("deactivated_at");
		}
	}
}

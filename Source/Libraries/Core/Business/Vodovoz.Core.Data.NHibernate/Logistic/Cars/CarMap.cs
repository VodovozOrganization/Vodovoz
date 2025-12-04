using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Logistics.Cars;
using Vodovoz.Core.Domain.Schemas.Logistics;

namespace Vodovoz.Core.Data.NHibernate.Logistic.Cars
{
	public class CarMap : ClassMap<CarEntity>
	{
		public CarMap()
		{
			Table(CarSchema.TableName);

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.RegistrationNumber).Column("reg_number");
			Map(x => x.IsUsedInDelivery).Column("is_used_in_delivery");

			HasMany(x => x.AttachedFileInformations).Cascade.AllDeleteOrphan().Inverse().KeyColumn("car_id");
		}
	}
}

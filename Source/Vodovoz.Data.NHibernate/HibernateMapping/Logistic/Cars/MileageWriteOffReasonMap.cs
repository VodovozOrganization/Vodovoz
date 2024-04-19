using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic.Cars
{
	public class MileageWriteOffReasonMap : ClassMap<MileageWriteOffReason>
	{
		public MileageWriteOffReasonMap()
		{
			Table("mileage_write_off_reason");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
			Map(x => x.Description).Column("description");
			Map(x => x.IsArchived).Column("is_archived");
		}
	}
}

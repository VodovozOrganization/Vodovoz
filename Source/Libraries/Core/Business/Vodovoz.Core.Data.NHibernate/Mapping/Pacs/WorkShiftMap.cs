using FluentNHibernate.Mapping;
using NHibernate.Type;
using Vodovoz.Core.Domain.Pacs;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Pacs
{
	public class WorkShiftMap : ClassMap<WorkShift>
	{
		public WorkShiftMap()
		{
			Table("pacs_workshifts");

			Id(x => x.Id).Column("id")
				.GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.Duration).Column("duration")
				.CustomType<TimeAsTimeSpanType>();
		}
	}
}

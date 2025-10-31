using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Common;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Common
{
	public class CalendarEntityMap : ClassMap<CalendarEntity>
	{
		public CalendarEntityMap()
		{
			Table("calendar");
			
			Id(x => x.Id).GeneratedBy.Native();
			
			Map(x => x.Date).Column("date");
		}
	}
}

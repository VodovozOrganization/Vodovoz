using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic.Cars
{
	public class CarEventTypeMap : ClassMap<CarEventType>
	{
		public CarEventTypeMap()
		{
			Table("car_event_types");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
			Map(x => x.ShortName).Column("short_name");
			Map(x => x.NeedComment).Column("need_comment");
			Map(x => x.IsArchive).Column("is_archive");
			Map(x => x.IsDoNotShowInOperation).Column("is_do_not_show_in_operation");
			Map(x => x.IsAttachWriteOffDocument).Column("is_attach_write_off_document");
			Map(x => x.AreaOfResponsibility).Column("area_of_responsibility");
		}
	}
}

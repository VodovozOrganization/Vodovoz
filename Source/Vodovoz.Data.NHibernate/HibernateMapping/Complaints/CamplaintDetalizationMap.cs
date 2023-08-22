using FluentNHibernate.Mapping;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Complaints
{
	public class CamplaintDetalizationMap : ClassMap<ComplaintDetalization>
	{
		public CamplaintDetalizationMap()
		{
			Table("complaint_detalizations");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.IsArchive).Column("is_archive");

			References(x => x.ComplaintKind).Column("complaint_kind_id");
		}
	}
}

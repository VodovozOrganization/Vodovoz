using FluentNHibernate.Mapping;
using VodovozBusiness.Domain.Complaints;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Complaints
{
	// TODO: Отключено до реализации 4963, мешает сборке
	//public class ComplaintFileInformationMap : ClassMap<ComplaintFileInformation>
	//{
	//	public ComplaintFileInformationMap()
	//	{
	//		Table("complaint_file_informations");

	//		Id(x => x.Id).Column("id").GeneratedBy.Native();

	//		Map(x => x.ComplaintId).Column("complaint_id");
	//		Map(x => x.FileName).Column("file_name");
	//	}
	//}
}

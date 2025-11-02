using FluentNHibernate.Mapping;
using VodovozBusiness.Domain.Service;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic
{
	public class ServiceDistrictCopyItemMap : ClassMap<ServiceDistrictCopyItem>
	{
		public ServiceDistrictCopyItemMap()
		{
			Table("service_district_copy_items");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			References(x => x.ServiceDistrict).Column("service_district_id");
			References(x => x.CopiedToServiceDistrict).Column("copied_to_service_district_id");
		}
	}
}

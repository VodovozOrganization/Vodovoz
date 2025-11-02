using FluentNHibernate.Mapping;
using VodovozBusiness.Domain.Cash.CashRequest;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Cash
{
	public class CashlessRequestFileInformationMap : ClassMap<CashlessRequestFileInformation>
	{
		public CashlessRequestFileInformationMap()
		{
			Table("cashless_request_file_informations");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.CashlessReqwuestId).Column("cashless_request_id");
			Map(x => x.FileName).Column("file_name");
		}
	}
}

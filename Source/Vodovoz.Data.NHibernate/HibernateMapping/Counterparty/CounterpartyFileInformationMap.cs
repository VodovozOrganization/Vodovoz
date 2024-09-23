using FluentNHibernate.Mapping;
using VodovozBusiness.Domain.Client;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Counterparty
{
	public class CounterpartyFileInformationMap : ClassMap<CounterpartyFileInformation>
	{
		public CounterpartyFileInformationMap()
		{
			Table("counterparty_file_informations");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.CounterpartyId).Column("counterparty_id");
			Map(x => x.FileName).Column("file_name");
		}
	}
}

using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;

namespace Vodovoz.HibernateMapping.Counterparty
{
    public class CounterpartyFileMap : ClassMap<CounterpartyFile>
	{
        public CounterpartyFileMap()
        {
			Table("counterparty_files");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.FileStorageId).Column("file_storage_id");
			Map(x => x.ByteFile).Column("binary_file").CustomSqlType("BinaryBlob").LazyLoad();
			References(x => x.Counterparty).Column("counterparty_id");
		}
	}
}

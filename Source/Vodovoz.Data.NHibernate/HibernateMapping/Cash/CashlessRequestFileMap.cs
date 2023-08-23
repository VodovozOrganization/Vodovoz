using FluentNHibernate.Mapping;
using Vodovoz.Domain.Cash;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Cash
{
	public class CashlessRequestFileMap : ClassMap<CashlessRequestFile>
	{
		public CashlessRequestFileMap()
		{
			Table("cashless_request_file");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.FileStorageId).Column("file_storage_id");
			Map(x => x.ByteFile).Column("binary_file").CustomSqlType("BinaryBlob").LazyLoad();
			References(x => x.CashlessRequest).Column("cashless_request_id");
		}
	}
}

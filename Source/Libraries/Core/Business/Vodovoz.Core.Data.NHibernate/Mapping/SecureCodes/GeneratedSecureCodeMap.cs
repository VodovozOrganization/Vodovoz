using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.SecureCodes;

namespace Vodovoz.Core.Data.NHibernate.Mapping.SecureCodes
{
	public class GeneratedSecureCodeMap : ClassMap<GeneratedSecureCode>
	{
		public GeneratedSecureCodeMap()
		{
			Table("generated_secure_codes");
			
			Id(x => x.Id).GeneratedBy.Native();
			
			Map(x => x.Created).Column("created");
			Map(x => x.Code).Column("code");
			Map(x => x.Method).Column("method");
			Map(x => x.Target).Column("target");
			Map(x => x.UserPhone).Column("user_phone");
			Map(x => x.Source).Column("source");
			Map(x => x.Ip).Column("ip");
			Map(x => x.UserAgent).Column("user_agent");
			Map(x => x.CounterpartyId).Column("counterparty_id");
			Map(x => x.ExternalCounterpartyId).Column("external_counterparty_id");
			Map(x => x.IsUsed).Column("is_used");
		}
	}
}

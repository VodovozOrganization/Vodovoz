using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark;

namespace Vodovoz.Core.Data.NHibernate.Mapping.TrueMark
{
	public class Tag1260CodeCheckResultMap : ClassMap<Tag1260CodeCheckResult>
	{
		public Tag1260CodeCheckResultMap()
		{
			Table("tag1260_code_check_results");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.ReqTimestamp).Column("req_time_stamp");
			Map(x => x.ReqId).Column("req_id");
		}
	}
}

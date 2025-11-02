using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Vodovoz.Core.Data.NHibernate.Mapping.TrueMark.TrueMarkProductCodes
{
	public class TrueMarkProductCodeMap : ClassMap<TrueMarkProductCode>
	{
		public TrueMarkProductCodeMap()
		{
			Table("true_mark_product_codes");

			Id(x => x.Id).Column("id")
				.GeneratedBy.Native();

			DiscriminateSubClassesOnColumn("code_owner");

			Map(x => x.SourceCodeStatus)
				.Column("source_code_status");

			References(x => x.SourceCode)
				.Column("source_code_id");

			References(x => x.ResultCode)
				.Column("result_code_id");

			Map(x => x.Problem)
				.Column("problem");

			Map(x => x.DuplicatesCount)
				.Column("duplicates_count");

			References(x => x.CustomerEdoRequest)
				.Column("customer_request_id")
				.Cascade.AllDeleteOrphan();
		}
	}
}

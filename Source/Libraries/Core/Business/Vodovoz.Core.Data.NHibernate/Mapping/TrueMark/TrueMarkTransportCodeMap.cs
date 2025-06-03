using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.TrueMark;

namespace Vodovoz.Core.Data.NHibernate.Mapping.TrueMark
{
	public class TrueMarkTransportCodeMap : ClassMap<TrueMarkTransportCode>
	{
		public TrueMarkTransportCodeMap()
		{
			Table("true_mark_transport_code");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.ParentTransportCodeId).Column("parent_transport_code_id");

			Map(x => x.RawCode).Column("raw_code");
			Map(x => x.IsInvalid).Column("is_invalid");

			HasMany(x => x.InnerTransportCodes)
				.KeyColumn("parent_transport_code_id")
				.Not.LazyLoad()
				.Fetch.Subselect()
				.Inverse()
				.Cascade.All();

			HasMany(x => x.InnerGroupCodes)
				.KeyColumn("parent_transport_code_id")
				.Not.LazyLoad()
				.Fetch.Subselect()
				.Inverse()
				.Cascade.All();

			HasMany(x => x.InnerWaterCodes)
				.KeyColumn("parent_transport_code_id")
				.Not.LazyLoad()
				.Fetch.Subselect()
				.Inverse()
				.Cascade.All();
		}
	}
}

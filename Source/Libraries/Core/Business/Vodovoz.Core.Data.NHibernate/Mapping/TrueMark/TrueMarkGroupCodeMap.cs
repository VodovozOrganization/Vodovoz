using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.TrueMark;

namespace Vodovoz.Core.Data.NHibernate.Mapping.TrueMark
{
	public class TrueMarkGroupCodeMap : ClassMap<TrueMarkGroupCode>
	{
		public TrueMarkGroupCodeMap()
		{
			Table("true_mark_group_code");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.ParentTransportCodeId).Column("parent_transport_code_id");
			Map(x => x.ParentGroupCodeId).Column("parent_group_code_id");

			Map(x => x.RawCode).Column("raw_code");
			Map(x => x.IsInvalid).Column("is_invalid");
			Map(x => x.GTIN).Column("gtin");
            Map(x => x.SerialNumber).Column("serial_number");
            Map(x => x.CheckCode).Column("check_code");
            Map(x => x.ProductGroup).Column("product_group");

            HasMany(x => x.InnerGroupCodes)
				.KeyColumn("parent_group_code_id")
				.Not.LazyLoad()
				.Fetch.Subselect()
				.Inverse()
				.Cascade.All()
				;

			HasMany(x => x.InnerIdentificationCodes)
				.KeyColumn("parent_group_code_id")
				.Not.LazyLoad()
				.Fetch.Subselect()
				.Inverse()
				.Cascade.All()
				;
		}
	}
}

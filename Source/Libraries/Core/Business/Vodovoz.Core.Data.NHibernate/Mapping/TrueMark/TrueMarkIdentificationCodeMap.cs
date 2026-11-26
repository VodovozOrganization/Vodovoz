using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark;

namespace Vodovoz.Core.Data.NHibernate.Mapping.TrueMark
{
	public class TrueMarkIdentificationCodeMap : ClassMap<TrueMarkIdentificationCode>
	{
		public TrueMarkIdentificationCodeMap()
		{
			Table("true_mark_identification_code");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id).Column("id")
				.GeneratedBy.Native();

			Map(x => x.ParentTransportCodeId)
				.Column("parent_transport_code_id");

			Map(x => x.ParentGroupCodeId)
				.Column("parent_group_code_id");

			Map(x => x.RawCode)
				.Column("raw_code");

			Map(x => x.IsInvalid)
				.Column("is_invalid");

			Map(x => x.Gtin)
				.Column("gtin");

			Map(x => x.SerialNumber)
				.Column("serial_number");

			Map(x => x.CheckCode)
				.Column("check_code");

			Map(x => x.ProductGroup)
				.Column("product_group");
		}
	}
}

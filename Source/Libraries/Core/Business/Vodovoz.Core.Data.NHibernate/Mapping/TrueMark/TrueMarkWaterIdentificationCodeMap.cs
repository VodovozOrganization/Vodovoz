using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.TrueMark
{
	public class TrueMarkWaterIdentificationCodeMap : ClassMap<TrueMarkWaterIdentificationCode>
	{
		public TrueMarkWaterIdentificationCodeMap()
		{
			Table("true_mark_identification_code");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id).Column("id")
				.GeneratedBy.Native();

			Map(x => x.ParentTransportCodeId)
				.Column("parent_transport_code_id");

			Map(x => x.ParentWaterGroupCodeId)
				.Column("parent_water_group_code_id");

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

			Map(x => x.IsTag1260Valid)
				.Column("is_tag1260_valid");

			References(x => x.Tag1260CodeCheckResult)
				.Column("tag1260_code_check_result_id")
				.Fetch.Join()
				.Not.LazyLoad();
		}
	}
}

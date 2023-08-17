using FluentNHibernate.Mapping;
using Vodovoz.Domain.TrueMark;

namespace Vodovoz.HibernateMapping.Suppliers
{
	public class TrueMarkWaterIdentificationCodeMap : ClassMap<TrueMarkWaterIdentificationCode>
	{
		public TrueMarkWaterIdentificationCodeMap()
		{
			Table("true_mark_identification_code");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.RawCode).Column("raw_code");
			Map(x => x.IsInvalid).Column("is_invalid");
			Map(x => x.GTIN).Column("gtin");
			Map(x => x.SerialNumber).Column("serial_number");
			Map(x => x.CheckCode).Column("check_code");
		}
	}
}

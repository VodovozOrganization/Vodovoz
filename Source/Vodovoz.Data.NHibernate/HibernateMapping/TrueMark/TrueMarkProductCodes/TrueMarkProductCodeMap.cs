using FluentNHibernate.Mapping;
using VodovozBusiness.Domain.TrueMark.TrueMarkProductCodes;

namespace Vodovoz.Data.NHibernate.HibernateMapping.TrueMark.TrueMarkProductCodes
{
	public class TrueMarkProductCodeMap : ClassMap<TrueMarkProductCode>
	{
		public TrueMarkProductCodeMap()
		{
			Table("true_mark_product_codes");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			DiscriminateSubClassesOnColumn("code_owner");

			References(x => x.TrueMarkCode).Column("true_mark_code_id").Cascade.AllDeleteOrphan();
		}
	}
}

using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;

namespace Vodovoz.HibernateMapping
{
	public class WaterSalesAgreementFixedPriceMap : ClassMap<WaterSalesAgreementFixedPrice>
	{
		public WaterSalesAgreementFixedPriceMap ()
		{
			Table ("additional_agreement_water_fixed_price");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Price).Column ("price");
			References (x => x.Nomenclature).Column ("nomenclature_id");
			References (x => x.AdditionalAgreement).Column ("agreement_id");
		}
	}
}
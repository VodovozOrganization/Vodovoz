using FluentNHibernate.Mapping;
using VodovozBusiness.Domain.TrueMark.TrueMarkProductCodes;

namespace Vodovoz.Data.NHibernate.HibernateMapping.TrueMark.TrueMarkProductCodes
{
	public class SelfDeliveryDocumentItemTrueMarkProductCodeMap : SubclassMap<SelfDeliveryDocumentItemTrueMarkProductCode>
	{
		public SelfDeliveryDocumentItemTrueMarkProductCodeMap()
		{
			DiscriminatorValue("SelfDeliveryDocumentItem");

			References(x => x.SelfDeliveryDocumentItem).Column("self_delivery_document_item_id");
		}
	}
}

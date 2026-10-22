using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Vodovoz.Core.Data.NHibernate.Mapping.TrueMark.TrueMarkProductCodes
{
	public class SelfDeliveryDocumentItemTrueMarkProductCodeMap : SubclassMap<SelfDeliveryDocumentItemTrueMarkProductCode>
	{
		public SelfDeliveryDocumentItemTrueMarkProductCodeMap()
		{
			DiscriminatorValue("SelfDeliveryDocumentItem");

			Map(x => x.SelfDeliveryDocumentItemId)
				.Column("self_delivery_document_item_id");
		}
	}
}

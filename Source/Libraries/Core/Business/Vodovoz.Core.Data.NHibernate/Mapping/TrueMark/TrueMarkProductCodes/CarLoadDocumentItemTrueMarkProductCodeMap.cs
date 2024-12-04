using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Vodovoz.Core.Data.NHibernate.Mapping.TrueMark.TrueMarkProductCodes
{
	public class CarLoadDocumentItemTrueMarkProductCodeMap : SubclassMap<CarLoadDocumentItemTrueMarkProductCode>
	{
		public CarLoadDocumentItemTrueMarkProductCodeMap()
		{
			DiscriminatorValue("CarLoadDocumentItem");

			References(x => x.CarLoadDocumentItem).Column("car_load_document_item_id");
		}
	}
}

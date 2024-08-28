using FluentNHibernate.Mapping;
using VodovozBusiness.Domain.TrueMark.TrueMarkProductCodes;

namespace Vodovoz.Data.NHibernate.HibernateMapping.TrueMark.TrueMarkProductCodes
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

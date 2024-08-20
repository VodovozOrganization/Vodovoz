using FluentNHibernate.Mapping;
using VodovozBusiness.Domain.TrueMark;

namespace Vodovoz.Data.NHibernate.HibernateMapping.TrueMark
{
	public class CarLoadDocumentItemProductCodeMap : ClassMap<CarLoadDocumentItemProductCode>
	{
		public CarLoadDocumentItemProductCodeMap()
		{
			Table("store_car_load_document_item_product_codes");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.SequenceNumber).Column("sequence_number");
			Map(x => x.NomenclatureId).Column("nomenclature_id");

			References(x => x.CarLoadDocumentItem).Column("car_load_document_item_id");
			References(x => x.TrueMarkCode).Column("true_mark_code_id");
		}
	}
}

using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Store;

namespace Vodovoz.HibernateMapping
{
	public class ProductSpecificationMaterialMap : ClassMap<ProductSpecificationMaterial>
	{
		public ProductSpecificationMaterialMap ()
		{
			Table ("specification_production_materials");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map(x => x.Amount).Column("amount");
			References (x => x.ProductSpec).Column("specification_production_id").Not.Nullable();
			References (x => x.Material).Column ("nomenclature_id").Not.Nullable ();
		}
	}
}
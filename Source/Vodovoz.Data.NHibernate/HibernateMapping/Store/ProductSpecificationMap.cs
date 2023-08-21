﻿using FluentNHibernate.Mapping;
using Vodovoz.Domain.Store;

namespace Vodovoz.HibernateMapping
{
	public class ProductSpecificationMap : ClassMap<ProductSpecification>
	{
		public ProductSpecificationMap ()
		{
			Table ("specification_production");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Name).Column ("name");
			References(x => x.Product).Column("nomenclature_id");
			HasMany (x => x.Materials).Cascade.AllDeleteOrphan ().Inverse().KeyColumn ("specification_production_id");
		}
	}
}
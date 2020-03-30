using System;
using SolrSearch;
using SolrSearch.Mapping;
using Vodovoz.Domain.Client;
using Vodovoz.SolrModel;

namespace Vodovoz.SolrMapping
{
	public class DeliveryPointSolrMap : SolrOrmSourceClassMap<DeliveryPointSolrEntity, DeliveryPoint, DeliveryPointSolrEntityFactory>
	{
		public DeliveryPointSolrMap()
		{
			Map(se => se.Id, e => e.Id, 999999);
			Map(se => se.CompiledAddress, e => e.CompiledAddress, 2);
		}
	}

	public class DeliveryPointSolrEntityFactory : SolrEntityFactoryBase<DeliveryPointSolrEntity>
	{
		public override DeliveryPointSolrEntity CreateEntity(EntityContentProvider entityContentProvider)
		{
			DeliveryPointSolrEntity entity = new DeliveryPointSolrEntity();
			entity.Id = entityContentProvider.GetPropertyContent<int>(nameof(entity.Id));
			entity.CompiledAddress = entityContentProvider.GetPropertyContent<string>(nameof(entity.CompiledAddress));
			return entity;
		}
	}
}

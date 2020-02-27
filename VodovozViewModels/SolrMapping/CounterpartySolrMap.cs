using SolrSearch.Mapping;
using Vodovoz.SolrModel;
using Vodovoz.Domain.Client;
using SolrSearch;

namespace Vodovoz.SolrMapping
{
	public class CounterpartySolrMap : SolrOrmSourceClassMap<CounterpartySolrEntity, Counterparty, CounterpartySolrEntityFactory>
	{
		public CounterpartySolrMap()
		{
			Map(se => se.Id, e => e.Id);
			Map(se => se.Name, e => e.Name);
			Map(se => se.FullName, e => e.FullName);
			Map(se => se.Inn, e => e.INN);
		}
	}

	public class CounterpartySolrEntityFactory : SolrEntityFactoryBase<CounterpartySolrEntity>
	{
		public override CounterpartySolrEntity CreateEntity(EntityContentProvider entityContentProvider)
		{
			CounterpartySolrEntity entity = new CounterpartySolrEntity();
			entity.Id = entityContentProvider.GetPropertyContent<int>(nameof(entity.Id));
			entity.Name = entityContentProvider.GetPropertyContent<string>(nameof(entity.Name));
			entity.FullName = entityContentProvider.GetPropertyContent<string>(nameof(entity.FullName));
			entity.Inn = entityContentProvider.GetPropertyContent<string>(nameof(entity.Inn));
			return entity;
		}
	}
}

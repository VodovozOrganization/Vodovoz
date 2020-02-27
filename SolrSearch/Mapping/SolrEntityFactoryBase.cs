using SolrSearch;

namespace SolrSearch.Mapping
{
	public abstract class SolrEntityFactoryBase
	{
		internal abstract SolrEntityBase CreateEntityBase(EntityContentProvider entityContentProvider);
	}

	public abstract class SolrEntityFactoryBase<TSolrEntity> : SolrEntityFactoryBase
		where TSolrEntity : SolrEntityBase
	{
		public abstract TSolrEntity CreateEntity(EntityContentProvider entityContentProvider);

		internal override SolrEntityBase CreateEntityBase(EntityContentProvider entityContentProvider)
		{
			SolrEntityBase solrEntity = CreateEntity(entityContentProvider);
			solrEntity.SolrId = entityContentProvider.GetSolrId();
			solrEntity.SolrEntityType = entityContentProvider.GetSolrEntityType();
			return solrEntity;
		}
	}
}

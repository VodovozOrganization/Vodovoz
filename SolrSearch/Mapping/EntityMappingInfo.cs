using System;
namespace SolrSearch.Mapping
{
	internal class EntityMappingInfo
	{
		public Type SolrType { get; }
		public Type OrmType { get; }
		public string TableName { get; }
		public SolrEntityFactoryBase SolrEntityFactory { get; }

		public EntityMappingInfo(Type solrType, Type ormType, string tableName, SolrEntityFactoryBase solrEntityFactory)
		{
			SolrType = solrType ?? throw new ArgumentNullException(nameof(solrType));
			OrmType = ormType ?? throw new ArgumentNullException(nameof(ormType));
			TableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
			SolrEntityFactory = solrEntityFactory ?? throw new ArgumentNullException(nameof(solrEntityFactory));
		}
	}
}

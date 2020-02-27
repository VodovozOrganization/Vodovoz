using System;
using System.Linq.Expressions;

namespace SolrSearch.Mapping
{
	public abstract class SolrOrmSourceClassMap<TSolrEntity, TOrmEntity, TSolrEntityFactory> : SolrOrmSourceClassMap
		where TSolrEntity : SolrEntityBase
		where TOrmEntity : class
		where TSolrEntityFactory : SolrEntityFactoryBase<TSolrEntity>
	{
		protected SolrOrmSourceClassMap() : base(typeof(TSolrEntity), typeof(TOrmEntity), typeof(TSolrEntityFactory))
		{
		}

		protected void Map(Expression<Func<TSolrEntity, object>> solrEntityPropertySelector, string fieldName, float? boost = null)
		{
			if(solrEntityPropertySelector == null) {
				throw new ArgumentNullException(nameof(solrEntityPropertySelector));
			}

			if(string.IsNullOrWhiteSpace(fieldName)) {
				throw new ArgumentException(nameof(fieldName));
			}

			string propertyName = Utils.GetPropertyName(solrEntityPropertySelector);

			Map(propertyName, fieldName, boost);
		}

		protected void Map(Expression<Func<TSolrEntity, object>> solrEntityPropertySelector, Expression<Func<TOrmEntity, object>> ormEntityPropertySelector, float? boost = null)
		{
			if(solrEntityPropertySelector == null) {
				throw new ArgumentNullException(nameof(solrEntityPropertySelector));
			}

			if(ormEntityPropertySelector == null) {
				throw new ArgumentNullException(nameof(ormEntityPropertySelector));
			}

			string propertyName = Utils.GetPropertyName(solrEntityPropertySelector);
			string ormPropertyName = Utils.GetPropertyName(ormEntityPropertySelector);

			Map(propertyName, (ormMappingProvider) => ormMappingProvider.GetMappedColumnName(ormEntityPropertySelector), ormPropertyName, boost);
		}
	}
}

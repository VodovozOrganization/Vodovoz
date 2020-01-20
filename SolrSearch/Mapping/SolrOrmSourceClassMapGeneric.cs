using System;
using System.Linq.Expressions;

namespace SolrSearch.Mapping
{
	public abstract class SolrOrmSourceClassMap<TSolrEntity, TOrmEntity> : SolrOrmSourceClassMap
		where TSolrEntity : SolrEntityBase
		where TOrmEntity : class
	{
		protected SolrOrmSourceClassMap() : base(typeof(TSolrEntity), typeof(TOrmEntity))
		{
		}

		protected void Map(Expression<Func<TSolrEntity, object>> propertySelector, string fieldName, float? boost = null)
		{
			if(propertySelector == null) {
				throw new ArgumentNullException(nameof(propertySelector));
			}

			if(string.IsNullOrWhiteSpace(fieldName)) {
				throw new ArgumentException(nameof(fieldName));
			}

			string propertyName = Utils.GetPropertyName(propertySelector);

			Map(propertyName, fieldName, boost);
		}

		protected void Map(Expression<Func<TSolrEntity, object>> propertySelector, Expression<Func<TOrmEntity, object>> ormEntityPropertySelector, float? boost = null)
		{
			if(propertySelector == null) {
				throw new ArgumentNullException(nameof(propertySelector));
			}

			if(ormEntityPropertySelector == null) {
				throw new ArgumentNullException(nameof(ormEntityPropertySelector));
			}

			string propertyName = Utils.GetPropertyName(propertySelector);

			Map(propertyName, (ormMappingProvider) => ormMappingProvider.GetMappedColumnName(ormEntityPropertySelector), boost);
		}
	}
}

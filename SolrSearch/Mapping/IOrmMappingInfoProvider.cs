using System;
using System.Linq.Expressions;

namespace SolrSearch.Mapping
{
	public interface IOrmMappingInfoProvider
	{
		string GetMappedTableName<TEntity>() where TEntity : class;
		string GetMappedTableName(Type entityType);
		string GetMappedColumnName<TEntity>(Expression<Func<TEntity, object>> propertySelector) where TEntity : class;
		string GetMappedColumnName<TEntity>(string propertyName) where TEntity : class;
	}
}


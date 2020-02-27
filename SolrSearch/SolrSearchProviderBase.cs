using System;
using SolrNet;
using SolrNet.Impl;
using System.Linq.Expressions;

namespace SolrSearch
{
	/*
	public abstract class SolrSearchProviderBase
	{
		private readonly ISolrConnection connection;
		private readonly IReadOnlyMappingManager mappingManager;

		public SolrSearchProviderBase(string connectionString, IReadOnlyMappingManager mappingManager)
		{
			if(string.IsNullOrWhiteSpace(connectionString)) {
				throw new ArgumentNullException(nameof(connectionString));
			}
			this.mappingManager = mappingManager ?? throw new ArgumentNullException(nameof(mappingManager));

			connection = new SolrConnection(connectionString);
		}

		protected SolrSearchProviderBase(ISolrConnection connection, IReadOnlyMappingManager mappingManager)
		{
			this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
			this.mappingManager = mappingManager ?? throw new ArgumentNullException(nameof(mappingManager));
		}

		protected virtual void InitSolrEntity<TSolrEntity>()
			where TSolrEntity : SolrEntityBase
		{
			Startup.Init<TSolrEntity>(connection);
		}

		public virtual string GetSolrField<TEntity>(Expression<Func<TEntity, object>> propertySelector)
			where TEntity : class
		{
			return GetSolrField<TEntity>(Utils.GetPropertyName(propertySelector));
		}

		public virtual string GetSolrField<TEntity>(string propertyName)
			where TEntity : class
		{
			return GetSolrField(typeof(TEntity), propertyName);
		}

		public virtual string GetSolrField(Type entityType, string propertyName)
		{
			if(typeof(SolrEntityBase).IsAssignableFrom(entityType)) {
				return GetSolrFieldName(entityType, propertyName);
			}
			return GetSolrFieldNameForOrmEntity(entityType, propertyName);
		}

		private string GetSolrFieldName(Type solrEntityType, string propertyName)
		{

		}

		private string GetSolrFieldNameForOrmEntity(Type ormEntityType, string ormPropertyName)
		{
			Type solrEntityType = xxx(ormEntityType, ormPropertyName);
			string solrPropertyName = xxx(ormEntityType, ormPropertyName);

			return GetSolrFieldName(solrEntityType, solrPropertyName);
		}
	}
	*/
}

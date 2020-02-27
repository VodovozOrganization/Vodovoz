using System;
using System.Collections.Generic;
using SolrSearch.Mapping;
using System.Linq;

namespace SolrSearch
{

	/*
	public class SolrEntityFactory
	{
		private readonly SolrOrmSourceMapping solrOrmSourceMapping;

		public SolrEntityFactory(SolrOrmSourceMapping solrOrmSourceMapping)
		{
			this.solrOrmSourceMapping = solrOrmSourceMapping ?? throw new ArgumentNullException(nameof(solrOrmSourceMapping));
		}

		Dictionary<string, Func<Dictionary<string, object>, SolrEntityBase>> createInstanceFunctions = new Dictionary<string, Func<Dictionary<string, object>, SolrEntityBase>>();

		public IEnumerable<SolrEntityBase> Create(IEnumerable<Dictionary<string, object>> entitiesContent)
		{
			if(entitiesContent == null) {
				throw new ArgumentNullException(nameof(entitiesContent));
			}

			List<SolrEntityBase> result = new List<SolrEntityBase>();

			foreach(var content in entitiesContent) {
				if(!content.TryGetValue(solrOrmSourceMapping.EntityTypeFieldName, out object entityTypeNameObject)) {
					throw new InvalidOperationException("Невозможно определить тип Solr сущности");
				}
				string entityTypeName = (string)entityTypeNameObject;
				Type solrEntityType = solrOrmSourceMapping.GetSolrEntityType(entityTypeName);

				if(createInstanceFunctions.ContainsKey(entityTypeName)) {
					result.Add(createInstanceFunctions[entityTypeName](content));
				} else {
					var properties = solrEntityType.GetProperties().Where(x => x.CanWrite);
					Dictionary<string, Func<Dictionary<string, object>, object>> propertyDataFunctions = new Dictionary<string, Func<Dictionary<string, object>, object>>();
					foreach(var property in properties) {
						string fieldName = solrOrmSourceMapping.GetSolrField(solrEntityType, property.Name);
						propertyDataFunctions.Add(property.Name, (entityContent) => {
							return entityContent[fieldName];
						});
					}

					Func<Dictionary<string, object>, SolrEntityBase> createInstanceFunc = (entityContent) => {
						SolrEntityBase instance = (SolrEntityBase)Activator.CreateInstance(solrEntityType);
						foreach(var property in properties) {
							property.SetValue(instance, propertyDataFunctions[property.Name](entityContent));
						}
						return instance;
					};
					createInstanceFunctions.Add(entityTypeName, createInstanceFunc);
				}
			}
			return result;
		}
	}
	*/
}

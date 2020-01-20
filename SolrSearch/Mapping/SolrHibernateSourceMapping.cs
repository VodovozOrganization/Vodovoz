using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SolrNet;

namespace SolrSearch.Mapping
{
	public class SolrOrmSourceMapping : IReadOnlyMappingManager
	{
		public string KeyFieldName { get; }
		public string EntityTypeFieldName { get; }

		internal IOrmMappingInfoProvider OrmMappingInfoProvider;
		private readonly Assembly[] assemblies;

		public SolrOrmSourceMapping(
			IOrmMappingInfoProvider ormMappingInfoProvider, 
			string keyFieldName = "solr_id", 
			string entityTypeFieldName = "solr_entity_type", 
			params Assembly[] assemblies)
		{
			this.OrmMappingInfoProvider = ormMappingInfoProvider ?? throw new ArgumentNullException(nameof(ormMappingInfoProvider));
			KeyFieldName = keyFieldName;
			EntityTypeFieldName = entityTypeFieldName;
			this.assemblies = assemblies ?? throw new ArgumentNullException(nameof(assemblies));
			UpdateMapConfig();
		}

		private Dictionary<Type, Dictionary<string, SolrFieldModel>> entitiesConfigs = new Dictionary<Type, Dictionary<string, SolrFieldModel>>();

		private IEnumerable<Type> GetSuitableTypes()
		{
			Type mappingClassType = typeof(SolrOrmSourceClassMap);
			List<Type> mappingClasses = new List<Type>();
			foreach(var assembly in assemblies) {
				var suitableClasses = assembly.GetTypes().Where(x => x.IsClass).Where(mappingClassType.IsAssignableFrom);
				mappingClasses.AddRange(suitableClasses);
			}
			return mappingClasses;
		}

		private void UpdateMapConfig()
		{
			var suitableMappingClasses = GetSuitableTypes();
			foreach(var type in GetSuitableTypes()) {
				SolrOrmSourceClassMap solrClassMap = (SolrOrmSourceClassMap)Activator.CreateInstance(type);
				var propertyConfig = new Dictionary<string, SolrFieldModel>();

				//SolrId
				string solrIdPropertyName = Utils.GetPropertyName<SolrEntityBase>(x => x.SolrId);
				var solrIdFieldModel = new SolrFieldModel(solrClassMap.SolrEntityType.GetProperty(solrIdPropertyName), KeyFieldName);
				propertyConfig.Add(solrIdPropertyName, solrIdFieldModel);

				//SolrEntityType
				string solrEntityTypePropertyName = Utils.GetPropertyName<SolrEntityBase>(x => x.SolrEntityType);
				var solrEntityTypeFieldModel = new SolrFieldModel(solrClassMap.SolrEntityType.GetProperty(solrEntityTypePropertyName), EntityTypeFieldName);
				propertyConfig.Add(solrEntityTypePropertyName, solrEntityTypeFieldModel);

				foreach(var mapInfo in solrClassMap.FieldsMapping.Values) {
					PropertyInfo propertyInfo = solrClassMap.SolrEntityType.GetProperty(mapInfo.PropertyName);
					SolrFieldModel fielModel = new SolrFieldModel(propertyInfo, mapInfo.FieldNameFunc(OrmMappingInfoProvider), mapInfo.Boost);
					propertyConfig.Add(mapInfo.PropertyName, fielModel);
				}
				entitiesConfigs.Add(solrClassMap.SolrEntityType, propertyConfig);
			}
		}

		#region IReadOnlyMappingManager implementation

		public IDictionary<string, SolrFieldModel> GetFields(Type type)
		{
			if(!entitiesConfigs.ContainsKey(type)) {
				throw new InvalidOperationException($"Не настроен маппинг для {type.FullName}");
			}
			return entitiesConfigs[type];
		}

		public ICollection<Type> GetRegisteredTypes()
		{
			return entitiesConfigs.Keys;
		}

		public SolrFieldModel GetUniqueKey(Type type)
		{
			var fields = GetFields(type);
			if(!fields.ContainsKey(KeyFieldName)) {
				throw new InvalidOperationException($"Для сущности {type.FullName}, не настроен уникальный ключ в маппинге");
			}
			return fields[KeyFieldName];
		}

		#endregion IReadOnlyMappingManager implementation
	}
}

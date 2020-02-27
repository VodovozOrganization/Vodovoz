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

		private List<EntityMappingInfo> entityMappings = new List<EntityMappingInfo>();
		private Dictionary<Type, Dictionary<string, SolrFieldModel>> ormEntityMappings = new Dictionary<Type, Dictionary<string, SolrFieldModel>>();
		private Dictionary<Type, Dictionary<string, SolrFieldModel>> solrEntityMappings = new Dictionary<Type, Dictionary<string, SolrFieldModel>>();
		private Dictionary<Type, Dictionary<string, string>> solrFieldToPropertyMappings = new Dictionary<Type, Dictionary<string, string>>();

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

		private void UpdateMapConfig()
		{
			var suitableMappingClasses = GetSuitableTypes();
			foreach(var type in GetSuitableTypes()) {
				SolrOrmSourceClassMap solrClassMap = (SolrOrmSourceClassMap)Activator.CreateInstance(type);
				CreateEntityTableMapping(solrClassMap);
				CreateFieldModels(solrClassMap);
				CreateOrmPropertiesMappingInfo(solrClassMap);
			}
		}

		private void CreateEntityTableMapping(SolrOrmSourceClassMap solrClassMap)
		{
			string tableName = OrmMappingInfoProvider.GetMappedTableName(solrClassMap.OrmEntityType);
			if(!entityMappings.Any(x => x.TableName == tableName)) {
				SolrEntityFactoryBase solrEntityFactory = (SolrEntityFactoryBase)Activator.CreateInstance(solrClassMap.SolrEntityFactoryType);
				entityMappings.Add(new EntityMappingInfo(solrClassMap.SolrEntityType, solrClassMap.OrmEntityType, tableName, solrEntityFactory));
			}
		}

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

		private void CreateFieldModels(SolrOrmSourceClassMap solrClassMap)
		{
			var solrFieldModels = new Dictionary<string, SolrFieldModel>();
			var solrFieldToPropertyDictionary = new Dictionary<string, string>();

			//SolrId
			string solrIdPropertyName = Utils.GetPropertyName<SolrEntityBase>(x => x.SolrId);
			var solrIdFieldModel = new SolrFieldModel(solrClassMap.SolrEntityType.GetProperty(solrIdPropertyName), KeyFieldName);
			solrFieldModels.Add(solrIdPropertyName, solrIdFieldModel);
			solrFieldToPropertyDictionary.Add(KeyFieldName, solrIdPropertyName);

			//SolrEntityType
			string solrEntityTypePropertyName = Utils.GetPropertyName<SolrEntityBase>(x => x.SolrEntityType);
			var solrEntityTypeFieldModel = new SolrFieldModel(solrClassMap.SolrEntityType.GetProperty(solrEntityTypePropertyName), EntityTypeFieldName);
			solrFieldModels.Add(solrEntityTypePropertyName, solrEntityTypeFieldModel);
			solrFieldToPropertyDictionary.Add(EntityTypeFieldName, solrEntityTypePropertyName);

			foreach(var mapInfo in solrClassMap.PropertyMapping.Values) {
				PropertyInfo solrPropertyInfo = solrClassMap.SolrEntityType.GetProperty(mapInfo.SolrProperty);
				string fieldName = mapInfo.SolrFieldNameFunc(OrmMappingInfoProvider);
				SolrFieldModel fielModel = new SolrFieldModel(solrPropertyInfo, fieldName, mapInfo.Boost);
				solrFieldModels.Add(mapInfo.SolrProperty, fielModel);
				solrFieldToPropertyDictionary.Add(fieldName, mapInfo.SolrProperty);
			}
			solrEntityMappings.Add(solrClassMap.SolrEntityType, solrFieldModels);
			solrFieldToPropertyMappings.Add(solrClassMap.SolrEntityType, solrFieldToPropertyDictionary);
		}

		/// <summary>
		/// Формирует информацию о свойствах ORM сущности, по которой добавлен маппинг для Solr сущности, 
		/// для возможности дальнейшего нахождения информации о Solr маппинге по любому свойсвту ORM сущности
		/// </summary>
		private void CreateOrmPropertiesMappingInfo(SolrOrmSourceClassMap solrClassMap)
		{
			Dictionary<string, SolrFieldModel> ormEntityPropertyMappings = new Dictionary<string, SolrFieldModel>();

			foreach(var mapInfo in solrClassMap.PropertyMapping.Values) {
				if(string.IsNullOrWhiteSpace(mapInfo.OrmProperty)) {
					continue;
				}
				PropertyInfo solrPropertyInfo = solrClassMap.SolrEntityType.GetProperty(mapInfo.SolrProperty);
				SolrFieldModel fieldModel = new SolrFieldModel(solrPropertyInfo, mapInfo.SolrFieldNameFunc(OrmMappingInfoProvider), mapInfo.Boost);
				if(ormEntityPropertyMappings.ContainsKey(mapInfo.OrmProperty)) {
					ormEntityPropertyMappings[mapInfo.OrmProperty] = fieldModel;
				}
				ormEntityPropertyMappings.Add(mapInfo.OrmProperty, fieldModel);
			}

			if(ormEntityMappings.ContainsKey(solrClassMap.OrmEntityType)) {
				ormEntityMappings[solrClassMap.OrmEntityType] = ormEntityPropertyMappings;
			}
			ormEntityMappings.Add(solrClassMap.OrmEntityType, ormEntityPropertyMappings);
		}

		public Type GetOrmEntityType(Type solrEntityType)
		{
			if(solrEntityType == null) {
				throw new ArgumentNullException(nameof(solrEntityType));
			}

			EntityMappingInfo entityMappingInfo = entityMappings.FirstOrDefault(x => x.SolrType == solrEntityType);
			if(entityMappingInfo == null) {
				throw new InvalidOperationException($"Для типа {solrEntityType.FullName} не настроен маппинг");
			}
			return entityMappingInfo.OrmType;
		}

		public SolrEntityFactoryBase GetSolrEntityFactory(Type solrType)
		{
			if(solrType == null) {
				throw new ArgumentNullException(nameof(solrType));
			}

			SolrEntityFactoryBase solrEntityFactory = entityMappings.Where(x => x.SolrType == solrType).Select(x => x.SolrEntityFactory).FirstOrDefault();

			if(solrEntityFactory == null) {
				throw new InvalidOperationException($"Не настроена фабрика для сущности {solrType.FullName}");
			}

			return solrEntityFactory;
		}

		public Type GetSolrEntityType(string solrEntityTypeName)
		{
			var entityTableInfo = entityMappings.FirstOrDefault(x => x.TableName == solrEntityTypeName);
			if(entityTableInfo == null) {
				throw new InvalidOperationException($"Не настроен маппинг для таблицы {solrEntityTypeName}");
			}
			return entityTableInfo.SolrType;
		}

		public string GetSolrEntityType<TEntity>()
			where TEntity : class
		{
			return GetSolrEntityType(typeof(TEntity));
		}

		public string GetSolrEntityType(Type entityType)
		{
			var entityTableInfo = entityMappings.FirstOrDefault(x => x.OrmType == entityType || x.SolrType == entityType);
			if(entityTableInfo == null) {
				throw new InvalidOperationException($"Не настроен маппинг для сущности {entityType.FullName}");
			}
			return entityTableInfo.TableName;
		}

		/// <summary>
		/// Получение имени Solr поля которое соответствует настроенной в маппинге Solr сущности
		/// </summary>
		/// <returns>Имя Solr поля</returns>
		/// <typeparam name="TEntity">Сущность, может быть как Solr сущность, 
		/// так и Orm сущность которая была использована в маппинге для Solr</typeparam>
		public string GetSolrField<TEntity>(string propertyName)
		{
			return GetSolrField(typeof(TEntity), propertyName);
		}

		/// <summary>
		/// Получение имени Solr поля которое соответствует настроенной в маппинге Solr сущности
		/// </summary>
		/// <returns>Имя Solr поля</returns>
		/// <param name="entityType">Сущность, может быть как Solr сущность, 
		/// так и Orm сущность которая была использована в маппинге для Solr</param>
		/// <param name="propertyName">Property name.</param>
		public string GetSolrField(Type entityType, string propertyName)
		{
			if(solrEntityMappings.ContainsKey(entityType)) {
				var fieldsMappings = solrEntityMappings[entityType];
				if(!fieldsMappings.ContainsKey(propertyName)) {
					throw new InvalidOperationException($"Для свойства {propertyName} не настроен Solr маппинг");
				}
				return fieldsMappings[propertyName].FieldName;
			}
			if(ormEntityMappings.ContainsKey(entityType)) {
				var fieldsMappings = ormEntityMappings[entityType];
				if(!fieldsMappings.ContainsKey(propertyName)) {
					throw new InvalidOperationException($"Для свойства {propertyName} не настроен Solr маппинг");
				}
				return fieldsMappings[propertyName].FieldName;
			}

			throw new InvalidOperationException($"Для типа {entityType.FullName} не настроен Solr маппинг");
		}

		public string GetSolrPropertyByFieldName(Type type, string fieldName)
		{
			return solrFieldToPropertyMappings[type][fieldName];
		}

		public Type GetSolrFieldType(Type entityType, string propertyName)
		{
			if(solrEntityMappings.ContainsKey(entityType)) {
				var fieldsMappings = solrEntityMappings[entityType];
				if(!fieldsMappings.ContainsKey(propertyName)) {
					throw new InvalidOperationException($"Для свойства {propertyName} не настроен Solr маппинг");
				}
				return fieldsMappings[propertyName].Property.PropertyType;
			}
			if(ormEntityMappings.ContainsKey(entityType)) {
				var fieldsMappings = ormEntityMappings[entityType];
				if(!fieldsMappings.ContainsKey(propertyName)) {
					throw new InvalidOperationException($"Для свойства {propertyName} не настроен Solr маппинг");
				}
				return fieldsMappings[propertyName].Property.PropertyType;
			}

			throw new InvalidOperationException($"Для типа {entityType.FullName} не настроен Solr маппинг");
		}

		#region IReadOnlyMappingManager implementation

		public IDictionary<string, SolrFieldModel> GetFields(Type type)
		{
			if(!solrEntityMappings.ContainsKey(type)) {
				throw new InvalidOperationException($"Не настроен маппинг для {type.FullName}");
			}
			return solrEntityMappings[type];
		}

		public ICollection<Type> GetRegisteredTypes()
		{
			return solrEntityMappings.Keys;
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

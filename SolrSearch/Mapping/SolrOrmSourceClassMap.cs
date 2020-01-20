using System;
using System.Collections.Generic;

namespace SolrSearch.Mapping
{
	public abstract class SolrOrmSourceClassMap
	{
		internal Type SolrEntityType { get; }
		internal Type OrmEntityType { get; }
		internal Dictionary<string, SolrFieldMapInfo> FieldsMapping { get; } = new Dictionary<string, SolrFieldMapInfo>();

		public SolrOrmSourceClassMap(Type solrEntityType, Type ormEntityType)
		{
			SolrEntityType = solrEntityType ?? throw new ArgumentNullException(nameof(solrEntityType));
			OrmEntityType = ormEntityType ?? throw new ArgumentNullException(nameof(ormEntityType));
		}

		protected void Map(string propertyName, string fieldName, float? boost = null)
		{
			if(string.IsNullOrWhiteSpace(propertyName)) {
				throw new ArgumentException(nameof(propertyName));
			}

			if(string.IsNullOrWhiteSpace(fieldName)) {
				throw new ArgumentException(nameof(fieldName));
			}

			var fieldModel = new SolrFieldMapInfo(propertyName, (ormMappingProvider) => fieldName, boost);

			if(FieldsMapping.ContainsKey(propertyName)) {
				FieldsMapping[propertyName] = fieldModel;
			} else {
				FieldsMapping.Add(propertyName, fieldModel);
			}
		}

		protected void Map(string propertyName, Func<IOrmMappingInfoProvider, string> fieldNameFunc, float? boost = null)
		{
			if(string.IsNullOrWhiteSpace(propertyName)) {
				throw new ArgumentException(nameof(propertyName));
			}

			if(fieldNameFunc == null) {
				throw new ArgumentNullException(nameof(fieldNameFunc));
			}

			var fieldModel = new SolrFieldMapInfo(propertyName, fieldNameFunc, boost);

			if(FieldsMapping.ContainsKey(propertyName)) {
				FieldsMapping[propertyName] = fieldModel;
			} else {
				FieldsMapping.Add(propertyName, fieldModel);
			}
		}
	}
}

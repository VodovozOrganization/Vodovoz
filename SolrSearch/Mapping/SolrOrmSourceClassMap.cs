using System;
using System.Collections.Generic;

namespace SolrSearch.Mapping
{
	public abstract class SolrOrmSourceClassMap
	{
		internal Type SolrEntityType { get; }
		internal Type OrmEntityType { get; }
		internal Type SolrEntityFactoryType { get; }

		internal Dictionary<string, SolrMapInfo> PropertyMapping = new Dictionary<string, SolrMapInfo>();

		protected SolrOrmSourceClassMap(Type solrEntityType, Type ormEntityType, Type solrEntityFactoryType)
		{
			SolrEntityType = solrEntityType ?? throw new ArgumentNullException(nameof(solrEntityType));
			OrmEntityType = ormEntityType ?? throw new ArgumentNullException(nameof(ormEntityType));
			SolrEntityFactoryType = solrEntityFactoryType ?? throw new ArgumentNullException(nameof(solrEntityFactoryType));
		}

		protected void Map(string solrEntityPropertyName, string solrFieldName, float? boost = null)
		{
			if(string.IsNullOrWhiteSpace(solrEntityPropertyName)) {
				throw new ArgumentException(nameof(solrEntityPropertyName));
			}

			if(string.IsNullOrWhiteSpace(solrFieldName)) {
				throw new ArgumentException(nameof(solrFieldName));
			}

			var fieldModel = new SolrMapInfo(solrEntityPropertyName, (ormMapProvider) => solrFieldName, boost);

			if(PropertyMapping.ContainsKey(solrEntityPropertyName)) {
				PropertyMapping[solrEntityPropertyName] = fieldModel;
			} else {
				PropertyMapping.Add(solrEntityPropertyName, fieldModel);
			}
		}

		internal void Map(string solrEntityPropertyName, Func<IOrmMappingInfoProvider, string> solrFieldNameFunc, string ormEntityPropertyName, float? boost = null)
		{
			if(string.IsNullOrWhiteSpace(solrEntityPropertyName)) {
				throw new ArgumentException(nameof(solrEntityPropertyName));
			}

			if(string.IsNullOrWhiteSpace(ormEntityPropertyName)) {
				throw new ArgumentException(nameof(ormEntityPropertyName));
			}

			if(solrFieldNameFunc == null) {
				throw new ArgumentNullException(nameof(solrFieldNameFunc));
			}

			var fieldModel = new SolrMapInfo(solrEntityPropertyName, solrFieldNameFunc, ormEntityPropertyName, boost);

			if(PropertyMapping.ContainsKey(solrEntityPropertyName)) {
				PropertyMapping[solrEntityPropertyName] = fieldModel;
			} else {
				PropertyMapping.Add(solrEntityPropertyName, fieldModel);
			}
		}
	}
}

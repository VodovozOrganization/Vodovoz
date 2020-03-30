using System;
using SolrSearch.Mapping;
using System.Collections.Generic;
using System.Globalization;

namespace SolrSearch
{
	public class EntityContentProvider
	{
		private readonly Type entityType;
		private readonly IDictionary<string, object> entityContent;
		private readonly SolrOrmSourceMapping solrOrmSourceMapping;
		Dictionary<string, string> propertyToSolrFieldMap = new Dictionary<string, string>();

		public EntityContentProvider(Type entityType, IDictionary<string, object> entityContent, SolrOrmSourceMapping solrOrmSourceMapping)
		{
			this.entityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
			this.entityContent = entityContent ?? throw new ArgumentNullException(nameof(entityContent));
			this.solrOrmSourceMapping = solrOrmSourceMapping ?? throw new ArgumentNullException(nameof(solrOrmSourceMapping));
		}

		internal string GetSolrId()
		{
			return (string)entityContent[solrOrmSourceMapping.KeyFieldName];
		}

		internal string GetSolrEntityType()
		{
			return (string)entityContent[solrOrmSourceMapping.EntityTypeFieldName];
		}

		public T GetPropertyContent<T>(string propertyName)
		{
			object result = GetPropertyContent(propertyName);
			return result != null ? (T)result : default(T);
		}

		public object GetPropertyContent(string propertyName)
		{
			if(!propertyToSolrFieldMap.TryGetValue(propertyName, out string fieldName)) {
				fieldName = solrOrmSourceMapping.GetSolrFieldName(entityType, propertyName);
				propertyToSolrFieldMap.Add(propertyName, fieldName);
			}
			if(entityContent.TryGetValue(fieldName, out object result)) {
				return result;
			}
			return null;
		}

		/*
		private object StringToType(Type type, string value)
		{
			var underlyingType = Nullable.GetUnderlyingType(type);
			if(underlyingType == null)
				return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
			return String.IsNullOrEmpty(value)
			  ? null
			  : Convert.ChangeType(value, underlyingType, CultureInfo.InvariantCulture);
		}*/
	}
}

using System;
namespace SolrSearch.Mapping
{
	internal class SolrFieldMapInfo
	{
		public string PropertyName { get; }
		public Func<IOrmMappingInfoProvider, string> FieldNameFunc { get; }
		public float? Boost { get; }

		public SolrFieldMapInfo(string propertyName, Func<IOrmMappingInfoProvider, string> fieldNameFunc, float? boost)
		{
			PropertyName = propertyName;
			FieldNameFunc = fieldNameFunc;
			Boost = boost;
		}
	}
}

using System;
namespace SolrSearch.Mapping
{
	internal class SolrMapInfo
	{
		public string SolrProperty { get; }

		public Func<IOrmMappingInfoProvider, string> SolrFieldNameFunc { get; }

		public float? Boost { get; }

		public string OrmProperty { get; }

		public SolrMapInfo(string solrProperty, Func<IOrmMappingInfoProvider, string> solrFieldNameFunc, string ormProperty, float? boost = null)
		{
			SolrProperty = solrProperty ?? throw new ArgumentNullException(nameof(solrProperty));
			SolrFieldNameFunc = solrFieldNameFunc ?? throw new ArgumentNullException(nameof(solrFieldNameFunc));
			OrmProperty = ormProperty ?? throw new ArgumentNullException(nameof(ormProperty));
			Boost = boost;
		}

		public SolrMapInfo(string solrProperty, Func<IOrmMappingInfoProvider, string> solrFieldNameFunc, float? boost = null)
		{
			SolrProperty = solrProperty ?? throw new ArgumentNullException(nameof(solrProperty));
			SolrFieldNameFunc = solrFieldNameFunc ?? throw new ArgumentNullException(nameof(solrFieldNameFunc));
			Boost = boost;
		}

	}
}

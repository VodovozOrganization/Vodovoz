using System;
using SolrSearch;
using System.Collections.Generic;
namespace Vodovoz.Views.Search
{
	public class SolrSearchResultNode
	{
		public int Id { get; set; }

		public string EntityType { get; set; }

		public string EntityTitle { get; set; }

		public SolrEntityBase Entity { get; set; }

		public SolrSearchResultNode(Dictionary<string, object> entityContent)
		{
			entityContent.TryGetValue("id", out object id);
			Id = (int)id;

			entityContent.TryGetValue("solr_entity_type", out object solrEntityType);
			EntityType = (string)solrEntityType;
		}
	}
}

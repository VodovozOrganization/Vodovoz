using System;
using System.Collections.Generic;
namespace SolrSearch
{
	public class SolrSearchResult
	{
		public SolrEntityBase Entity { get; }
		public IDictionary<string, string> Highlights { get; }

		public SolrSearchResult(SolrEntityBase entity, IDictionary<string, string> highlights = null)
		{
			Entity = entity ?? throw new ArgumentNullException(nameof(entity));
			Highlights = highlights ?? new Dictionary<string, string>();
		}
	}
}

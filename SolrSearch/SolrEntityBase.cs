using System.Collections.Generic;

namespace SolrSearch
{
	public abstract class SolrEntityBase
	{
		public string SolrId { get; protected internal set; }
		public string SolrEntityType { get; protected internal set; }

		public abstract string GetTitle();
		public abstract string GetTitle(IDictionary<string, string> hightlightedContent);
	}
}

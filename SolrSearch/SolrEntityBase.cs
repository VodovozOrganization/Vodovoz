using System;
namespace SolrSearch
{
	public abstract class SolrEntityBase
	{
		public string SolrId { get; protected set; }
		public string SolrEntityType { get; protected set; }
	}
}

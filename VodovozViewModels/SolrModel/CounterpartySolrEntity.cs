using System;
using SolrSearch;

namespace Vodovoz.SolrModel
{
	public class CounterpartySolrEntity : SolrEntityBase
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public string FullName { get; set; }

		public string Inn { get; set; }
	}
}

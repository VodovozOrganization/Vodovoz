using System;
using System.Collections.Generic;
namespace SolrSearch
{
	public class SolrSearchResults
	{
		/// <summary>
		/// Найдено записей
		/// </summary>
		public int FoundCount { get; set; }
		public int LoadCount { get; set; }

		public IEnumerable<SolrSearchResult> Results { get; set; }

		public SolrSearchResults(int foundCount, int loadCount, IEnumerable<SolrSearchResult> results)
		{
			FoundCount = foundCount;
			LoadCount = loadCount;
			Results = results ?? throw new ArgumentNullException(nameof(results));
		}

		public SolrSearchResults()
		{
			Results = new SolrSearchResult[0];
		}
	}
}

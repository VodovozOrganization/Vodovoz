using SolrSearch.Mapping;
using Vodovoz.SolrModel;
using Vodovoz.Domain.Client;
namespace Vodovoz.SolrMapping
{
	public class CounterpartySolrMap : SolrOrmSourceClassMap<CounterpartySolrEntity, Counterparty>
	{
		public CounterpartySolrMap()
		{
			Map(se => se.Id, e => e.Id);
			Map(se => se.Name, e => e.Name);
			Map(se => se.FullName, e => e.FullName);
			Map(se => se.Inn, e => e.INN);
		}
	}
}

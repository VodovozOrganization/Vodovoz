using System;
using System.Collections.Generic;
using QS.DomainModel.Entity;
using SolrSearch;

namespace Vodovoz.SolrModel
{
	[Appellative(
		Gender = GrammaticalGender.Masculine,
		Nominative = "контрагент"
	)]
	public class CounterpartySolrEntity : SolrEntityBase, IDomainObject
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public string FullName { get; set; }

		public string Inn { get; set; }

		public override string GetTitle()
		{
			return FullName;
		}

		public override string GetTitle(IDictionary<string, string> hightlightedContent)
		{
			if(hightlightedContent == null) {
				throw new ArgumentNullException(nameof(hightlightedContent));
			}

			if(!hightlightedContent.TryGetValue(nameof(Id), out string id)) {
				id = Id.ToString();
			}

			if(!hightlightedContent.TryGetValue(nameof(FullName), out string fullName)){
				fullName = FullName;
			}
			return $"{id} {fullName}";
		}
	}
}

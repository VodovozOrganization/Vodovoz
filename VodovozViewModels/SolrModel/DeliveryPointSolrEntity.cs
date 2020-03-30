using System;
using System.Collections.Generic;
using QS.DomainModel.Entity;
using SolrSearch;

namespace Vodovoz.SolrModel
{
	[Appellative(
		Gender = GrammaticalGender.Masculine,
		Nominative = "Точка доставки"
	)]
	public class DeliveryPointSolrEntity : SolrEntityBase, IDomainObject
	{
		public int Id { get; set; }

		public string CompiledAddress { get; set; }

		public override string GetTitle()
		{
			return CompiledAddress;
		}

		public override string GetTitle(IDictionary<string, string> hightlightedContent)
		{
			if(hightlightedContent == null) {
				throw new ArgumentNullException(nameof(hightlightedContent));
			}
			if(!hightlightedContent.TryGetValue(nameof(CompiledAddress), out string compiledAddress)) {
				compiledAddress = CompiledAddress;
			}
			return $"{compiledAddress}";
		}
	}
}

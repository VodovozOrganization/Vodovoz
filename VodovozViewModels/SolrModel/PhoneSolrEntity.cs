using System;
using System.Collections.Generic;
using QS.DomainModel.Entity;
using SolrSearch;

namespace Vodovoz.SolrModel
{
	[Appellative(
		Gender = GrammaticalGender.Masculine,
		Nominative = "телефон"
	)]
	public class PhoneSolrEntity : SolrEntityBase, IDomainObject
	{
		public int Id { get; set; }

		public string Number { get; set; }

		public string DigitsNumber { get; set; }

		public override string GetTitle()
		{
			return $"{Number} ({DigitsNumber})";
		}

		public override string GetTitle(IDictionary<string, string> hightlightedContent)
		{
			if(hightlightedContent == null) {
				throw new ArgumentNullException(nameof(hightlightedContent));
			}
			if(!hightlightedContent.TryGetValue(nameof(Number), out string number)) {
				number = Number;
			}

			if(!hightlightedContent.TryGetValue(nameof(DigitsNumber), out string digitNumber)) {
				digitNumber = DigitsNumber;
			}
			return $"{number} ({digitNumber})";
		}
	}
}

using System;
using System.Collections.Generic;
using QS.DomainModel.Entity;
using SolrSearch;

namespace Vodovoz.SolrModel
{
	[Appellative(
		Gender = GrammaticalGender.Masculine,
		Nominative = "сотрудник"
	)]
	public class EmployeeSolrEntity : SolrEntityBase, IDomainObject
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public string LastName { get; set; }

		public string Patronymic { get; set; }

		public override string GetTitle()
		{
			return $"{LastName} {Name} {Patronymic}";
		}

		public override string GetTitle(IDictionary<string, string> hightlightedContent)
		{
			if(hightlightedContent == null) {
				throw new ArgumentNullException(nameof(hightlightedContent));
			}

			if(!hightlightedContent.TryGetValue(nameof(LastName), out string lastName)) {
				lastName = LastName;
			}

			if(!hightlightedContent.TryGetValue(nameof(Name), out string name)) {
				name = Name;
			}

			if(!hightlightedContent.TryGetValue(nameof(Patronymic), out string patronymic)) {
				patronymic = Patronymic;
			}

			return $"{lastName} {name} {patronymic}";
		}
	}
}

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Client
{
	[Appellative(
		Gender = GrammaticalGender.Feminine,
		NominativePlural = "виды деятельности контрагента",
		Nominative = "вид деятельности контрагента"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class CounterpartyActivityKind : PropertyChangedBase, IDomainObject
	{
		public CounterpartyActivityKind() { }

		#region Свойства

		public virtual int Id { get; set; }

		string name;
		[Display(Name = "Название вида")]
		[Required(ErrorMessage = "Вид деятельности должен быть заполнен")]
		public virtual string Name {
			get => name;
			set => SetField(ref name, value, () => Name);
		}

		string substrings;
		[Display(Name = "Искомые подстроки")]
		[Required(ErrorMessage = "Добавьте строки для поиска")]
		public virtual string Substrings {
			get => substrings;
			set => SetField(ref substrings, value, () => Substrings);
		}

		#endregion Свойства

		#region Methods

		public virtual IList<SubstringToSearch> GetListOfSubstrings()
		{
			IList<SubstringToSearch> result = new List<SubstringToSearch>();
			foreach(var str in Substrings.Split('\n')) {
				var s = str.Trim().ToLower();
				if(!string.IsNullOrEmpty(s))
					result.Add(new SubstringToSearch(s));
			}
			return result;
		}

		#endregion Methods
	}

	public class SubstringToSearch
	{
		public SubstringToSearch(string substring)
		{
			Substring = substring;
			Selected = true;
		}

		public string Substring { get; set; }

		public bool Selected { get; set; }
	}
}

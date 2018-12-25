using System;
using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Employees
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Граждаства",
		Nominative = "Граждаство")]
	public class Citizenship: PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		string name;

		[Required(ErrorMessage = "Название должно быть заполнено.")]
		[Display(Name = "Название")]
		public virtual string Name {
			get { return name; }
			set { SetField(ref name, value, () => Name); }
		}

		public Citizenship()
		{
			Name = string.Empty;
		}
	}
}

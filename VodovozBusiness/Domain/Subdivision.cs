using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Employees;
using System.Collections.Generic;

namespace Vodovoz
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Feminine,
		NominativePlural = "подразделения",
		Nominative = "подразделение")]
	public class Subdivision : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		private string name;

		[Display(Name = "Название подразделения")]
		[Required (ErrorMessage = "Название подразделения должно быть заполнено.")]
		public virtual string Name
		{
			get { return name; }
			set { SetField(ref name, value, () => Name); }
		}

		private Employee chief;

		[Display (Name = "Начальник подразделения")]
		public virtual Employee Chief {
		get { return chief; }
		set { SetField (ref chief, value, () => Chief); }
		}
		#endregion

		public Subdivision()
		{
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(Name))
				yield return new ValidationResult ("Название подразделения должно быть заполнено.",
					new[] {"Name"});
		}

		#endregion
	}
}


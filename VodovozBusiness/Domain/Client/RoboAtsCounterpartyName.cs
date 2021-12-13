using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Domain.Client
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "имена контргентов",
		Nominative = "имя контрагента")]
	[EntityPermission]
	[HistoryTrace]

	public class RoboAtsCounterpartyName : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private string _name;
		private string _accent;

		#region Свойства

		public virtual int Id { get; set; }

		[Display(Name = "Имя ")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		[Display(Name = "Ударение")]
		public virtual string Accent
		{
			get => _accent;
			set => SetField(ref _accent, value);
		}


		#endregion

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrEmpty(Name))
			{
				yield return new ValidationResult("Имя должно быть заполнено.",
					new[] { nameof(CarEventType) });
			}

			if(string.IsNullOrEmpty(Accent))
			{
				yield return new ValidationResult("Ударение должно быть заполнено.",
					new[] { nameof(CarEventType) });
			}

			if(Name?.Length > 50)
			{
				yield return new ValidationResult($"Превышена максимально допустимая длина имени ({Name.Length}/255).",
					new[] { nameof(Name) });
			}

			if(Accent?.Length > 50)
			{
				yield return new ValidationResult($"Превышена максимально допустимая длина ударения ({Accent.Length}/255).",
					new[] { nameof(Accent) });
			}
		}
		
		#endregion
	}
}

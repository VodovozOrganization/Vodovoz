using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Domain.Client
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "отчества контргентов",
		Nominative = "отчество контрагента")]
	[EntityPermission]
	[HistoryTrace]

	public class RoboAtsCounterpartyPatronymic : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private string _patronymic;
		private string _accent;

		#region Свойства

		public virtual int Id { get; set; }

		[Display(Name = "Отчество")]
		public virtual string Patronymic
		{
			get => _patronymic;
			set => SetField(ref _patronymic, value);
		}

		[Display(Name = "Ударение")]
		public virtual string Accent
		{
			get => _accent;
			set => SetField(ref _accent, value);
		}

		#endregion

		public virtual string Title => Patronymic;


		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrEmpty(Patronymic))
			{
				yield return new ValidationResult("Отчество должно быть заполнено.",
					new[] { nameof(CarEventType) });
			}

			if(string.IsNullOrEmpty(Accent))
			{
				yield return new ValidationResult("Ударение должно быть заполнено.",
					new[] { nameof(CarEventType) });
			}

			if(Patronymic?.Length > 20)
			{
				yield return new ValidationResult($"Превышена максимально допустимая длина отчества ({Patronymic.Length}/20).",
					new[] { nameof(Patronymic) });
			}

			if(Accent?.Length > 20)
			{
				yield return new ValidationResult($"Превышена максимально допустимая длина ударения ({Accent.Length}/20).",
					new[] { nameof(Accent) });
			}
		}
		
		#endregion
	}
}

using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "ответственные",
		Nominative = "ответственный")]
	[EntityPermission]
	[HistoryTrace]
	public class Responsible : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private string _name;
		private bool _isArchived;

		public virtual int Id { get; set; }

		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		[Display(Name = "В архиве")]
		public virtual bool IsArchived
		{
			get => _isArchived;
			set => SetField(ref _isArchived, value);
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrEmpty(Name))
			{
				yield return new ValidationResult("Название должно быть заполнено.",
					new[] { nameof(Name) });
			}


			if(Name?.Length > 255)
			{
				yield return new ValidationResult($"Превышена максимально допустимая длина отчества ({Name.Length}/255).",
					new[] { nameof(Name) });
			}
		}

		#endregion
	}
}

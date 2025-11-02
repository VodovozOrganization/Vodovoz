using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Logistic.Cars
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "причины списаний киломатража",
		Nominative = "причина списания километража",
		Genitive = "причины списания километража")]
	[EntityPermission]
	[HistoryTrace]
	public class MileageWriteOffReason : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private const int _nameMaxLength = 100;
		private const int _descriptionMaxLength = 500;

		private string _name;
		private string _description;
		private bool _isArchived;

		public virtual int Id { get; set; }

		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		[Display(Name = "Описание")]
		public virtual string Description
		{
			get => _description;
			set => SetField(ref _description, value);
		}

		[Display(Name = "Архивная")]
		public virtual bool IsArchived
		{
			get => _isArchived;
			set => SetField(ref _isArchived, value);
		}

		public virtual string Title => Name;

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(Name))
			{
				yield return new ValidationResult(
					"Наименование должно быть обязательно указано",
					new[] { nameof(Name) });
			}

			if(Name?.Length > _nameMaxLength)
			{
				yield return new ValidationResult(
					$"Наименование не должно быть длиннее {_nameMaxLength} символов",
					new[] { nameof(Name) });
			}

			if(Description?.Length > _descriptionMaxLength)
			{
				yield return new ValidationResult(
					$"Описание не должно быть длиннее {_descriptionMaxLength} символов",
					new[] { nameof(Name) });
			}
		}
	}
}

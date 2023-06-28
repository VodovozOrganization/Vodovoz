using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "детализации рекламации",
		Nominative = "детализация рекламации",
		Prepositional = "детализации рекламации",
		PrepositionalPlural = "детализациях рекламаций")]
	[HistoryTrace]
	[EntityPermission]
	public class UndeliveryDetalization : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private string _name;
		bool _isArchive;
		private UndeliveryKind _complaintKind;

		public virtual int Id { get; set; }

		[Display(Name = "Название детализации")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		[Display(Name = "Вид недовоза")]
		public virtual UndeliveryKind UndeliveryKind
		{
			get => _complaintKind;
			set => SetField(ref _complaintKind, value);
		}

		[Display(Name = "Архивная")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}

		public virtual string GetFullName => !IsArchive ? Name : $"(Архив) {Name}";

		public virtual string Title => Name;

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(Name))
			{
				yield return new ValidationResult(
					"Укажите название вида недовоза",
					new[] { nameof(Name) });
			}

			if(Name?.Length > 100)
			{
				yield return new ValidationResult(
					$"Превышена максимально допустимая длина названия ({Name.Length}/100).",
					new[] { nameof(Name) });
			}
		}
	}
}

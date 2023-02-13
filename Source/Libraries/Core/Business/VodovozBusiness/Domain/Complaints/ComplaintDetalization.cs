using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Complaints
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "детализации рекламации",
		Nominative = "детализация рекламации",
		Prepositional = "детализации рекламации",
		PrepositionalPlural = "детализациях рекламаций")]
	[HistoryTrace]
	[EntityPermission]
	public class ComplaintDetalization : BusinessObjectBase<Complaint>, IDomainObject, IValidatableObject
	{
		private string _name;
		bool _isArchive;
		private ComplaintKind _complaintKind;

		public virtual int Id { get; set; }

		[Display(Name = "Название детализации")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		[Display(Name = "Вид рекламаций")]
		public virtual ComplaintKind ComplaintKind
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
					"Укажите название вида рекламации",
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

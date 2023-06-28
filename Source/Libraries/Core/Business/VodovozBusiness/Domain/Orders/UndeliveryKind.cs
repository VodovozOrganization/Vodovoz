using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "виды недовоза",
		Nominative = "вид рекламации",
		Prepositional = "виде недовоза",
		PrepositionalPlural = "видах недовозов"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class UndeliveryKind : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private string _name;
		private UndeliveryObject _undeliveryObject;
		private bool _isArchive;

		public virtual int Id { get; set; }

		[Display(Name = "Название вида")]
		public virtual string Name {
			get => _name;
			set => SetField(ref _name, value);
		}

		[Display(Name = "Объект недовоза")]
		public virtual UndeliveryObject UndeliveryObject
		{
			get => _undeliveryObject;
			set => SetField(ref _undeliveryObject, value);
		}

		[Display(Name = "Архивный")]
		public virtual bool IsArchive {
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

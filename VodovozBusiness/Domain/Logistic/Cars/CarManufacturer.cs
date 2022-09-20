using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Logistic.Cars
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		Nominative = "производитель автомобиля",
		NominativePlural = "производители автомобилей")]
	[EntityPermission]
	[HistoryTrace]
	public class CarManufacturer : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private string _name;

		public virtual int Id { get; set; }

		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(Name))
			{
				yield return new ValidationResult("Название производителя должно быть заполнено", new[] { nameof(Name) });
			}
			if(Name?.Length > 100)
			{
				yield return new ValidationResult($"Превышена максимально допустимая длина названия ({Name.Length}/100).",
					new[] { nameof(Name) });
			}
		}
	}
}

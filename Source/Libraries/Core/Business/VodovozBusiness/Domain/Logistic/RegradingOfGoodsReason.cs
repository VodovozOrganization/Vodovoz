using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Logistics.Cars;

namespace Vodovoz.Domain.Logistic
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "причины пересортицы",
		Nominative = "причина пересортицы")]
	[EntityPermission]
	[HistoryTrace]

	public class RegradingOfGoodsReason : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private string _name;

		#region Свойства

		public virtual int Id { get; set; }

		[Display(Name = "Название ")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		#endregion

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrEmpty(Name))
			{
				yield return new ValidationResult("Название должно быть заполнено.",
					new[] { nameof(CarEventType) });
			}

			if(Name?.Length > 255)
			{
				yield return new ValidationResult($"Превышена максимально допустимая длина названия ({Name.Length}/255).",
					new[] { nameof(Name) });
			}
		}

		#endregion
	}
}

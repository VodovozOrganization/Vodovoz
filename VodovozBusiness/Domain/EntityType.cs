using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Utilities;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain
{
	[Appellative(Gender = GrammaticalGender.Feminine,
	NominativePlural = "типы документов",
	Nominative = "тип документа")]
	public class EntityType : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public EntityType() { }

		#region Свойства

		public virtual int Id { get; set; }

		string customName;
		[Display(Name = "Название документа")]
		public virtual string CustomName {
			get => customName;
			set => SetField(ref customName, value, () => CustomName);
		}

		string type;
		[Display(Name = "Тип документа")]
		public virtual string Type {
			get => type;
			set => SetField(ref type, value, () => Type);
		}

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(CustomName))
				yield return new ValidationResult(
					"Название документа должно быть заполнено.",
					new[] { this.GetPropertyName(o => o.CustomName) }
				);

			if(string.IsNullOrWhiteSpace(Type))
				yield return new ValidationResult(
					"Тип документа должен быть выбран.",
					new[] { this.GetPropertyName(o => o.Type) }
				);
		}

		#endregion
	}
}

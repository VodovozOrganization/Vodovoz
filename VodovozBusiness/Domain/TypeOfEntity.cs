using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Utilities;
using System.Linq;
using QS.DomainModel.Entity;
using Vodovoz.Repositories;

namespace Vodovoz
{
	[Appellative(Gender = GrammaticalGender.Feminine,
	NominativePlural = "типы документов",
	Nominative = "тип документа")]
	public class TypeOfEntity : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public TypeOfEntity() { }

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

		public virtual bool IsActive => Id == 0 || TypeOfEntityRepository.GetEntityTypesMarkedByEntityPermissionAttribute().Select(t => t.Name).Any(n => n == Type);

		#endregion

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
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
	}
}

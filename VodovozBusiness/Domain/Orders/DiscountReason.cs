using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "основания скидок",
		Nominative = "основание скидки")]
	[EntityPermission]
	public class DiscountReason : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }
		private string _name;
		private bool _isArchive;

		[Display(Name = "Название")]
		public virtual string Name 
		{
			get => _name;
			set => SetField(ref _name, value);
		}
		
		[Display(Name = "В архиве")]
		public virtual bool IsArchive {
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Id == 0 && IsArchive)
			{
				yield return new ValidationResult("Нельзя создать новое архивное основание", new[] { nameof(IsArchive) });
			}
			
			if(string.IsNullOrEmpty(Name))
			{
				yield return new ValidationResult("Название должно быть заполнено", new[] { nameof(Name) });
			}
			
			if(Name?.Length > 45)
			{
				yield return new ValidationResult($"Превышена длина названия ({Name.Length}/45)", new[] { nameof(Name) });
			}
		}
	}
}

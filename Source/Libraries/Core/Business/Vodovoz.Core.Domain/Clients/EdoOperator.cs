using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Core.Domain.Clients
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "операторы ЭДО",
		Nominative = "оператор ЭДО")]
	[EntityPermission]
	[HistoryTrace]
	public class EdoOperator : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private string _name;
		private string _brandName;
		private string _code;

		public virtual int Id { get; set; }

		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		[Display(Name = "Брендовое название")]
		public virtual string BrandName
		{
			get => _brandName;
			set => SetField(ref _brandName, value);
		}

		[Display(Name = "Трёхзначный код")]
		public virtual string Code
		{
			get => _code;
			set => SetField(ref _code, value);
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(Name))
			{
				yield return new ValidationResult("Название должно быть заполнено.", new[] { nameof(Name) });
			}

			if(Name?.Length > 255)
			{
				yield return new ValidationResult($"Превышена максимально допустимая длина названия ({Name.Length}/255).",
					new[] { nameof(Name) });
			}

			if(string.IsNullOrWhiteSpace(BrandName))
			{
				yield return new ValidationResult("Брендовое название должно быть заполнено.", new[] { nameof(BrandName) });
			}

			if(BrandName?.Length > 255)
			{
				yield return new ValidationResult($"Превышена максимально допустимая длина брендового названия ({BrandName.Length}/255).",
					new[] { nameof(BrandName) });
			}

			if(string.IsNullOrWhiteSpace(Code))
			{
				yield return new ValidationResult("Трёхзначный код должен быть заполнен.", new[] { nameof(Code) });
			}

			if(Code?.Length != 3)
			{
				yield return new ValidationResult($"Трёхзначный код должен содержать 3 символа ({Code.Length}/3).",
					new[] { nameof(Code) });
			}

			if(!Regex.IsMatch(Code, @"^[a-zA-Z0-9]+$"))
			{
				yield return new ValidationResult($"Трёхзначный код должен содержать только латинские буквы и цифры.",
					new[] { nameof(Code) });
			}
		}

		#endregion
	}
}

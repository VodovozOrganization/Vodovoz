using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Vodovoz.Core.Domain.Employees
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "внутренние телефоны",
		Nominative = "внутренний телефон")]
	[EntityPermission]
	[HistoryTrace]
	public class InnerPhone : PropertyChangedBase, IValidatableObject
	{
		private string _phoneNumber;
		private string _description;

		[Display(Name = "Номер телефона")]
		public virtual string PhoneNumber
		{
			get => _phoneNumber;
			set => SetField(ref _phoneNumber, value);
		}

		[Display(Name = "Описание")]
		public virtual string Description
		{
			get => _description;
			set => SetField(ref _description, value);
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(PhoneNumber))
			{
				yield return new ValidationResult("Номер телефона должен быть заполнен", new[] { nameof(PhoneNumber) });
			}
			else if(PhoneNumber.Length > 100)
			{
				yield return new ValidationResult("Длина номера телефона не должна превышать 100 знаков");
			}
		}
	}
}

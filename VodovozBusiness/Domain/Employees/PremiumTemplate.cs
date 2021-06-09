using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;

namespace Vodovoz.Domain.Employees
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "шаблоны комментариев для премий",
		Nominative = "шаблон комментария для премии")]
	[EntityPermission]
	public class PremiumTemplate : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private string _reason;
		private decimal _premiumMoney;

		#region Свойства

		public virtual int Id { get; set; }

		[Display(Name = "Причина")]
		public virtual string Reason
		{
			get => _reason; 
			set => SetField(ref _reason, value);
		}

		[Display(Name = "Сумма премии")]
		public virtual decimal PremiumMoney
		{
			get => _premiumMoney; 
			set => SetField(ref _premiumMoney, value);
		}

		#endregion

		public PremiumTemplate()
		{
			Reason = String.Empty;
		}

		public override string ToString()
		{
			return $"Шаблон премии №{Id}: {Reason}";
		}

		#region IValidatableObject implementation

		public virtual System.Collections.Generic.IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(String.IsNullOrEmpty(Reason))
				yield return new ValidationResult("Текст комментария должен быть заполнен.", new[] { "Comment" });
		}

		#endregion
	}
}

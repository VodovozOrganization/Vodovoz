using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.Domain.Logistic.Organizations
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "версия организации",
		NominativePlural = "версии организации")]
	[HistoryTrace]
	public class OrganizationVersion : OrganizationVersionEntity, IValidatableObject
	{
		private Employee _leader;
		private Employee _accountant;
		private Organization _organization;


		[Display(Name = "Организация")]
		public virtual new Organization Organization
		{
			get => _organization;
			set => SetField(ref _organization, value);
		}

		[Display(Name = "Руководитель")]
		public virtual new Employee Leader
		{
			get => _leader;
			set => SetField(ref _leader, value);
		}

		[Display(Name = "Бухгалтер")]
		public virtual new Employee Accountant
		{
			get => _accountant;
			set => SetField(ref _accountant, value);
		}

		public override string ToString() => $"Версия организации №{Id}";
		public virtual string LeaderShortName => Leader?.ShortName;
		public virtual string AccountantShortName => Accountant?.ShortName;

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Leader == null)
			{
				yield return new ValidationResult("Руководитель не выбран.", new[] { nameof(Leader) });
			}

			if(Accountant == null)
			{
				yield return new ValidationResult("Бухгалтер не выбран.", new[] { nameof(Accountant) });
			}

			if(SignatureAccountant == null)
			{
				yield return new ValidationResult("Подпись бухгалтера не выбрана.", new[] { nameof(SignatureAccountant) });
			}

			if(SignatureLeader == null)
			{
				yield return new ValidationResult("Подпись руководителя не выбрана.", new[] { nameof(SignatureLeader) });
			}

			if(string.IsNullOrWhiteSpace(Address))
			{
				yield return new ValidationResult("Заполните адрес.", new[] { nameof(Address) });
			}

			if(string.IsNullOrWhiteSpace(JurAddress))
			{
				yield return new ValidationResult("Заполните юридичсекий адрес.", new[] { nameof(JurAddress) });
			}
		}
	}
}

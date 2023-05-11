using QS.DomainModel.Entity;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.StoredResources;

namespace Vodovoz.Domain.Logistic.Organizations
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "версия организации",
		NominativePlural = "версии организации")]
	[HistoryTrace]
	public class OrganizationVersion : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private string _address;
		private string _jurAddress;
		private Employee _leader;
		private Employee _accountant;
		private DateTime _startDate;
		private DateTime? _endDate;
		private Organization _organization;
		private StoredResource _signatureAccountant;
		private StoredResource _signatureLeader;

		public virtual int Id { get; set; }

		[Display(Name = "Организация")]
		public virtual Organization Organization
		{
			get => _organization;
			set => SetField(ref _organization, value);
		}

		[Display(Name = "Фактический адрес")]
		public virtual string Address
		{
			get => _address;
			set => SetField(ref _address, value);
		}

		[Display(Name = "Юридический адрес")]
		public virtual string JurAddress
		{
			get => _jurAddress;
			set => SetField(ref _jurAddress, value);
		}

		[Display(Name = "Руководитель")]
		public virtual Employee Leader
		{
			get => _leader;
			set => SetField(ref _leader, value);
		}

		[Display(Name = "Бухгалтер")]
		public virtual Employee Accountant
		{
			get => _accountant;
			set => SetField(ref _accountant, value);
		}

		[Display(Name = "Дата начала")]
		public virtual DateTime StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		[Display(Name = "Дата окончания")]
		public virtual DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		[Display(Name = "Подпись руководителя")]
		public virtual StoredResource SignatureLeader
		{
			get => _signatureLeader;
			set => SetField(ref _signatureLeader, value);
		}


		[Display(Name = "Подпись бухгалтера")]
		public virtual StoredResource SignatureAccountant
		{
			get => _signatureAccountant;
			set => SetField(ref _signatureAccountant, value);
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

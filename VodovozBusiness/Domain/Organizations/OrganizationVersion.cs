using QS.DomainModel.Entity;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.Domain.Logistic.Organizations
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "версия организации",
		NominativePlural = "версии организации")]
	[HistoryTrace]
	public class OrganizationVersion : PropertyChangedBase, IDomainObject
	{
		private string _address;
		private string _jurAddress;
		private Employee _leader;
		private Employee _accountant;
		private DateTime _startDate;
		private DateTime? _endDate;
		private Organization _organization;

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

		public override string ToString() => $"Версия организации №{Id}";
		public virtual string LeaderShortName => Leader?.ShortName;
		public virtual string AccountantShortName => Accountant?.ShortName;
	}

}

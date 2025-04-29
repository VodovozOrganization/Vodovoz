using QS.DomainModel.Entity;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.StoredResources;

namespace Vodovoz.Core.Domain.Organizations
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "версия организации",
		NominativePlural = "версии организации")]
	[HistoryTrace]
	public class OrganizationVersionEntity : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private DateTime _startDate;
		private DateTime? _endDate;
		private string _address;
		private string _jurAddress;
		private EmployeeEntity _leader;
		private EmployeeEntity _accountant;
		private OrganizationEntity _organization;
		private StoredResource _signatureAccountant;
		private StoredResource _signatureLeader;


		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
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

		[Display(Name = "Организация")]
		public virtual OrganizationEntity Organization
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
		public virtual EmployeeEntity Leader
		{
			get => _leader;
			set => SetField(ref _leader, value);
		}

		[Display(Name = "Бухгалтер")]
		public virtual EmployeeEntity Accountant
		{
			get => _accountant;
			set => SetField(ref _accountant, value);
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
	}
}


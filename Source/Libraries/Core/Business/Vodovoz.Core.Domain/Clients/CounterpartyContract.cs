using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Domain.Clients
{
	[Appellative(
		Gender = GrammaticalGender.Masculine,
		NominativePlural = "договоры контрагента",
		Nominative = "договор",
		Genitive = " договора",
		Accusative = "договор"
	)]
	[EntityPermission]
	public class CounterpartyContractEntity : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private bool _onCancellation;
		private bool _isArchive;
		private int _maxDelay;
		private int _contractSubNumber;
		private DateTime _issueDate;
		private ContractType _contractType;
		private OrganizationEntity _organization;
		private CounterpartyEntity _counterparty;
		private byte[] _changedTemplateFile;
		private string _number;


		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "Максимальный срок отсрочки")]
		public virtual int MaxDelay
		{
			get => _maxDelay;
			set => SetField(ref _maxDelay, value);
		}

		[Display(Name = "Архивный")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}

		[Display(Name = "На расторжении")]
		public virtual bool OnCancellation
		{
			get => _onCancellation;
			set => SetField(ref _onCancellation, value);
		}

		[Display(Name = "Номер")]
		public virtual string Number
		{
			get => _number;
			set => SetField(ref _number, value);
		}

		[Display(Name = "Дата подписания")]
		public virtual DateTime IssueDate
		{
			get => _issueDate;
			set => SetField(ref _issueDate, value);
		}

		[Display(Name = "Организация")]
		public virtual OrganizationEntity Organization
		{
			get => _organization;
			set => SetField(ref _organization, value);
		}

		[Display(Name = "Контрагент")]
		public virtual CounterpartyEntity Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}

		[Display(Name = "Номер договора внутренний")]
		public virtual int ContractSubNumber
		{
			get => _contractSubNumber;
			set => SetField(ref _contractSubNumber, value);
		}

		[Display(Name = "Тип договора")]
		public virtual ContractType ContractType
		{
			get => _contractType;
			set => SetField(ref _contractType, value);
		}

		[Display(Name = "Измененный договор")]
		public virtual byte[] ChangedTemplateFile
		{
			get => _changedTemplateFile;
			set => SetField(ref _changedTemplateFile, value);
		}

		public virtual string Title => $"Договор №{Number} от {IssueDate:d}";
		public virtual string TitleIn1c => $"{Number} от {IssueDate:d}";
	}
}

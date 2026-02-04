using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Domain.Client;

namespace Vodovoz.Core.Domain.Clients
{
	/// <summary>
	/// Договор контрагента
	/// </summary>
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

		/// <summary>
		/// Идентификатор<br/>
		/// Код
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Максимальный срок отсрочки
		/// </summary>
		[Display(Name = "Максимальный срок отсрочки")]
		public virtual int MaxDelay
		{
			get => _maxDelay;
			set => SetField(ref _maxDelay, value);
		}

		/// <summary>
		/// Архивный
		/// </summary>
		[Display(Name = "Архивный")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}

		/// <summary>
		/// На расторжении
		/// </summary>
		[Display(Name = "На расторжении")]
		public virtual bool OnCancellation
		{
			get => _onCancellation;
			set => SetField(ref _onCancellation, value);
		}

		/// <summary>
		/// Номер
		/// </summary>
		[Display(Name = "Номер")]
		public virtual string Number
		{
			get => _number;
			set => SetField(ref _number, value);
		}

		/// <summary>
		/// Дата подписания
		/// </summary>
		[Display(Name = "Дата подписания")]
		public virtual DateTime IssueDate
		{
			get => _issueDate;
			set => SetField(ref _issueDate, value);
		}

		/// <summary>
		/// Организация
		/// </summary>
		[Display(Name = "Организация")]
		public virtual OrganizationEntity Organization
		{
			get => _organization;
			set => SetField(ref _organization, value);
		}

		/// <summary>
		/// Контрагент
		/// </summary>
		[Display(Name = "Контрагент")]
		public virtual CounterpartyEntity Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}

		/// <summary>
		/// Номер договора внутренний
		/// </summary>
		[Display(Name = "Номер договора внутренний")]
		public virtual int ContractSubNumber
		{
			get => _contractSubNumber;
			set => SetField(ref _contractSubNumber, value);
		}

		/// <summary>
		/// Тип договора
		/// </summary>
		[Display(Name = "Тип договора")]
		public virtual ContractType ContractType
		{
			get => _contractType;
			set => SetField(ref _contractType, value);
		}

		/// <summary>
		/// Измененный договор
		/// </summary>
		[Display(Name = "Измененный договор")]
		public virtual byte[] ChangedTemplateFile
		{
			get => _changedTemplateFile;
			set => SetField(ref _changedTemplateFile, value);
		}

		/// <summary>
		/// Заголовок
		/// </summary>
		public virtual string Title => $"Договор №{Number} от {IssueDate:d}";

		/// <summary>
		/// Заголовок в 1С
		/// </summary>
		public virtual string TitleIn1c => $"{Number} от {IssueDate:dd.MM.yyyy}";
		
		public static ContractType GetContractTypeForPaymentType(PersonType clientType, PaymentType paymentType)
		{
			switch(paymentType)
			{
				case PaymentType.Cash:
				case PaymentType.DriverApplicationQR:
				case PaymentType.SmsQR:
				case PaymentType.PaidOnline:
				case PaymentType.Terminal:
					if(clientType == PersonType.legal)
					{
						return ContractType.CashUL;
					}

					return ContractType.CashFL;
				case PaymentType.Cashless:
				case PaymentType.ContractDocumentation:
					return ContractType.Cashless;
				case PaymentType.Barter:
					return ContractType.Barter;
				default:
					return ContractType.Cashless;
			}
		}
	}
}

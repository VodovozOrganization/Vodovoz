using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Counterparties;

namespace Vodovoz.Domain.Client
{
	[Appellative(
		Gender = GrammaticalGender.Masculine,
		NominativePlural = "договоры контрагента",
		Nominative = "договор",
		Genitive = " договора",
		Accusative = "договор"
	)]
	[EntityPermission]
	public class CounterpartyContract : BusinessObjectBase<CounterpartyContract>, IDomainObject, IValidatableObject
	{
		private bool _onCancellation;
		private bool _isArchive;
		private int _maxDelay;
		private int _contractSubNumber;
		private DateTime _issueDate;
		private ContractType _contractType;
		private Organization _organization;
		private Counterparty _counterparty;
		private DocTemplate _contractTemplate;
		private byte[] _changedTemplateFile;
		private string _number;

		#region Сохраняемые поля

		public virtual int Id { get; set; }

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

		[Required(ErrorMessage = "Организация должна быть заполнена.")]
		[Display(Name = "Организация")]
		public virtual Organization Organization
		{
			get => _organization;
			set => SetField(ref _organization, value);
		}

		[Required(ErrorMessage = "Контрагент должен быть указан.")]
		[Display(Name = "Контрагент")]
		public virtual Counterparty Counterparty
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

		[Display(Name = "Шаблон договора")]
		public virtual DocTemplate DocumentTemplate
		{
			get => _contractTemplate;
			protected set => SetField(ref _contractTemplate, value);
		}

		[Display(Name = "Измененный договор")]
		public virtual byte[] ChangedTemplateFile
		{
			get => _changedTemplateFile;
			set => SetField(ref _changedTemplateFile, value);
		}

		#endregion

		#region Вычисляемые

		public virtual string Title => $"Договор №{Number} от {IssueDate:d}";

		public virtual string TitleIn1c => $"{Number} от {IssueDate:d}";

		#endregion

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var counterpartyContractRepository = validationContext.GetRequiredService<ICounterpartyContractRepository>();
			var uowFactory = validationContext.GetRequiredService<IUnitOfWorkFactory>();

			if(!IsArchive && !OnCancellation)
			{
				using(var uow = uowFactory.CreateWithoutRoot("Валидация договора контрагента"))
				{
					var contracts =
						counterpartyContractRepository.GetActiveContractsWithOrganization(uow, Counterparty, Organization, ContractType);
					if(contracts.Any(c => c.Id != Id))
					{
						yield return new ValidationResult(
							$"У контрагента '{Counterparty.Name}' уже есть активный договор с организацией '{Organization.Name}'",
							new[] { nameof(Organization) });
					}
				}
			}
		}

		#endregion

		public virtual void UpdateNumber()
		{
			GenerateSubNumber();
			Number = $"{Counterparty.Id}-{ContractSubNumber}";
		}

		/// <summary>
		/// Вычисляет вторую часть номера для нового договора, клиент должен быть установлен
		/// </summary>
		/// <returns>Максимальный внутренний номер договора у клиента</returns>
		public virtual void GenerateSubNumber()
		{
			if(Counterparty is null)
			{
				return;
			} 
			
			if(Counterparty.CounterpartyContracts.Any())
			{
				ContractSubNumber = Counterparty.CounterpartyContracts.Max(c => c.ContractSubNumber) + 1;
			}
			else
			{
				ContractSubNumber = 1;
			}
		}

		#region Функции

		/// <summary>
		/// Updates template for the contract.
		/// </summary>
		/// <returns><c>true</c>, in case of successful update, <c>false</c> if template for the contract was not found.</returns>
		/// <param name="uow">Unit of Work.</param>
		public virtual bool UpdateContractTemplate(IUnitOfWork uow, IDocTemplateRepository docTemplateRepository)
		{
			if(Organization == null)
			{
				DocumentTemplate = null;
				ChangedTemplateFile = null;
			}
			else
			{
				var newTemplate = docTemplateRepository.GetTemplate(uow, TemplateType.Contract, Organization, ContractType);
				if(newTemplate == null)
				{
					DocumentTemplate = null;
					ChangedTemplateFile = null;
					return false;
				}

				if(!DomainHelper.EqualDomainObjects(newTemplate, DocumentTemplate))
				{
					DocumentTemplate = newTemplate;
					ChangedTemplateFile = null;
					return true;
				}
			}

			return false;
		}

		#endregion
	}

	public interface IContractSaved
	{
		event EventHandler<ContractSavedEventArgs> ContractSaved;
	}

	public class ContractSavedEventArgs : EventArgs
	{
		public CounterpartyContract Contract { get; private set; }

		public ContractSavedEventArgs(CounterpartyContract contract)
		{
			Contract = contract;
		}
	}
}


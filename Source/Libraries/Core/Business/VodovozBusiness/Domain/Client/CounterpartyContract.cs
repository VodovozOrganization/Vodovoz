using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.Utilities;
using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.Models;
using Vodovoz.Parameters;

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
		#region Сохраняемые поля

		public virtual int Id { get; set; }

		int maxDelay;

		[Display(Name = "Максимальный срок отсрочки")]
		public virtual int MaxDelay {
			get { return maxDelay; }
			set { SetField(ref maxDelay, value, () => MaxDelay); }
		}

		bool isArchive;

		[Display(Name = "Архивный")]
		public virtual bool IsArchive {
			get { return isArchive; }
			set { SetField(ref isArchive, value, () => IsArchive); }
		}

		bool onCancellation;

		[Display(Name = "На расторжении")]
		public virtual bool OnCancellation {
			get { return onCancellation; }
			set { SetField(ref onCancellation, value, () => OnCancellation); }
		}

		[Display(Name = "Номер")]
		public virtual string Number {
			get => String.Format("{0}-{1}", Counterparty.VodovozInternalId, ContractSubNumber);
			set { }
		}

		DateTime issueDate;

		[Display(Name = "Дата подписания")]
		public virtual DateTime IssueDate {
			get { return issueDate; }
			set { SetField(ref issueDate, value, () => IssueDate); }
		}

		Organization organization;

		[Required(ErrorMessage = "Организация должна быть заполнена.")]
		[Display(Name = "Организация")]
		public virtual Organization Organization {
			get { return organization; }
			set { SetField(ref organization, value, () => Organization); }
		}

		Counterparty counterparty;

		[Required(ErrorMessage = "Контрагент должен быть указан.")]
		[Display(Name = "Контрагент")]
		public virtual Counterparty Counterparty {
			get { return counterparty; }
			set { SetField(ref counterparty, value, () => Counterparty); }
		}

		int contractSubNumber;

		[Display(Name = "Номер договора внутренний")]
		public virtual int ContractSubNumber {
			get { return contractSubNumber; }
			set { SetField(ref contractSubNumber, value, () => ContractSubNumber); }
		}

		ContractType contractType;

		[Display(Name = "Тип договора")]
		public virtual ContractType ContractType {
			get { return contractType; }
			set { SetField(ref contractType, value, () => ContractType); }
		}

		DocTemplate contractTemplate;

		[Display(Name = "Шаблон договора")]
		public virtual DocTemplate DocumentTemplate {
			get { return contractTemplate; }
			protected set { SetField(ref contractTemplate, value, () => DocumentTemplate); }
		}

		byte[] changedTemplateFile;

		[Display(Name = "Измененный договор")]
		//[PropertyChangedAlso("FileSize")]
		public virtual byte[] ChangedTemplateFile {
			get { return changedTemplateFile; }
			set { SetField(ref changedTemplateFile, value, () => ChangedTemplateFile); }
		}

		[Display(Name = "Полный номер договора")]
		//FIXME Удалить дублирование в ContractFullNumber, протому как есть аналогичное посто Number
		public virtual string ContractFullNumber {
			get => String.Format("{0}-{1}", Counterparty.VodovozInternalId, ContractSubNumber);
			set { }
		}

		#endregion

		#region Вычисляемые

		public virtual string Title {
			get { return String.Format("Договор №{0}-{1} от {2:d}", Counterparty.VodovozInternalId, ContractSubNumber, IssueDate); }
		}

		public virtual string TitleIn1c {
			get { return String.Format("{0}-{1} от {2:d}", Counterparty.VodovozInternalId, ContractSubNumber, IssueDate); }
		}

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

		/// <summary>
		/// Вычисляет номер для нового договора.
		/// </summary>
		/// <returns>Максимальный внутренний номер договора у передаваемого клиента</returns>
		/// <param name="counterparty">Клиент</param>
		public virtual void GenerateSubNumber(Counterparty counterparty)
		{
			if(counterparty.CounterpartyContracts.Any())
			{
				ContractSubNumber = counterparty.CounterpartyContracts.Max(c => c.ContractSubNumber) + 1;
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
			if(Organization == null) {
				DocumentTemplate = null;
				ChangedTemplateFile = null;
			} else {
				var newTemplate = docTemplateRepository.GetTemplate(uow, TemplateType.Contract, Organization, ContractType);
				if(newTemplate == null) {
					DocumentTemplate = null;
					ChangedTemplateFile = null;
					return false;
				}
				if(!DomainHelper.EqualDomainObjects(newTemplate, DocumentTemplate)) {
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


using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Core.Domain.Clients;
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
	public class CounterpartyContract : CounterpartyContractEntity, IDomainObject, IBusinessObject, IValidatableObject
	{
		private Organization _organization;
		private Counterparty _counterparty;
		private DocTemplate _contractTemplate;

		public virtual IUnitOfWork UoW { get; set; }

		#region Сохраняемые поля

		[Required(ErrorMessage = "Организация должна быть заполнена.")]
		[Display(Name = "Организация")]
		public virtual new Organization Organization
		{
			get => _organization;
			set => SetField(ref _organization, value);
		}

		[Required(ErrorMessage = "Контрагент должен быть указан.")]
		[Display(Name = "Контрагент")]
		public virtual new Counterparty Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}

		[Display(Name = "Шаблон договора")]
		public virtual DocTemplate DocumentTemplate
		{
			get => _contractTemplate;
			protected set => SetField(ref _contractTemplate, value);
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


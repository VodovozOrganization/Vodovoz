using QS.DomainModel.UoW;
using System;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Organizations;

namespace Vodovoz.Domain.Cash
{
	public class IncomeCashOrganisationDistributor : IIncomeCashOrganisationDistributor
	{
		private readonly IOrganizationRepository _organizationRepository;

		public IncomeCashOrganisationDistributor(IOrganizationRepository organizationRepository)
		{
			this._organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
		}

		public void DistributeCashForIncome(IUnitOfWork uow, Income income, Organization organisation = null)
		{
			var org = organisation ?? _organizationRepository.GetCommonOrganisation(uow);
			var operation = CreateOrganisationCashMovementOperation(income, org);
			var incomeCashDistributionDoc = CreateIncomeCashDistributionDocument(income, operation);
			Save(operation, incomeCashDistributionDoc, uow);
		}

		public void UpdateRecords(IUnitOfWork uow, IncomeCashDistributionDocument document, Income income, Employee editor)
		{
			UpdateIncomeCashDistributionDocument(document, income, editor);
			UpdateOrganisationCashMovementOperation(document.OrganisationCashMovementOperation, income);
			Save(document.OrganisationCashMovementOperation, document, uow);
		}

		private void Save(OrganisationCashMovementOperation operation, IncomeCashDistributionDocument document, IUnitOfWork uow)
		{
			uow.Save(operation);
			uow.Save(document);
		}

		private IncomeCashDistributionDocument CreateIncomeCashDistributionDocument(Income income,
			OrganisationCashMovementOperation operation)
		{
			return new IncomeCashDistributionDocument
			{
				Author = income.Casher,
				Income = income,
				Organisation = operation.Organisation,
				CreationDate = DateTime.Now,
				LastEditor = income.Casher,
				Employee = income.Employee,
				LastEditedTime = DateTime.Now,
				OrganisationCashMovementOperation = operation,
				Amount = operation.Amount
			};
		}

		private OrganisationCashMovementOperation CreateOrganisationCashMovementOperation(Income income, Organization org)
		{
			return new OrganisationCashMovementOperation
			{
				Organisation = org,
				OperationTime = DateTime.Now,
				Amount = income.Money
			};
		}

		private void UpdateIncomeCashDistributionDocument(IncomeCashDistributionDocument doc, Income income, Employee editor)
		{
			doc.LastEditor = editor;
			doc.LastEditedTime = DateTime.Now;
			doc.Amount = income.Money;
		}

		private void UpdateOrganisationCashMovementOperation(OrganisationCashMovementOperation operation, Income income)
		{
			operation.Amount = income.Money;
			operation.OperationTime = DateTime.Now;
		}
	}
}

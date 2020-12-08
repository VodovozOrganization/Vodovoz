using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Cash
{
    public class AdvanceCashOrganisationDistributor
    {
        public void DistributeCashForIncomeAdvance(IUnitOfWork uow, Income income, AdvanceReport advanceReport)
        {
            var operation = CreateOrganisationCashMovementOperation(advanceReport);
            operation.Amount = income.Money;

            var advanceIncomeCashDistributionDoc = CreateAdvanceIncomeCashDistributionDocument(advanceReport, operation);
            SaveIncome(uow, operation, advanceIncomeCashDistributionDoc);
        }

        public void DistributeCashForExpenseAdvance(IUnitOfWork uow, Expense expense, AdvanceReport advanceReport)
        {
            var operation = CreateOrganisationCashMovementOperation(advanceReport);
            operation.Amount = -expense.Money;

            var advanceExpenseCashDistributionDoc = CreateAdvanceExpenseCashDistributionDocument(advanceReport, operation);
            SaveExpense(uow, operation, advanceExpenseCashDistributionDoc);
        }

        private AdvanceIncomeCashDistributionDocument CreateAdvanceIncomeCashDistributionDocument(AdvanceReport advanceReport,
            OrganisationCashMovementOperation operation)
        {
            return new AdvanceIncomeCashDistributionDocument
            {
                AdvanceReport = advanceReport,
                CashExpenseCategory = advanceReport.ExpenseCategory,
                Author = advanceReport.Casher,
                CreationDate = DateTime.Now,
                Organisation = advanceReport.Organisation,
                LastEditor = advanceReport.Casher,
                LastEditedTime = DateTime.Now,
                OrganisationCashMovementOperation = operation,
                Amount = operation.Amount
            };
        }
        
        private AdvanceExpenseCashDistributionDocument CreateAdvanceExpenseCashDistributionDocument(AdvanceReport advanceReport,
            OrganisationCashMovementOperation operation)
        {
            return new AdvanceExpenseCashDistributionDocument
            {
                AdvanceReport = advanceReport,
                CashExpenseCategory = advanceReport.ExpenseCategory,
                Author = advanceReport.Casher,
                CreationDate = DateTime.Now,
                Organisation = advanceReport.Organisation,
                LastEditor = advanceReport.Casher,
                LastEditedTime = DateTime.Now,
                OrganisationCashMovementOperation = operation,
                Amount = operation.Amount
            };
        }

        private OrganisationCashMovementOperation CreateOrganisationCashMovementOperation(AdvanceReport advanceReport)
        {
            return new OrganisationCashMovementOperation
            {
                OperationTime = DateTime.Now,
                Organisation = advanceReport.Organisation
            };
        }
        
        private void SaveIncome(IUnitOfWork uow, OrganisationCashMovementOperation operation, 
            AdvanceIncomeCashDistributionDocument advanceIncomeCashDistributionDoc)
        {
            uow.Save(operation);
            uow.Save(advanceIncomeCashDistributionDoc);
        }
        
        private void SaveExpense(IUnitOfWork uow, OrganisationCashMovementOperation operation, 
            AdvanceExpenseCashDistributionDocument advanceExpenseCashDistributionDoc)
        {
            uow.Save(operation);
            uow.Save(advanceExpenseCashDistributionDoc);
        }
    }
}
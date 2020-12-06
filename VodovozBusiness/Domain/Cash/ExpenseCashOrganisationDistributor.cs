using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Cash
{
    public class ExpenseCashOrganisationDistributor
    {
        public void DistributeCashForExpense(IUnitOfWork uow, Expense expense, bool isSalary = false)
        {
            var operation = CreateOrganisationCashMovementOperation(expense, isSalary);
            var expenseCashDistributionDoc = CreateExpenseCashDistributionDocument(expense, operation);
            Save(operation, expenseCashDistributionDoc, uow);
        }

        private void Save(OrganisationCashMovementOperation operation, ExpenseCashDistributionDocument document, IUnitOfWork uow)
        {
            uow.Save(operation);
            uow.Save(document);
        }

        private ExpenseCashDistributionDocument CreateExpenseCashDistributionDocument(Expense expense, 
            OrganisationCashMovementOperation operation)
        {
            return new ExpenseCashDistributionDocument
            {
                Author = expense.Casher,
                Expense = expense,
                CreationDate = DateTime.Now,
                LastEditor = expense.Casher,
                Organisation = operation.Organisation,
                CashExpenseCategory = expense.ExpenseCategory,
                LastEditedTime = DateTime.Now,
                OrganisationCashMovementOperation = operation,
                Amount = operation.Amount
            };
        }

        private OrganisationCashMovementOperation CreateOrganisationCashMovementOperation(Expense expense, bool isSalary)
        {
            return new OrganisationCashMovementOperation
            {
                Organisation = isSalary ? expense.Employee.OrganisationForSalary : expense.Organisation,
                OperationTime = DateTime.Now,
                Amount = -expense.Money
            };
        }
    }
}
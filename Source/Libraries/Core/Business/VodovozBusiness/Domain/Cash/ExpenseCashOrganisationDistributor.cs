using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Cash
{
	public class ExpenseCashOrganisationDistributor : IExpenseCashOrganisationDistributor
	{
		public void DistributeCashForExpense(IUnitOfWork uow, Expense expense, bool isSalary = false)
		{
			var operation = CreateOrganisationCashMovementOperation(expense, isSalary);
			var expenseCashDistributionDoc = CreateExpenseCashDistributionDocument(expense, operation);

			if(isSalary)
			{
				expense.Organisation = operation.Organisation;
			}

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
				Employee = expense.Employee,
				Organisation = operation.Organisation,
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

		public void UpdateRecords(IUnitOfWork uow, ExpenseCashDistributionDocument document, Expense expense, Employee editor)
		{
			UpdateExpenseCashDistributionDocument(document, expense, editor);
			UpdateOrganisationCashMovementOperation(document.OrganisationCashMovementOperation, expense);
			Save(document.OrganisationCashMovementOperation, document, uow);
		}

		private void UpdateExpenseCashDistributionDocument(ExpenseCashDistributionDocument doc, Expense expense, Employee editor)
		{
			doc.LastEditor = editor;
			doc.LastEditedTime = DateTime.Now;
			doc.Amount = -expense.Money;
		}

		private void UpdateOrganisationCashMovementOperation(OrganisationCashMovementOperation operation, Expense expense)
		{
			operation.Amount = -expense.Money;
			operation.OperationTime = DateTime.Now;
		}
	}
}

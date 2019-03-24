using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Cash.CashTransfer
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "документы перемещения денежных средств по ордерам",
		Nominative = "документ перемещения денежных средств по ордерам",
		Prepositional = "документе перемещения денежных средств по ордерам",
		PrepositionalPlural = "документах перемещения общих денежных средств по ордерам"
	)]
	public class IncomeCashTransferDocument : CashTransferDocumentBase
	{
		IList<IncomeCashTransferedItem> cashTransferDocumentIncomeItems = new List<IncomeCashTransferedItem>();
		[Display(Name = "Строки документа транспортировки с приходными ордерами")]
		public virtual IList<IncomeCashTransferedItem> CashTransferDocumentIncomeItems {
			get { return cashTransferDocumentIncomeItems; }
			set { SetField(ref cashTransferDocumentIncomeItems, value, () => CashTransferDocumentIncomeItems); }
		}

		GenericObservableList<IncomeCashTransferedItem> observableCashTransferDocumentIncomeItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<IncomeCashTransferedItem> ObservableCashTransferDocumentIncomeItems {
			get {
				if(observableCashTransferDocumentIncomeItems == null) {
					observableCashTransferDocumentIncomeItems = new GenericObservableList<IncomeCashTransferedItem>(CashTransferDocumentIncomeItems);
					observableCashTransferDocumentIncomeItems.ListContentChanged +=
					(obj, e) => {
						OnPropertyChanged(() => IncomesSummary);
						UpdateTransferedSum();
					};
				}
				return observableCashTransferDocumentIncomeItems;
			}
		}

		IList<ExpenseCashTransferedItem> cashTransferDocumentExpenseItems = new List<ExpenseCashTransferedItem>();
		[Display(Name = "Строки документа транспортировки с расходными ордерами")]
		public virtual IList<ExpenseCashTransferedItem> CashTransferDocumentExpenseItems {
			get { return cashTransferDocumentExpenseItems; }
			set { SetField(ref cashTransferDocumentExpenseItems, value, () => CashTransferDocumentExpenseItems); }
		}

		GenericObservableList<ExpenseCashTransferedItem> observableCashTransferDocumentExpenseItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<ExpenseCashTransferedItem> ObservableCashTransferDocumentExpenseItems {
			get {
				if(observableCashTransferDocumentExpenseItems == null) {
					observableCashTransferDocumentExpenseItems = new GenericObservableList<ExpenseCashTransferedItem>(CashTransferDocumentExpenseItems);
					ObservableCashTransferDocumentExpenseItems.ListContentChanged +=
					(obj, e) => {
						OnPropertyChanged(() => ExpensesSummary);
						UpdateTransferedSum();
					};
				}
				return observableCashTransferDocumentExpenseItems;
			}
		}

		public virtual decimal IncomesSummary => CashTransferDocumentIncomeItems.Sum(x => x.IncomeMoney);
		public virtual decimal ExpensesSummary => CashTransferDocumentExpenseItems.Sum(x => x.ExpenseMoney);

		protected override decimal CalculateTransferedSum()
		{
			decimal incomeMoneySum = CashTransferDocumentIncomeItems.Sum(x => x.IncomeMoney);
			decimal expenseMoneySum = CashTransferDocumentExpenseItems.Sum(x => x.ExpenseMoney);
			return incomeMoneySum - expenseMoneySum;
		}

		protected virtual void UpdateTransferedSum()
		{
			TransferedSum = CalculateTransferedSum();
		}

		public virtual void DeleteTransferedIncomes(IEnumerable<IncomeCashTransferedItem> deletedItems)
		{
			if(deletedItems == null) {
				return;
			}
			foreach(IncomeCashTransferedItem item in deletedItems) {
				if(ObservableCashTransferDocumentIncomeItems.Contains(item)) {
					ObservableCashTransferDocumentIncomeItems.Remove(item);
				}
			}
		}

		public virtual void DeleteTransferedExpenses(IEnumerable<ExpenseCashTransferedItem> deletedItems)
		{
			if(deletedItems == null) {
				return;
			}
			foreach(ExpenseCashTransferedItem item in deletedItems) {
				if(ObservableCashTransferDocumentExpenseItems.Contains(item)) {
					ObservableCashTransferDocumentExpenseItems.Remove(item);
				}
			}
		}

		public virtual void AddIncomeItem(Income income)
		{
			if(!CashTransferDocumentIncomeItems.Any(x => x.Income.Id == income.Id)) {
				var newItem = new IncomeCashTransferedItem {
					Income = income,
					Document = this
				};
				ObservableCashTransferDocumentIncomeItems.Add(newItem);
				newItem.Income.TransferedBy = newItem;
			}
		}

		public virtual void AddExpenseItem(Expense expense)
		{
			if(!CashTransferDocumentExpenseItems.Any(x => x.Expense.Id == expense.Id)) {
				var newItem = new ExpenseCashTransferedItem {
					Expense = expense,
					Document = this
				};
				ObservableCashTransferDocumentExpenseItems.Add(newItem);
				newItem.Expense.TransferedBy = newItem;
			}
		}
	}
}

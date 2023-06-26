using QS.DomainModel.Entity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;

namespace Vodovoz.Domain.Cash.CashTransfer
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "документы перемещения денежных средств по ордерам",
		Nominative = "документ перемещения денежных средств по ордерам",
		Prepositional = "документе перемещения денежных средств по ордерам",
		PrepositionalPlural = "документах перемещения общих денежных средств по ордерам",
		Accusative = "документ перемещения денежных средств по ордерам"
	)]
	public class IncomeCashTransferDocument : CashTransferDocumentBase
	{
		private IList<IncomeCashTransferedItem> _cashTransferDocumentIncomeItems = new List<IncomeCashTransferedItem>();
		private GenericObservableList<IncomeCashTransferedItem> _observableCashTransferDocumentIncomeItems;
		private IList<ExpenseCashTransferedItem> _cashTransferDocumentExpenseItems = new List<ExpenseCashTransferedItem>();
		private GenericObservableList<ExpenseCashTransferedItem> _observableCashTransferDocumentExpenseItems;

		[Display(Name = "Строки документа транспортировки с приходными ордерами")]
		public virtual IList<IncomeCashTransferedItem> CashTransferDocumentIncomeItems
		{
			get { return _cashTransferDocumentIncomeItems; }
			set { SetField(ref _cashTransferDocumentIncomeItems, value); }
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<IncomeCashTransferedItem> ObservableCashTransferDocumentIncomeItems
		{
			get
			{
				if(_observableCashTransferDocumentIncomeItems == null)
				{
					_observableCashTransferDocumentIncomeItems = new GenericObservableList<IncomeCashTransferedItem>(CashTransferDocumentIncomeItems);
					_observableCashTransferDocumentIncomeItems.ListContentChanged +=
					(obj, e) =>
					{
						OnPropertyChanged(() => IncomesSummary);
						UpdateTransferedSum();
					};
				}
				return _observableCashTransferDocumentIncomeItems;
			}
		}

		[Display(Name = "Строки документа транспортировки с расходными ордерами")]
		public virtual IList<ExpenseCashTransferedItem> CashTransferDocumentExpenseItems
		{
			get => _cashTransferDocumentExpenseItems;
			set => SetField(ref _cashTransferDocumentExpenseItems, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<ExpenseCashTransferedItem> ObservableCashTransferDocumentExpenseItems
		{
			get
			{
				if(_observableCashTransferDocumentExpenseItems == null)
				{
					_observableCashTransferDocumentExpenseItems = new GenericObservableList<ExpenseCashTransferedItem>(CashTransferDocumentExpenseItems);
					ObservableCashTransferDocumentExpenseItems.ListContentChanged +=
					(obj, e) =>
					{
						OnPropertyChanged(() => ExpensesSummary);
						UpdateTransferedSum();
					};
				}

				return _observableCashTransferDocumentExpenseItems;
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
			if(deletedItems == null)
			{
				return;
			}

			foreach(IncomeCashTransferedItem item in deletedItems)
			{
				if(ObservableCashTransferDocumentIncomeItems.Contains(item))
				{
					ObservableCashTransferDocumentIncomeItems.Remove(item);
				}
			}
		}

		public virtual void DeleteTransferedExpenses(IEnumerable<ExpenseCashTransferedItem> deletedItems)
		{
			if(deletedItems == null)
			{
				return;
			}

			foreach(ExpenseCashTransferedItem item in deletedItems)
			{
				if(ObservableCashTransferDocumentExpenseItems.Contains(item))
				{
					ObservableCashTransferDocumentExpenseItems.Remove(item);
				}
			}
		}

		public virtual void AddIncomeItem(Income income)
		{
			if(!CashTransferDocumentIncomeItems.Any(x => x.Income.Id == income.Id))
			{
				var newItem = new IncomeCashTransferedItem
				{
					Income = income,
					Document = this
				};
				ObservableCashTransferDocumentIncomeItems.Add(newItem);
				newItem.Income.TransferedBy = newItem;
			}
		}

		public virtual void AddExpenseItem(Expense expense)
		{
			if(!CashTransferDocumentExpenseItems.Any(x => x.Expense.Id == expense.Id))
			{
				var newItem = new ExpenseCashTransferedItem
				{
					Expense = expense,
					Document = this
				};
				ObservableCashTransferDocumentExpenseItems.Add(newItem);
				newItem.Expense.TransferedBy = newItem;
			}
		}
	}
}

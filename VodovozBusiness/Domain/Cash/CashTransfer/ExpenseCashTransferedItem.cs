using System;
using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Cash.CashTransfer
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "расходные ордера документа перемещения денежных средств по ордерам",
		Nominative = "расходный ордер документа перемещения денежных средств по ордерам"
	)]
	public class ExpenseCashTransferedItem : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		private IncomeCashTransferDocument document;
		[Display(Name = "Документ перемещения")]
		public virtual IncomeCashTransferDocument Document {
			get => document;
			set => SetField(ref document, value, () => Document);
		}

		private Expense expense;
		[Display(Name = "Расходный ордер")]
		public virtual Expense Expense {
			get => expense;
			set => SetField(ref expense, value, () => Expense);
		}

		private string comment;
		[Display(Name = "Комментарий к перемещению")]
		public virtual string Comment {
			get => comment;
			set => SetField(ref comment, value, () => Comment);
		}

		public decimal ExpenseMoney => Expense.Money;

		public ExpenseCashTransferedItem()
		{
		}

		public ExpenseCashTransferedItem(Expense expense)
		{
			this.expense = expense;
		}
	}
}

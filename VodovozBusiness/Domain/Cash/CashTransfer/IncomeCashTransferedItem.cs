using System;
using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Cash.CashTransfer
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "приходные ордера документа перемещения денежных средств по ордерам",
		Nominative = "приходный ордер документа перемещения денежных средств по ордерам"
	)]
	public class IncomeCashTransferedItem : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		private IncomeCashTransferDocument document;
		[Display(Name = "Документ перемещения")]
		public virtual IncomeCashTransferDocument Document {
			get => document;
			set => SetField(ref document, value, () => Document);
		}

		private Income income;
		[Display(Name = "Приходный ордер")]
		public virtual Income Income {
			get => income;
			set => SetField(ref income, value, () => Income);
		}

		private string comment;
		[Display(Name = "Комментарий к перемещению")]
		public virtual string Comment {
			get => comment;
			set => SetField(ref comment, value, () => Comment);
		}

		public decimal IncomeMoney => Income.Money;

		public IncomeCashTransferedItem()
		{
		}
	}
}

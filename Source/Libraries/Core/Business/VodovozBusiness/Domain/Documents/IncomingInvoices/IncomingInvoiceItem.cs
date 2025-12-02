using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Cash;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents.IncomingInvoices
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки входящей накладной",
		Nominative = "строка входящей накладной")]
	[HistoryTrace]
	public abstract class IncomingInvoiceItem : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private decimal _amount;
		private decimal _primeCost;
		private Nomenclature _nomenclature;
		private VatRate _vatRate;

		public virtual int Id { get; set; }

		public virtual IncomingInvoice Document { get; set; }
		
		[Required(ErrorMessage = "Номенклатура должна быть заполнена.")]
		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature
		{
			get => _nomenclature;
			set
			{
				if(!SetField(ref _nomenclature, value))
				{
					return;
				}
				GoodsAccountingOperation.Nomenclature = value;
			}
		}

		[Display(Name = "Количество")]
		public virtual decimal Amount
		{
			get => _amount;
			set
			{
				if(!SetField(ref _amount, value))
				{
					return;
				}
				GoodsAccountingOperation.Amount = value;
			}
		}

		[Display(Name = "Цена")]
		public virtual decimal PrimeCost
		{
			get => _primeCost;
			set => SetField(ref _primeCost, value);
		}
		
		public virtual VatRate VatRate
		{
			get => _vatRate;
			set => SetField(ref _vatRate, value);
		}

		public virtual decimal Sum => PrimeCost * Amount;

		public abstract string Name { get; }

		public abstract string InventoryNumberString { get; }

		public abstract bool CanEditAmount { get; }

		public virtual GoodsAccountingOperation GoodsAccountingOperation { get; set; }

		public abstract void UpdateWarehouseOperation();

		public abstract AccountingType AccountingType { get; }
		public abstract int EntityId { get; }
		
		public override string ToString() => $"[{Document.Title}] {Nomenclature.Name} - {Nomenclature.Unit.MakeAmountShortStr(Amount)}";

		public virtual string Title => $"[{Document.Title}] {Nomenclature.Name} - {Nomenclature.Unit.MakeAmountShortStr(Amount)}";

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Amount < 1)
			{
				yield return new ValidationResult("Количество должно быть больше 1", new [] { nameof(Amount) });
			}

			if(PrimeCost < 0)
			{
				yield return new ValidationResult("Цена должна быть больше 0", new[] { nameof(PrimeCost) });
			}
		}
	}
}

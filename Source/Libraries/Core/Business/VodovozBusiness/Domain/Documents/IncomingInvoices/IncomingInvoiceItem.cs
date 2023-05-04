using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents.IncomingInvoices
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки входящей накладной",
		Nominative = "строка входящей накладной")]
	[HistoryTrace]
	public class IncomingInvoiceItem : PropertyChangedBase, IDomainObject
	{
		private decimal _amount;
		private decimal _primeCost;
		private VAT _vat;
		private Nomenclature _nomenclature;

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
				UpdateOperation(nameof(GoodsAccountingOperation.Nomenclature), value);
			}
		}

		[Min(1)]
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
				UpdateOperation(nameof(GoodsAccountingOperation.Amount), value);
			}
		}

		[Min(0)]
		[Display(Name = "Цена")]
		public virtual decimal PrimeCost
		{
			get => _primeCost;
			set => SetField(ref _primeCost, value);
		}

		public virtual VAT VAT
		{
			get => _vat;
			set => SetField(ref _vat, value);
		}

		public virtual decimal Sum => PrimeCost * Amount;

		public virtual string Name { get; }

		public virtual string InventoryNumberString { get; }

		public virtual bool CanEditAmount { get; }

		public virtual GoodsAccountingOperation GoodsAccountingOperation { get; set; }

		public virtual void UpdateOperation(string propertyName, object value)
		{
			if(GoodsAccountingOperation is null)
			{
				return;
			}

			var property = GoodsAccountingOperation.GetPropertyValue(propertyName);

			if(property != value)
			{
				GoodsAccountingOperation.SetPropertyValue(propertyName, value);
			}
		}
		
		public virtual AccountingType AccountingType { get; }
		
		public override string ToString() => $"[{Document.Title}] {Nomenclature.Name} - {Nomenclature.Unit.MakeAmountShortStr(Amount)}";
		
		public virtual int EntityId { get; }
		
		public virtual string Title => $"[{Document.Title}] {Nomenclature.Name} - {Nomenclature.Unit.MakeAmountShortStr(Amount)}";
	}

	public enum AccountingType
	{
		Bulk,
		Instance
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Documents.IncomingInvoices;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents.WriteOffDocuments
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки списания",
		Nominative = "строка списания")]
	[HistoryTrace]
	public abstract class WriteOffDocumentItem : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private Nomenclature _nomenclature;
		private CullingCategory _cullingCategory;
		private decimal _amount;
		private string _comment;
		private Fine _fine;
		private decimal _amountOnStock = 10000000;
		private GoodsAccountingOperation _goodsAccountingOperation;

		public virtual int Id { get; set; }

		public virtual WriteOffDocument Document { get; set; }

		[Required(ErrorMessage = "Номенклатура должна быть заполнена.")]
		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature
		{
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}

		[Display(Name = "Вид выбраковки")]
		public virtual CullingCategory CullingCategory
		{
			get => _cullingCategory;
			set => SetField(ref _cullingCategory, value);
		}

		[Display(Name = "Количество")]
		[PropertyChangedAlso("SumOfDamage")]
		public virtual decimal Amount
		{
			get => _amount;
			set
			{
				if(!SetField(ref _amount, value))
				{
					return;
				}

				if(GoodsAccountingOperation is null)
				{
					return;
				}
				GoodsAccountingOperation.Amount = -value;
			}
		}

		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}

		[Display(Name = "Сумма ущерба")]
		public virtual decimal SumOfDamage =>
			Document?.WriteOffType == WriteOffType.Car
			? Nomenclature.GetPurchasePriceOnDate(Document.TimeStamp) * Amount
			: Nomenclature.SumOfDamage * Amount;

		[Display(Name = "Штраф")]
		public virtual Fine Fine
		{
			get => _fine;
			set => SetField (ref _fine, value);
		}

		//FIXME пока не реализуем способ загружать количество на складе на конкретный день
		[Display(Name = "Имеется на складе")]
		public virtual decimal AmountOnStock
		{
			get => _amountOnStock;
			set => SetField(ref _amountOnStock, value);
		}

		public abstract WriteOffDocumentItemType Type { get; }
		public abstract AccountingType AccountingType { get; }

		public virtual string Title => $"[{Document.Title}] {Nomenclature.Name} - {Nomenclature.Unit.MakeAmountShortStr(Amount)}";

		public virtual string Name => Nomenclature != null ? Nomenclature.Name : "";
		public virtual string InventoryNumber => "-";
		public virtual string CullingCategoryString => CullingCategory != null ? CullingCategory.Name : "-";
		public virtual bool CanEditAmount => Nomenclature != null && !Nomenclature.IsSerial;

		public virtual GoodsAccountingOperation GoodsAccountingOperation
		{
			get => _goodsAccountingOperation;
			set => SetField(ref _goodsAccountingOperation, value);
		}

		protected virtual void FillOperation()
		{
			if(GoodsAccountingOperation is null)
			{
				throw new InvalidOperationException("Не создана операция списания!");
			}

			GoodsAccountingOperation.Amount = -Amount;
			GoodsAccountingOperation.Nomenclature = Nomenclature;
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Amount < 1)
			{
				yield return new ValidationResult("Количество должно быть больше 1", new[] { nameof(Amount) });
			}

			if((Type == WriteOffDocumentItemType.InstanceWriteOffFromCarDocumentItem 
				|| Type == WriteOffDocumentItemType.BulkWriteOffFromCarDocumentItem) 
					&& CullingCategory is null)
			{
				yield return new ValidationResult("Поле \"Причина выбраковки\" не должно быть пустым", new[] { nameof(Type) });
			}
		}
	}
}

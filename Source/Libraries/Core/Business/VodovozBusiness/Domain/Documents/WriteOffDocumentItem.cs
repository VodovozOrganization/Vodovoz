﻿using System;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки списания",
		Nominative = "строка списания")]
	[HistoryTrace]
	public class WriteOffDocumentItem: PropertyChangedBase, IDomainObject
	{
		private Nomenclature _nomenclature;
		private CullingCategory _cullingCategory;
		private decimal _amount;
		private string _comment;
		private Fine _fine;
		private decimal _amountOnStock = 10000000;
		private GoodsAccountingOperation _warehouseWriteOffOperation;
		//private CounterpartyMovementOperation _counterpartyWriteoffOperation;

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

		[Min(1)]
		[Display(Name = "Количество")]
		[PropertyChangedAlso("SumOfDamage")]
		public virtual decimal Amount
		{
			get => _amount;
			set => SetField(ref _amount, value);
		}

		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}

		[Display(Name = "Сумма ущерба")]
		public virtual decimal SumOfDamage => Nomenclature.SumOfDamage * Amount;

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

		public virtual AccountingType AccountingType { get; }

		public virtual string Name => Nomenclature != null ? Nomenclature.Name : "";
		public virtual string InventoryNumber => "-";
		public virtual string CullingCategoryString => CullingCategory != null ? CullingCategory.Name : "-";
		public virtual bool CanEditAmount => Nomenclature != null && !Nomenclature.IsSerial;

		public virtual GoodsAccountingOperation WarehouseWriteOffOperation
		{
			get => _warehouseWriteOffOperation;
			set => SetField(ref _warehouseWriteOffOperation, value);
		}

		/*public virtual CounterpartyMovementOperation CounterpartyWriteoffOperation
		{
			get => _counterpartyWriteoffOperation;
			set => SetField (ref _counterpartyWriteoffOperation, value);
		}*/

		protected virtual void FillOperation()
		{
			if(WarehouseWriteOffOperation is null)
			{
				throw new InvalidOperationException("Не создана операция списания!");
			}

			WarehouseWriteOffOperation.Amount = -Amount;
			WarehouseWriteOffOperation.Nomenclature = Nomenclature;
		}

		public virtual string Title => $"[{Document.Title}] {Nomenclature.Name} - {Nomenclature.Unit.MakeAmountShortStr(Amount)}";
	}
}


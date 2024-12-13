using QS.DomainModel.Entity;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки пересортицы",
		Nominative = "строка пересортицы")]
	[HistoryTrace]
	public class RegradingOfGoodsDocumentItem : PropertyChangedBase, IDomainObject
	{
		private RegradingOfGoodsReason _regradingOfGoodsReason;

		public virtual int Id { get; set; }

		public virtual RegradingOfGoodsDocument Document { get; set; }

		private Nomenclature _nomenclatureOld;

		[Required(ErrorMessage = "Старая номенклатура должна быть заполнена.")]
		[Display(Name = "Старая номенклатура")]
		public virtual Nomenclature NomenclatureOld
		{
			get => _nomenclatureOld;
			set
			{
				SetField(ref _nomenclatureOld, value);

				if(WarehouseWriteOffOperation != null && WarehouseWriteOffOperation.Nomenclature != _nomenclatureOld)
				{
					WarehouseWriteOffOperation.Nomenclature = _nomenclatureOld;
				}
			}
		}

		private Nomenclature _nomenclatureNew;

		[Required(ErrorMessage = "Новая номенклатура должна быть заполнена.")]
		[Display(Name = "Новая номенклатура")]
		public virtual Nomenclature NomenclatureNew
		{
			get => _nomenclatureNew;
			set
			{
				SetField(ref _nomenclatureNew, value);

				if(WarehouseIncomeOperation != null && WarehouseIncomeOperation.Nomenclature != _nomenclatureNew)
				{
					WarehouseIncomeOperation.Nomenclature = _nomenclatureNew;
				}
			}
		}

		private decimal _amount;

		[Display(Name = "Количество")]
		public virtual decimal Amount
		{
			get => _amount;
			set
			{
				SetField(ref _amount, value);

				if(WarehouseIncomeOperation != null && WarehouseIncomeOperation.Amount != Amount)
				{
					WarehouseIncomeOperation.Amount = Amount;
				}

				if(WarehouseWriteOffOperation != null && WarehouseWriteOffOperation.Amount != Amount)
				{
					WarehouseWriteOffOperation.Amount = -Amount;
				}
			}
		}

		private string _comment;

		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}

		private Fine _fine;

		[Display(Name = "Штраф")]
		public virtual Fine Fine
		{
			get => _fine;
			set => SetField(ref _fine, value);
		}

		private CullingCategory _typeOfDefect;
		[Display(Name = "Тип брака")]
		public virtual CullingCategory TypeOfDefect
		{
			get => _typeOfDefect;
			set => SetField(ref _typeOfDefect, value);
		}

		private DefectSource _source;
		[Display(Name = "Источник брака")]
		public virtual DefectSource Source
		{
			get => _source;
			set => SetField(ref _source, value);
		}

		private WarehouseBulkGoodsAccountingOperation _warehouseWriteOffOperation = new WarehouseBulkGoodsAccountingOperation();

		public virtual WarehouseBulkGoodsAccountingOperation WarehouseWriteOffOperation
		{
			get => _warehouseWriteOffOperation;
			set => SetField(ref _warehouseWriteOffOperation, value);
		}

		private WarehouseBulkGoodsAccountingOperation _warehouseIncomeOperation = new WarehouseBulkGoodsAccountingOperation();

		public virtual WarehouseBulkGoodsAccountingOperation WarehouseIncomeOperation
		{
			get => _warehouseIncomeOperation;
			set => SetField(ref _warehouseIncomeOperation, value);
		}

		[Display(Name = "Причина пересортицы")]
		public virtual RegradingOfGoodsReason RegradingOfGoodsReason
		{
			get => _regradingOfGoodsReason;
			set => SetField(ref _regradingOfGoodsReason, value);
		}

		#region Не сохраняемые

		[Display(Name = "Сумма ущерба")]
		public virtual decimal SumOfDamage => Amount == 0 ? 0 : NomenclatureOld.SumOfDamage * Amount;

		private decimal _amountInStock;

		[Display(Name = "Количество на складе")]
		public virtual decimal AmountInStock
		{
			get => _amountInStock;
			set => SetField(ref _amountInStock, value);
		}

		#endregion

		#region Расчетные

		public virtual string Title
		{
			get
			{
				return String.Format("{0} -> {1} - {2}",
					NomenclatureOld.Name,
					NomenclatureNew.Name,
					NomenclatureOld.Unit.MakeAmountShortStr(Amount));
			}
		}

		#endregion

		#region Функции

		public virtual void CreateOperation(Warehouse warehouse, DateTime time)
		{
			WarehouseWriteOffOperation = new WarehouseBulkGoodsAccountingOperation
			{
				Warehouse = warehouse,
				Amount = -Amount,
				OperationTime = time,
				Nomenclature = NomenclatureOld
			};
			WarehouseIncomeOperation = new WarehouseBulkGoodsAccountingOperation
			{
				Warehouse = warehouse,
				Amount = Amount,
				OperationTime = time,
				Nomenclature = NomenclatureNew
			};
		}

		#endregion
	}
}

using QS.DomainModel.Entity;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;

namespace VodovozBusiness.Domain.Documents
{
	/// <summary>
	/// Строка пересортицы товаров
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки пересортицы",
		Nominative = "строка пересортицы")]
	[HistoryTrace]
	public class RegradingOfGoodsDocumentItem : PropertyChangedBase, IDomainObject
	{
		private RegradingOfGoodsReason _regradingOfGoodsReason;
		private Nomenclature _nomenclatureOld;
		private Nomenclature _nomenclatureNew;
		private decimal _amount;
		private string _comment;
		private Fine _fine;
		private CullingCategory _typeOfDefect;
		private WarehouseBulkGoodsAccountingOperation _warehouseWriteOffOperation = new WarehouseBulkGoodsAccountingOperation();
		private WarehouseBulkGoodsAccountingOperation _warehouseIncomeOperation = new WarehouseBulkGoodsAccountingOperation();
		private DefectSource _source;
		private decimal _amountInStock;

		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual int Id { get; set; }

		/// <summary>
		/// Документ пересортицы потоваро (Не использовать, необходимо для NHibernate)
		/// </summary>
		public virtual RegradingOfGoodsDocument Document { get; set; }

		/// <summary>
		/// Старая номенклатура
		/// </summary>
		[Required(ErrorMessage = "Старая номенклатура должна быть заполнена.")]
		[Display(Name = "Старая номенклатура")]
		public virtual Nomenclature NomenclatureOld
		{
			get => _nomenclatureOld;
			set
			{
				SetField(ref _nomenclatureOld, value);

				if(WarehouseWriteOffOperation != null
					&& WarehouseWriteOffOperation.Nomenclature != _nomenclatureOld)
				{
					WarehouseWriteOffOperation.Nomenclature = _nomenclatureOld;
				}
			}
		}

		/// <summary>
		/// Новая номенклатура
		/// </summary>
		[Required(ErrorMessage = "Новая номенклатура должна быть заполнена.")]
		[Display(Name = "Новая номенклатура")]
		public virtual Nomenclature NomenclatureNew
		{
			get => _nomenclatureNew;
			set
			{
				SetField(ref _nomenclatureNew, value);

				if(WarehouseIncomeOperation != null
					&& WarehouseIncomeOperation.Nomenclature != _nomenclatureNew)
				{
					WarehouseIncomeOperation.Nomenclature = _nomenclatureNew;
				}
			}
		}

		/// <summary>
		/// Количество
		/// </summary>
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

		/// <summary>
		/// Комментарий
		/// </summary>
		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}

		/// <summary>
		/// Штраф
		/// </summary>
		[Display(Name = "Штраф")]
		public virtual Fine Fine
		{
			get => _fine;
			set => SetField(ref _fine, value);
		}

		/// <summary>
		/// Тип брака
		/// </summary>
		[Display(Name = "Тип брака")]
		public virtual CullingCategory TypeOfDefect
		{
			get => _typeOfDefect;
			set => SetField(ref _typeOfDefect, value);
		}

		/// <summary>
		/// Источник брака
		/// </summary>
		[Display(Name = "Источник брака")]
		public virtual DefectSource Source
		{
			get => _source;
			set => SetField(ref _source, value);
		}

		/// <summary>
		/// Операция списания
		/// </summary>
		public virtual WarehouseBulkGoodsAccountingOperation WarehouseWriteOffOperation
		{
			get => _warehouseWriteOffOperation;
			set => SetField(ref _warehouseWriteOffOperation, value);
		}

		/// <summary>
		/// Операция пополнения
		/// </summary>
		public virtual WarehouseBulkGoodsAccountingOperation WarehouseIncomeOperation
		{
			get => _warehouseIncomeOperation;
			set => SetField(ref _warehouseIncomeOperation, value);
		}

		/// <summary>
		/// Причина пересортицы
		/// </summary>
		[Display(Name = "Причина пересортицы")]
		public virtual RegradingOfGoodsReason RegradingOfGoodsReason
		{
			get => _regradingOfGoodsReason;
			set => SetField(ref _regradingOfGoodsReason, value);
		}

		#region Не сохраняемые

		/// <summary>
		/// Количество на складе
		/// </summary>
		[Obsolete("Не используется в классе, нужно убрать, оставлено для обратной совместимости")]
		[Display(Name = "Количество на складе")]
		public virtual decimal AmountInStock
		{
			get => _amountInStock;
			set => SetField(ref _amountInStock, value);
		}

		#endregion Не сохраняемые

		#region Расчетные

		public virtual bool IsDefective =>
			NomenclatureNew.IsDefectiveBottle
			|| (NomenclatureNew.Category == NomenclatureCategory.bottle
				&& NomenclatureOld.Category == NomenclatureCategory.water);

		/// <summary>
		/// Сумма ущерба
		/// </summary>
		[Display(Name = "Сумма ущерба")]
		public virtual decimal SumOfDamage => Amount == 0 ? 0 : NomenclatureOld.SumOfDamage * Amount;

		/// <summary>
		/// Заголовок
		/// </summary>
		public virtual string Title => $"{NomenclatureOld.Name} -> {NomenclatureNew.Name} - {NomenclatureOld.Unit.MakeAmountShortStr(Amount)}";

		#endregion Расчетные

		#region Функции

		/// <summary>
		/// СОздание операций
		/// </summary>
		/// <param name="warehouse">Склад пересортицы</param>
		/// <param name="time">Время операций</param>
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

		#endregion Функции
	}
}

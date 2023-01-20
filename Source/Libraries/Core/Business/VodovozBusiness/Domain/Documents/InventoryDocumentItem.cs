using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Documents
{
	//TODO поправить класс
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки инвентаризации",
		Nominative = "строка инвентаризации")]
	[HistoryTrace]
	public class InventoryDocumentItem: PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private Nomenclature _nomenclature;
		private decimal _amountInDb;
		private decimal _amountInFact;
		private string _comment;
		private Fine _fine;
		private GoodsAccountingOperation _warehouseChangeOperation;

		public virtual int Id { get; set; }

		public virtual InventoryDocument Document { get; set; }

		[Required (ErrorMessage = "Номенклатура должна быть заполнена.")]
		[Display (Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature
		{
			get => _nomenclature;
			set
			{
				SetField (ref _nomenclature, value);

				if(WarehouseChangeOperation != null && WarehouseChangeOperation.Nomenclature != _nomenclature)
				{
					WarehouseChangeOperation.Nomenclature = _nomenclature;
				}
			}
		}

		[Display (Name = "Количество по базе")]
		public virtual decimal AmountInDB
		{
			get => _amountInDb;
			set => SetField (ref _amountInDb, value);
		}

		[Display (Name = "Количество по базе")]
		[PropertyChangedAlso("SumOfDamage")]
		public virtual decimal AmountInFact
		{
			get => _amountInFact;
			set => SetField (ref _amountInFact, value);
		}

		[Display (Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField (ref _comment, value);
		}

		[Display (Name = "Штраф")]
		public virtual Fine Fine
		{
			get => _fine;
			set => SetField (ref _fine, value);
		}

		public virtual GoodsAccountingOperation WarehouseChangeOperation
		{
			get => _warehouseChangeOperation;
			set => SetField (ref _warehouseChangeOperation, value);
		}

		#region Расчетные

		[Display(Name = "Сумма ущерба")]
		public virtual decimal SumOfDamage => Difference <= 0 ? Nomenclature.SumOfDamage * Math.Abs(Difference) : 0;

		public virtual string Title =>
			String.Format("[{2}] {0} - {1}",
				Nomenclature.Name,
				Nomenclature.Unit == null ? "" : Nomenclature.Unit.MakeAmountShortStr(AmountInFact),
				Document.Title);

		public virtual decimal Difference => AmountInFact - AmountInDB;

		#endregion

		#region Функции

		public virtual void CreateOperation(Warehouse warehouse, DateTime time)
		{
			if(Difference < 0)
			{
				WarehouseChangeOperation = new GoodsAccountingOperation
					{
						//WriteOffWarehouse = warehouse,
						Amount = Math.Abs(Difference),
						OperationTime = time,
						Nomenclature = Nomenclature
					};
			}
			if(Difference > 0)
			{
				WarehouseChangeOperation = new GoodsAccountingOperation
					{
						//IncomingWarehouse = warehouse,
						Amount = Math.Abs(Difference),
						OperationTime = time,
						Nomenclature = Nomenclature
					};
			}
		}

		public virtual void UpdateOperation(Warehouse warehouse)
		{
			if(Difference < 0)
			{
				//WarehouseChangeOperation.WriteOffWarehouse = warehouse;
				//WarehouseChangeOperation.IncomingWarehouse = null;
				WarehouseChangeOperation.Amount = Math.Abs(Difference);
			}
			if(Difference > 0)
			{
				//WarehouseChangeOperation.WriteOffWarehouse = null;
				//WarehouseChangeOperation.IncomingWarehouse = warehouse;
				WarehouseChangeOperation.Amount = Math.Abs(Difference);
			}
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Comment?.Length > 255)
			{
				yield return new ValidationResult(
					$"Превышена длина комментария для номенклатуры: {Nomenclature.Name} ({Comment.Length}/255)",
					new[] {nameof(Comment)});
			}
		}

		#endregion
	}
}


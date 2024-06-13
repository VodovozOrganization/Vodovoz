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
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки инвентаризации",
		Nominative = "строка инвентаризации")]
	[HistoryTrace]
	public class InventoryDocumentItem: PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		public virtual InventoryDocument Document { get; set; }

		Nomenclature nomenclature;

		[Required (ErrorMessage = "Номенклатура должна быть заполнена.")]
		[Display (Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set {
				SetField (ref nomenclature, value, () => Nomenclature);

				if (WarehouseChangeOperation != null && WarehouseChangeOperation.Nomenclature != nomenclature)
					WarehouseChangeOperation.Nomenclature = nomenclature;
			}
		}

		decimal amountInDB;

		[Display (Name = "Количество по базе")]
		public virtual decimal AmountInDB {
			get { return amountInDB; }
			set {
				SetField (ref amountInDB, value);
			}
		}

		decimal amountInFact;

		[Display (Name = "Количество по базе")]
		[PropertyChangedAlso("SumOfDamage")]
		public virtual decimal AmountInFact {
			get { return amountInFact; }
			set {
				SetField (ref amountInFact, value);
			}
		}

		string comment;

		[Display (Name = "Комментарий")]
		public virtual string Comment {
			get { return comment; }
			set { SetField (ref comment, value, () => Comment); }
		}

		Fine fine;

		[Display (Name = "Штраф")]
		public virtual Fine Fine {
			get { return fine; }
			set { SetField (ref fine, value, () => Fine); }
		}

		WarehouseMovementOperation warehouseChangeOperation;

		public virtual WarehouseMovementOperation WarehouseChangeOperation {
			get { return warehouseChangeOperation; }
			set { SetField (ref warehouseChangeOperation, value, () => WarehouseChangeOperation); }
		}

		#region Расчетные

		[Display(Name = "Сумма ущерба")]
		public virtual decimal SumOfDamage => Difference <= 0 ? Nomenclature.SumOfDamage * Math.Abs(Difference) : 0;

		public virtual string Title {
			get{
				return String.Format("[{2}] {0} - {1}",
					Nomenclature.Name,
					Nomenclature.Unit == null ? "" : Nomenclature.Unit.MakeAmountShortStr(AmountInFact),
					Document.Title);
			}
		}

		public virtual decimal Difference{
			get {
				return AmountInFact - AmountInDB;
			}
		}

		#endregion

		#region Функции

		public virtual void CreateOperation(Warehouse warehouse, DateTime time)
		{
			if(Difference < 0)
			{
				WarehouseChangeOperation = new WarehouseMovementOperation
					{
						WriteoffWarehouse = warehouse,
						Amount = Math.Abs(Difference),
						OperationTime = time,
						Nomenclature = Nomenclature
					};
			}
			if(Difference > 0)
			{
				WarehouseChangeOperation = new WarehouseMovementOperation
					{
						IncomingWarehouse = warehouse,
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
				WarehouseChangeOperation.WriteoffWarehouse = warehouse;
				WarehouseChangeOperation.IncomingWarehouse = null;
				WarehouseChangeOperation.Amount = Math.Abs(Difference);
			}
			if(Difference > 0)
			{
				WarehouseChangeOperation.WriteoffWarehouse = null;
				WarehouseChangeOperation.IncomingWarehouse = warehouse;
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


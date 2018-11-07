using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QSOrmProject;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Store;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Documents
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки инвентаризации",
		Nominative = "строка инвентаризации")]
	public class InventoryDocumentItem: PropertyChangedBase, IDomainObject
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

		//FIXME убрать если не понадобится.
		/*Equipment equipment;
				[Display (Name = "Оборудование")]
		public virtual Equipment Equipment {
			get { return equipment; }
			set {
				SetField (ref equipment, value, () => Equipment);
				if (WarehouseChangeOperation != null && WarehouseChangeOperation.Equipment != equipment)
					WarehouseChangeOperation.Equipment = equipment;

				if (CounterpartyWriteoffOperation != null && CounterpartyWriteoffOperation.Equipment != equipment)
					CounterpartyWriteoffOperation.Equipment = equipment;
			}
		}*/

		decimal amountInDB;

		[Display (Name = "Количество по базе")]
		public virtual decimal AmountInDB {
			get { return amountInDB; }
			set {
				SetField (ref amountInDB, value, () => AmountInDB);
			}
		}

		decimal amountInFact;

		[Display (Name = "Количество по базе")]
		[PropertyChangedAlso("SumOfDamage")]
		public virtual decimal AmountInFact {
			get { return amountInFact; }
			set {
				SetField (ref amountInFact, value, () => AmountInFact);
			}
		}

		string comment;

		[Display (Name = "Комментарий")]
		public virtual string Comment {
			get { return comment; }
			set { SetField (ref comment, value, () => Comment); }
		}

		[Display (Name = "Сумма ущерба")]
		public virtual decimal SumOfDamage {
			get { if (Difference > 0)
					return 0;
			else
				return Nomenclature.SumOfDamage * Math.Abs(Difference); }
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

		public virtual string Title {
			get{
				return String.Format("[{2}] {0} - {1}",
					Nomenclature.Name, 
				                     Nomenclature.Unit.MakeAmountShortStr(AmountInFact),
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

		#endregion
	}
}


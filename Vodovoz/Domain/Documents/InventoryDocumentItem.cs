using System;
using System.ComponentModel.DataAnnotations;
using QSOrmProject;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Documents
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Feminine,
		NominativePlural = "строки инвенторизации",
		Nominative = "строка инвенторизации")]
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

		Equipment equipment;
		//FIXME убрать если не понадобится.
		/*		[Display (Name = "Оборудование")]
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

		WarehouseMovementOperation warehouseChangeOperation;

		public virtual WarehouseMovementOperation WarehouseChangeOperation {
			get { return warehouseChangeOperation; }
			set { SetField (ref warehouseChangeOperation, value, () => WarehouseChangeOperation); }
		}

		#region Расчетные

		public virtual string Title {
			get{
				return String.Format("{0} - {1}", 
					Nomenclature.Name, 
					Nomenclature.Unit.MakeAmountShortStr(AmountInFact));
			}
		}

		public decimal Shortage{
			get {
				return AmountInDB - AmountInFact;
			}
		}

		#endregion

		#region Функции

		public virtual void CreateOperation(Warehouse warehouse, DateTime time)
		{
			if(Shortage > 0)
			{
				WarehouseChangeOperation = new WarehouseMovementOperation
					{
						WriteoffWarehouse = warehouse,
						Amount = Shortage,
						OperationTime = time,
						Nomenclature = Nomenclature
					};
			}
			if(Shortage < 0)
			{
				WarehouseChangeOperation = new WarehouseMovementOperation
					{
						IncomingWarehouse = warehouse,
						Amount = -Shortage,
						OperationTime = time,
						Nomenclature = Nomenclature
					};
			}
		}

		#endregion
	}
}


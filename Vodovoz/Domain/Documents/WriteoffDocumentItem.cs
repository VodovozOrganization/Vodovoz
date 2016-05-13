using System;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;
using QSOrmProject;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Store;
using Vodovoz.Domain.Client;

namespace Vodovoz.Domain.Documents
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Feminine,
		NominativePlural = "строки списания",
		Nominative = "строка списания")]
	public class WriteoffDocumentItem: PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		public virtual WriteoffDocument Document { get; set; }

		Nomenclature nomenclature;

		[Required (ErrorMessage = "Номенклатура должна быть заполнена.")]
		[Display (Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set {
				SetField (ref nomenclature, value, () => Nomenclature);

				if (WarehouseWriteoffOperation != null && WarehouseWriteoffOperation.Nomenclature != nomenclature)
					WarehouseWriteoffOperation.Nomenclature = nomenclature;

				if (CounterpartyWriteoffOperation != null && CounterpartyWriteoffOperation.Nomenclature != nomenclature)
					CounterpartyWriteoffOperation.Nomenclature = nomenclature;
			}
		}

		Equipment equipment;

		[Display (Name = "Оборудование")]
		public virtual Equipment Equipment {
			get { return equipment; }
			set {
				SetField (ref equipment, value, () => Equipment);
				if (WarehouseWriteoffOperation != null && WarehouseWriteoffOperation.Equipment != equipment)
					WarehouseWriteoffOperation.Equipment = equipment;

				if (CounterpartyWriteoffOperation != null && CounterpartyWriteoffOperation.Equipment != equipment)
					CounterpartyWriteoffOperation.Equipment = equipment;
			}
		}

		CullingCategory cullingCategory;

		[Display (Name = "Вид выбраковки")]
		public virtual CullingCategory CullingCategory {
			get { return cullingCategory; }
			set { SetField (ref cullingCategory, value, () => CullingCategory); }
		}

		decimal amount;

		[Min (1)]
		[Display (Name = "Количество")]
		public virtual decimal Amount {
			get { return amount; }
			set {
				SetField (ref amount, value, () => Amount);
				if (WarehouseWriteoffOperation != null && WarehouseWriteoffOperation.Amount != amount)
					WarehouseWriteoffOperation.Amount = amount;

				if (CounterpartyWriteoffOperation != null && CounterpartyWriteoffOperation.Amount != amount)
					CounterpartyWriteoffOperation.Amount = amount;
			}
		}

		string comment;

		[Display (Name = "Комментарий")]
		public virtual string Comment {
			get { return comment; }
			set { SetField (ref comment, value, () => Comment); }
		}

/*		Employee guiltyEmployee;

		[Display (Name = "Виновное лицо")]
		public virtual Employee GuiltyEmployee {
			get { return guiltyEmployee; }
			set { SetField (ref guiltyEmployee, value, () => GuiltyEmployee); }
		}*/

		decimal amountOnStock = 10000000;
		//FIXME пока не реализуем способ загружать количество на складе на конкретный день

		[Display (Name = "Имеется на складе")]
		public virtual decimal AmountOnStock {
			get { return amountOnStock; }
			set {
				SetField (ref amountOnStock, value, () => AmountOnStock);
			}
		}

		public virtual string Name {
			get { return Nomenclature != null ? Nomenclature.Name : ""; }
		}

		public virtual string EquipmentString { 
			get { return Equipment != null ? Equipment.Serial : "-"; } 
		}

		public virtual string CullingCategoryString {
			get { return CullingCategory != null ? CullingCategory.Name : "-"; }
		}

		public virtual bool CanEditAmount { 
			get { return Nomenclature != null && !Nomenclature.Serial; }
		}

		WarehouseMovementOperation warehouseWriteoffOperation;

		public virtual WarehouseMovementOperation WarehouseWriteoffOperation {
			get { return warehouseWriteoffOperation; }
			set { SetField (ref warehouseWriteoffOperation, value, () => WarehouseWriteoffOperation); }
		}

		CounterpartyMovementOperation counterpartyWriteoffOperation;

		public virtual CounterpartyMovementOperation CounterpartyWriteoffOperation {
			get { return counterpartyWriteoffOperation; }
			set { SetField (ref counterpartyWriteoffOperation, value, () => CounterpartyWriteoffOperation); }
		}

		public virtual string Title {
			get{
				return String.Format("{0} - {1}", 
					Nomenclature.Name, 
					Nomenclature.Unit.MakeAmountShortStr(Amount));
			}
		}

		#region Функции

		public virtual void CreateOperation(Warehouse warehouse, DateTime time)
		{
			CounterpartyWriteoffOperation = null;
			WarehouseWriteoffOperation = new WarehouseMovementOperation
			{
				WriteoffWarehouse = warehouse,
				Amount = Amount,
				OperationTime = time,
				Nomenclature = Nomenclature,
				Equipment = Equipment
			};
		}

		public virtual void CreateOperation(Counterparty counterparty, DeliveryPoint piont, DateTime time)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}


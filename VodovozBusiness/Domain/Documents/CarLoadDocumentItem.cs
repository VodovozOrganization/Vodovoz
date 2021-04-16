using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Documents
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки талона погрузки",
		Nominative = "строка талона погрузки")]
	[HistoryTrace]
	public class CarLoadDocumentItem: PropertyChangedBase, IDomainObject
	{
		#region Свойства
		public virtual int Id { get; set; }

		CarLoadDocument document;

		public virtual CarLoadDocument Document {
			get { return document; }
			set { SetField (ref document, value); }
		}

		WarehouseMovementOperation warehouseMovementOperation;

		public virtual WarehouseMovementOperation WarehouseMovementOperation { 
			get { return warehouseMovementOperation; }
			set { SetField (ref warehouseMovementOperation, value); }
		}

		EmployeeNomenclatureMovementOperation employeeNomenclatureMovementOperation;
		public virtual EmployeeNomenclatureMovementOperation EmployeeNomenclatureMovementOperation { 
			get => employeeNomenclatureMovementOperation;
			set => SetField (ref employeeNomenclatureMovementOperation, value);
		}
		
		Nomenclature nomenclature;

		[Display (Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set {
				SetField (ref nomenclature, value);

				if (WarehouseMovementOperation != null && WarehouseMovementOperation.Nomenclature != nomenclature)
					WarehouseMovementOperation.Nomenclature = nomenclature;
			}
		}

		Equipment equipment;

		[Display (Name = "Оборудование")]
		public virtual Equipment Equipment {
			get { return equipment; }
			set {
				SetField (ref equipment, value);
				if (WarehouseMovementOperation != null && WarehouseMovementOperation.Equipment != equipment)
					WarehouseMovementOperation.Equipment = equipment;
			}
		}

		OwnTypes ownType;

		[Display(Name = "Принадлежность")]
		public virtual OwnTypes OwnType
		{
			get => ownType;
			set => SetField(ref ownType, value);
		}

		decimal amount;

		[Display (Name = "Количество")]
		public virtual decimal Amount {
			get => amount;
			set => SetField (ref amount, value);
		}

		decimal? expireDatePercent;
		[Display(Name = "Процент срока годности")]
		public virtual decimal? ExpireDatePercent {
			get => expireDatePercent; 
			set {
				SetField(ref expireDatePercent, value);
			} 
		}

		#endregion

		#region Не сохраняемые

		decimal amountInStock;

		[Display (Name = "Количество на складе")]
		public virtual decimal AmountInStock {
			get { return amountInStock; }
			set {
				SetField (ref amountInStock, value);
			}
		}

		decimal amountInRouteList;

		[Display (Name = "Количество в машрутном листе")]
		public virtual decimal AmountInRouteList {
			get { return amountInRouteList; }
			set {
				SetField (ref amountInRouteList, value);
			}
		}

		decimal amountLoaded;

		[Display (Name = "Уже отгружено")]
		public virtual decimal AmountLoaded {
			get { return amountLoaded; }
			set {
				SetField (ref amountLoaded, value);
			}
		}
			
		public virtual string Title =>
			WarehouseMovementOperation == null ? Nomenclature.Name : String.Format("[{2}] {0} - {1}",
				WarehouseMovementOperation.Nomenclature.Name,
				WarehouseMovementOperation.Nomenclature.Unit?.MakeAmountShortStr(WarehouseMovementOperation.Amount) ?? WarehouseMovementOperation.Amount.ToString(),
				document.Title);

        #endregion

        #region Функции

        public virtual void CreateWarehouseMovementOperation(Warehouse warehouse, DateTime time)
		{
			WarehouseMovementOperation = new WarehouseMovementOperation {
				WriteoffWarehouse = warehouse,
				Amount = Amount,
				OperationTime = time,
				Nomenclature = Nomenclature,
				Equipment = Equipment
			};
		}

		public virtual void UpdateWarehouseMovementOperation(Warehouse warehouse)
		{
			WarehouseMovementOperation.WriteoffWarehouse = warehouse;
			WarehouseMovementOperation.IncomingWarehouse = null;
			WarehouseMovementOperation.Amount = Amount;
			WarehouseMovementOperation.Equipment = Equipment;
		}
		
		public virtual void CreateEmployeeNomenclatureMovementOperation(DateTime time)
		{
			EmployeeNomenclatureMovementOperation = new EmployeeNomenclatureMovementOperation {
				Amount = Amount,
				OperationTime = time,
				Nomenclature = Nomenclature,
				Employee = Document.RouteList.Driver ?? throw new ArgumentNullException(nameof(Document.RouteList.Driver))
			};
		}
		
		public virtual void UpdateEmployeeNomenclatureMovementOperation()
		{
			EmployeeNomenclatureMovementOperation.Amount = Amount;
			EmployeeNomenclatureMovementOperation.Nomenclature = Nomenclature;
			EmployeeNomenclatureMovementOperation.Employee = Document.RouteList.Driver ?? throw new ArgumentNullException(nameof(Document.RouteList.Driver));
		}

		#endregion
	}
}


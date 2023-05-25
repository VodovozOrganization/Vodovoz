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
		DeliveryFreeBalanceOperation _deliveryFreeBalanceOperation;

		#region Свойства
		public virtual int Id { get; set; }

		CarLoadDocument document;

		public virtual CarLoadDocument Document {
			get { return document; }
			set { SetField (ref document, value); }
		}

		WarehouseBulkGoodsAccountingOperation _goodsAccountingOperation;

		public virtual WarehouseBulkGoodsAccountingOperation GoodsAccountingOperation { 
			get { return _goodsAccountingOperation; }
			set { SetField (ref _goodsAccountingOperation, value); }
		}

		EmployeeNomenclatureMovementOperation employeeNomenclatureMovementOperation;
		public virtual EmployeeNomenclatureMovementOperation EmployeeNomenclatureMovementOperation { 
			get => employeeNomenclatureMovementOperation;
			set => SetField (ref employeeNomenclatureMovementOperation, value);
		}

		public virtual DeliveryFreeBalanceOperation DeliveryFreeBalanceOperation
		{
			get => _deliveryFreeBalanceOperation;
			set => SetField(ref _deliveryFreeBalanceOperation, value);
		}

		Nomenclature nomenclature;

		[Display (Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set {
				SetField (ref nomenclature, value);

				if (GoodsAccountingOperation != null && GoodsAccountingOperation.Nomenclature != nomenclature)
					GoodsAccountingOperation.Nomenclature = nomenclature;
			}
		}

		Equipment equipment;

		[Display (Name = "Оборудование")]
		public virtual Equipment Equipment {
			get => equipment;
			set => SetField(ref equipment, value);
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
			GoodsAccountingOperation == null ? Nomenclature.Name : String.Format("[{2}] {0} - {1}",
				GoodsAccountingOperation.Nomenclature.Name,
				GoodsAccountingOperation.Nomenclature.Unit?.MakeAmountShortStr(GoodsAccountingOperation.Amount) ?? GoodsAccountingOperation.Amount.ToString(),
				document.Title);

        #endregion

        #region Функции

        public virtual void CreateWarehouseMovementOperation(Warehouse warehouse, DateTime time)
		{
			GoodsAccountingOperation = new WarehouseBulkGoodsAccountingOperation
			{
				Warehouse = warehouse,
				Amount = -Amount,
				OperationTime = time,
				Nomenclature = Nomenclature
			};
		}

		public virtual void UpdateWarehouseMovementOperation(Warehouse warehouse)
		{
			GoodsAccountingOperation.Warehouse = warehouse;
			GoodsAccountingOperation.Amount = -Amount;
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

		public virtual void CreateOrUpdateDeliveryFreeBalanceOperation()
		{
			if(DeliveryFreeBalanceOperation == null)
			{
				DeliveryFreeBalanceOperation = new DeliveryFreeBalanceOperation();
			}

			DeliveryFreeBalanceOperation.Amount = Amount;
			DeliveryFreeBalanceOperation.Nomenclature = Nomenclature;
			DeliveryFreeBalanceOperation.RouteList = Document.RouteList;
		}

		#endregion
	}
}


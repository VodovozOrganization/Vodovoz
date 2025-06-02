using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Documents
{
	public class CarLoadDocumentItem: CarLoadDocumentItemEntity
	{
		private CarLoadDocument _document;
		private WarehouseBulkGoodsAccountingOperation _goodsAccountingOperation;
		private EmployeeNomenclatureMovementOperation _employeeNomenclatureMovementOperation;
		private DeliveryFreeBalanceOperation _deliveryFreeBalanceOperation;
		private Nomenclature _nomenclature;
		private Equipment _equipment;
		private OwnTypes _ownType;
		private decimal _amountInStock;
		private decimal _amountInRouteList;
		private decimal _amountLoaded;

		#region Свойства

		public virtual new CarLoadDocument Document {
			get { return _document; }
			set { SetField (ref _document, value); }
		}

		public virtual WarehouseBulkGoodsAccountingOperation GoodsAccountingOperation { 
			get { return _goodsAccountingOperation; }
			set { SetField (ref _goodsAccountingOperation, value); }
		}

		public virtual EmployeeNomenclatureMovementOperation EmployeeNomenclatureMovementOperation { 
			get => _employeeNomenclatureMovementOperation;
			set => SetField (ref _employeeNomenclatureMovementOperation, value);
		}

		public virtual DeliveryFreeBalanceOperation DeliveryFreeBalanceOperation
		{
			get => _deliveryFreeBalanceOperation;
			set => SetField(ref _deliveryFreeBalanceOperation, value);
		}

		[Display (Name = "Номенклатура")]
		public virtual new Nomenclature Nomenclature {
			get { return _nomenclature; }
			set {
				SetField (ref _nomenclature, value);

				if (GoodsAccountingOperation != null && GoodsAccountingOperation.Nomenclature != _nomenclature)
					GoodsAccountingOperation.Nomenclature = _nomenclature;
			}
		}

		[Display (Name = "Оборудование")]
		public virtual Equipment Equipment {
			get => _equipment;
			set => SetField(ref _equipment, value);
		}

		[Display(Name = "Принадлежность")]
		public virtual OwnTypes OwnType
		{
			get => _ownType;
			set => SetField(ref _ownType, value);
		}

		#endregion

		#region Не сохраняемые

		[Display (Name = "Количество на складе")]
		public virtual decimal AmountInStock {
			get { return _amountInStock; }
			set {
				SetField (ref _amountInStock, value);
			}
		}

		[Display (Name = "Количество в машрутном листе")]
		public virtual decimal AmountInRouteList {
			get { return _amountInRouteList; }
			set {
				SetField (ref _amountInRouteList, value);
			}
		}

		[Display (Name = "Уже отгружено")]
		public virtual decimal AmountLoaded {
			get { return _amountLoaded; }
			set {
				SetField (ref _amountLoaded, value);
			}
		}
			
		public virtual string Title =>
			GoodsAccountingOperation == null ? Nomenclature.Name : String.Format("[{2}] {0} - {1}",
				GoodsAccountingOperation.Nomenclature.Name,
				GoodsAccountingOperation.Nomenclature.Unit?.MakeAmountShortStr(GoodsAccountingOperation.Amount) ?? GoodsAccountingOperation.Amount.ToString(),
				_document.Title);

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


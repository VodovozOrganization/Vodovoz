using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Store;
using VodovozBusiness.Domain.TrueMark.TrueMarkProductCodes;

namespace Vodovoz.Domain.Documents
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки талона погрузки",
		Nominative = "строка талона погрузки")]
	[HistoryTrace]
	public class CarLoadDocumentItem: PropertyChangedBase, IDomainObject
	{
		private CarLoadDocument _document;
		private WarehouseBulkGoodsAccountingOperation _goodsAccountingOperation;
		private EmployeeNomenclatureMovementOperation _employeeNomenclatureMovementOperation;
		private DeliveryFreeBalanceOperation _deliveryFreeBalanceOperation;
		private Nomenclature _nomenclature;
		private Equipment _equipment;
		private OwnTypes _ownType;
		private decimal _amount;
		private decimal? _expireDatePercent;
		private int? _orderId;
		private bool _isIndividualSetForOrder;
		private decimal _amountInStock;
		private decimal _amountInRouteList;
		private decimal _amountLoaded;
		private IObservableList<CarLoadDocumentItemTrueMarkProductCode> _trueMarkCodes = new ObservableList<CarLoadDocumentItemTrueMarkProductCode>();

		#region Свойства
		public virtual int Id { get; set; }

		public virtual CarLoadDocument Document {
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
		public virtual Nomenclature Nomenclature {
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

		[Display (Name = "Количество")]
		public virtual decimal Amount {
			get => _amount;
			set => SetField (ref _amount, value);
		}

		[Display(Name = "Процент срока годности")]
		public virtual decimal? ExpireDatePercent {
			get => _expireDatePercent; 
			set {
				SetField(ref _expireDatePercent, value);
			} 
		}

		[Display(Name ="Номер заказа")]
		public virtual int? OrderId
		{
			get => _orderId;
			set => SetField(ref _orderId, value);
		}

		[Display(Name = "Отделить номенклатуры заказа при погрузке")]
		public virtual bool IsIndividualSetForOrder
		{
			get => _isIndividualSetForOrder;
			set => SetField(ref _isIndividualSetForOrder, value);
		}

		[Display(Name = "Коды ЧЗ товаров")]
		public virtual IObservableList<CarLoadDocumentItemTrueMarkProductCode> TrueMarkCodes
		{
			get => _trueMarkCodes;
			set => SetField(ref _trueMarkCodes, value);
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

		public virtual CarLoadDocumentLoadOperationState GetDocumentItemLoadOperationState()
		{
			if(OrderId is null)
			{
				throw new InvalidOperationException("Получение статуса погрузки строки документа погрузки доступно только для товаров сетвых клиентов");
			}

			if(Nomenclature.Category != NomenclatureCategory.water)
			{
				throw new InvalidOperationException("Получение статуса погрузки строки документа погрузки доступно только для товаров категории \"Вода\"");
			}

			var loadedItemsCount = TrueMarkCodes.Count;

			var state =
				loadedItemsCount == 0
				? CarLoadDocumentLoadOperationState.NotStarted
				: loadedItemsCount < Amount
					? CarLoadDocumentLoadOperationState.InProgress
					: CarLoadDocumentLoadOperationState.Done;

			return state;
		}

		#endregion
	}
}


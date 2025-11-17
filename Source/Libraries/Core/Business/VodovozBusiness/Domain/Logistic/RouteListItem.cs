using Autofac;
using Gamma.Utilities;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Utilities.Debug;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Controllers;
using Vodovoz.Core.Domain.BasicHandbooks;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Logistics;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.Settings.Delivery;
using Vodovoz.Tools.CallTasks;
using Vodovoz.Tools.Logistic;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Domain.Logistic
{
	public class RouteListItem : RouteListItemEntity, IValidatableObject
	{
		private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

		private AddressTransferType? _addressTransferType;
		private RouteListItem _transferredTo;

		private INomenclatureRepository _nomenclatureRepository => ScopeProvider.Scope
			.Resolve<INomenclatureRepository>();
		private IDeliveryRulesSettings _deliveryRulesSettings => ScopeProvider.Scope
			.Resolve<IDeliveryRulesSettings>();
		private IEmployeeRepository _employeeRepository => ScopeProvider.Scope
			.Resolve<IEmployeeRepository>();
		private IUndeliveredOrdersRepository _undeliveredOrdersRepository => ScopeProvider.Scope
			.Resolve<IUndeliveredOrdersRepository>();
		private ISubdivisionRepository _subdivisionRepository => ScopeProvider.Scope
			.Resolve<ISubdivisionRepository>();
		private IRouteListItemRepository _routeListItemRepository => ScopeProvider.Scope
			.Resolve<IRouteListItemRepository>();
		private IOrderService _orderService => ScopeProvider.Scope
			.Resolve<IOrderService>();

		private Order _order;
		private RouteList _routeList;
		private RouteListItemStatus _status;
		private DateTime? _statusLastUpdate;
		private DateTime _creationDate;
		private bool _wasTransfered;
		private string _cashierComment;
		private DateTime? _cashierCommentCreateDate;
		private DateTime? _cashierCommentLastUpdate;
		private Employee _cashierCommentAuthor;
		private string _comment;
		private bool _withForwarder;
		private int _indexInRoute;
		private int _bottlesReturned;
		private int? _driverBottlesReturned;
		private decimal _oldBottleDepositsCollected;
		private decimal _oldEquipmentDepositsCollected;
		private decimal _totalCash;
		private decimal _extraCash;
		private decimal _driverWage;
		private decimal _driverWageSurcharge;
		private decimal _forwarderWage;
		private TimeSpan? _planTimeStart;
		private TimeSpan? _planTimeEnd;
		private WageDistrictLevelRate _forwarderWageCalulationMethodic;
		private WageDistrictLevelRate _driverWageCalulationMethodic;
		private LateArrivalReason _lateArrivalReason;
		private Employee _lateArrivalReasonAuthor;
		private string _commentForFine;
		private Employee _commentForFineAuthor;
		private IList<Fine> _fines = new List<Fine>();
		private bool _isDriverForeignDistrict;
		private GenericObservableList<Fine> _observableFines;
		private Dictionary<int, decimal> _goodsByRouteColumns;
		private DateTime? _recievedTransferAt;

		public RouteListItem() { }

		//Конструктор создания новой строки
		public RouteListItem(RouteList routeList, Order order, RouteListItemStatus status)
		{
			this._routeList = routeList;
			if(order.OrderStatus == OrderStatus.Accepted)
			{
				order.OrderStatus = OrderStatus.InTravelList;
			}
			this.Order = order;
			this._status = status;
		}

		#region Свойства

		[Display(Name = "Заказ")]
		public virtual Order Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}

		[Display(Name = "Маршрутный лист")]
		public virtual RouteList RouteList
		{
			get => _routeList;
			set => SetField(ref _routeList, value);
		}

		[Display(Name = "Статус адреса")]
		public virtual RouteListItemStatus Status
		{
			get => _status;
			set => SetField(ref _status, value);
		}

		[Display(Name = "Время изменения статуса")]
		public virtual DateTime? StatusLastUpdate
		{
			get => _statusLastUpdate;
			set => SetField(ref _statusLastUpdate, value);
		}

		[Display(Name = "Дата создания")]
		public virtual DateTime CreationDate =>
			_creationDate == default
			? DateTime.Now
			: _creationDate;

		[Display(Name = "Перенесен в другой маршрутный лист")]
		public virtual RouteListItem TransferedTo
		{
			get => _transferredTo;
			protected set => SetField(ref _transferredTo, value);
		}

		[Display(Name = "Был перенесен")]
		public virtual bool WasTransfered
		{
			get => _wasTransfered;
			set => SetField(ref _wasTransfered, value);
		}

		[Display(Name = "Перенос принят в:")]
		public virtual DateTime? RecievedTransferAt
		{
			get => _recievedTransferAt;
			set => SetField(ref _recievedTransferAt, value);
		}

		[Display(Name = "Комментарий кассира")]
		public virtual string CashierComment
		{
			get => _cashierComment;
			set => SetField(ref _cashierComment, value);
		}

		[Display(Name = "Дата создания комментария кассира")]
		public virtual DateTime? CashierCommentCreateDate
		{
			get => _cashierCommentCreateDate;
			set => SetField(ref _cashierCommentCreateDate, value);
		}

		[Display(Name = "Дата обновления комментария кассира")]
		public virtual DateTime? CashierCommentLastUpdate
		{
			get => _cashierCommentLastUpdate;
			set => SetField(ref _cashierCommentLastUpdate, value);
		}

		[Display(Name = "Автор комментария")]
		public virtual Employee CashierCommentAuthor
		{
			get => _cashierCommentAuthor;
			set => SetField(ref _cashierCommentAuthor, value);
		}

		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}

		[Display(Name = "С экспедитором")]
		public virtual bool WithForwarder
		{
			get => _withForwarder;
			set => SetField(ref _withForwarder, value);
		}

		[Display(Name = "Порядковый номер в МЛ")]
		public virtual int IndexInRoute
		{
			get => _indexInRoute;
			set => SetField(ref _indexInRoute, value);
		}

		[Display(Name = "Возвращено бутылей")]
		public virtual int BottlesReturned
		{
			get => _bottlesReturned;
			set => SetField(ref _bottlesReturned, value);
		}

		[Display(Name = "Возвращено бутылей - водитель")]
		public virtual int? DriverBottlesReturned
		{
			get => _driverBottlesReturned;
			set => SetField(ref _driverBottlesReturned, value);
		}

		[Display(Name = "Тип переноа адреса")]
		public virtual AddressTransferType? AddressTransferType
		{
			get => _addressTransferType;
			set => SetField(ref _addressTransferType, value);
		}

		/// <summary>
		/// Устаревший залог за бутыли. Который раньше вводился пользователем вручную при закрытии МЛ
		/// </summary>
		[Display(Name = "Старый залог за бутыли")]
		public virtual decimal OldBottleDepositsCollected
		{
			get => _oldBottleDepositsCollected;
			set => SetField(ref _oldBottleDepositsCollected, value);
		}

		public virtual decimal BottleDepositsCollected
		{
			get
			{
				if(Order.PaymentType == PaymentType.ContractDocumentation || Order.PaymentType == PaymentType.Cashless)
				{
					return 0;
				}

				if(_oldBottleDepositsCollected != 0m)
				{
					return _oldBottleDepositsCollected;
				}

				return 0 - Order.BottleDepositSum;
			}
		}

		/// <summary>
		/// Устаревший залог за оборудование. Который раньше вводился пользователем вручную при закрытии МЛ
		/// </summary>
		[Display(Name = "Старый залог за оборудование")]
		public virtual decimal OldEquipmentDepositsCollected
		{
			get => _oldEquipmentDepositsCollected;
			set => SetField(ref _oldEquipmentDepositsCollected, value);
		}

		public virtual decimal EquipmentDepositsCollected
		{
			get
			{
				if(Order.PaymentType == PaymentType.ContractDocumentation || Order.PaymentType == PaymentType.Cashless)
				{
					return 0;
				}

				if(_oldEquipmentDepositsCollected != 0m)
				{
					return _oldEquipmentDepositsCollected;
				}

				return 0 - Order.EquipmentDepositSum;
			}
		}

		public virtual decimal AddressCashSum
		{
			get
			{
				if(!IsDelivered())
				{
					return 0;
				}
				if(Order.PaymentType != PaymentType.Cash)
				{
					return 0;
				}
				return Order.OrderCashSum + OldBottleDepositsCollected + OldEquipmentDepositsCollected + ExtraCash;
			}
		}

		[Display(Name = "Всего наличных")]
		public virtual decimal TotalCash
		{
			get => _totalCash;
			set => SetField(ref _totalCash, value);
		}

		[Display(Name = "Дополнительно наличных")]
		public virtual decimal ExtraCash
		{
			get => _extraCash;
			set => SetField(ref _extraCash, value);
		}

		[Display(Name = "ЗП водителя")]
		public virtual decimal DriverWage
		{
			get => _driverWage;
			set => SetField(ref _driverWage, value);
		}

		[Display(Name = "Надбавка к ЗП водителя")]
		public virtual decimal DriverWageSurcharge
		{
			get => _driverWageSurcharge;
			set => SetField(ref _driverWageSurcharge, value);
		}

		[Display(Name = "ЗП экспедитора")]
		//Зарплана с уже включенной надбавкой ForwarderWageSurcharge
		public virtual decimal ForwarderWage
		{
			get => _forwarderWage;
			set => SetField(ref _forwarderWage, value);
		}

		[Display(Name = "Оповещение за 30 минут")]
		[IgnoreHistoryTrace]
		public virtual bool Notified30Minutes { get; set; }

		[Display(Name = "Время оповещения прошло")]
		[IgnoreHistoryTrace]
		public virtual bool NotifiedTimeout { get; set; }

		[Display(Name = "Запланированное время приезда min")]
		public virtual TimeSpan? PlanTimeStart
		{
			get => _planTimeStart;
			set => SetField(ref _planTimeStart, value);
		}

		[Display(Name = "Запланированное время приезда max")]
		public virtual TimeSpan? PlanTimeEnd
		{
			get => _planTimeEnd;
			set => SetField(ref _planTimeEnd, value);
		}

		[Display(Name = "Методика расчёта ЗП экспедитора")]
		public virtual WageDistrictLevelRate ForwarderWageCalculationMethodic
		{
			get => _forwarderWageCalulationMethodic;
			set => SetField(ref _forwarderWageCalulationMethodic, value);
		}

		[Display(Name = "Методика расчёта ЗП водителя")]
		public virtual WageDistrictLevelRate DriverWageCalculationMethodic
		{
			get => _driverWageCalulationMethodic;
			set => SetField(ref _driverWageCalulationMethodic, value);
		}

		[Display(Name = "Причина опоздания водителя")]
		public virtual LateArrivalReason LateArrivalReason
		{
			get => _lateArrivalReason;
			set => SetField(ref _lateArrivalReason, value);
		}

		[Display(Name = "Автор причины опоздания водителя")]
		public virtual Employee LateArrivalReasonAuthor
		{
			get => _lateArrivalReasonAuthor;
			set => SetField(ref _lateArrivalReasonAuthor, value);
		}

		[Display(Name = "Комментарий по штрафу")]
		public virtual string CommentForFine
		{
			get => _commentForFine;
			set => SetField(ref _commentForFine, value);
		}

		[Display(Name = "Последний редактор комментария по штрафу")]
		public virtual Employee CommentForFineAuthor
		{
			get => _commentForFineAuthor;
			set
			{
				if(_commentForFineAuthor != value)
				{
					SetField(ref _commentForFineAuthor, value);
				}
			}
		}

		[Display(Name = "Штрафы")]
		public virtual IList<Fine> Fines
		{
			get => _fines;
			set => SetField(ref _fines, value);
		}

		[Display(Name = "Чужой район для водителя")]
		public virtual bool IsDriverForeignDistrict
		{
			get => _isDriverForeignDistrict;
			set => SetField(ref _isDriverForeignDistrict, value);
		}

		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<Fine> ObservableFines
		{
			get
			{
				if(_observableFines == null)
				{
					_observableFines = new GenericObservableList<Fine>(Fines);
				}
				return _observableFines;
			}
		}

		#endregion Свойства

		#region Runtime свойства (не мапятся)

		public virtual bool AddressIsValid { get; set; } = true;

		#endregion Runtime свойства (не мапятся)

		#region Расчетные

		public virtual string Title => $"Адрес в МЛ №{RouteList.Id} - {Order.DeliveryPoint.CompiledAddress}";

		//FIXME запуск оборудования - временный фикс
		public virtual int CoolersToClient
		{
			get
			{
				return Order.OrderEquipments
					.Where(item => item.Direction == Direction.Deliver)
					.Where(item => item.Confirmed)
					.Count(item => item.Equipment != null ? item.Equipment.Nomenclature.Category == NomenclatureCategory.equipment
							: (item.Nomenclature.Category == NomenclatureCategory.equipment));
			}
		}

		public virtual int PlannedCoolersToClient
		{
			get
			{
				return Order.OrderEquipments
					.Where(item => item.Direction == Direction.Deliver)
					.Where(item => item.Equipment != null)
					.Count(item => item.Equipment.Nomenclature.Kind.WarrantyCardType == WarrantyCardType.CoolerWarranty);
			}
		}

		public virtual int PumpsToClient
		{
			get
			{
				return Order.OrderEquipments
					.Where(item => item.Direction == Direction.Deliver)
					.Where(item => item.Equipment != null)
					.Where(item => item.Confirmed)
					.Count(item => item.Equipment.Nomenclature.Kind.WarrantyCardType == WarrantyCardType.PumpWarranty);
			}
		}

		public virtual int PlannedPumpsToClient
		{
			get
			{
				return Order.OrderEquipments
					.Where(item => item.Direction == Direction.Deliver)
					.Where(item => item.Equipment != null)
					.Count(item => item.Equipment.Nomenclature.Kind.WarrantyCardType == WarrantyCardType.PumpWarranty);
			}
		}

		public virtual int UncategorisedEquipmentToClient
		{
			get
			{
				return Order.OrderEquipments
					.Where(item => item.Direction == Direction.Deliver)
					.Where(item => item.Equipment != null)
					.Where(item => item.Confirmed)
					.Count(item => item.Equipment.Nomenclature.Kind.WarrantyCardType == WarrantyCardType.WithoutCard);
			}
		}

		public virtual int PlannedUncategorisedEquipmentToClient
		{
			get
			{
				return Order.OrderEquipments
					.Where(item => item.Direction == Direction.Deliver)
					.Where(item => item.Equipment != null)
					.Count(item => item.Equipment.Nomenclature.Kind.WarrantyCardType == WarrantyCardType.WithoutCard);
			}
		}

		//FIXME запуск оборудования - временный фикс
		public virtual int CoolersFromClient
		{
			get
			{
				return Order.OrderEquipments
					.Where(item => item.Direction == Direction.PickUp)
					.Where(item => item.Confirmed)
					.Where(item => item.Equipment != null)
					.Count(item => item.Equipment.Nomenclature.Category == NomenclatureCategory.equipment);
			}
		}

		public virtual int PumpsFromClient
		{
			get
			{
				return Order.OrderEquipments
					.Where(item => item.Direction == Direction.PickUp)
					.Where(item => item.Equipment != null)
					.Where(item => item.Confirmed)
					.Count(item => item.Equipment.Nomenclature.Kind.WarrantyCardType == WarrantyCardType.PumpWarranty);
			}
		}

		public virtual int PlannedCoolersFromClient
		{
			get
			{
				return Order.OrderEquipments
					.Where(item => item.Direction == Direction.PickUp)
					.Where(item => item.Equipment != null)
					.Count(item => item.Equipment.Nomenclature.Kind.WarrantyCardType == WarrantyCardType.CoolerWarranty);
			}
		}

		public virtual int PlannedPumpsFromClient
		{
			get
			{
				return Order.OrderEquipments
					.Where(item => item.Direction == Direction.PickUp)
					.Where(item => item.Equipment != null)
					.Count(item => item.Equipment.Nomenclature.Kind.WarrantyCardType == WarrantyCardType.PumpWarranty);
			}
		}

		public virtual string EquipmentsToClientText
		{
			get
			{
				//Если это старый заказ со старой записью оборудования в виде строки, то выводим только его
				if(!string.IsNullOrWhiteSpace(Order.ToClientText))
				{
					return Order.ToClientText;
				}

				var orderEquipment = string.Join(
					Environment.NewLine,
					Order.OrderEquipments
						.Where(x => x.Direction == Direction.Deliver)
						.Select(x => $"{x.NameString}: {x.Count:N0}"));

				var orderItemEquipment = string.Join(
					Environment.NewLine,
					Order.OrderItems
						.Where(x => x.Nomenclature.Category == NomenclatureCategory.equipment)
						.Select(x => $"{x.Nomenclature.Name}: {x.Count:N0}"));

				if(string.IsNullOrWhiteSpace(orderItemEquipment))
				{
					return orderEquipment;
				}

				return $"{orderEquipment}{Environment.NewLine}{orderItemEquipment}";
			}
		}

		public virtual string EquipmentsFromClientText
		{
			get
			{
				//Если это старый заказ со старой записью оборудования в виде строки, то выводим только его
				if(!string.IsNullOrWhiteSpace(Order.FromClientText))
				{
					return Order.FromClientText;
				}

				return string.Join("\n",
					Order.OrderEquipments
						.Where(x => x.Direction == Direction.PickUp)
						.Select(x => $"{x.NameString}: {x.Count}"));
			}
		}

		public virtual WageDistrictLevelRate DriverWageCalcMethodicTemporaryStore { get; set; }

		public virtual WageDistrictLevelRate ForwarderWageCalcMethodicTemporaryStore { get; set; }

		public virtual bool NeedToLoad => Order.HasItemsNeededToLoad;

		public virtual Dictionary<int, decimal> GoodsByRouteColumns
		{
			get
			{
				if(_goodsByRouteColumns == null)
				{
					_goodsByRouteColumns = Order.OrderItems.Where(i => i.Nomenclature.RouteListColumn != null)
						.GroupBy(i => i.Nomenclature.RouteListColumn.Id, i => i.Count)
						.ToDictionary(g => g.Key, g => g.Sum());
				}
				return _goodsByRouteColumns;
			}
		}

		#endregion Расчетные

		#region Функции

		public virtual void UpdateStatusAndCreateTask(
			IUnitOfWork uow,
			RouteListItemStatus status,
			ICallTaskWorker callTaskWorker,
			IOnlineOrderService onlineOrderService,
			bool isEditAtCashier = false)
		{
			if(Status == status)
			{
				return;
			}

			if(!isEditAtCashier)
			{
				CreateDeliveryFreeBalanceOperation(uow, Status, status);
			}

			switch(status)
			{
				case RouteListItemStatus.Canceled:
					Order.ChangeStatusAndCreateTasks(OrderStatus.DeliveryCanceled, callTaskWorker);
					SetOrderActualCountsToZeroOnCanceled();
					break;
				case RouteListItemStatus.Completed:
					Order.ChangeStatusAndCreateTasks(OrderStatus.Shipped, callTaskWorker);

					if(Order.TimeDelivered == null)
					{
						Order.TimeDelivered = DateTime.Now;
					}

					RestoreOrder(status);
					_orderService.AutoCancelAutoTransfer(uow, Order);
					break;
				case RouteListItemStatus.EnRoute:
					Order.ChangeStatusAndCreateTasks(OrderStatus.OnTheWay, callTaskWorker);
					RestoreOrder(status);
					onlineOrderService.NotifyClientOfOnlineOrderStatusChange(uow, Order.OnlineOrder);
					break;
				case RouteListItemStatus.Overdue:
					Order.ChangeStatusAndCreateTasks(OrderStatus.NotDelivered, callTaskWorker);
					SetOrderActualCountsToZeroOnCanceled();
					break;
			}
			
			Status = status;
			StatusLastUpdate = DateTime.Now;

			uow.Save(Order);

			UpdateRouteListDebt();			
		}

		public virtual void UpdateRouteListDebt()
		{
			if(Order.PaymentType == PaymentType.Cash)
			{
				RecalculateTotalCash();
				RouteList.UpdateRouteListDebt();
			}
		}

		public virtual void SetTransferTo(RouteListItem targetAddress)
		{
			TransferedTo = targetAddress;
		}

		public virtual void TransferTo(IUnitOfWork uow, RouteListItem targetAddress)
		{
			SetTransferTo(targetAddress);
			SetStatusWithoutOrderChange(uow, RouteListItemStatus.Transfered);
		}

		public virtual void RevertTransferAddress(
			IUnitOfWork uow,
			IWageParameterService wageParameterService,
			RouteListItem revertedAddress)
		{
			SetStatusWithoutOrderChange(uow, revertedAddress.Status);
			SetTransferTo(null);
			DriverBottlesReturned = revertedAddress.DriverBottlesReturned;

			if(RouteList.ClosingFilled)
			{
				FirstFillClosing(wageParameterService);
			}
		}

		public virtual void RecalculateTotalCash() => TotalCash = CalculateTotalCash();

		public virtual decimal CalculateTotalCash() => IsDelivered() ? AddressCashSum : 0;

		public virtual bool RouteListIsUnloaded() =>
			new[] { RouteListStatus.EnRoute, RouteListStatus.OnClosing, RouteListStatus.Closed }.Contains(RouteList.Status);

		public virtual bool IsDelivered()
		{
			return Status == RouteListItemStatus.Completed || Status == RouteListItemStatus.EnRoute && RouteListIsUnloaded();
		}

		public virtual bool IsValidForWageCalculation()
		{
			return !GetNotDeliveredStatuses().Contains(Status);
		}

		public virtual int GetFullBottlesDeliveredCount()
		{
			return (int)Order.OrderItems
				.Where(item => item.Nomenclature.Category == NomenclatureCategory.water
					&& item.Nomenclature.TareVolume == TareVolume.Vol19L)
				.Sum(item => item.ActualCount ?? 0);
		}

		public virtual int GetFullBottlesToDeliverCount()
		{
			return (int)Order.OrderItems
				.Where(item => item.Nomenclature.Category == NomenclatureCategory.water
					&& item.Nomenclature.TareVolume == TareVolume.Vol19L)
				.Sum(item => item.Count);
		}

		/// <summary>
		/// Функция вызывается при переходе адреса в закрытие.
		/// Если адрес в пути, при закрытии МЛ он считается автоматически доставленным.
		/// </summary>
		public virtual void FirstFillClosing(IWageParameterService wageParameterService)
		{
			//В этом месте изменяем статус для подстраховки.
			if(Status == RouteListItemStatus.EnRoute)
			{
				Status = RouteListItemStatus.Completed;
			}

			foreach(var item in Order.OrderItems)
			{
				item.SetActualCount(IsDelivered() ? item.Count : 0);
			}

			foreach(var equip in Order.OrderEquipments)
			{
				equip.ActualCount = IsDelivered() ? equip.Count : 0;
			}

			foreach(var deposit in Order.OrderDepositItems)
			{
				deposit.ActualCount = IsDelivered() ? deposit.Count : 0;
			}

			if(!Order.IsBottleStockDiscrepancy)
			{
				Order.BottlesByStockActualCount = Order.BottlesByStockCount;
			}

			PerformanceHelper.AddTimePoint(_logger, "Обработали номенклатуры");
			BottlesReturned = IsDelivered() ? (DriverBottlesReturned ?? Order.BottlesReturn ?? 0) : 0;
			RecalculateTotalCash();
			RouteList.RecalculateWagesForRouteListItem(this, wageParameterService);
		}

		/// <summary>
		/// Обнуляет фактическое количетво
		/// Использовать если заказ отменен или полностью не доставлен
		/// </summary>
		public virtual void SetOrderActualCountsToZeroOnCanceled() => Order.SetActualCountsToZeroOnCanceled();

		public virtual void RestoreOrder(RouteListItemStatus? status = null)
		{
			var newStatus = status ?? Status;

			foreach(var item in Order.OrderItems)
			{
				item.RestoreOriginalDiscountFromRestoreOrder();

				if(newStatus == RouteListItemStatus.EnRoute)
				{
					item.SetActualCount(null);
				}
				else
				{
					item.PreserveActualCount(true);
				}
			}

			foreach(var equip in Order.OrderEquipments)
			{
				equip.ActualCount = equip.Count;
			}

			foreach(var deposit in Order.OrderDepositItems)
			{
				deposit.ActualCount = deposit.Count;
			}
		}

		public virtual decimal GetGoodsAmountForColumn(int columnId) =>
			GoodsByRouteColumns.ContainsKey(columnId) ? GoodsByRouteColumns[columnId] : 0;

		public virtual decimal GetGoodsActualAmountForColumn(int columnId)
		{
			if(Status == RouteListItemStatus.Transfered)
			{
				return 0;
			}

			return Order.OrderItems
				.Where(i => i.Nomenclature.RouteListColumn != null
					&& i.Nomenclature.RouteListColumn.Id == columnId)
				.Sum(i => i.ActualCount ?? 0);
		}

		public virtual void ChangeOrderStatus(OrderStatus orderStatus) => Order.OrderStatus = orderStatus;

		public virtual void SetStatusWithoutOrderChange(IUnitOfWork uow, RouteListItemStatus status, bool needCreateDeliveryFreeBalanceOperation = true)
		{
			if(needCreateDeliveryFreeBalanceOperation)
			{
				CreateDeliveryFreeBalanceOperation(uow, Status, status);
			}

			Status = status;			
		}

		public virtual void CreateDeliveryFreeBalanceOperation(IUnitOfWork uow, RouteListItemStatus oldStatus, RouteListItemStatus newStatus)
		{
			RouteListAddressKeepingDocumentController routeListAddressKeepingDocumentController =
				new RouteListAddressKeepingDocumentController(_employeeRepository, _nomenclatureRepository);

			routeListAddressKeepingDocumentController.CreateOrUpdateRouteListKeepingDocument(uow, this, oldStatus, newStatus);
		}

		public virtual string GetTransferText(bool isShort = false)
		{
			if(Status == RouteListItemStatus.Transfered)
			{
				var transferredTo = _routeListItemRepository.GetTransferredTo(RouteList.UoW, this);

				if(transferredTo is null)
				{
					return "ОШИБКА! Адрес имеет статус перенесенного в другой МЛ, но куда он перенесен не указано.";
				}

				var addressTransferType = _routeListItemRepository.GetAddressTransferType(_routeList.UoW, Id, transferredTo.Id);
				var transferType = addressTransferType?.GetEnumTitle();

				var result = isShort
					? transferType
					: $"Заказ был перенесен в МЛ №{transferredTo.RouteList.Id} " +
					$"водителя {transferredTo.RouteList.Driver.ShortName}" +
					$" {transferType}.";

				return result;
			}

			if(WasTransfered)
			{
				var transferredFrom = _routeListItemRepository.GetTransferredFrom(RouteList.UoW, this);

				if(transferredFrom != null)
				{
					var transferType = AddressTransferType?.GetEnumTitle();

					var result = isShort
						? transferType
						: $"Заказ из МЛ №{transferredFrom.RouteList.Id}" +
						$" водителя {transferredFrom.RouteList.Driver.ShortName}" +
						$" {transferType}.";

					return result;
				}

				return "ОШИБКА! Адрес помечен как перенесенный из другого МЛ, но строка откуда он был перенесен не найдена.";
			}

			if(AddressTransferType != null)
			{
				var transferType = AddressTransferType?.GetEnumTitle();

				var result = isShort
						? transferType
						: $"Заказ был добавлен в МЛ в пути " +
						$"{transferType}.";

				return result;
			}

			return null;
		}

		public virtual void AddFine(Fine fine)
		{
			if(!ObservableFines.Contains(fine))
			{
				ObservableFines.Add(fine);
			}
		}

		public virtual void RemoveAllFines()
		{
			if(ObservableFines.Any())
			{
				ObservableFines.Clear();
			}
		}

		public virtual void RemoveFine(Fine fine)
		{
			if(ObservableFines.Any() && ObservableFines.Contains(fine))
			{
				ObservableFines.Remove(fine);
			}
		}

		public virtual string GetAllFines()
		{
			return ObservableFines.Any() ?
				string.Join(
					"\n",
					ObservableFines
						.SelectMany(x => x.ObservableItems)
						.Select(x => x.Title))
				: string.Empty;
		}

		#endregion Функции

		#region Для расчетов в логистике

		/// <summary>
		/// Время разгрузки на адресе в секундах.
		/// </summary>
		public virtual int TimeOnPoint => Order.CalculateTimeOnPoint(RouteList.Forwarder != null);

		public virtual TimeSpan CalculatePlanedTime(RouteGeometryCalculator sputnikCache)
		{
			DateTime time = default;

			for(int ix = 0; ix < RouteList.Addresses.Count; ix++)
			{
				var address = RouteList.Addresses[ix];

				if(ix == 0)
				{
					time = time.Add(RouteList.Addresses[ix].Order.DeliverySchedule.From);
				}
				else
				{
					time = time.AddSeconds(sputnikCache.TimeSec(
						RouteList.Addresses[ix - 1].Order.DeliveryPoint.PointCoordinates,
						RouteList.Addresses[ix].Order.DeliveryPoint.PointCoordinates));
				}

				if(address == this)
				{
					break;
				}

				time = time.AddSeconds(RouteList.Addresses[ix].TimeOnPoint);
			}

			sputnikCache?.Dispose();
			return time.TimeOfDay;
		}

		public virtual TimeSpan? CalculateTimeLateArrival()
		{
			if(StatusLastUpdate.HasValue)
			{
				var late = StatusLastUpdate.Value.TimeOfDay - Order.DeliverySchedule.To;

				if(late.TotalSeconds > 0)
				{
					return late;
				}
			}

			return null;
		}

		#endregion Для расчетов в логистике

		#region Зарплата

		public virtual IRouteListItemWageCalculationSource DriverWageCalculationSrc =>
			new RouteListItemWageCalculationSource(this, EmployeeCategory.driver, _deliveryRulesSettings);

		public virtual IRouteListItemWageCalculationSource ForwarderWageCalculationSrc =>
			new RouteListItemWageCalculationSource(this, EmployeeCategory.forwarder, _deliveryRulesSettings);

		public virtual void SaveWageCalculationMethodics()
		{
			DriverWageCalculationMethodic = DriverWageCalcMethodicTemporaryStore;
			ForwarderWageCalculationMethodic = ForwarderWageCalcMethodicTemporaryStore;
		}

		#endregion Зарплата

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(CommentForFine?.Length > 1000)
			{
				yield return new ValidationResult(
					$"В адресе: '{Title}' превышена максимально допустимая длина комментария по штрафу ({CommentForFine.Length}/1000)");
			}

			if(CashierComment?.Length > 255)
			{
				yield return new ValidationResult(
					$"В адресе: '{Title}' превышена максимально допустимая длина комментария кассира ({CashierComment.Length}/255)");
			}
		}

		public static RouteListItemStatus[] GetUndeliveryStatuses()
		{
			return new RouteListItemStatus[]
			{
				RouteListItemStatus.Canceled,
				RouteListItemStatus.Overdue
			};
		}

		/// <summary>
		/// Возвращает все возможные конечные статусы <see cref="RouteListItem"/>, при которых <see cref="RouteListItem"/> не был довезён
		/// </summary>
		/// <returns></returns>
		public static RouteListItemStatus[] GetNotDeliveredStatuses()
		{
			return new RouteListItemStatus[]
			{
				RouteListItemStatus.Canceled,
				RouteListItemStatus.Overdue,
				RouteListItemStatus.Transfered
			};
		}
	}
}

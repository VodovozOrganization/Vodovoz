﻿using Autofac;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Utilities.Debug;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Controllers;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
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

namespace Vodovoz.Domain.Logistic
{

	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "адреса маршрутного листа",
		Nominative = "адрес маршрутного листа")]
	[HistoryTrace]
	public class RouteListItem : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

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


		#region Свойства

		public virtual int Id { get; set; }

		DateTime version;
		[Display(Name = "Версия")]
		public virtual DateTime Version
		{
			get => version;
			set => SetField(ref version, value);
		}

		private Orders.Order order;
		[Display(Name = "Заказ")]
		public virtual Orders.Order Order {
			get => order;
			set => SetField(ref order, value);
		}

		private RouteList routeList;
		[Display(Name = "Маршрутный лист")]
		public virtual RouteList RouteList {
			get => routeList;
			set => SetField(ref routeList, value);
		}

		private RouteListItemStatus status;
		[Display(Name = "Статус адреса")]
		public virtual RouteListItemStatus Status {
			get => status;
			protected set => SetField(ref status, value);
		}

		private DateTime? statusLastUpdate;
		[Display(Name = "Время изменения статуса")]
		public virtual DateTime? StatusLastUpdate {
			get => statusLastUpdate;
			set => SetField(ref statusLastUpdate, value);
		}

		private DateTime _creationDate;
		[Display(Name = "Дата создания")]
		public virtual DateTime CreationDate => _creationDate == default
			? DateTime.Now
			: _creationDate;

		[Display(Name = "Перенесен в другой маршрутный лист")]
		public virtual RouteListItem TransferedTo {
			get => _transferredTo;
			protected set => SetField(ref _transferredTo, value);
		}

		private bool wasTransfered;
		[Display(Name = "Был перенесен")]
		public virtual bool WasTransfered {
			get => wasTransfered;
			set => SetField(ref wasTransfered, value);
		}

		private string cashierComment;
		[Display(Name = "Комментарий кассира")]
		public virtual string CashierComment {
			get => cashierComment;
			set => SetField(ref cashierComment, value);
		}

		private DateTime? cashierCommentCreateDate;
		[Display(Name = "Дата создания комментария кассира")]
		public virtual DateTime? CashierCommentCreateDate {
			get => cashierCommentCreateDate;
			set => SetField(ref cashierCommentCreateDate, value);
		}

		private DateTime? cashierCommentLastUpdate;
		[Display(Name = "Дата обновления комментария кассира")]
		public virtual DateTime? CashierCommentLastUpdate {
			get => cashierCommentLastUpdate;
			set => SetField(ref cashierCommentLastUpdate, value);
		}

		private Employee cashierCommentAuthor;
		[Display(Name = "Автор комментария")]
		public virtual Employee CashierCommentAuthor {
			get => cashierCommentAuthor;
			set => SetField(ref cashierCommentAuthor, value);
		}

		private string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get => comment;
			set => SetField(ref comment, value);
		}

		private bool withForwarder;
		[Display(Name = "С экспедитором")]
		public virtual bool WithForwarder {
			get => withForwarder;
			set => SetField(ref withForwarder, value);
		}

		private int indexInRoute;
		[Display(Name = "Порядковый номер в МЛ")]
		public virtual int IndexInRoute {
			get => indexInRoute;
			set => SetField(ref indexInRoute, value);
		}

		private int bottlesReturned;
		[Display(Name = "Возвращено бутылей")]
		public virtual int BottlesReturned {
			get => bottlesReturned;
			set => SetField(ref bottlesReturned, value);
		}

		private int? driverBottlesReturned;
		[Display(Name = "Возвращено бутылей - водитель")]
		public virtual int? DriverBottlesReturned {
			get => driverBottlesReturned;
			set => SetField(ref driverBottlesReturned, value);
		}

		[Display(Name = "Тип переноа адреса")]
		public virtual AddressTransferType? AddressTransferType
		{
			get => _addressTransferType;
			set => SetField(ref _addressTransferType, value);
		}

		private decimal oldBottleDepositsCollected;
		/// <summary>
		/// Устаревший залог за бутыли. Который раньше вводился пользователем вручную при закрытии МЛ
		/// </summary>
		[Display(Name = "Старый залог за бутыли")]
		public virtual decimal OldBottleDepositsCollected {
			get => oldBottleDepositsCollected;
			set => SetField(ref oldBottleDepositsCollected, value);
		}

		public virtual decimal BottleDepositsCollected {
			get {
				if(Order.PaymentType == PaymentType.ContractDocumentation || Order.PaymentType == PaymentType.Cashless) {
					return 0;
				}

				if(oldBottleDepositsCollected != 0m) {
					return oldBottleDepositsCollected;
				}

				return 0 - Order.BottleDepositSum;
			}
		}

		private decimal oldEquipmentDepositsCollected;
		/// <summary>
		/// Устаревший залог за оборудование. Который раньше вводился пользователем вручную при закрытии МЛ
		/// </summary>
		[Display(Name = "Старый залог за оборудование")]
		public virtual decimal OldEquipmentDepositsCollected {
			get => oldEquipmentDepositsCollected;
			set => SetField(ref oldEquipmentDepositsCollected, value);
		}

		public virtual decimal EquipmentDepositsCollected {
			get {
				if(Order.PaymentType == PaymentType.ContractDocumentation || Order.PaymentType == PaymentType.Cashless) {
					return 0;
				}

				if(oldEquipmentDepositsCollected != 0m) {
					return oldEquipmentDepositsCollected;
				}

				return 0 - Order.EquipmentDepositSum;
			}
		}

		public virtual decimal AddressCashSum {
			get {
				if(!IsDelivered()) {
					return 0;
				}
				if(Order.PaymentType != PaymentType.Cash) {
					return 0;
				}
				return Order.OrderCashSum + OldBottleDepositsCollected + OldEquipmentDepositsCollected + ExtraCash;
			}
		}

		private decimal totalCash;
		[Display(Name = "Всего наличных")]
		public virtual decimal TotalCash {
			get => totalCash;
			set => SetField(ref totalCash, value);
		}

		private decimal extraCash;
		[Display(Name = "Дополнительно наличных")]
		public virtual decimal ExtraCash {
			get => extraCash;
			set => SetField(ref extraCash, value);
		}

		private decimal driverWage;
		[Display(Name = "ЗП водителя")]
		public virtual decimal DriverWage {
			get => driverWage;
			set => SetField(ref driverWage, value);
		}

		private decimal driverWageSurcharge;
		[Display(Name = "Надбавка к ЗП водителя")]
		public virtual decimal DriverWageSurcharge {
			get => driverWageSurcharge;
			set => SetField(ref driverWageSurcharge, value);
		}

		private decimal forwarderWage;
		[Display(Name = "ЗП экспедитора")]
		//Зарплана с уже включенной надбавкой ForwarderWageSurcharge
		public virtual decimal ForwarderWage {
			get => forwarderWage;
			set => SetField(ref forwarderWage, value);
		}

		[Display(Name = "Оповещение за 30 минут")]
		[IgnoreHistoryTrace]
		public virtual bool Notified30Minutes { get; set; }

		[Display(Name = "Время оповещения прошло")]
		[IgnoreHistoryTrace]
		public virtual bool NotifiedTimeout { get; set; }

		private TimeSpan? planTimeStart;
		[Display(Name = "Запланированное время приезда min")]
		public virtual TimeSpan? PlanTimeStart {
			get => planTimeStart;
			set => SetField(ref planTimeStart, value);
		}

		private TimeSpan? planTimeEnd;
		[Display(Name = "Запланированное время приезда max")]
		public virtual TimeSpan? PlanTimeEnd {
			get => planTimeEnd;
			set => SetField(ref planTimeEnd, value);
		}

		private WageDistrictLevelRate forwarderWageCalulationMethodic;
		[Display(Name = "Методика расчёта ЗП экспедитора")]
		public virtual WageDistrictLevelRate ForwarderWageCalculationMethodic {
			get => forwarderWageCalulationMethodic;
			set => SetField(ref forwarderWageCalulationMethodic, value);
		}

		private WageDistrictLevelRate driverWageCalulationMethodic;
		[Display(Name = "Методика расчёта ЗП водителя")]
		public virtual WageDistrictLevelRate DriverWageCalculationMethodic {
			get => driverWageCalulationMethodic;
			set => SetField(ref driverWageCalulationMethodic, value);
		}

		private LateArrivalReason lateArrivalReason;
		[Display(Name = "Причина опоздания водителя")]
		public virtual LateArrivalReason LateArrivalReason {
			get => lateArrivalReason;
			set => SetField(ref lateArrivalReason, value);
		}

		private Employee lateArrivalReasonAuthor;
		[Display(Name = "Автор причины опоздания водителя")]
		public virtual Employee LateArrivalReasonAuthor {
			get => lateArrivalReasonAuthor;
			set => SetField(ref lateArrivalReasonAuthor, value);
		}

		private string commentForFine;
		[Display(Name = "Комментарий по штрафу")]
		public virtual string CommentForFine {
			get => commentForFine;
			set => SetField(ref commentForFine, value);
		}

		private Employee commentForFineAuthor;
		[Display(Name = "Последний редактор комментария по штрафу")]
		public virtual Employee CommentForFineAuthor {
			get => commentForFineAuthor;
			set
			{
				if(commentForFineAuthor != value)
					SetField(ref commentForFineAuthor, value);
			}
		}

		private IList<Fine> fines = new List<Fine>();
		[Display(Name = "Штрафы")]
		public virtual IList<Fine> Fines {
			get => fines;
			set => SetField(ref fines, value);
		}

		private bool isDriverForeignDistrict;

		[Display(Name = "Чужой район для водителя")]
		public virtual bool IsDriverForeignDistrict
		{
			get => isDriverForeignDistrict;
			set => SetField(ref isDriverForeignDistrict, value);
		}

		GenericObservableList<Fine> observableFines;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<Fine> ObservableFines {
			get {
				if(observableFines == null) {
					observableFines = new GenericObservableList<Fine>(Fines);
				}
				return observableFines;
			}
		}

		#endregion

		#region Runtime свойства (не мапятся)

		public virtual bool AddressIsValid { get; set; } = true;

		#endregion

		#region Расчетные

		public virtual string Title => string.Format("Адрес в МЛ №{0} - {1}", RouteList.Id, Order.DeliveryPoint.CompiledAddress);

		//FIXME запуск оборудования - временный фикс
		public virtual int CoolersToClient {
			get {
				return Order.OrderEquipments.Where(item => item.Direction == Direction.Deliver)
					.Where(item => item.Confirmed)
							.Count(item => item.Equipment != null ? item.Equipment.Nomenclature.Category == NomenclatureCategory.equipment
								   : (item.Nomenclature.Category == NomenclatureCategory.equipment));
			}
		}

		public virtual int PlannedCoolersToClient {
			get {
				return Order.OrderEquipments.Where(item => item.Direction == Direction.Deliver).Where(item => item.Equipment != null)
					.Count(item => item.Equipment.Nomenclature.Kind.WarrantyCardType == WarrantyCardType.CoolerWarranty);
			}
		}

		public virtual int PumpsToClient {
			get {
				return Order.OrderEquipments.Where(item => item.Direction == Direction.Deliver).Where(item => item.Equipment != null)
					.Where(item => item.Confirmed)
					.Count(item => item.Equipment.Nomenclature.Kind.WarrantyCardType == WarrantyCardType.PumpWarranty);
			}
		}

		public virtual int PlannedPumpsToClient {
			get {
				return Order.OrderEquipments.Where(item => item.Direction == Direction.Deliver).Where(item => item.Equipment != null)
					.Count(item => item.Equipment.Nomenclature.Kind.WarrantyCardType == WarrantyCardType.PumpWarranty);
			}
		}

		public virtual int UncategorisedEquipmentToClient {
			get {
				return Order.OrderEquipments.Where(item => item.Direction == Direction.Deliver).Where(item => item.Equipment != null)
					.Where(item => item.Confirmed)
					.Count(item => item.Equipment.Nomenclature.Kind.WarrantyCardType == WarrantyCardType.WithoutCard);
			}
		}

		public virtual int PlannedUncategorisedEquipmentToClient {
			get {
				return Order.OrderEquipments.Where(item => item.Direction == Direction.Deliver).Where(item => item.Equipment != null)
					.Count(item => item.Equipment.Nomenclature.Kind.WarrantyCardType == WarrantyCardType.WithoutCard);
			}
		}

		//FIXME запуск оборудования - временный фикс
		public virtual int CoolersFromClient {
			get {
				return Order.OrderEquipments.Where(item => item.Direction == Direction.PickUp)
					.Where(item => item.Confirmed).Where(item => item.Equipment != null)
							.Count(item => item.Equipment.Nomenclature.Category == NomenclatureCategory.equipment);
			}
		}

		public virtual int PumpsFromClient {
			get {
				return Order.OrderEquipments.Where(item => item.Direction == Direction.PickUp).Where(item => item.Equipment != null)
					.Where(item => item.Confirmed)
					.Count(item => item.Equipment.Nomenclature.Kind.WarrantyCardType == WarrantyCardType.PumpWarranty);
			}
		}

		public virtual int PlannedCoolersFromClient {
			get {
				return Order.OrderEquipments.Where(item => item.Direction == Direction.PickUp)
					.Where(item => item.Equipment != null)
					.Count(item => item.Equipment.Nomenclature.Kind.WarrantyCardType == WarrantyCardType.CoolerWarranty);
			}
		}

		public virtual int PlannedPumpsFromClient {
			get {
				return Order.OrderEquipments.Where(item => item.Direction == Direction.PickUp)
					.Where(item => item.Equipment != null)
					.Count(item => item.Equipment.Nomenclature.Kind.WarrantyCardType == WarrantyCardType.PumpWarranty);
			}
		}

		public virtual string EquipmentsToClientText {
			get {
				//Если это старый заказ со старой записью оборудования в виде строки, то выводим только его
				if(!string.IsNullOrWhiteSpace(Order.ToClientText))
					return Order.ToClientText;

				var orderEquipment = string.Join(
								Environment.NewLine,
								Order.OrderEquipments
									.Where(x => x.Direction == Direction.Deliver)
									.Select(x => $"{x.NameString}: {x.Count:N0}")
				);

				var orderItemEquipment = string.Join(
								Environment.NewLine,
								Order.OrderItems
									.Where(x => x.Nomenclature.Category == NomenclatureCategory.equipment)
									.Select(x => $"{x.Nomenclature.Name}: {x.Count:N0}")
				);

				if(String.IsNullOrWhiteSpace(orderItemEquipment))
					return orderEquipment;
				return $"{orderEquipment}{Environment.NewLine}{orderItemEquipment}";
			}
		}

		public virtual string EquipmentsFromClientText {
			get {
				//Если это старый заказ со старой записью оборудования в виде строки, то выводим только его
				if(!string.IsNullOrWhiteSpace(Order.FromClientText))
					return Order.FromClientText;

				return string.Join("\n",
								   Order.OrderEquipments
										   .Where(x => x.Direction == Direction.PickUp)
								   .Select(x => $"{x.NameString}: {x.Count}")
								  );
			}
		}

		public virtual WageDistrictLevelRate DriverWageCalcMethodicTemporaryStore { get; set; }

		public virtual WageDistrictLevelRate ForwarderWageCalcMethodicTemporaryStore { get; set; }

		public virtual bool NeedToLoad => Order.HasItemsNeededToLoad;

		#endregion

		public RouteListItem() { }

		//Конструктор создания новой строки
		public RouteListItem(RouteList routeList, Order order, RouteListItemStatus status)
		{
			this.routeList = routeList;
			if(order.OrderStatus == OrderStatus.Accepted) {
				order.OrderStatus = OrderStatus.InTravelList;
			}
			this.Order = order;
			this.status = status;
		}

		#region Функции

		protected internal virtual void UpdateStatusAndCreateTask(IUnitOfWork uow, RouteListItemStatus status, ICallTaskWorker callTaskWorker, bool isEditAtCashier = false)
		{
			if(Status == status)
				return;

			if(!isEditAtCashier)
			{
				CreateDeliveryFreeBalanceOperation(uow, Status, status);
			}

			Status = status;
			StatusLastUpdate = DateTime.Now;

			switch(Status) {
				case RouteListItemStatus.Canceled:
					Order.ChangeStatusAndCreateTasks(OrderStatus.DeliveryCanceled, callTaskWorker);
					SetOrderActualCountsToZeroOnCanceled();
					break;
				case RouteListItemStatus.Completed:
					Order.ChangeStatusAndCreateTasks(OrderStatus.Shipped, callTaskWorker);
					if (Order.TimeDelivered == null)
					{
						Order.TimeDelivered = DateTime.Now;
					}
					RestoreOrder();
					AutoCancelAutoTransfer(uow);
					break;
				case RouteListItemStatus.EnRoute:
					Order.ChangeStatusAndCreateTasks(OrderStatus.OnTheWay, callTaskWorker);
					RestoreOrder();
					break;
				case RouteListItemStatus.Overdue:
					Order.ChangeStatusAndCreateTasks(OrderStatus.NotDelivered, callTaskWorker);
					SetOrderActualCountsToZeroOnCanceled();
					break;
			}

			uow.Save(Order);

			UpdateRouteListDebt();
		}

		/// <summary>
		/// Автоотмена автопереноса - недовоз, созданный из возвращенного в путь заказа , получает комментарий, а также ответственного "Нет (не недовоз)"
		/// Заказ, созданный из недовоза переходит в статус "Отменен".
		/// Недовоз, созданный из автопереноса получает ответственного "Автоотмена автопереноса", а также аналогичный комментарий.
		/// </summary>
		private void AutoCancelAutoTransfer(IUnitOfWork uow)
		{
			var oldUndeliveries = _undeliveredOrdersRepository.GetListOfUndeliveriesForOrder(uow, Order);

			var currentEmployee = _employeeRepository.GetEmployeeForCurrentUser(uow);

			var oldUndeliveryCommentText = "Доставлен в тот же день";

			foreach(var oldUndelivery in oldUndeliveries)
			{
				oldUndelivery.NewOrder?.ChangeStatus(OrderStatus.Canceled);

				var oldUndeliveredOrderResultComment = new UndeliveredOrderResultComment
				{
					Author = currentEmployee,
					Comment = oldUndeliveryCommentText,
					CreationTime = DateTime.Now,
					UndeliveredOrder = oldUndelivery
				};

				oldUndelivery.ResultComments.Add(oldUndeliveredOrderResultComment);

				var oldOrderGuiltyInUndelivery = new GuiltyInUndelivery
				{
					GuiltySide = GuiltyTypes.None,
					UndeliveredOrder = oldUndelivery
				};

				oldUndelivery.GuiltyInUndelivery.Clear();
				oldUndelivery.GuiltyInUndelivery.Add(oldOrderGuiltyInUndelivery);

				oldUndelivery.AddAutoCommentToOkkDiscussion(uow, oldUndeliveryCommentText);

				uow.Save(oldUndelivery);

				if(oldUndelivery.NewOrder == null)
				{
					continue;
				}

				var newUndeliveries = _undeliveredOrdersRepository.GetListOfUndeliveriesForOrder(uow, oldUndelivery.NewOrder);

				if(newUndeliveries.Any())
				{
					return;
				}

				var newUndeliveredOrder = new UndeliveredOrder
				{
					Author = currentEmployee,
					OldOrder = oldUndelivery.NewOrder,
					EmployeeRegistrator = currentEmployee,
					TimeOfCreation = DateTime.Now,
					InProcessAtDepartment = _subdivisionRepository.GetQCDepartment(uow)
				};

				var undeliveredOrderResultComment = new UndeliveredOrderResultComment
				{
					Author = currentEmployee,
					Comment = GuiltyTypes.AutoСancelAutoTransfer.GetEnumTitle(),
					CreationTime = DateTime.Now,
					UndeliveredOrder = newUndeliveredOrder
				};

				newUndeliveredOrder.ResultComments.Add(undeliveredOrderResultComment);

				var newOrderGuiltyInUndelivery = new GuiltyInUndelivery
				{
					GuiltySide = GuiltyTypes.AutoСancelAutoTransfer,
					UndeliveredOrder = newUndeliveredOrder
				};

				newUndeliveredOrder.GuiltyInUndelivery = new List<GuiltyInUndelivery> { newOrderGuiltyInUndelivery };

				newUndeliveredOrder.AddAutoCommentToOkkDiscussion(uow, GuiltyTypes.AutoСancelAutoTransfer.GetEnumTitle());

				uow.Save(newUndeliveredOrder);
			}
		}


		private void UpdateRouteListDebt()
		{
			if(Order.PaymentType == PaymentType.Cash)
			{
				RecalculateTotalCash();
				RouteList.UpdateRouteListDebt();
			}
		}

		protected internal virtual void UpdateStatus(IUnitOfWork uow, RouteListItemStatus status)
		{
			if(Status == status)
			{
				return;
			}

			var oldStatus = Status;
			Status = status;
			StatusLastUpdate = DateTime.Now;

			switch(Status) {
				case RouteListItemStatus.Canceled:
					Order.ChangeStatus(OrderStatus.DeliveryCanceled);
					SetOrderActualCountsToZeroOnCanceled();
					break;
				case RouteListItemStatus.Completed:
					Order.ChangeStatus(OrderStatus.Shipped);
					if (Order.TimeDelivered == null)
					{
						Order.TimeDelivered = DateTime.Now;
					}
					RestoreOrder();
					AutoCancelAutoTransfer(uow);
					break;
				case RouteListItemStatus.EnRoute:
					Order.ChangeStatus(OrderStatus.OnTheWay);
					RestoreOrder();
					break;
				case RouteListItemStatus.Overdue:
					Order.ChangeStatus(OrderStatus.NotDelivered);
					SetOrderActualCountsToZeroOnCanceled();
					break;
			}
			uow.Save(Order);

			CreateDeliveryFreeBalanceOperation(uow, oldStatus, status);

			UpdateRouteListDebt();
		}

		public virtual void SetTransferTo(RouteListItem targetAddress)
		{
			TransferedTo = targetAddress;
		}

		protected internal virtual void TransferTo(IUnitOfWork uow, RouteListItem targetAddress)
		{
			SetTransferTo(targetAddress);
			SetStatusWithoutOrderChange(uow, RouteListItemStatus.Transfered);
		}
		
		protected internal virtual void RevertTransferAddress(IUnitOfWork uow, IWageParameterService wageParameterService, RouteListItem revertedAddress)
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
			return (int)Order.OrderItems.Where(item => item.Nomenclature.Category == NomenclatureCategory.water && item.Nomenclature.TareVolume == TareVolume.Vol19L)
								   .Sum(item => item.ActualCount ?? 0);
		}

		public virtual int GetFullBottlesToDeliverCount()
		{
			return (int)Order.OrderItems.Where(item => item.Nomenclature.Category == NomenclatureCategory.water && item.Nomenclature.TareVolume == TareVolume.Vol19L)
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
				Status = RouteListItemStatus.Completed;

			foreach(var item in Order.OrderItems)
			{
				item.SetActualCount(IsDelivered() ? item.Count : 0);
			}

			foreach(var equip in Order.OrderEquipments)
				equip.ActualCount = IsDelivered() ? equip.Count : 0;

			foreach(var deposit in Order.OrderDepositItems)
				deposit.ActualCount = IsDelivered() ? deposit.Count : 0;

			if(!Order.IsBottleStockDiscrepancy)
			{
				Order.BottlesByStockActualCount = Order.BottlesByStockCount;
			}

			PerformanceHelper.AddTimePoint(logger, "Обработали номенклатуры");
			BottlesReturned = IsDelivered() ? (DriverBottlesReturned ?? Order.BottlesReturn ?? 0) : 0;
			RecalculateTotalCash();
			RouteList.RecalculateWagesForRouteListItem(this, wageParameterService);
		}

		/// <summary>
		/// Обнуляет фактическое количетво
		/// Использовать если заказ отменен или полностью не доставлен
		/// </summary>
		public virtual void SetOrderActualCountsToZeroOnCanceled() => Order.SetActualCountsToZeroOnCanceled();

		public virtual void RestoreOrder()
		{
			foreach(var item in Order.OrderItems) {
				item.RestoreOriginalDiscount();
				item.PreserveActualCount(true);
			}

			foreach(var equip in Order.OrderEquipments)
				equip.ActualCount = equip.Count;
			foreach(var deposit in Order.OrderDepositItems)
				deposit.ActualCount = deposit.Count;
		}

		private Dictionary<int, decimal> goodsByRouteColumns;

		public virtual Dictionary<int, decimal> GoodsByRouteColumns {
			get {
				if(goodsByRouteColumns == null) {
					goodsByRouteColumns = Order.OrderItems.Where(i => i.Nomenclature.RouteListColumn != null)
						.GroupBy(i => i.Nomenclature.RouteListColumn.Id, i => i.Count)
						.ToDictionary(g => g.Key, g => g.Sum());
				}
				return goodsByRouteColumns;
			}
		}

		public virtual decimal GetGoodsAmountForColumn(int columnId) => GoodsByRouteColumns.ContainsKey(columnId) ? GoodsByRouteColumns[columnId] : 0;

		public virtual decimal GetGoodsActualAmountForColumn(int columnId)
		{
			if(Status == RouteListItemStatus.Transfered)
				return 0;
			return Order.OrderItems.Where(i => i.Nomenclature.RouteListColumn != null && i.Nomenclature.RouteListColumn.Id == columnId)
								   .Sum(i => i.ActualCount ?? 0);
		}

		public virtual void ChangeOrderStatus(OrderStatus orderStatus) => Order.OrderStatus = orderStatus;

		protected internal virtual void SetStatusWithoutOrderChange(IUnitOfWork uow, RouteListItemStatus status)
		{
			var oldStatus = Status;
			Status = status;
			CreateDeliveryFreeBalanceOperation(uow, oldStatus, status);
		}

		public virtual void CreateDeliveryFreeBalanceOperation(IUnitOfWork uow, RouteListItemStatus oldStatus, RouteListItemStatus newStatus)
		{
			RouteListAddressKeepingDocumentController routeListAddressKeepingDocumentController =
				new RouteListAddressKeepingDocumentController(new EmployeeRepository(), _nomenclatureRepository);

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
				
				var addressTransferType = _routeListItemRepository.GetAddressTransferType(routeList.UoW, Id, transferredTo.Id);
				var transferType =  addressTransferType?.GetEnumTitle();

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
				ObservableFines.Add(fine);
		}
		
		public virtual void RemoveAllFines()
		{
			if(ObservableFines.Any())
				ObservableFines.Clear();
		}
		
		public virtual void RemoveFine(Fine fine)
		{
			if(ObservableFines.Any() && ObservableFines.Contains(fine))
				ObservableFines.Remove(fine);
		}

		public virtual string GetAllFines()
		{
			return ObservableFines.Any() ? 
				string.Join("\n", ObservableFines.SelectMany(x => x.ObservableItems)
					.Select(x => x.Title)) 
				: String.Empty;
		}

		#endregion

		#region Для расчетов в логистике

		/// <summary>
		/// Время разгрузки на адресе в секундах.
		/// </summary>
		public virtual int TimeOnPoint => Order.CalculateTimeOnPoint(RouteList.Forwarder != null);

		public virtual TimeSpan CalculatePlanedTime(RouteGeometryCalculator sputnikCache)
		{
			DateTime time = default(DateTime);

			for(int ix = 0; ix < RouteList.Addresses.Count; ix++) {
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
					break;

				time = time.AddSeconds(RouteList.Addresses[ix].TimeOnPoint);
			}
			sputnikCache?.Dispose();
			return time.TimeOfDay;
		}

		public virtual TimeSpan? CalculateTimeLateArrival()
		{
			if (StatusLastUpdate.HasValue)
			{
				var late = StatusLastUpdate.Value.TimeOfDay - Order.DeliverySchedule.To;

				if (late.TotalSeconds > 0)
					return late;
			}

			return null;
		}
		
		#endregion

		#region Зарплата

		public virtual IRouteListItemWageCalculationSource DriverWageCalculationSrc => new RouteListItemWageCalculationSource(this, EmployeeCategory.driver, _deliveryRulesSettings);

		public virtual IRouteListItemWageCalculationSource ForwarderWageCalculationSrc => new RouteListItemWageCalculationSource(this, EmployeeCategory.forwarder, _deliveryRulesSettings);

		public virtual void SaveWageCalculationMethodics()
		{
			DriverWageCalculationMethodic = DriverWageCalcMethodicTemporaryStore;
			ForwarderWageCalculationMethodic = ForwarderWageCalcMethodicTemporaryStore;
		}

		#endregion Зарплата

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (CommentForFine?.Length > 1000)
			{
				yield return new ValidationResult($"В адресе: '{Title}' превышена максимально допустимая длина комментария по штрафу ({CommentForFine.Length}/1000)");
			}

			if (CashierComment?.Length > 255)
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

	public enum RouteListItemStatus
	{
		[Display(Name = "В пути")]
		EnRoute,
		[Display(Name = "Выполнен")]
		Completed,
		[Display(Name = "Доставка отменена")]
		Canceled,
		[Display(Name = "Недовоз")]
		Overdue,
		[Display(Name = "Передан")]
		Transfered
	}

	public class RouteListItemStatusStringType : NHibernate.Type.EnumStringType
	{
		public RouteListItemStatusStringType() : base(typeof(RouteListItemStatus)) { }
	}

	public class AddressTransferTypeStringType : NHibernate.Type.EnumStringType
	{
		public AddressTransferTypeStringType() : base(typeof(AddressTransferType)) { }
	}
}

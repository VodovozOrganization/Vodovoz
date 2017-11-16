using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QSOrmProject;
using QSProjectsLib;
using QSReport;
using QSValidation;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Repository;
using Vodovoz.Tools.Logistic;

namespace Vodovoz.Domain.Logistic
{
	[OrmSubject(Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "Журнал МЛ",
		Nominative = "маршрутный лист")]
	public class RouteList : BusinessObjectBase<RouteList>, IDomainObject, IValidatableObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		#region Свойства

		public virtual int Id { get; set; }

		Employee driver;

		[Display(Name = "Водитель")]
		public virtual Employee Driver {
			get { return driver; }
			set { SetField(ref driver, value, () => Driver); }
		}

		Employee forwarder;

		[Display(Name = "Экспедитор")]
		public virtual Employee Forwarder {
			get { return forwarder; }
			set {
				if(NHibernate.NHibernateUtil.IsInitialized(Addresses) && (forwarder == null || value == null)) {
					foreach(var address in Addresses)
						address.WithForwarder = value != null;
				}
				SetField(ref forwarder, value, () => Forwarder);
			}
		}

		Employee logistican;

		[Display(Name = "Логист")]
		public virtual Employee Logistican {
			get { return logistican; }
			set { SetField(ref logistican, value, () => Logistican); }
		}

		Car car;

		[Display(Name = "Машина")]
		public virtual Car Car {
			get { return car; }
			set {
				SetField(ref car, value, () => Car);
				if(value?.Driver != null)
					Driver = value.Driver;
			}
		}

		DeliveryShift shift;

		[Display(Name = "Смена доставки")]
		public virtual DeliveryShift Shift {
			get { return shift; }
			set {
				SetField(ref shift, value, () => Shift);
			}
		}

		DateTime date;

		[Display(Name = "Дата")]
		public virtual DateTime Date {
			get { return date; }
			set { SetField(ref date, value, () => Date); }
		}

		Decimal actualDistance;

		/// <summary>
		/// Расстояние в километрах
		/// </summary>
		[Display(Name = "Расстояние по кассе")]
		public virtual Decimal ActualDistance {
			get { return actualDistance; }
			set { SetField(ref actualDistance, value, () => ActualDistance); }
		}

		Decimal confirmedDistance;

		/// <summary>
		/// Расстояние в километрах
		/// </summary>
		[Display(Name = "Подтверждённое расстояние")]
		public virtual Decimal ConfirmedDistance {
			get { return confirmedDistance; }
			set {
				SetField(ref confirmedDistance, value, () => ConfirmedDistance);
			}
		}

		private decimal? planedDistance;

		/// <summary>
		/// Расстояние в километрах
		/// </summary>
		[Display(Name = "Планируемое расстояние")]
		public virtual decimal? PlanedDistance {
			get { return planedDistance; }
			protected set { SetField(ref planedDistance, value, () => PlanedDistance); }
		}

		decimal? recalculatedDistance;

		/// <summary>
		/// Расстояние в километрах.
		/// </summary>
		[Display(Name = "Пересчитанное расстояние")]
		public virtual decimal? RecalculatedDistance {
			get { return recalculatedDistance; }
			set {SetField(ref recalculatedDistance, value, () => RecalculatedDistance); }
		}

		RouteListStatus status;

		[Display(Name = "Статус")]
		public virtual RouteListStatus Status {
			get { return status; }
			set { SetField(ref status, value, () => Status); }
		}

		DateTime? closingDate;
		public virtual DateTime? ClosingDate {
			get {
				return closingDate;
			}
			set {
				SetField(ref closingDate, value, () => ClosingDate);
			}
		}

		string closingComment;

		[Display(Name = "Комментарий")]
		public virtual string ClosingComment {
			get { return closingComment; }
			set { SetField(ref closingComment, value, () => ClosingComment); }
		}

		Employee cashier;
		public virtual Employee Cashier {
			get {
				return cashier;
			}
			set {
				SetField(ref cashier, value, () => Cashier);
			}
		}

		Fine bottleFine;

		[Display(Name = "Штраф за бутыли")]
		public virtual Fine BottleFine {
			get { return bottleFine; }
			set { SetField(ref bottleFine, value, () => BottleFine); }
		}

		private FuelOperation fuelOutlayedOperation;

		[Display(Name = "Операции расхода топлива")]
		public virtual FuelOperation FuelOutlayedOperation {
			get { return fuelOutlayedOperation; }
			set { SetField(ref fuelOutlayedOperation, value, () => FuelOutlayedOperation); }
		}

		private FuelDocument fuelGivedDocument;

		[Display(Name = "Документ выдачи топлива")]
		public virtual FuelDocument FuelGivedDocument {
			get { return fuelGivedDocument; }
			set { SetField(ref fuelGivedDocument, value, () => FuelGivedDocument); }
		}

		private bool differencesConfirmed;

		[Display(Name = "Расхождения подтверждены")]
		public virtual bool DifferencesConfirmed {
			get { return differencesConfirmed; }
			set { SetField(ref differencesConfirmed, value, () => DifferencesConfirmed); }
		}

		private DateTime? lastCallTime;

		[Display(Name = "Время последнего созвона")]
		public virtual DateTime? LastCallTime {
			get { return lastCallTime; }
			set { SetField(ref lastCallTime, value, () => LastCallTime); }
		}

		private bool closingFilled;

		/// <summary>
		/// Внутренее поле говорящее о том что первоначалная подготовка маршрутного листа к закрытию выполнена.
		/// Эта операция выполняется 1 раз при первом открытии диалога закрытия МЛ, тут оставляется пометка о том что операция выполнена.
		/// </summary>
		public virtual bool ClosingFilled {
			get { return closingFilled; }
			set { SetField(ref closingFilled, value, () => ClosingFilled); }
		}

		IList<RouteListItem> addresses = new List<RouteListItem>();

		[Display(Name = "Адреса в маршрутном листе")]
		public virtual IList<RouteListItem> Addresses {
			get { return addresses; }
			set {
				SetField(ref addresses, value, () => Addresses);
				SetNullToObservableAddresses();
			}
		}

		GenericObservableList<RouteListItem> observableAddresses;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<RouteListItem> ObservableAddresses {
			get {
				if(observableAddresses == null) {
					observableAddresses = new GenericObservableList<RouteListItem>(addresses);
					observableAddresses.ElementAdded += ObservableAddresses_ElementAdded;
					observableAddresses.ElementRemoved += ObservableAddresses_ElementRemoved;
				}
				return observableAddresses;
			}
		}

		private WagesMovementOperations driverWageOperation;

		[Display(Name = "Операция начисления зарплаты водителю")]
		public virtual WagesMovementOperations DriverWageOperation {
			get { return driverWageOperation; }
			set { SetField(ref driverWageOperation, value, () => DriverWageOperation); }
		}

		private WagesMovementOperations forwarderWageOperation;

		[Display(Name = "Операция начисления зарплаты экспедитору")]
		public virtual WagesMovementOperations ForwarderWageOperation {
			get { return forwarderWageOperation; }
			set { SetField(ref forwarderWageOperation, value, () => ForwarderWageOperation); }
		}

		private bool isManualAccounting;
		public virtual bool IsManualAccounting {
			get { return isManualAccounting; }
			set { SetField(ref isManualAccounting, value, () => IsManualAccounting); }
		}

		private TimeSpan? onLoadTimeStart;

		[Display(Name = "На погрузку в")]
		public virtual TimeSpan? OnLoadTimeStart {
			get { return onLoadTimeStart; }
			set { SetField(ref onLoadTimeStart, value, () => OnLoadTimeStart); }
		}

		private TimeSpan? onLoadTimeEnd;

		[Display(Name = "Закончить погрузку в")]
		public virtual TimeSpan? OnLoadTimeEnd {
			get { return onLoadTimeEnd; }
			set { SetField(ref onLoadTimeEnd, value, () => OnLoadTimeEnd); }
		}

		private int? onLoadGate;

		[Display(Name = "Ворота на погрузку")]
		public virtual int? OnLoadGate {
			get { return onLoadGate; }
			set { SetField(ref onLoadGate, value, () => OnLoadGate); }
		}

		private bool onLoadTimeFixed;

		[Display(Name = "Время погрузки установлено в ручную")]
		public virtual bool OnloadTimeFixed {
			get { return onLoadTimeFixed; }
			set { SetField(ref onLoadTimeFixed, value, () => OnloadTimeFixed); }
		}

		private bool printed;

		[Display(Name = "МЛ уже напечатан")]
		public virtual bool Printed {
			get { return printed; }
			set { SetField(ref printed, value, () => Printed); }
		}

		#endregion

		#region readonly Свойства

		public virtual string Title { get { return String.Format("МЛ №{0}", Id); } }

		public virtual decimal UniqueAddressCount {
			get {
				return Addresses.Where(item => item.IsDelivered()).Select(item => item.Order.DeliveryPoint.Id).Distinct().Count();
			}
		}

		public virtual decimal PhoneSum {
			get {

				return Wages.GetDriverRates(Date).PhoneServiceCompensationRate * UniqueAddressCount;
			}
		}

		public virtual decimal Total {
			get {
				return Addresses.Sum(address => address.TotalCash + address.DepositsCollected + address.EquipmentDepositsCollected) - PhoneSum;
			}
		}

		public virtual decimal MoneyToReturn {
			get {
				decimal payedForFuel = 0;
				if(FuelGivedDocument != null && FuelGivedDocument.PayedForFuel.HasValue)
					payedForFuel = FuelGivedDocument.PayedForFuel.Value;

				return Total - payedForFuel;
			}
		}

		#endregion

		void ObservableAddresses_ElementRemoved(object aList, int[] aIdx, object aObject)
		{
			CheckAddressOrder();
		}

		void ObservableAddresses_ElementAdded(object aList, int[] aIdx)
		{
			CheckAddressOrder();
		}

		#region Функции

		public virtual RouteListItem AddAddressFromOrder(Order order)
		{
			if(order.DeliveryPoint == null)
				throw new NullReferenceException("В маршрутный нельзя добавить заказ без точки доставки.");
			var item = new RouteListItem(this, order, RouteListItemStatus.EnRoute);
			item.WithForwarder = Forwarder != null;
			ObservableAddresses.Add(item);
			return item;
		}

		public virtual void RemoveAddress(RouteListItem address)
		{
			address.RemovedFromRoute();
			UoW.Delete(address);
			ObservableAddresses.Remove(address);
		}

		//TODO Убрать в будущем когда будет понятно что такая сортировка не нужна.
		public virtual void ReorderAddressesByTime()
		{
			throw new InvalidOperationException("Вызван метод, который может нарушить последовательность адресов. Убирая этот эксепшен убедитесь что вы хорошо подумали.");
			var orderedList = Addresses
				.OrderBy(x => x.Order.DeliverySchedule.From)
				.ThenBy(x => x.Order.DeliverySchedule.To)
				.ToList();
			for(int i = 0; i < ObservableAddresses.Count; i++) {
				if(orderedList[i] == ObservableAddresses[i])
					continue;

				ObservableAddresses.Remove(orderedList[i]);
				ObservableAddresses.Insert(i, orderedList[i]);
			}
		}

		//TODO Убрать в будущем когда будет понятно что такая сортировка не нужна.
		public virtual void ReorderAddressesByDailiNumber()
		{
			throw new InvalidOperationException("Вызван метод, который может нарушить последовательность адресов. Убирая этот эксепшен убедитесь что вы хорошо подумали.");
			var orderedList = Addresses.Where(x => x != null)
				.OrderBy(x => x.Order?.DailyNumber)
				.ToList();
			for(int i = 0; i < ObservableAddresses.Count; i++) {
				if(ObservableAddresses[i] == null) {
					ObservableAddresses.RemoveAt(i);
					i--;
					continue;
				}

				if(orderedList[i] == ObservableAddresses[i])
					continue;

				ObservableAddresses.Remove(orderedList[i]);
				ObservableAddresses.Insert(i, orderedList[i]);
			}
		}

		public virtual void CheckAddressOrder()
		{
			for(int i = 0; i < Addresses.Count; i++) {
				if(Addresses[i] == null) {
					Addresses.RemoveAt(i);
					i--;
					continue;
				}

				if(Addresses[i].IndexInRoute != i)
					Addresses[i].IndexInRoute = i;
			}
		}

		private void SetNullToObservableAddresses()
		{
			if(observableAddresses == null)
				return;
			observableAddresses.ElementAdded -= ObservableAddresses_ElementAdded;
			observableAddresses.ElementRemoved -= ObservableAddresses_ElementRemoved;
			observableAddresses = null;
		}

		public virtual void CompleteRoute()
		{
			Status = RouteListStatus.OnClosing;
			foreach(var item in Addresses.Where(x => x.Status == RouteListItemStatus.Completed || x.Status == RouteListItemStatus.EnRoute)) {
				item.Order.OrderStatus = OrderStatus.UnloadingOnStock;
			}
			var track = Repository.Logistics.TrackRepository.GetTrackForRouteList(UoW, Id);
			if(track != null) {
				track.CalculateDistance();
				track.CalculateDistanceToBase();
				UoW.Save(track);
			}
			FirstFillClosing();
			UoW.Save(this);
		}

		public virtual void RollBackEnRouteStatus()
		{
			Status = RouteListStatus.EnRoute;
			ClosingFilled = false;
			foreach(var item in Addresses.Where(x => x.Status == RouteListItemStatus.Completed)) {
				item.Order.OrderStatus = OrderStatus.OnTheWay;
			}

			UoW.Save(this);
		}

		public virtual bool ShipIfCan(IUnitOfWork uow)
		{
			var inLoaded = Repository.Logistics.RouteListRepository.AllGoodsLoaded(uow, this);
			var goods = Repository.Logistics.RouteListRepository.GetGoodsInRLWithoutEquipments(uow, this);

			bool closed = true;
			foreach(var good in goods) {
				var loaded = inLoaded.FirstOrDefault(x => x.NomenclatureId == good.NomenclatureId);
				if(loaded == null || loaded.Amount < good.Amount) {
					closed = false;
					break;
				}
			}
#if !SHORT
			if(closed == true)
			{
				var equipmentsInRoute = Repository.Logistics.RouteListRepository.GetEquipmentsInRL(uow, this);
				foreach(var equipment in equipmentsInRoute)
				{
					var loaded = inLoaded.FirstOrDefault(x => x.EquipmentId == equipment.EquipmentId);
					if(loaded == null || loaded.Amount < equipment.Amount)
					{
						closed = false;
						break;
					}
				}
			}
#endif

			if(closed)
				ChangeStatus(RouteListStatus.EnRoute);

			return closed;
		}

		public virtual void ConfirmMileage()
		{
			Status = RouteListStatus.Closed;
			ClosingDate = DateTime.Now;
		}

		public virtual void ChangeStatus(RouteListStatus newStatus)
		{
			if(newStatus == Status)
				return;

			if(newStatus == RouteListStatus.EnRoute) {
				if(Status == RouteListStatus.InLoading) {
					Status = RouteListStatus.EnRoute;
					foreach(var item in Addresses) {
						item.Order.OrderStatus = OrderStatus.OnTheWay;
					}
				} else
					throw new NotImplementedException();
			} else if(newStatus == RouteListStatus.InLoading) {
				if(Status == RouteListStatus.EnRoute) {
					Status = RouteListStatus.InLoading;
					foreach(var item in Addresses) {
						item.Order.ChangeStatus(OrderStatus.OnLoading);
					}
				} else if(Status == RouteListStatus.New)
					Status = RouteListStatus.InLoading;
				else
					throw new NotImplementedException();
			} else if(newStatus == RouteListStatus.New) {
				if(Status == RouteListStatus.InLoading)
					Status = RouteListStatus.New;
				else
					throw new NotImplementedException();
			}
		}

		#endregion

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(validationContext.Items.ContainsKey("NewStatus")) {
				RouteListStatus newStatus = (RouteListStatus)validationContext.Items["NewStatus"];
				if(newStatus == RouteListStatus.InLoading) {
				}
				if(newStatus == RouteListStatus.Closed) {
					if(ConfirmedDistance <= 0)
						yield return new ValidationResult("Подтвержденное расстояние не может быть меньше 0",
							new[] { Gamma.Utilities.PropertyUtil.GetPropertyName(this, o => o.ConfirmedDistance) });
				}
				if(newStatus == RouteListStatus.MileageCheck) {
					foreach(var address in Addresses) {
						var valid = new QSValidator<Order>(address.Order,
							new Dictionary<object, object> {
							{ "NewStatus", OrderStatus.Closed }
						});

						foreach(var result in valid.Results) {
							yield return result;
						}
					}
				}
			}

			if(Driver == null)
				yield return new ValidationResult("Не заполнен водитель.",
					new[] { Gamma.Utilities.PropertyUtil.GetPropertyName(this, o => o.Driver) });
			if(Car == null)
				yield return new ValidationResult("На заполнен автомобиль.",
					new[] { Gamma.Utilities.PropertyUtil.GetPropertyName(this, o => o.Car) });
		}

		#endregion

		#region Функции относящиеся к закрытию МЛ

		//FIXME потом метод скрыть. Должен вызываться только при переходе в статус на закрытии.
		public virtual void FirstFillClosing()
		{
			PerformanceHelper.StartMeasurement("Первоначальное заполнение");
			var addresesDelivered = Addresses.Where(x => x.Status != RouteListItemStatus.Transfered).ToList();
			foreach(var routeListItem in addresesDelivered) {
				PerformanceHelper.StartPointsGroup($"Заказ {routeListItem.Order.Id}");
				//				var nomenclatures = routeListItem.Order.OrderItems
				//					.Where(item => Nomenclature.GetCategoriesForShipment().Contains(item.Nomenclature.Category))
				//					.Where(item => !item.Nomenclature.Serial).ToList();

				logger.Debug("Количество элементов в заказе {0}", routeListItem.Order.OrderItems.Count);
				routeListItem.FirstFillClosing(UoW);
				PerformanceHelper.EndPointsGroup();
			}

			PerformanceHelper.AddTimePoint("Закончили");
			PerformanceHelper.Main.PrintAllPoints(logger);
			ClosingFilled = true;
		}

		public virtual List<BottlesMovementOperation> UpdateBottlesMovementOperation()
		{
			var result = new List<BottlesMovementOperation>();
			var addresesDelivered = Addresses.Where(x => x.Status != RouteListItemStatus.Transfered).ToList();
			foreach(RouteListItem address in addresesDelivered) {
				int amountDelivered = address.Order.OrderItems
					.Where(item => item.Nomenclature.Category == NomenclatureCategory.water)
					.Sum(item => item.ActualCount);
				if(address.Order.BottlesMovementOperation == null) {
					if(amountDelivered != 0 || address.BottlesReturned != 0) {
						var bottlesMovementOperation = new BottlesMovementOperation {
							OperationTime = address.Order.DeliveryDate.Value.Date.AddHours(23).AddMinutes(59),
							Order = address.Order,
							Delivered = amountDelivered,
							Returned = address.BottlesReturned,
							Counterparty = address.Order.Client,
							DeliveryPoint = address.Order.DeliveryPoint
						};
						address.Order.BottlesMovementOperation = bottlesMovementOperation;
						result.Add(bottlesMovementOperation);
					}
				} else {
					var bottlesMovementOperation = address.Order.BottlesMovementOperation;
					bottlesMovementOperation.Delivered = amountDelivered;
					bottlesMovementOperation.Returned = address.BottlesReturned;
					result.Add(bottlesMovementOperation);
				}
			}
			return result;
		}

		public virtual List<CounterpartyMovementOperation> UpdateCounterpartyMovementOperations()
		{
			var result = new List<CounterpartyMovementOperation>();
			var addresesDelivered = Addresses.Where(x => x.Status != RouteListItemStatus.Transfered).ToList();
			foreach(var orderItem in addresesDelivered.SelectMany(item => item.Order.OrderItems)
				.Where(item => Nomenclature.GetCategoriesForShipment().Contains(item.Nomenclature.Category))
				.Where(item => !item.Nomenclature.Serial)) {
				var operation = orderItem.UpdateCounterpartyOperation(UoW);
				if(operation != null)
					result.Add(operation);
			}

			//FIXME запуск оборудования - временный фикс
			//FIXME Проверка на время тестирования, с более понятным сообщением что прозошло. Если отладим процес можно будет убрать.
			if(addresesDelivered.SelectMany(item => item.Order.OrderEquipments).Any(item => item.Equipment == null))
				throw new InvalidOperationException("В заказе присутстует оборудование без указания серийного номера. К моменту закрытия такого быть не должно.");

			foreach(var orderEquipment in addresesDelivered.SelectMany(item => item.Order.OrderEquipments)
				.Where(item => Nomenclature.GetCategoriesForShipment().Contains(item.Equipment.Nomenclature.Category))) {
				var operation = orderEquipment.UpdateCounterpartyOperation();
				if(operation != null)
					result.Add(operation);
			}
			return result;
		}

		public virtual List<DepositOperation> UpdateDepositOperations(IUnitOfWork UoW)
		{
#if !SHORT
			var result = new List<DepositOperation> ();
			var bottleDepositNomenclature = NomenclatureRepository.GetBottleDeposit (UoW);
			var bottleDepositPrice = bottleDepositNomenclature.GetPrice (1);
			var addresesDelivered = Addresses.Where(x => x.Status != RouteListItemStatus.Transfered).ToList();
			foreach (RouteListItem item in addresesDelivered)//.Where(address=>address.Order.PaymentType == PaymentType.cash))
			{
				var deliveredEquipmentForRent = item.Order.OrderEquipments.Where (eq => eq.Confirmed)
					.Where (eq => eq.Direction == Vodovoz.Domain.Orders.Direction.Deliver)
					.Where (eq => eq.Reason == Reason.Rent);

				var paidRentDepositsFromClient = item.Order.OrderDepositItems
					.Where (deposit => deposit.PaymentDirection == PaymentDirection.FromClient)
					.Where (deposit => deposit.PaidRentItem != null
						 && deliveredEquipmentForRent.Any (eq => eq.Id == deposit.PaidRentItem.Equipment.Id));

				var freeRentDepositsFromClient = item.Order.OrderDepositItems
					.Where (deposit => deposit.PaymentDirection == PaymentDirection.FromClient)
					.Where (deposit => deposit.FreeRentItem != null
						 && deliveredEquipmentForRent.Any (eq => eq.Id == deposit.FreeRentItem.Equipment.Id));

				foreach (var deposit in paidRentDepositsFromClient.Union (freeRentDepositsFromClient)) {
					DepositOperation operation = deposit.DepositOperation;
					if (operation == null) {
						operation = new DepositOperation {
							Order = item.Order,
							OperationTime = item.Order.DeliveryDate.Value.Date.AddHours (23).AddMinutes (59),
							DepositType = DepositType.Equipment,
							Counterparty = item.Order.Client,
							DeliveryPoint = item.Order.DeliveryPoint,
							ReceivedDeposit = deposit.Total
						};

					} else {
						operation.ReceivedDeposit = deposit.Total;
					}
					deposit.DepositOperation = operation;
					result.Add (operation);
				}

				var pickedUpEquipmentForRent = item.Order.OrderEquipments.Where (eq => eq.Confirmed)
					.Where (eq => eq.Direction == Vodovoz.Domain.Orders.Direction.PickUp)
					.Where (eq => eq.Reason == Reason.Rent);

				var paidRentDepositsToClient = item.Order.OrderDepositItems
					.Where (deposit => deposit.PaymentDirection == PaymentDirection.ToClient)
					.Where (deposit => deposit.PaidRentItem != null
						 && pickedUpEquipmentForRent.Any (eq => eq.Id == deposit.PaidRentItem.Equipment.Id));

				var freeRentDepositsToClient = item.Order.OrderDepositItems
					.Where (deposit => deposit.PaymentDirection == PaymentDirection.ToClient)
					.Where (deposit => deposit.FreeRentItem != null
						 && pickedUpEquipmentForRent.Any (eq => eq.Id == deposit.FreeRentItem.Equipment.Id));

				foreach (var deposit in paidRentDepositsToClient.Union (freeRentDepositsToClient)) {
					DepositOperation operation = deposit.DepositOperation;
					if (operation == null) {
						operation = new DepositOperation {
							Order = item.Order,
							OperationTime = item.Order.DeliveryDate.Value.Date.AddHours (23).AddMinutes (59),
							DepositType = DepositType.Equipment,
							Counterparty = item.Order.Client,
							DeliveryPoint = item.Order.DeliveryPoint,
							RefundDeposit = deposit.Total
						};
					} else {
						operation.RefundDeposit = deposit.Total;
					}
					deposit.DepositOperation = operation;
					result.Add (operation);
				}

				//TODO Добавить далее обновление операций,если потребуется раскоментировать код ниже!
				//
				//				var bottleDepositsOperation = new DepositOperation()
				//				{
				//					Order = item.Order,
				//					OperationTime = item.Order.DeliveryDate.Value.Date.AddHours(23).AddMinutes(59),
				//					DepositType = DepositType.Bottles,
				//					Counterparty = item.Order.Client,
				//					DeliveryPoint = item.Order.DeliveryPoint,
				//					ReceivedDeposit = item.DepositsCollected>0 ? item.DepositsCollected : 0,
				//					RefundDeposit = item.DepositsCollected<0 ? -item.DepositsCollected : 0
				//				};					

				//				var depositsCount = (int)(Math.Abs(item.DepositsCollected) / bottleDepositPrice);
				//				var depositOrderItem = item.Order.ObservableOrderItems.FirstOrDefault (i => i.Nomenclature.Id == bottleDepositNomenclature.Id);
				//				var depositItem = item.Order.ObservableOrderDepositItems.FirstOrDefault (i => i.DepositType == DepositType.Bottles);

				//				if (item.DepositsCollected>0) {
				//					if (depositItem != null) {
				//						depositItem.Deposit = bottleDepositPrice;
				//						depositItem.Count = depositsCount;
				//						depositItem.PaymentDirection = PaymentDirection.FromClient;
				//						depositItem.DepositOperation = bottleDepositsOperation;
				//					}
				//					if (depositOrderItem != null)
				//					{
				//						depositOrderItem.Count = depositsCount;
				//						depositOrderItem.ActualCount = depositsCount;
				//					}
				//					else {
				//						item.Order.ObservableOrderItems.Add (new OrderItem {
				//							Order = item.Order,
				//							AdditionalAgreement = null,
				//							Count = depositsCount,
				//							ActualCount = depositsCount,
				//							Equipment = null,
				//							Nomenclature = bottleDepositNomenclature,
				//							Price = bottleDepositPrice
				//						});
				//						item.Order.ObservableOrderDepositItems.Add (new OrderDepositItem {
				//							Order = item.Order,
				//							Count = depositsCount,
				//							Deposit = bottleDepositPrice,
				//							DepositOperation = bottleDepositsOperation,
				//							DepositType = DepositType.Bottles,
				//							FreeRentItem = null,
				//							PaidRentItem = null,
				//							PaymentDirection = PaymentDirection.FromClient
				//						});
				//					}
				//				}
				//				if (item.DepositsCollected==0) {
				//					if (depositItem != null)
				//						item.Order.ObservableOrderDepositItems.Remove (depositItem);
				//					if (depositOrderItem != null)
				//						item.Order.ObservableOrderItems.Remove (depositOrderItem);					
				//				}
				//				if (item.DepositsCollected<0) {
				//					if (depositOrderItem != null)
				//						item.Order.ObservableOrderItems.Remove (depositOrderItem);
				//					if (depositItem != null) {
				//						depositItem.Deposit = bottleDepositPrice;
				//						depositItem.Count = depositsCount;
				//						depositItem.PaymentDirection = PaymentDirection.ToClient;
				//						depositItem.DepositOperation = bottleDepositsOperation;
				//					} else
				//						item.Order.ObservableOrderDepositItems.Add (new OrderDepositItem {
				//							Order = item.Order,
				//							DepositOperation = bottleDepositsOperation,
				//							DepositType = DepositType.Bottles,
				//							Deposit = bottleDepositPrice,
				//							PaidRentItem = null,
				//							FreeRentItem = null,
				//							PaymentDirection = PaymentDirection.ToClient,
				//							Count = depositsCount
				//						});
				//				}
				//				if(bottleDepositsOperation.RefundDeposit!=0 || bottleDepositsOperation.ReceivedDeposit!=0)
				//					result.Add(bottleDepositsOperation);
			}
#else
			var result = new List<DepositOperation>();
			var addresesDelivered = Addresses.Where(x => x.Status != RouteListItemStatus.Transfered).ToList();
			foreach(RouteListItem item in addresesDelivered) {
				if(item.DepositsCollected != 0) {
					DepositOperation bottlesOperation;

					if(item.Order.DepositOperations.Where(x => x.DepositType == DepositType.Bottles).ToList().Count >= 1) {
						bottlesOperation = item.Order.DepositOperations.Where(x => x.DepositType == DepositType.Bottles).FirstOrDefault();
						bottlesOperation.ReceivedDeposit = item.DepositsCollected;
					} else {
						bottlesOperation = new DepositOperation {
							Order = item.Order,
							OperationTime = item.Order.DeliveryDate.Value.Date.AddHours(23).AddMinutes(59),
							DepositType = DepositType.Bottles,
							Counterparty = item.Order.Client,
							DeliveryPoint = item.Order.DeliveryPoint,
							ReceivedDeposit = item.DepositsCollected
						};
					}
					result.Add(bottlesOperation);
				}

				if(item.EquipmentDepositsCollected != 0) {
					DepositOperation equipmentOperation;

					if(item.Order.DepositOperations.Where(x => x.DepositType == DepositType.Equipment).ToList().Count >= 1) {
						equipmentOperation = item.Order.DepositOperations.Where(x => x.DepositType == DepositType.Equipment).FirstOrDefault();
						equipmentOperation.ReceivedDeposit = item.EquipmentDepositsCollected;
					} else {
						equipmentOperation = new DepositOperation {
							Order = item.Order,
							OperationTime = item.Order.DeliveryDate.Value.Date.AddHours(23).AddMinutes(59),
							DepositType = DepositType.Equipment,
							Counterparty = item.Order.Client,
							DeliveryPoint = item.Order.DeliveryPoint,
							ReceivedDeposit = item.EquipmentDepositsCollected
						};
					}
					result.Add(equipmentOperation);
				}
			}
#endif
			return result;
		}

		public virtual List<MoneyMovementOperation> UpdateMoneyMovementOperations()
		{
			var result = new List<MoneyMovementOperation>();
			var addresesDelivered = Addresses.Where(x => x.Status != RouteListItemStatus.Transfered).ToList();
			foreach(var address in addresesDelivered) {
				var order = address.Order;
				var depositsTotal = order.OrderDepositItems.Sum(dep => dep.Count * dep.Deposit);
				Decimal? money = null;
				if(order.PaymentType == PaymentType.cash)
					money = address.TotalCash;
				MoneyMovementOperation moneyMovementOperation = order.MoneyMovementOperation;
				if(moneyMovementOperation == null) {
					moneyMovementOperation = new MoneyMovementOperation() {
						OperationTime = order.DeliveryDate.Value.Date.AddHours(23).AddMinutes(59),
						Order = order,
						Counterparty = order.Client,
						PaymentType = order.PaymentType,
						Debt = order.ActualGoodsTotalSum,
						Money = money,
						Deposit = depositsTotal
					};
				} else {
					moneyMovementOperation.PaymentType = order.PaymentType;
					moneyMovementOperation.Debt = order.ActualGoodsTotalSum;
					moneyMovementOperation.Money = money;
					moneyMovementOperation.Deposit = depositsTotal;
				}
				order.MoneyMovementOperation = moneyMovementOperation;
				result.Add(moneyMovementOperation);
			}
			return result;
		}

		public virtual string[] UpdateCashOperations(ref Income cashIncome, ref Expense cashExpense)
		{
			var messages = new List<string>();
			cashIncome = Repository.Cash.CashRepository.GetIncomeByRouteList(UoW, this.Id);
			cashExpense = Repository.Cash.CashRepository.GetExpenseByRouteListId(UoW, this.Id);
			var oldCashOrder = Repository.Cash.CashRepository.CurrentRouteListCash(UoW, this.Id);
			var isManual = this.IsManualAccounting;
			var totalSum = Total - oldCashOrder;
			if(isManual) {
				if(totalSum > 0) {
					cashIncome = new Income {
						IncomeCategory = Repository.Cash.CategoryRepository.RouteListClosingIncomeCategory(UoW),
						TypeOperation = IncomeType.DriverReport,
						Date = DateTime.Now,
						Casher = cashier,
						Employee = Driver,
						Description = $"Закрытие МЛ №{Id} от {Date:d}",
						Money = Math.Round(totalSum, 0, MidpointRounding.AwayFromZero),
						RouteListClosing = this
					};
					messages.Add(String.Format("Создан приходный ордер на сумму {1:C0}", cashIncome.Id, cashIncome.Money));
				} else {
					cashExpense = new Expense {
						ExpenseCategory = Repository.Cash.CategoryRepository.RouteListClosingExpenseCategory(UoW),
						TypeOperation = ExpenseType.Expense,
						Date = DateTime.Now,
						Casher = cashier,
						Employee = Driver,
						Description = $"Закрытие МЛ #{Id} от {Date:d}",
						Money = Math.Round(-totalSum, 0, MidpointRounding.AwayFromZero),
						RouteListClosing = this
					};
					messages.Add(String.Format("Создан расходный ордер на сумму {1:C0}", cashExpense.Id, cashExpense.Money));
				}
			} else {
				if(Total > 0) {
					if(cashIncome == null) {
						cashIncome = new Income {
							IncomeCategory = Repository.Cash.CategoryRepository.RouteListClosingIncomeCategory(UoW),
							TypeOperation = IncomeType.DriverReport,
							Date = DateTime.Now,
							Casher = cashier,
							Employee = Driver,
							Description = $"Закрытие МЛ №{Id} от {Date:d}",
							Money = Math.Round(Total, 0, MidpointRounding.AwayFromZero)
						};
						messages.Add(String.Format("Создан приходный ордер на сумму {1:C0}", cashIncome.Id, cashIncome.Money));
					} else {
						var newSum = Math.Round(Total, 0, MidpointRounding.AwayFromZero);
						if(cashIncome.Money != newSum) {
							cashIncome.Casher = cashier;
							messages.Add(String.Format("В приходном ордере №{0} изменилась сумма на {1:C0}({2::+#;-#})",
														 cashIncome.Id, newSum, newSum - cashIncome.Money));
							cashIncome.Money = newSum;
						}
					}
					cashIncome.RouteListClosing = this;
					if(cashExpense != null) {
						messages.Add(String.Format("Расходный ордер №{0} на сумму {1:C0} был удалён.", cashExpense.Id, cashExpense.Money));
						UoW.Delete(cashExpense);
					}
				} else {
					if(cashExpense == null) {
						cashExpense = new Expense {
							ExpenseCategory = Repository.Cash.CategoryRepository.RouteListClosingExpenseCategory(UoW),
							TypeOperation = ExpenseType.Expense,
							Date = DateTime.Now,
							Casher = cashier,
							Employee = Driver,
							Description = $"Закрытие МЛ #{Id} от {Date:d}",
							Money = Math.Round(-Total, 0, MidpointRounding.AwayFromZero)
						};
						messages.Add(String.Format("Создан расходный ордер на сумму {1:C0}", cashExpense.Id, cashExpense.Money));
					} else {
						var newSum = Math.Round(-Total, 0, MidpointRounding.AwayFromZero);
						if(cashExpense.Money != newSum) {
							cashExpense.Casher = cashier;
							messages.Add(String.Format("В расходном ордере №{0} изменилась сумма на {1:C0}({2::+#;-#})",
								 cashExpense.Id, newSum, newSum - cashExpense.Money));
							cashExpense.Money = newSum;
						}
					}
					cashExpense.RouteListClosing = this;
					if(cashIncome != null) {
						messages.Add(String.Format("Приходный ордер №{0} на сумму {1:C0} был удалён.", cashIncome.Id, cashIncome.Money));
						UoW.Delete(cashIncome);
					}
				}
			}
			return messages.ToArray();

		}

		public virtual string[] ManualCashOperations(ref Income cashIncome, ref Expense cashExpense, decimal casheInput)
		{
			var messages = new List<string>();
			if(casheInput > 0) {
				cashIncome = new Income {
					IncomeCategory = Repository.Cash.CategoryRepository.RouteListClosingIncomeCategory(UoW),
					TypeOperation = IncomeType.DriverReport,
					Date = DateTime.Now,
					Casher = cashier,
					Employee = Driver,
					Description = $"Дополнение к МЛ №{this.Id} от {Date:d}",
					Money = Math.Round(casheInput, 0, MidpointRounding.AwayFromZero),
					RouteListClosing = this
				};

				messages.Add(String.Format("Создан приходный ордер на сумму {1:C0}", cashIncome.Id, cashIncome.Money));

			} else {
				cashExpense = new Expense {
					ExpenseCategory = Repository.Cash.CategoryRepository.RouteListClosingExpenseCategory(UoW),
					TypeOperation = ExpenseType.Expense,
					Date = DateTime.Now,
					Casher = cashier,
					Employee = Driver,
					Description = $"Дополнение к МЛ #{this.Id} от {Date:d}",
					Money = Math.Round(-casheInput, 0, MidpointRounding.AwayFromZero),
					RouteListClosing = this
				};
				messages.Add(String.Format("Создан расходный ордер на сумму {1:C0}", cashExpense.Id, cashExpense.Money));
			}
			IsManualAccounting = true;
			return messages.ToArray();
		}

		public virtual string EmployeeAdvanceOperation(ref Expense cashExpense, decimal cashInput)  // Метод для создания расходника выдачи аванса из МЛ. @Дима
		{
			string message;

			cashExpense = new Expense {
				ExpenseCategory = Repository.Cash.CategoryRepository.EmployeeSalaryExpenseCategory(UoW),
				TypeOperation = ExpenseType.EmployeeAdvance,
				Date = DateTime.Now,
				Casher = cashier,
				Employee = Driver,
				Description = $"Выдача аванса к МЛ #{this.Id} от {Date:d}", // Уточнить дескрипшен.
				Money = Math.Round(cashInput, 0, MidpointRounding.AwayFromZero),
				RouteListClosing = this
			};

			message = String.Format("Создан расходный ордер на сумму {1:C0}", cashExpense.Id, cashExpense.Money);
			return (message);
		}

		public virtual void Confirm(bool sendForMileageCheck)
		{
			if(Status != RouteListStatus.OnClosing)
				throw new InvalidOperationException(String.Format("Закрыть маршрутный лист можно только если он находится в статусе {0}", RouteListStatus.OnClosing));

			if(Driver != null && Driver.FirstWorkDay == null) {
				Driver.FirstWorkDay = date;
				UoW.Save(Driver);
			}

			if(Forwarder != null && Forwarder.FirstWorkDay == null) {
				Forwarder.FirstWorkDay = date;
				UoW.Save(Forwarder);
			}


			Status = sendForMileageCheck ? RouteListStatus.MileageCheck : RouteListStatus.Closed;
			foreach(var address in Addresses) {
				if(address.Status == RouteListItemStatus.Completed || address.Status == RouteListItemStatus.EnRoute) {
					address.Order.ChangeStatus(OrderStatus.Closed);
					address.UpdateStatus(UoW, RouteListItemStatus.Completed);
				}
				if(address.Status == RouteListItemStatus.Canceled)
					address.Order.ChangeStatus(OrderStatus.DeliveryCanceled);
				if(address.Status == RouteListItemStatus.Overdue)
					address.Order.ChangeStatus(OrderStatus.NotDelivered);
			}

			if(Status == RouteListStatus.Closed)
			{
				ClosingDate = DateTime.Now;
			}

		}

		public virtual void UpdateFuelOperation()
		{
			//Необхомо для того что бы случайно не пересчитать операцию расхода топлива. После массовой смены расхода.
			if(FuelOutlayedOperation != null && Date < new DateTime(2017, 6, 6))
				return;

			if(ActualDistance == 0) {
				if(FuelOutlayedOperation != null) {
					UoW.Delete(FuelOutlayedOperation);
					FuelOutlayedOperation = null;
				}
			} else {
				if(FuelOutlayedOperation == null) {
					FuelOutlayedOperation = new FuelOperation();
				}
				decimal litresOutlayed = (decimal)Car.FuelConsumption
					/ 100 * ActualDistance;

				Car car = Car;
				Employee driver = Driver;

				if(car.IsCompanyHavings)
					driver = null;
				else
					car = null;

				FuelOutlayedOperation.Driver = driver;
				FuelOutlayedOperation.Car = car;
				FuelOutlayedOperation.Fuel = Car.FuelType;
				FuelOutlayedOperation.OperationTime = DateTime.Now;
				FuelOutlayedOperation.LitersOutlayed = litresOutlayed;
			}
		}

		public virtual void RecalculateFuelOutlay()
		{
			if(this.ConfirmedDistance == 0)
				return;
			
			if(FuelOutlayedOperation == null)
			{
				FuelOutlayedOperation = new FuelOperation() {
					OperationTime = DateTime.Now,
					Driver = this.Driver,
					Car = this.Car,
					Fuel = this.Car.FuelType
				};
			}

			FuelOutlayedOperation.LitersOutlayed = GetLitersOutlayed();
		}

		public virtual decimal GetLitersOutlayed()
		{
			return (decimal)Car.FuelConsumption
				/ 100 * this.ConfirmedDistance;
		}

		public virtual decimal GetLitersOutlayed(decimal km)
		{
			return (decimal)Car.FuelConsumption
				/ 100 * km;
		}

		public virtual void UpdateWageOperation()
		{
			var driverWage = Addresses
				.Where(item => item.IsDelivered()).Sum(item => item.DriverWageTotal);
			if(DriverWageOperation == null) {
				DriverWageOperation = new WagesMovementOperations {
					OperationTime = this.Date,
					Employee = Driver,
					Money = driverWage,
					OperationType = WagesType.AccrualWage
				};

			} else {
				DriverWageOperation.Employee = Driver;
				DriverWageOperation.Money = driverWage;
			}
			UoW.Save(DriverWageOperation);

			var forwarderWage = Addresses
				.Where(item => item.IsDelivered()).Sum(item => item.ForwarderWageTotal);

			if(ForwarderWageOperation == null && forwarderWage > 0) {
				ForwarderWageOperation = new WagesMovementOperations {
					OperationTime = this.Date,
					Employee = Forwarder,
					Money = forwarderWage,
					OperationType = WagesType.AccrualWage
				};
			} else if(ForwarderWageOperation != null && forwarderWage > 0) {
				ForwarderWageOperation.Money = forwarderWage;
				ForwarderWageOperation.Employee = Forwarder;
			} else if(ForwarderWageOperation != null) {
				UoW.Delete(ForwarderWageOperation);
				ForwarderWageOperation = null;
			}

			if(ForwarderWageOperation != null)
				UoW.Save(ForwarderWageOperation);
		}

		#endregion

		public RouteList()
		{
			Date = DateTime.Today;
		}

		public virtual ReportInfo OrderOfAddressesRep(int id)
		{
			var reportInfo = new ReportInfo {
				Title = String.Format("Отчёт по порядку адресов в МЛ №{0}", id),
				Identifier = "Logistic.OrderOfAddresses",
				Parameters = new Dictionary<string, object> {
						{ "RouteListId",  id }
				}
			};

			//	var report = new QSReport.ReportViewDlg(reportInfo);

			return reportInfo;
		}

		public virtual List<string> UpdateMovementOperations()
		{
			var messages = new List<string>();

			this.UpdateFuelOperation();

			var counterpartyMovementOperations = this.UpdateCounterpartyMovementOperations();
			var moneyMovementOperations = this.UpdateMoneyMovementOperations();
			var bottleMovementOperations = this.UpdateBottlesMovementOperation();
			var depositsOperations = this.UpdateDepositOperations(UoW);

			counterpartyMovementOperations.ForEach(op => UoW.Save(op));
			bottleMovementOperations.ForEach(op => UoW.Save(op));
			depositsOperations.ForEach(op => UoW.Save(op));
			moneyMovementOperations.ForEach(op => UoW.Save(op));

			this.UpdateWageOperation();

			//Закрываем наличку.
			Income cashIncome = null;
			Expense cashExpense = null;
			messages.AddRange(this.UpdateCashOperations(ref cashIncome, ref cashExpense));

			if(cashIncome != null) UoW.Save(cashIncome);
			if(cashExpense != null) UoW.Save(cashExpense);

			return messages;
		}

		#region Для логистических расчетов

		public virtual TimeSpan FirstAddressTime {
			get {
				return Addresses.First().Order.DeliverySchedule.From;
			}
		}

		public virtual void RecalculatePlanTime(RouteGeometryCalculator sputnikCache)
		{
			TimeSpan minTime;
			//Расчет минимального времени к которому нужно\можно подъехать.
			for(int ix = 0; ix < Addresses.Count; ix++) {

				if(ix == 0) {
					minTime = Addresses[ix].Order.DeliverySchedule.From;

					var timeFromBase = TimeSpan.FromSeconds(sputnikCache.TimeFromBase(Addresses[ix].Order.DeliveryPoint));
					var onBase = minTime - timeFromBase;
					if(Shift != null && onBase < Shift.StartTime)
						minTime = Shift.StartTime + timeFromBase;
				} else
					minTime += TimeSpan.FromSeconds(sputnikCache.TimeSec(Addresses[ix - 1].Order.DeliveryPoint, Addresses[ix].Order.DeliveryPoint));

				Addresses[ix].PlanTimeStart = minTime > Addresses[ix].Order.DeliverySchedule.From ? minTime : Addresses[ix].Order.DeliverySchedule.From;

				minTime += TimeSpan.FromMinutes(Addresses[ix].TimeOnPoint);
			}
			//Расчет максимального времени до которого нужно подъехать.
			TimeSpan maxTime;
			for(int ix = Addresses.Count - 1; ix >= 0; ix--) {

				if(ix == Addresses.Count - 1) {
					maxTime = Addresses[ix].Order.DeliverySchedule.To;
					var timeToBase = TimeSpan.FromSeconds(sputnikCache.TimeToBase(Addresses[ix].Order.DeliveryPoint));
					var onBase = maxTime + timeToBase;
					if(Shift != null && onBase > Shift.EndTime)
						maxTime = Shift.EndTime - timeToBase;
				} else
					maxTime -= TimeSpan.FromSeconds(sputnikCache.TimeSec(Addresses[ix].Order.DeliveryPoint, Addresses[ix + 1].Order.DeliveryPoint));

				if(maxTime > Addresses[ix].Order.DeliverySchedule.To)
					maxTime = Addresses[ix].Order.DeliverySchedule.To;

				maxTime -= TimeSpan.FromMinutes(Addresses[ix].TimeOnPoint);

				if(maxTime < Addresses[ix].PlanTimeStart) { //Расписание испорчено, успеть нельзя. Пытаемся его более менее адекватно отобразить.
					TimeSpan beforeMin = new TimeSpan(1, 0, 0, 0);
					if(ix > 0)
						beforeMin = Addresses[ix - 1].PlanTimeStart.Value
													 + TimeSpan.FromSeconds(sputnikCache.TimeSec(Addresses[ix - 1].Order.DeliveryPoint, Addresses[ix].Order.DeliveryPoint))
													 + TimeSpan.FromMinutes(Addresses[ix - 1].TimeOnPoint);
					if(beforeMin < Addresses[ix].Order.DeliverySchedule.From) {
						Addresses[ix].PlanTimeStart = beforeMin < maxTime ? maxTime : beforeMin;
					}
					maxTime = Addresses[ix].PlanTimeStart.Value;
				}
				Addresses[ix].PlanTimeEnd = maxTime;
			}
		}

		public virtual void RecalculatePlanedDistance(RouteGeometryCalculator distanceCalculator)
		{
			if(Addresses.Count == 0)
				PlanedDistance = 0;
			else
				PlanedDistance = distanceCalculator.GetRouteDistance(GenerateHashPiontsOfRoute()) / 1000m;
		}

		public static void RecalculateOnLoadTime(IList<RouteList> routelists, RouteGeometryCalculator sputnikCache)
		{
			var sorted = routelists.Where(x => x.Addresses.Any() && !x.OnloadTimeFixed)
								   .Select(x => new Tuple<TimeSpan, RouteList>(
									   x.FirstAddressTime - TimeSpan.FromSeconds(sputnikCache.TimeFromBase(x.Addresses.First().Order.DeliveryPoint)),
												 x
									  ))
								   .OrderByDescending(x => x.Item1);
			var fixedTime = routelists.Where(x => x.Addresses.Any() && x.OnloadTimeFixed).ToList();
			var paralellLoading = 4;
			var loadingPlaces = Enumerable.Range(0, paralellLoading).Select(x => new TimeSpan(1, 0, 0, 0)).ToArray();
			foreach(var route in sorted) {
			repeat:
				int selectedPlace = Array.IndexOf(loadingPlaces, loadingPlaces.Max());
				var endLoading = loadingPlaces[selectedPlace] < route.Item1 ? loadingPlaces[selectedPlace] : route.Item1;
				var startLoading = endLoading - TimeSpan.FromMinutes(route.Item2.TimeOnLoadMinuts);
				//Проверяем, не перекрываем ли мы фиксированное время.
				var interfere = fixedTime.Where(x => x.OnLoadTimeEnd >= startLoading).ToList();
				var freeplaces = interfere.Count == 0 ? paralellLoading
										  : loadingPlaces.Count(x => x.TotalSeconds >= interfere.Min(y => y.OnLoadTimeEnd.Value.TotalSeconds));
				if(endLoading == loadingPlaces[selectedPlace])
					logger.Debug("Нехватило места на погрузке");

				if(freeplaces <= interfere.Count || endLoading <= interfere.Min(x => x.OnLoadTimeStart)) {
					var selectedTime = interfere.Max(x => x.OnLoadTimeEnd);
					var selectedFixed = interfere.First(x => x.OnLoadTimeEnd == selectedTime);
					if(loadingPlaces[selectedPlace] >= selectedFixed.OnLoadTimeEnd.Value) {
						loadingPlaces[selectedPlace] = selectedFixed.onLoadTimeStart.Value;
						selectedFixed.OnLoadGate = selectedPlace + 1;
					} else {
						logger.Warn("Маршрутный лист {0} с фиксированным временем погрузки {1:hh\\:mm}-{2:hh\\:mm} не влезает целиком в расписание прогрузки.",
									selectedFixed.Id, selectedFixed.onLoadTimeStart, selectedFixed.OnLoadTimeEnd
								   );
						selectedFixed.OnLoadGate = null;
						if(loadingPlaces[selectedPlace] < selectedFixed.onLoadTimeStart.Value)
							loadingPlaces[selectedPlace] = selectedFixed.onLoadTimeStart.Value;
					}
					fixedTime.Remove(selectedFixed);
					goto repeat;
				}

				route.Item2.OnLoadTimeEnd = endLoading;
				route.Item2.onLoadTimeStart = loadingPlaces[selectedPlace] = startLoading;
				route.Item2.onLoadGate = selectedPlace + 1;
			}
		}

		public virtual long TimeOnLoadMinuts {
			get {
				return Car.TypeOfUse == CarTypeOfUse.Largus ? 15 : 30;
			}
		}

		public virtual long[] GenerateHashPiontsOfRoute()
		{
			var result = new List<long>();
			result.Add(CachedDistance.BaseHash);
			result.AddRange(Addresses.Where(x => x.Order.DeliveryPoint.СoordinatesExist).Select(x => CachedDistance.GetHash(x.Order.DeliveryPoint)));
			result.Add(CachedDistance.BaseHash);
			return result.ToArray();
		}

		#endregion
	}

	public enum RouteListStatus
	{
		[Display(Name = "Новый")]
		New,
		[Display(Name = "На погрузке")]
		InLoading,
		[Display(Name = "В пути")]
		EnRoute,
		[Display(Name = "Сдаётся")]
		OnClosing,
		[Display(Name = "Проверка километража")]
		MileageCheck,
		[Display(Name = "Закрыт")]
		Closed
	}

	public class RouteListStatusStringType : NHibernate.Type.EnumStringType
	{
		public RouteListStatusStringType() : base(typeof(RouteListStatus))
		{
		}
	}
}


using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QSOrmProject;
using QSProjectsLib;
using QSValidation;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Repository;

namespace Vodovoz.Domain.Logistic
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "маршрутные листы",
		Nominative = "маршрутный лист")]
	public class RouteList : BusinessObjectBase<RouteList>, IDomainObject, IValidatableObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		#region Свойства

		public virtual int Id { get; set; }

		Employee driver;

		[Display (Name = "Водитель")]
		public virtual Employee Driver {
			get { return driver; }
			set { SetField (ref driver, value, () => Driver); }
		}

		Employee forwarder;

		[Display (Name = "Экспедитор")]
		public virtual Employee Forwarder {
			get { return forwarder; }
			set {
				if (NHibernate.NHibernateUtil.IsInitialized (Addresses) && (forwarder == null || value == null)) {
					foreach (var address in Addresses)
						address.WithForwarder = value != null;
				}
				SetField (ref forwarder, value, () => Forwarder);
			}
		}

		Employee logistican;

		[Display (Name = "Логист")]
		public virtual Employee Logistican {
			get { return logistican; }
			set { SetField (ref logistican, value, () => Logistican); }
		}

		Car car;

		[Display (Name = "Машина")]
		public virtual Car Car {
			get { return car; }
			set {
				SetField (ref car, value, () => Car);
				if (value?.Driver != null)
					Driver = value.Driver;
			}
		}

		DeliveryShift shift;

		[Display (Name = "Смена доставки")]
		public virtual DeliveryShift Shift {
			get { return shift; }
			set {
				SetField (ref shift, value, () => Shift);
			}
		}

		DateTime date;

		[Display (Name = "Дата")]
		public virtual DateTime Date {
			get { return date; }
			set { SetField (ref date, value, () => Date); }
		}

		Decimal actualDistance;

		[Display (Name = "Фактическое расстояние")]
		public virtual Decimal ActualDistance {
			get { return actualDistance; }
			set { SetField (ref actualDistance, value, () => ActualDistance); }
		}

		Decimal confirmedDistance;

		public virtual Decimal ConfirmedDistance {
			get { return confirmedDistance; }
			set {
				SetField (ref confirmedDistance, value, () => ConfirmedDistance);
			}
		}

		RouteListStatus status;

		[Display (Name = "Статус")]
		public virtual RouteListStatus Status {
			get { return status; }
			set { SetField (ref status, value, () => Status); }
		}

		DateTime? closingDate;
		public virtual DateTime? ClosingDate {
			get {
				return closingDate;
			}
			set {
				SetField (ref closingDate, value, () => ClosingDate);
			}
		}

		string closingComment;

		[Display (Name = "Комментарий")]
		public virtual string ClosingComment {
			get { return closingComment; }
			set { SetField (ref closingComment, value, () => ClosingComment); }
		}

		Employee cashier;
		public virtual Employee Cashier {
			get {
				return cashier;
			}
			set {
				SetField (ref cashier, value, () => Cashier);
			}
		}

		Fine bottleFine;

		[Display (Name = "Штраф за бутыли")]
		public virtual Fine BottleFine {
			get { return bottleFine; }
			set { SetField (ref bottleFine, value, () => BottleFine); }
		}

		private FuelOperation fuelOutlayedOperation;

		[Display (Name = "Операции расхода топлива")]
		public virtual FuelOperation FuelOutlayedOperation {
			get { return fuelOutlayedOperation; }
			set { SetField (ref fuelOutlayedOperation, value, () => FuelOutlayedOperation); }
		}

		private FuelDocument fuelGivedDocument;

		[Display (Name = "Документ выдачи топлива")]
		public virtual FuelDocument FuelGivedDocument {
			get { return fuelGivedDocument; }
			set { SetField (ref fuelGivedDocument, value, () => FuelGivedDocument); }
		}

		private bool differencesConfirmed;

		[Display (Name = "Расхождения подтверждены")]
		public virtual bool DifferencesConfirmed {
			get { return differencesConfirmed; }
			set { SetField (ref differencesConfirmed, value, () => DifferencesConfirmed); }
		}

		private DateTime? lastCallTime;

		[Display (Name = "Время последнего созвона")]
		public virtual DateTime? LastCallTime {
			get { return lastCallTime; }
			set { SetField (ref lastCallTime, value, () => LastCallTime); }
		}

		private bool closingFilled;

		/// <summary>
		/// Внутренее поле говорящее о том что первоначалная подготовка маршрутного листа к закрытию выполнена.
		/// Эта операция выполняется 1 раз при первом открытии диалога закрытия МЛ, тут оставляется пометка о том что операция выполнена.
		/// </summary>
		public virtual bool ClosingFilled {
			get { return closingFilled; }
			set { SetField (ref closingFilled, value, () => ClosingFilled); }
		}

		IList<RouteListItem> addresses = new List<RouteListItem> ();

		[Display (Name = "Адреса в маршрутном листе")]
		public virtual IList<RouteListItem> Addresses {
			get { return addresses; }
			set {
				SetField (ref addresses, value, () => Addresses);
				SetNullToObservableAddresses ();
			}
		}

		GenericObservableList<RouteListItem> observableAddresses;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<RouteListItem> ObservableAddresses {
			get {
				if (observableAddresses == null) {
					observableAddresses = new GenericObservableList<RouteListItem> (addresses);
					observableAddresses.ElementAdded += ObservableAddresses_ElementAdded;
					observableAddresses.ElementRemoved += ObservableAddresses_ElementRemoved;
				}
				return observableAddresses;
			}
		}

		private WagesMovementOperations driverWageOperation;

		[Display (Name = "Операция начисления зарплаты водителю")]
		public virtual WagesMovementOperations DriverWageOperation {
			get { return driverWageOperation; }
			set { SetField (ref driverWageOperation, value, () => DriverWageOperation); }
		}

		private WagesMovementOperations forwarderWageOperation;

		[Display (Name = "Операция начисления зарплаты экспедитору")]
		public virtual WagesMovementOperations ForwarderWageOperation {
			get { return forwarderWageOperation; }
			set { SetField (ref forwarderWageOperation, value, () => ForwarderWageOperation); }
		}

		#endregion

		#region readonly Свойства

		public virtual string Title { get { return String.Format ("Маршрутный лист №{0}", Id); } }

		public virtual decimal UniqueAddressCount {
			get {
				return Addresses.Where (item => item.IsDelivered ()).Select (item => item.Order.DeliveryPoint.Id).Distinct ().Count ();
			}
		}

		public virtual decimal PhoneSum {
			get {

				return Wages.GetDriverRates ().PhoneServiceCompensationRate * UniqueAddressCount;
			}
		}

		public virtual decimal Total {
			get {
				return Addresses.Sum (address => address.TotalCash + address.DepositsCollected) - PhoneSum;
			}
		}

		public virtual decimal MoneyToReturn {
			get {
				decimal payedForFuel = 0;
				if (FuelGivedDocument != null && FuelGivedDocument.PayedForFuel.HasValue)
					payedForFuel = FuelGivedDocument.PayedForFuel.Value;

				return Total - payedForFuel;
			}
		}

		#endregion

		void ObservableAddresses_ElementRemoved (object aList, int [] aIdx, object aObject)
		{
			CheckAddressOrder ();
		}

		void ObservableAddresses_ElementAdded (object aList, int [] aIdx)
		{
			CheckAddressOrder ();
		}

		#region Функции

		public virtual RouteListItem AddAddressFromOrder (Order order)
		{
			if (order.DeliveryPoint == null)
				throw new NullReferenceException ("В маршрутный нельзя добавить заказ без точки доставки.");
			var item = new RouteListItem (this, order, RouteListItemStatus.EnRoute);
			item.WithForwarder = Forwarder != null;
			ObservableAddresses.Add (item);
			return item;
		}

		public virtual void RemoveAddress (RouteListItem address)
		{
			address.RemovedFromRoute ();
			UoW.Delete (address);
			ObservableAddresses.Remove (address);
		}

		public virtual void ReorderAddressesByTime ()
		{
			var orderedList = Addresses
				.OrderBy (x => x.Order.DeliverySchedule.From)
				.ThenBy (x => x.Order.DeliverySchedule.To)
				.ToList ();
			for (int i = 0; i < ObservableAddresses.Count; i++) {
				if (orderedList [i] == ObservableAddresses [i])
					continue;

				ObservableAddresses.Remove (orderedList [i]);
				ObservableAddresses.Insert (i, orderedList [i]);
			}
		}

		public virtual void ReorderAddressesByDailiNumber ()
		{
			var orderedList = Addresses
				.OrderBy (x => x.Order?.DailyNumber1c)
				.ToList ();
			for (int i = 0; i < ObservableAddresses.Count; i++) {
				if (orderedList [i] == ObservableAddresses [i])
					continue;

				ObservableAddresses.Remove (orderedList [i]);
				ObservableAddresses.Insert (i, orderedList [i]);
			}
		}

		private void CheckAddressOrder ()
		{
			int i = 0;
			foreach (var address in Addresses) {
				if (address.IndexInRoute != i)
					address.IndexInRoute = i;
				i++;
			}
		}

		private void SetNullToObservableAddresses ()
		{
			if (observableAddresses == null)
				return;
			observableAddresses.ElementAdded -= ObservableAddresses_ElementAdded;
			observableAddresses.ElementRemoved -= ObservableAddresses_ElementRemoved;
			observableAddresses = null;
		}

		public virtual void CompleteRoute ()
		{
			Status = RouteListStatus.OnClosing;
			foreach (var item in Addresses.Where (x => x.Status == RouteListItemStatus.Completed || x.Status == RouteListItemStatus.EnRoute)) {
				item.Order.OrderStatus = OrderStatus.UnloadingOnStock;
			}
			var track = Repository.Logistics.TrackRepository.GetTrackForRouteList (UoW, Id);
			if (track != null) {
				track.CalculateDistance ();
				track.CalculateDistanceToBase ();
				UoW.Save (track);
			}
			FirstFillClosing ();
			UoW.Save (this);
		}

		public virtual bool ShipIfCan (IUnitOfWork uow)
		{
			var inLoaded = Repository.Logistics.RouteListRepository.AllGoodsLoaded (uow, this);
			var goods = Repository.Logistics.RouteListRepository.GetGoodsInRLWithoutEquipments (uow, this);

			bool closed = true;
			foreach (var good in goods) {
				var loaded = inLoaded.FirstOrDefault (x => x.NomenclatureId == good.NomenclatureId);
				if (loaded == null || loaded.Amount < good.Amount) {
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

			if (closed)
				ChangeStatus (RouteListStatus.EnRoute);

			return closed;
		}

		public virtual void ConfirmMileage ()
		{
			Status = RouteListStatus.Closed;
		}

		public virtual void ChangeStatus (RouteListStatus newStatus)
		{
			if (newStatus == Status)
				return;

			if (newStatus == RouteListStatus.EnRoute) {
				if (Status == RouteListStatus.InLoading) {
					Status = RouteListStatus.EnRoute;
					foreach (var item in Addresses) {
						item.Order.OrderStatus = OrderStatus.OnTheWay;
					}
				} else
					throw new NotImplementedException ();
			} else if (newStatus == RouteListStatus.InLoading) {
				if (Status == RouteListStatus.EnRoute) {
					Status = RouteListStatus.InLoading;
					foreach (var item in Addresses) {
						item.Order.ChangeStatus (OrderStatus.OnLoading);
					}
				} else if (Status == RouteListStatus.New)
					Status = RouteListStatus.InLoading;
				else
					throw new NotImplementedException ();
			} else if (newStatus == RouteListStatus.New) {
				if (Status == RouteListStatus.InLoading)
					Status = RouteListStatus.New;
				else
					throw new NotImplementedException ();
			}
		}

		#endregion

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (validationContext.Items.ContainsKey ("NewStatus")) {
				RouteListStatus newStatus = (RouteListStatus)validationContext.Items ["NewStatus"];
				if (newStatus == RouteListStatus.InLoading) {
				}
				if (newStatus == RouteListStatus.Closed) {
					if (ConfirmedDistance <= 0)
						yield return new ValidationResult ("Подтвержденное расстояние не может быть меньше 0",
							new [] { Gamma.Utilities.PropertyUtil.GetPropertyName (this, o => o.ConfirmedDistance) });
				}
				if (newStatus == RouteListStatus.MileageCheck) {
					foreach (var address in Addresses) {
						var valid = new QSValidator<Order> (address.Order,
							new Dictionary<object, object> {
							{ "NewStatus", OrderStatus.Closed }
						});

						foreach (var result in valid.Results) {
							yield return result;
						}
					}
				}
			}

			if (Shift == null)
				yield return new ValidationResult ("Смена маршрутного листа должна быть заполнена.",
					new [] { Gamma.Utilities.PropertyUtil.GetPropertyName (this, o => o.Shift) });

			if (Driver == null)
				yield return new ValidationResult ("Не заполнен водитель.",
					new [] { Gamma.Utilities.PropertyUtil.GetPropertyName (this, o => o.Driver) });
			if (Car == null)
				yield return new ValidationResult ("На заполнен автомобиль.",
					new [] { Gamma.Utilities.PropertyUtil.GetPropertyName (this, o => o.Car) });
		}

		#endregion

		#region Функции относящиеся к закрытию МЛ

		//FIXME потом метод скрыть. Должен вызываться только при переходе в статус на закрытии.
		public virtual void FirstFillClosing ()
		{
			PerformanceHelper.StartMeasurement ("Первоначальное заполнение");

			foreach (var routeListItem in Addresses) {
				PerformanceHelper.StartPointsGroup ($"Заказ {routeListItem.Order.Id}");
				//				var nomenclatures = routeListItem.Order.OrderItems
				//					.Where(item => Nomenclature.GetCategoriesForShipment().Contains(item.Nomenclature.Category))
				//					.Where(item => !item.Nomenclature.Serial).ToList();

				logger.Debug ("Количество элементов в заказе {0}", routeListItem.Order.OrderItems.Count);
				routeListItem.FirstFillClosing (UoW);
				PerformanceHelper.EndPointsGroup ();
			}

			PerformanceHelper.AddTimePoint ("Закончили");
			PerformanceHelper.Main.PrintAllPoints (logger);
			ClosingFilled = true;
		}

		public virtual List<BottlesMovementOperation> UpdateBottlesMovementOperation ()
		{
			var result = new List<BottlesMovementOperation> ();
			foreach (RouteListItem address in Addresses) {
				int amountDelivered = address.Order.OrderItems
					.Where (item => item.Nomenclature.Category == NomenclatureCategory.water)
					.Sum (item => item.ActualCount);
				if (address.Order.BottlesMovementOperation == null) {
					if (amountDelivered != 0 || address.BottlesReturned != 0) {
						var bottlesMovementOperation = new BottlesMovementOperation {
							OperationTime = address.Order.DeliveryDate.Value.Date.AddHours (23).AddMinutes (59),
							Order = address.Order,
							Delivered = amountDelivered,
							Returned = address.BottlesReturned,
							Counterparty = address.Order.Client,
							DeliveryPoint = address.Order.DeliveryPoint
						};
						address.Order.BottlesMovementOperation = bottlesMovementOperation;
						result.Add (bottlesMovementOperation);
					}
				} else {
					var bottlesMovementOperation = address.Order.BottlesMovementOperation;
					bottlesMovementOperation.Delivered = amountDelivered;
					bottlesMovementOperation.Returned = address.BottlesReturned;
					result.Add (bottlesMovementOperation);
				}
			}
			return result;
		}

		public virtual List<CounterpartyMovementOperation> UpdateCounterpartyMovementOperations ()
		{
			var result = new List<CounterpartyMovementOperation> ();
			foreach (var orderItem in Addresses.SelectMany (item => item.Order.OrderItems)
				.Where (item => Nomenclature.GetCategoriesForShipment ().Contains (item.Nomenclature.Category))
				.Where (item => !item.Nomenclature.Serial)) {
				var operation = orderItem.UpdateCounterpartyOperation (UoW);
				if (operation != null)
					result.Add (operation);
			}

			//FIXME Проверка на время тестирования, с более понятным сообщением что прозошло. Если отладим процес можно будет убрать.
			if (Addresses.SelectMany (item => item.Order.OrderEquipments).Any (item => item.Equipment == null))
				throw new InvalidOperationException ("В заказе присутстует оборудование без указания серийного номера. К моменту закрытия такого быть не должно.");

			foreach (var orderEquipment in Addresses.SelectMany (item => item.Order.OrderEquipments)
				.Where (item => Nomenclature.GetCategoriesForShipment ().Contains (item.Equipment.Nomenclature.Category))) {
				var operation = orderEquipment.UpdateCounterpartyOperation ();
				if (operation != null)
					result.Add (operation);
			}
			return result;
		}

		public virtual List<DepositOperation> UpdateDepositOperations (IUnitOfWork UoW)
		{
			var result = new List<DepositOperation> ();
			var bottleDepositNomenclature = NomenclatureRepository.GetBottleDeposit (UoW);
			var bottleDepositPrice = bottleDepositNomenclature.GetPrice (1);
			foreach (RouteListItem item in Addresses)//.Where(address=>address.Order.PaymentType == PaymentType.cash))
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
			return result;
		}

		public virtual List<MoneyMovementOperation> UpdateMoneyMovementOperations ()
		{
			var result = new List<MoneyMovementOperation> ();
			foreach (var address in Addresses) {
				var order = address.Order;
				var depositsTotal = order.OrderDepositItems.Sum (dep => dep.Count * dep.Deposit);
				Decimal? money = null;
				if (order.PaymentType == PaymentType.cash)
					money = address.TotalCash;
				MoneyMovementOperation moneyMovementOperation = order.MoneyMovementOperation;
				if (moneyMovementOperation == null) {
					moneyMovementOperation = new MoneyMovementOperation () {
						OperationTime = order.DeliveryDate.Value.Date.AddHours (23).AddMinutes (59),
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
				result.Add (moneyMovementOperation);
			}
			return result;
		}

		public virtual string [] UpdateCashOperations (ref Income cashIncome, ref Expense cashExpense)
		{
			var messages = new List<string> ();
			cashIncome = Repository.Cash.CashRepository.GetIncomeByRouteList (UoW, this.Id);
			cashExpense = Repository.Cash.CashRepository.GetExpenseByRouteListId (UoW, this.Id);

			if (Total > 0) {
				if (cashIncome == null) {
					cashIncome = new Income {
						IncomeCategory = Repository.Cash.CategoryRepository.RouteListClosingIncomeCategory (UoW),
						TypeOperation = IncomeType.DriverReport,
						Date = DateTime.Now,
						Casher = cashier,
						Employee = Driver,
						Description = $"Закрытие МЛ №{Id} от {Date:d}",
						Money = Math.Round (Total, 0, MidpointRounding.AwayFromZero)
					};
					messages.Add (String.Format ("Создан приходный ордер на сумму {1:C0}", cashIncome.Id, cashIncome.Money));
				} else {
					var newSum = Math.Round (Total, 0, MidpointRounding.AwayFromZero);
					if (cashIncome.Money != newSum) {
						cashIncome.Casher = cashier;
						messages.Add (String.Format ("В приходном ордере №{0} изменилась сумма на {1:C0}({2::+#;-#})",
													 cashIncome.Id, newSum, newSum - cashIncome.Money));
						cashIncome.Money = newSum;
					}
				}
				cashIncome.RouteListClosing = this;
				if (cashExpense != null) {
					messages.Add (String.Format ("Расходный ордер №{0} на сумму {1:C0} был удалён.", cashExpense.Id, cashExpense.Money));
					UoW.Delete (cashExpense);
				}
			} else {
				if (cashExpense == null) {
					cashExpense = new Expense {
						ExpenseCategory = Repository.Cash.CategoryRepository.RouteListClosingExpenseCategory (UoW),
						TypeOperation = ExpenseType.Expense,
						Date = DateTime.Now,
						Casher = cashier,
						Employee = Driver,
						Description = $"Закрытие МЛ #{Id} от {Date:d}",
						Money = Math.Round (-Total, 0, MidpointRounding.AwayFromZero)
					};
					messages.Add (String.Format ("Создан расходный ордер на сумму {1:C0}", cashExpense.Id, cashExpense.Money));
				} else {
					var newSum = Math.Round (-Total, 0, MidpointRounding.AwayFromZero);
					if (cashExpense.Money != newSum) {
						cashExpense.Casher = cashier;
						messages.Add (String.Format ("В расходном ордере №{0} изменилась сумма на {1:C0}({2::+#;-#})",
							 cashExpense.Id, newSum, newSum - cashExpense.Money));
						cashExpense.Money = newSum;
					}
				}
				cashExpense.RouteListClosing = this;
				if (cashIncome != null) {
					messages.Add (String.Format ("Приходный ордер №{0} на сумму {1:C0} был удалён.", cashIncome.Id, cashIncome.Money));
					UoW.Delete (cashIncome);
				}
			}
			return messages.ToArray ();
		}

		public virtual void Confirm ()
		{
			if (Status != RouteListStatus.OnClosing)
				throw new InvalidOperationException (String.Format ("Закрыть маршрутный лист можно только если он находится в статусе {0}", RouteListStatus.OnClosing));

			Status = RouteListStatus.MileageCheck;
			foreach (var address in Addresses) {
				if (address.Status == RouteListItemStatus.Completed || address.Status == RouteListItemStatus.EnRoute) {
					address.Order.ChangeStatus (OrderStatus.Closed);
					address.UpdateStatus (UoW, RouteListItemStatus.Completed);
				}
				if (address.Status == RouteListItemStatus.Canceled)
					address.Order.ChangeStatus (OrderStatus.DeliveryCanceled);
				if (address.Status == RouteListItemStatus.Overdue)
					address.Order.ChangeStatus (OrderStatus.NotDelivered);
			}
			ClosingDate = DateTime.Now;
		}

		public virtual void UpdateFuelOperation ()
		{
			if (ActualDistance == 0) {
				if (FuelOutlayedOperation != null) {
					UoW.Delete (FuelOutlayedOperation);
					FuelOutlayedOperation = null;
				}
			} else {
				if (FuelOutlayedOperation == null) {
					FuelOutlayedOperation = new FuelOperation ();
				}
				decimal litresOutlayed = (decimal)Car.FuelConsumption
					/ 100 * ActualDistance;

				Car car = Car;
				Employee driver = Driver;

				if (car.IsCompanyHavings)
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

		public virtual void RecalculateFuelOutlay ()
		{
			if (this.ConfirmedDistance == 0)
				return;

			FuelOutlayedOperation.LitersOutlayed = GetLitersOutlayed ();
		}

		public virtual decimal GetLitersOutlayed ()
		{
			return (decimal)Car.FuelConsumption
				/ 100 * this.ConfirmedDistance;
		}

		public virtual decimal GetLitersOutlayed (decimal km)
		{
			return (decimal)Car.FuelConsumption
				/ 100 * km;
		}

		public virtual void UpdateWageOperation ()
		{
			var driverWage = Addresses
				.Where (item => item.IsDelivered ()).Sum (item => item.DriverWageTotal);
			if (DriverWageOperation == null) {
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
			UoW.Save (DriverWageOperation);

			var forwarderWage = Addresses
				.Where (item => item.IsDelivered ()).Sum (item => item.ForwarderWageTotal);

			if (ForwarderWageOperation == null && forwarderWage > 0) {
				ForwarderWageOperation = new WagesMovementOperations {
					OperationTime = this.Date,
					Employee = Forwarder,
					Money = forwarderWage,
					OperationType = WagesType.AccrualWage
				};
			} else if (ForwarderWageOperation != null && forwarderWage > 0) {
				ForwarderWageOperation.Money = forwarderWage;
				ForwarderWageOperation.Employee = Forwarder;
			} else if (ForwarderWageOperation != null) {
				UoW.Delete (ForwarderWageOperation);
				ForwarderWageOperation = null;
			}

			if (ForwarderWageOperation != null)
				UoW.Save (ForwarderWageOperation);
		}

		#endregion

		public RouteList ()
		{
			Date = DateTime.Today;
		}
	}

	public enum RouteListStatus
	{
		[Display (Name = "Новый")]
		New,
		[Display (Name = "На погрузке")]
		InLoading,
		[Display (Name = "В пути")]
		EnRoute,
		[Display (Name = "Сдаётся")]
		OnClosing,
		[Display (Name = "Проверка километража")]
		MileageCheck,
		[Display (Name = "Закрыт")]
		Closed
	}

	public class RouteListStatusStringType : NHibernate.Type.EnumStringType
	{
		public RouteListStatusStringType () : base (typeof (RouteListStatus))
		{
		}
	}
}


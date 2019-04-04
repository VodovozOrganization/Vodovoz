using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using NHibernate.Util;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Report;
using QSProjectsLib;
using QSSupportLib;
using QSValidation;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.Repositories.HumanResources;
using Vodovoz.Repositories.Permissions;
using Vodovoz.Repository.Cash;
using Vodovoz.Repository.Logistics;
using Vodovoz.Tools.Logistic;

namespace Vodovoz.Domain.Logistic
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Журнал МЛ",
		Nominative = "маршрутный лист")]
	[HistoryTrace]
	[EntityPermission]
	public class RouteList : BusinessObjectBase<RouteList>, IDomainObject, IValidatableObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		#region Свойства

		public virtual int Id { get; set; }

		DateTime version;
		[Display(Name = "Версия")]
		public virtual DateTime Version {
			get => version;
			set => SetField(ref version, value, () => Version);
		}

		Employee driver;

		[Display(Name = "Водитель")]
		public virtual Employee Driver {
			get => driver;
			set {
				Employee oldDriver = driver;
				if(SetField(ref driver, value, () => Driver)) {
					ChangeFuelDocumentsOnChangeDriver(oldDriver);
					if(Id == 0 || oldDriver != driver)
						Forwarder = GetDefaultForwarder(driver);
				}
			}
		}

		Employee forwarder;

		[Display(Name = "Экспедитор")]
		public virtual Employee Forwarder {
			get => forwarder;
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
			get => logistican;
			set => SetField(ref logistican, value, () => Logistican);
		}

		Car car;

		[Display(Name = "Машина")]
		public virtual Car Car {
			get => car;
			set {
				Car oldCar = car;
				if(SetField(ref car, value, () => Car)) {
					ChangeFuelDocumentsChangeCar(oldCar);

					if(value?.Driver != null && value?.Driver.IsFired == false)
						Driver = value.Driver;

					if(Id == 0) {
						while(ObservableGeographicGroups.Any())
							ObservableGeographicGroups.Remove(ObservableGeographicGroups.FirstOrDefault());
						foreach(var group in value.GeographicGroups)
							ObservableGeographicGroups.Add(group);
					}
				}
			}
		}

		DeliveryShift shift;

		[Display(Name = "Смена доставки")]
		public virtual DeliveryShift Shift {
			get => shift;
			set => SetField(ref shift, value, () => Shift);
		}

		DateTime date;

		[Display(Name = "Дата")]
		[HistoryDateOnly]
		public virtual DateTime Date {
			get => date;
			set => SetField(ref date, value, () => Date);
		}

		Decimal confirmedDistance;

		/// <summary>
		/// Расстояние в километрах
		/// </summary>
		[Display(Name = "Подтверждённое расстояние")]
		public virtual Decimal ConfirmedDistance {
			get => confirmedDistance;
			set => SetField(ref confirmedDistance, value, () => ConfirmedDistance);
		}

		private decimal? planedDistance;

		/// <summary>
		/// Расстояние в километрах
		/// </summary>
		[Display(Name = "Планируемое расстояние")]
		public virtual decimal? PlanedDistance {
			get => planedDistance;
			protected set => SetField(ref planedDistance, value, () => PlanedDistance);
		}

		decimal? recalculatedDistance;

		/// <summary>
		/// Расстояние в километрах.
		/// </summary>
		[Display(Name = "Пересчитанное расстояние")]
		public virtual decimal? RecalculatedDistance {
			get => recalculatedDistance;
			set => SetField(ref recalculatedDistance, value, () => RecalculatedDistance);
		}

		RouteListStatus status;

		[Display(Name = "Статус")]
		public virtual RouteListStatus Status {
			get => status;
			protected set => SetField(ref status, value, () => Status);
		}

		DateTime? closingDate;
		[Display(Name = "Дата закрытия")]
		[HistoryDateOnly]
		public virtual DateTime? ClosingDate {
			get => closingDate;
			set => SetField(ref closingDate, value, () => ClosingDate);
		}

		string closingComment;

		[Display(Name = "Комментарий")]
		public virtual string ClosingComment {
			get => closingComment;
			set => SetField(ref closingComment, value, () => ClosingComment);
		}

		Employee cashier;
		[IgnoreHistoryTrace]
		public virtual Employee Cashier {
			get => cashier;
			set => SetField(ref cashier, value, () => Cashier);
		}

		decimal fixedDriverWage;

		[Display(Name = "Фиксированная заработанная плата водителя")]
		[IgnoreHistoryTrace]
		public virtual decimal FixedDriverWage {
			get => fixedDriverWage;
			set => SetField(ref fixedDriverWage, value, () => FixedDriverWage);
		}

		decimal fixedForwarderWage;

		[Display(Name = "Фиксированная заработанная плата экспедитора")]
		[IgnoreHistoryTrace]
		public virtual decimal FixedForwarderWage {
			get => fixedForwarderWage;
			set => SetField(ref fixedForwarderWage, value, () => FixedForwarderWage);
		}

		Fine bottleFine;

		[Display(Name = "Штраф за бутыли")]
		public virtual Fine BottleFine {
			get => bottleFine;
			set => SetField(ref bottleFine, value, () => BottleFine);
		}

		private FuelOperation fuelOutlayedOperation;

		[Display(Name = "Операции расхода топлива")]
		[IgnoreHistoryTrace]
		public virtual FuelOperation FuelOutlayedOperation {
			get => fuelOutlayedOperation;
			set => SetField(ref fuelOutlayedOperation, value, () => FuelOutlayedOperation);
		}

		private bool differencesConfirmed;

		[Display(Name = "Расхождения подтверждены")]
		public virtual bool DifferencesConfirmed {
			get => differencesConfirmed;
			set => SetField(ref differencesConfirmed, value, () => DifferencesConfirmed);
		}

		private DateTime? lastCallTime;

		[Display(Name = "Время последнего созвона")]
		public virtual DateTime? LastCallTime {
			get => lastCallTime;
			set => SetField(ref lastCallTime, value, () => LastCallTime);
		}

		private bool closingFilled;

		/// <summary>
		/// Внутренее поле говорящее о том что первоначалная подготовка маршрутного листа к закрытию выполнена.
		/// Эта операция выполняется 1 раз при первом открытии диалога закрытия МЛ, тут оставляется пометка о том что операция выполнена.
		/// </summary>
		[IgnoreHistoryTrace]
		public virtual bool ClosingFilled {
			get => closingFilled;
			set => SetField(ref closingFilled, value, () => ClosingFilled);
		}

		IList<RouteListItem> addresses = new List<RouteListItem>();

		[Display(Name = "Адреса в маршрутном листе")]
		public virtual IList<RouteListItem> Addresses {
			get => addresses;
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

		IList<FuelDocument> fuelDocuments = new List<FuelDocument>();

		[Display(Name = "Документы выдачи топлива")]
		public virtual IList<FuelDocument> FuelDocuments {
			get => fuelDocuments;
			set => SetField(ref fuelDocuments, value, () => FuelDocuments);
		}

		GenericObservableList<FuelDocument> observableFuelDocuments;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<FuelDocument> ObservableFuelDocuments {
			get {
				if(observableFuelDocuments == null) {
					observableFuelDocuments = new GenericObservableList<FuelDocument>(fuelDocuments);
				}
				return observableFuelDocuments;
			}
		}

		private bool normalWage;

		/// <summary>
		/// Расчет ЗП по нормальной ставке, вне зависимости от того какой тип ЗП стоит у водителя.
		/// </summary>
		public virtual bool NormalWage {
			get => normalWage;
			set => SetField(ref normalWage, value, () => NormalWage);
		}

		private WagesMovementOperations driverWageOperation;

		[Display(Name = "Операция начисления зарплаты водителю")]
		[IgnoreHistoryTrace]
		public virtual WagesMovementOperations DriverWageOperation {
			get => driverWageOperation;
			set => SetField(ref driverWageOperation, value, () => DriverWageOperation);
		}

		private WagesMovementOperations forwarderWageOperation;

		[Display(Name = "Операция начисления зарплаты экспедитору")]
		[IgnoreHistoryTrace]
		public virtual WagesMovementOperations ForwarderWageOperation {
			get => forwarderWageOperation;
			set => SetField(ref forwarderWageOperation, value, () => ForwarderWageOperation);
		}

		private bool isManualAccounting;
		[Display(Name = "Расчёт наличных вручную?")]
		public virtual bool IsManualAccounting {
			get => isManualAccounting;
			set => SetField(ref isManualAccounting, value, () => IsManualAccounting);
		}

		private TimeSpan? onLoadTimeStart;

		[Display(Name = "На погрузку в")]
		public virtual TimeSpan? OnLoadTimeStart {
			get => onLoadTimeStart;
			set => SetField(ref onLoadTimeStart, value, () => OnLoadTimeStart);
		}

		private TimeSpan? onLoadTimeEnd;

		[Display(Name = "Закончить погрузку в")]
		public virtual TimeSpan? OnLoadTimeEnd {
			get => onLoadTimeEnd;
			set => SetField(ref onLoadTimeEnd, value, () => OnLoadTimeEnd);
		}

		private int? onLoadGate;

		[Display(Name = "Ворота на погрузку")]
		public virtual int? OnLoadGate {
			get => onLoadGate;
			set => SetField(ref onLoadGate, value, () => OnLoadGate);
		}

		private bool onLoadTimeFixed;

		[Display(Name = "Время погрузки установлено в ручную")]
		public virtual bool OnloadTimeFixed {
			get => onLoadTimeFixed;
			set => SetField(ref onLoadTimeFixed, value, () => OnloadTimeFixed);
		}

		private bool printed;

		[Display(Name = "МЛ напечатан")]
		public virtual bool Printed {
			get => printed;
			set => SetField(ref printed, value, () => Printed);
		}

		string mileageComment;

		[Display(Name = "Комментарий к километражу")]
		public virtual string MileageComment {
			get => mileageComment;
			set => SetField(ref mileageComment, value, () => MileageComment);
		}

		bool mileageCheck;

		[Display(Name = "Проверка километража")]
		public virtual bool MileageCheck {
			get => mileageCheck;
			set => SetField(ref mileageCheck, value, () => MileageCheck);
		}

		Employee closedBy;
		[Display(Name = "Закрыт сотрудником")]
		[IgnoreHistoryTrace]
		public virtual Employee ClosedBy {
			get => closedBy;
			set => SetField(ref closedBy, value, () => ClosedBy);
		}

		private Subdivision closingSubdivision;
		[Display(Name = "Сдается в подразделение")]
		public virtual Subdivision ClosingSubdivision {
			get => closingSubdivision;
			set => SetField(ref closingSubdivision, value, () => ClosingSubdivision);
		}


		IList<GeographicGroup> geographicGroups = new List<GeographicGroup>();
		[Display(Name = "Группа района")]
		public virtual IList<GeographicGroup> GeographicGroups {
			get => geographicGroups;
			set => SetField(ref geographicGroups, value, () => GeographicGroups);
		}

		GenericObservableList<GeographicGroup> observableGeographicGroups;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<GeographicGroup> ObservableGeographicGroups {
			get {
				if(observableGeographicGroups == null)
					observableGeographicGroups = new GenericObservableList<GeographicGroup>(GeographicGroups);
				return observableGeographicGroups;
			}
		}

		bool? notFullyLoaded;
		[Display(Name = "МЛ погружен не полностью")]
		public virtual bool? NotFullyLoaded {
			get => notFullyLoaded;
			set => SetField(ref notFullyLoaded, value, () => NotFullyLoaded);
		}

		#endregion

		#region readonly Свойства

		public virtual string Title => string.Format("МЛ №{0}", Id);

		public virtual decimal UniqueAddressCount => Addresses.Where(item => item.IsDelivered())
															  .Select(item => item.Order.DeliveryPoint.Id)
															  .Distinct()
															  .Count();

		public virtual bool NeedMileageCheck => Car.TypeOfUse != CarTypeOfUse.Truck && !Driver.VisitingMaster;

		public virtual decimal PhoneSum {
			get {
				if(Car.TypeOfUse == CarTypeOfUse.Truck || Driver.VisitingMaster || Driver.WageCalcType == WageCalculationType.withoutPayment)
					return 0;

				return Wages.GetDriverRates(Date).PhoneServiceCompensationRate * UniqueAddressCount;
			}
		}

		public virtual decimal Total => Addresses.Sum(x => x.TotalCash) - PhoneSum;

		public virtual decimal MoneyToReturn {
			get {
				decimal payedForFuel = FuelDocuments.Where(x => x.PayedForFuel.HasValue).Sum(x => x.PayedForFuel.Value);

				return Total - payedForFuel;
			}
		}

		public virtual decimal ByTerminalTotal {
			get {
				decimal terminalSum = 0;
				Addresses.Where((arg) => arg.Order.PaymentType == PaymentType.CourierByCard).ForEach((item) => terminalSum += item.Order.ActualTotalSum);
				return terminalSum;
			}
		}

		/// <summary>
		/// Количество полных 19л бутылей в МЛ для клиентов
		/// </summary>
		/// <returns>Количество полных 19л бутылей</returns>
		public virtual int TotalFullBottlesToClient => Addresses.Sum(a => a.GetFullBottlesToDeliverCount());

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

		/// <summary>
		/// Возврат экспедитора по умолчанию для водителя <paramref name="driver"/>
		/// </summary>
		/// <returns>Экспедитор по умолчание если не уволен</returns>
		/// <param name="driver">Водитель</param>
		Employee GetDefaultForwarder(Employee driver)
		{
			if(driver?.DefaultForwarder?.IsFired == false)
				return driver.DefaultForwarder;
			//если больше не с нами,то не нужно его держать умолчальным в водителе
			if(driver?.DefaultForwarder != null)
				driver.DefaultForwarder = null;
			return null;
		}

		public virtual void ChangeFuelDocumentsChangeCar(Car oldCar)
		{
			if(oldCar == null || Car == oldCar || !FuelDocuments.Any()) {
				return;
			}

			foreach(FuelDocument item in ObservableFuelDocuments) {
				item.Car = Car;
				item.Operation.Car = Car;
			}
		}

		public virtual void ChangeFuelDocumentsOnChangeDriver(Employee oldDriver)
		{
			if(oldDriver == null || Driver == oldDriver || !FuelDocuments.Any()) {
				return;
			}

			foreach(FuelDocument item in ObservableFuelDocuments) {
				item.Driver = Driver;
				item.Operation.Driver = Driver;
			}
		}

		public virtual bool FuelOperationHaveDiscrepancy()
		{
			if(FuelOutlayedOperation == null) {
				return false;
			}
			var carDiff = FuelDocuments.Select(x => x.Operation).Any(x => x.Car != null && x.Car.Id != Car.Id)
									   || (FuelOutlayedOperation.Car != null && FuelOutlayedOperation.Car.Id != Car.Id);
			var driverDiff = FuelDocuments.Select(x => x.Operation).Any(x => x.Driver != null && x.Driver.Id != Driver.Id)
										  || (FuelOutlayedOperation.Driver != null && FuelOutlayedOperation.Driver.Id != Driver.Id);
			return carDiff || driverDiff;
		}

		public virtual RouteListItem AddAddressFromOrder(Order order)
		{
			if(order.DeliveryPoint == null)
				throw new NullReferenceException("В маршрутный нельзя добавить заказ без точки доставки.");
			var item = new RouteListItem(this, order, RouteListItemStatus.EnRoute) {
				WithForwarder = Forwarder != null
			};
			ObservableAddresses.Add(item);
			return item;
		}

		public virtual void RemoveAddress(RouteListItem address)
		{
			address.RemovedFromRoute();
			UoW.Delete(address);
			ObservableAddresses.Remove(address);
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

		public virtual void CheckFuelDocumentOrder()
		{
			for(int i = 0; i < FuelDocuments.Count; i++) {
				if(FuelDocuments[i] == null) {
					FuelDocuments.RemoveAt(i);
					i--;
					continue;
				}

				if(FuelDocuments[i].RouteList.Id != i)
					FuelDocuments[i].RouteList.Id = i;
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
			var goods = Repository.Logistics.RouteListRepository.GetGoodsAndEquipsInRL(uow, this);

			bool closed = true;
			foreach(var good in goods) {
				var loaded = inLoaded.FirstOrDefault(x => x.NomenclatureId == good.NomenclatureId);
				if(loaded == null || loaded.Amount < good.Amount) {
					closed = false;
					break;
				}
			}

			if(closed) {
				if(NotFullyLoaded.HasValue)
					NotFullyLoaded = false;
				if(new[] { RouteListStatus.Confirmed, RouteListStatus.InLoading }.Contains(Status))
					ChangeStatus(RouteListStatus.EnRoute);
			}

			return closed;
		}


		public virtual List<Discrepancy> GetDiscrepancies(IList<RouteListControlNotLoadedNode> itemsLoaded, List<RouteListRepository.ReturnsNode> allReturnsToWarehouse)
		{
			List<Discrepancy> result = new List<Discrepancy>();

			//ТОВАРЫ
			var orderClosingItems = Addresses.Where(item => item.TransferedTo == null || item.TransferedTo.NeedToReload)
										 .SelectMany(item => item.Order.OrderItems)
										 .Where(item => Nomenclature.GetCategoriesForShipment().Contains(item.Nomenclature.Category))
										 .Where(item => item.Nomenclature.Category != NomenclatureCategory.bottle)
										 .ToList();

			foreach(var orderItem in orderClosingItems) {
				var discrepancy = new Discrepancy {
					Nomenclature = orderItem.Nomenclature,
					ClientRejected = orderItem.ReturnedCount,
					Name = orderItem.Nomenclature.Name
				};
				AddDiscrepancy(result, discrepancy);
			}

			//ОБОРУДОВАНИЕ
			var orderEquipments = Addresses.Where(item => item.TransferedTo == null || item.TransferedTo.NeedToReload)
									   .SelectMany(item => item.Order.OrderEquipments)
									   .Where(item => Nomenclature.GetCategoriesForShipment().Contains(item.Nomenclature.Category))
									   .ToList();
			foreach(var orderEquip in orderEquipments) {
				var discrepancy = new Discrepancy {
					Nomenclature = orderEquip.Nomenclature,
					Name = orderEquip.Nomenclature.Name
				};

				if(orderEquip.Direction == Domain.Orders.Direction.Deliver)
					discrepancy.ClientRejected = orderEquip.ReturnedCount;
				else
					discrepancy.PickedUpFromClient = orderEquip.ActualCount ?? 0;

				AddDiscrepancy(result, discrepancy);
			}

			//ДОСТАВЛЕНО НА СКЛАД
			var warehouseItems = allReturnsToWarehouse.Where(x => x.NomenclatureCategory != NomenclatureCategory.bottle)
													  .ToList();
			foreach(var whItem in warehouseItems) {
				var discrepancy = new Discrepancy {
					Nomenclature = whItem.Nomenclature,
					ToWarehouse = whItem.Amount,
					Name = whItem.Name
				};
				AddDiscrepancy(result, discrepancy);
			}

			if(itemsLoaded != null && itemsLoaded.Any()) {
				var loadedItems = itemsLoaded.Where(x => x.Nomenclature.Category != NomenclatureCategory.bottle);
				foreach(var item in loadedItems) {
					var discrepancy = new Discrepancy {
						Nomenclature = item.Nomenclature,
						FromWarehouse = item.CountNotLoaded,
						Name = item.Nomenclature.Name
					};

					AddDiscrepancy(result, discrepancy);
				}
			}

			return result;
		}

		/// <summary>
		/// Добавляет новое расхождение если такой номенклатуры нет в списке, 
		/// иначе прибавляет все значения к найденной в списке номенклатуре
		/// </summary>
		void AddDiscrepancy(List<Discrepancy> list, Discrepancy item)
		{
			var existingDiscrepancy = list.FirstOrDefault(x => x.Nomenclature == item.Nomenclature);
			if(existingDiscrepancy == null) {
				list.Add(item);
			} else {
				existingDiscrepancy.ClientRejected += item.ClientRejected;
				existingDiscrepancy.PickedUpFromClient += item.PickedUpFromClient;
				existingDiscrepancy.ToWarehouse += item.ToWarehouse;
				existingDiscrepancy.FromWarehouse += item.FromWarehouse;
			}
		}

		public virtual bool IsConsistentWithUnloadDocument()
		{
			var returnedBottlesNom = int.Parse(MainSupport.BaseParameters.All["returned_bottle_nomenclature_id"]);
			var bottlesReturnedToWarehouse = (int)RouteListRepository.GetReturnsToWarehouse(
				UoW,
				Id,
				returnedBottlesNom)
			.Sum(item => item.Amount);

			var notloadedNomenclatures = NotLoadedNomenclatures();
			var allReturnsToWarehouse = RouteListRepository.GetReturnsToWarehouse(UoW, Id, Nomenclature.GetCategoriesForShipment());
			var discrepancies = GetDiscrepancies(notloadedNomenclatures, allReturnsToWarehouse);

			var hasItemsDiscrepancies = discrepancies.Any(discrepancy => discrepancy.Remainder != 0);
			bool hasFine = BottleFine != null;
			var items = Addresses.Where(item => item.IsDelivered());
			int bottlesReturnedTotal = items.Sum(item => item.BottlesReturned);
			var hasTotalBottlesDiscrepancy = bottlesReturnedToWarehouse != bottlesReturnedTotal;
			return hasFine || (!hasTotalBottlesDiscrepancy && !hasItemsDiscrepancies) || DifferencesConfirmed;
		}

		public virtual void ChangeStatus(RouteListStatus newStatus)
		{
			if(newStatus == Status)
				return;

			string exceptionMessage = $"Некорректная операция. Не предусмотрена смена статуса с {Status} на {newStatus}";

			switch(newStatus) {
				case RouteListStatus.New:
					if(Status == RouteListStatus.Confirmed || Status == RouteListStatus.InLoading) {
						Status = RouteListStatus.New;
					} else {
						throw new InvalidOperationException(exceptionMessage);
					}
					break;
				case RouteListStatus.Confirmed:
					if(Status == RouteListStatus.New || Status == RouteListStatus.InLoading) {
						Status = RouteListStatus.Confirmed;
						foreach(var address in Addresses) {
							if(address.Order.OrderStatus < OrderStatus.OnLoading)
								address.Order.ChangeStatus(OrderStatus.OnLoading);
						}
					} else {
						throw new InvalidOperationException(exceptionMessage);
					}
					break;
				case RouteListStatus.InLoading:
					if(Status == RouteListStatus.EnRoute) {
						Status = RouteListStatus.InLoading;
						foreach(var item in Addresses) {
							if(item.Order.OrderStatus != OrderStatus.OnLoading) {
								item.Order.ChangeStatus(OrderStatus.OnLoading);
							}
						}
					} else if(Status == RouteListStatus.Confirmed) {
						Status = RouteListStatus.InLoading;
					} else {
						throw new InvalidOperationException(exceptionMessage);
					}
					break;
				case RouteListStatus.EnRoute:
					if(Status == RouteListStatus.InLoading || Status == RouteListStatus.Confirmed) {
						Status = RouteListStatus.EnRoute;
						foreach(var item in Addresses) {
							item.Order.OrderStatus = OrderStatus.OnTheWay;
						}
					} else {
						throw new InvalidOperationException(exceptionMessage);
					}
					break;
				case RouteListStatus.OnClosing:
					if(
					(Status == RouteListStatus.EnRoute && (Car.TypeOfUse == CarTypeOfUse.Truck || Driver.VisitingMaster))
					|| (Status == RouteListStatus.Confirmed && (Car.TypeOfUse == CarTypeOfUse.Truck))
					|| Status == RouteListStatus.MileageCheck
					|| Status == RouteListStatus.Closed) {
						Status = newStatus;
						foreach(var item in Addresses.Where(x => x.Status == RouteListItemStatus.Completed || x.Status == RouteListItemStatus.EnRoute)) {
							item.Order.ChangeStatus(OrderStatus.UnloadingOnStock);
						}
					} else {
						throw new InvalidOperationException(exceptionMessage);
					}
					break;
				case RouteListStatus.MileageCheck:
					if(Status == RouteListStatus.EnRoute || Status == RouteListStatus.OnClosing) {
						Status = newStatus;
						foreach(var item in Addresses.Where(x => x.Status == RouteListItemStatus.Completed || x.Status == RouteListItemStatus.EnRoute)) {
							item.Order.ChangeStatus(OrderStatus.UnloadingOnStock);
						}
					} else {
						throw new InvalidOperationException(exceptionMessage);
					}
					break;
				case RouteListStatus.Closed:
					if(Status == RouteListStatus.OnClosing || Status == RouteListStatus.MileageCheck) {
						Status = newStatus;
						CloseAddresses();
					} else {
						throw new InvalidOperationException(exceptionMessage);
					}
					break;
				default:
					throw new NotImplementedException($"Не реализовано изменение статуса для {newStatus}");
			}

			UpdateClosedInformation();
		}

		private void UpdateClosedInformation()
		{
			if(Status == RouteListStatus.Closed) {
				ClosedBy = EmployeeRepository.GetEmployeeForCurrentUser(UoW);
				ClosingDate = DateTime.Now;
			} else {
				ClosedBy = null;
				ClosingDate = null;
			}
		}

		private void CloseAddresses()
		{
			if(Status != RouteListStatus.Closed) {
				return;
			}

			foreach(var address in Addresses) {
				if(address.Status == RouteListItemStatus.Completed || address.Status == RouteListItemStatus.EnRoute) {
					if(address.Status == RouteListItemStatus.EnRoute) {
						address.UpdateStatus(UoW, RouteListItemStatus.Completed);
					}
					address.Order.ChangeStatus(OrderStatus.Closed);
				}

				if(address.Status == RouteListItemStatus.Canceled) {
					address.Order.ChangeStatus(OrderStatus.DeliveryCanceled);
				}

				if(address.Status == RouteListItemStatus.Overdue) {
					address.Order.ChangeStatus(OrderStatus.NotDelivered);
				}
			}
		}

		public virtual void RecalculateAllWages()
		{
			Addresses.ToList().ForEach(x => x.RecalculateWages());
		}

		#endregion

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(validationContext.Items.ContainsKey("NewStatus")) {
				RouteListStatus newStatus = (RouteListStatus)validationContext.Items["NewStatus"];
				switch(newStatus) {
					case RouteListStatus.New:
					case RouteListStatus.Confirmed:
					case RouteListStatus.InLoading:
					case RouteListStatus.Closed: break;
					case RouteListStatus.MileageCheck:
						foreach(var address in Addresses) {
							var valid = new QSValidator<Order>(
									address.Order,
									new Dictionary<object, object> {
										{ "NewStatus", OrderStatus.Closed }
									}
								);

							foreach(var result in valid.Results)
								yield return result;
						}
						break;
					case RouteListStatus.EnRoute: break;
					case RouteListStatus.OnClosing: break;
				}
			}

			if(!GeographicGroups.Any())
				yield return new ValidationResult(
						"Необходимо указать район",
						new[] { Gamma.Utilities.PropertyUtil.GetPropertyName(this, o => o.GeographicGroups) }
					);

			if(ClosingSubdivision == null)
				yield return new ValidationResult("Не выбрана касса в которую должен будет сдаваться водитель.",
					new[] { Gamma.Utilities.PropertyUtil.GetPropertyName(this, o => o.ClosingSubdivision) });

			if(Driver == null)
				yield return new ValidationResult("Не заполнен водитель.",
					new[] { Gamma.Utilities.PropertyUtil.GetPropertyName(this, o => o.Driver) });

			if(Car == null)
				yield return new ValidationResult("На заполнен автомобиль.",
					new[] { Gamma.Utilities.PropertyUtil.GetPropertyName(this, o => o.Car) });
		}

		#endregion

		#region Функции относящиеся к закрытию МЛ

		public virtual void CompleteRoute()
		{
			if(Car.TypeOfUse == CarTypeOfUse.Truck || Driver.VisitingMaster) {
				ChangeStatus(RouteListStatus.OnClosing);
			} else {
				ChangeStatus(RouteListStatus.MileageCheck);
			}

			var track = Repository.Logistics.TrackRepository.GetTrackForRouteList(UoW, Id);
			if(track != null) {
				track.CalculateDistance();
				track.CalculateDistanceToBase();
				UoW.Save(track);
			}
			//FirstFillClosing();
			UoW.Save(this);
		}

		/// <summary>
		/// Возвращает пересчитанную заного зарплату водителя (не записывает)
		/// </summary>
		public virtual decimal GetRecalculatedDriverWage()
		{
			if(Driver.WageCalcType == WageCalculationType.fixedDay || Driver.WageCalcType == WageCalculationType.fixedRoute) {
				return FixedDriverWage;
			}
			decimal result = 0m;
			foreach(var address in Addresses) {
				result += address.CalculateDriverWage() + address.DriverWageSurcharge;
			}
			return result;
		}

		/// <summary>
		/// Возвращает пересчитанную заного зарплату экспедитора (не записывает)
		/// </summary>
		public virtual decimal GetRecalculatedForwarderWage()
		{
			if(Forwarder == null) {
				return 0;
			}
			if(Forwarder.WageCalcType == WageCalculationType.fixedDay || Forwarder.WageCalcType == WageCalculationType.fixedRoute) {
				return FixedForwarderWage;
			}
			decimal result = 0m;
			foreach(var address in Addresses) {
				result += address.CalculateForwarderWage() + address.ForwarderWageSurcharge;
			}
			return result;
		}

		/// <summary>
		/// Возвращает текущую зарплату водителя
		/// </summary>
		public virtual decimal GetDriversTotalWage()
		{
			if(Driver.WageCalcType == WageCalculationType.fixedDay
			  || Driver.WageCalcType == WageCalculationType.fixedRoute) {
				//Если все заказы не выполнены, то нет зарплаты
				if(ObservableAddresses.Any(x => x.Status == RouteListItemStatus.Completed)) {
					return FixedDriverWage;
				} else {
					return 0m;
				}
			}
			return Addresses.Sum(item => item.DriverWage) + Addresses.Sum(item => item.DriverWageSurcharge);
		}

		/// <summary>
		/// Возвращает текущую зарплату экспедитора
		/// </summary>
		public virtual decimal GetForwardersTotalWage()
		{
			if(Forwarder == null) {
				return 0;
			}
			if(Forwarder.WageCalcType == WageCalculationType.fixedDay
			  || Forwarder.WageCalcType == WageCalculationType.fixedRoute) {
				//Если все заказы не выполнены, то нет зарплаты
				if(ObservableAddresses.Any(x => x.Status == RouteListItemStatus.Completed)) {
					return FixedForwarderWage;
				} else {
					return 0m;
				}
			}
			return Addresses.Sum(item => item.ForwarderWage);
		}

		/// <summary>
		/// Расчитывает и записывает зарплату
		/// </summary>
		public virtual void CalculateWages()
		{
			if(Driver.WageCalcType == WageCalculationType.fixedDay || Driver.WageCalcType == WageCalculationType.fixedRoute)
				FixedDriverWage = GetDriverFixedWage();

			if(Forwarder != null && (Forwarder.WageCalcType == WageCalculationType.fixedDay || Forwarder.WageCalcType == WageCalculationType.fixedRoute))
				FixedForwarderWage = Forwarder.WageCalcRate;

			Addresses.ToList().ForEach(x => x.RecalculateWages());
		}

		//FIXME
		/// <summary>
		/// Костыльный метод для расчёта ЗП водилы фуры при заборе воды с Семиозерья или Вартемяг.
		/// Удалить после задачи I-1626.
		/// </summary>
		/// <returns>The driver fixed wage.</returns>
		decimal GetDriverFixedWage()
		{
			var address = Addresses.FirstOrDefault();
			if(Driver.DriverOf.HasValue && Driver.DriverOf.Value == CarTypeOfUse.Truck && address != null) {
				var truckRates = "ставки_водителя_фуры_id=rate=dateStart";
				if(!MainSupport.BaseParameters.All.ContainsKey(truckRates))
					throw new InvalidProgramException($"В параметрах базы не определены ставки для водителей фур [{truckRates}]");
				var rates = MainSupport.BaseParameters.All[truckRates].Trim(';').Split(';');//парсим строку из параметров

				//создаём лист и заполняем его. можно было бы не делать так, если бы не нужна была сортировка по дате
				List<object[]> ratesList = new List<object[]>();
				foreach(var r in rates) {
					var rate = r.Split('=');
					ratesList.Add(new[] { int.Parse(rate[0]), decimal.Parse(rate[1]), (object)DateTime.Parse(rate[2]) });
				}
				var sortedRatesList = ratesList.OrderByDescending<object[], object>(x => x[2]).ToList();

				foreach(var r in sortedRatesList) {
					if(address.Order.DeliveryPoint.Id == (int)r[0]) {
						if(ClosingDate.HasValue && ClosingDate.Value >= (DateTime)r[2] || !ClosingDate.HasValue && Date >= (DateTime)r[2])
							return (decimal)r[1];
					}
				}
			}
			return Driver.WageCalcRate;
		}

		//FIXME потом метод скрыть. Должен вызываться только при переходе в статус на закрытии.
		public virtual void FirstFillClosing()
		{
			PerformanceHelper.StartMeasurement("Первоначальное заполнение");
			var addresesDelivered = Addresses.Where(x => x.Status != RouteListItemStatus.Transfered).ToList();
			foreach(var routeListItem in addresesDelivered) {
				PerformanceHelper.StartPointsGroup($"Заказ {routeListItem.Order.Id}");

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
			var addresesDelivered = Addresses.Where(x => x.Status != RouteListItemStatus.Transfered && x.Status != RouteListItemStatus.Canceled).ToList();
			foreach(RouteListItem address in addresesDelivered) {
				int amountDelivered = address.Order.OrderItems
												   .Where(item => (item.Nomenclature.Category == NomenclatureCategory.water) && !item.Nomenclature.IsDisposableTare)
												   .Sum(item => item.ActualCount ?? 0);
				var bottlesMovementOperation = address.Order.BottlesMovementOperation;
				if(amountDelivered != 0 || address.BottlesReturned != 0) {
					if(bottlesMovementOperation == null) {
						bottlesMovementOperation = new BottlesMovementOperation {
							Order = address.Order,
							Counterparty = address.Order.Client,
							DeliveryPoint = address.Order.DeliveryPoint
						};
					}
					bottlesMovementOperation.OperationTime = address.Order.DeliveryDate.Value.Date.AddHours(23).AddMinutes(59);
					bottlesMovementOperation.Delivered = amountDelivered;
					bottlesMovementOperation.Returned = address.BottlesReturned;
					if(MainSupport.BaseParameters.All.ContainsKey("forfeit_nomenclature_id")) 
					{
						if(int.TryParse(MainSupport.BaseParameters.All["forfeit_nomenclature_id"], out int forfeitId)) 
						{
							bottlesMovementOperation.Returned += address.Order.OrderItems.Where(arg => arg.Nomenclature.Id == forfeitId && arg.ActualCount != null)
																			   .Select(arg => arg.ActualCount.Value).Sum();
						}
					}
					address.Order.BottlesMovementOperation = bottlesMovementOperation;
					result.Add(bottlesMovementOperation);

				} else if(bottlesMovementOperation != null) {
					UoW.Delete(bottlesMovementOperation);
					address.Order.BottlesMovementOperation = null;
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
				.Where(item => !item.Nomenclature.IsSerial)) {
				var operation = orderItem.UpdateCounterpartyOperation(UoW);
				if(operation != null)
					result.Add(operation);
			}

			foreach(var orderEquipment in addresesDelivered.SelectMany(item => item.Order.OrderEquipments)
					.Where(item => Nomenclature.GetCategoriesForShipment().Contains(item.Nomenclature.Category))
				   ) {
				var operation = orderEquipment.UpdateCounterpartyOperation();
				if(operation != null)
					result.Add(operation);
			}
			return result;
		}

		public virtual List<DepositOperation> UpdateDepositOperations(IUnitOfWork UoW)
		{
			var result = new List<DepositOperation>();
			var addresesDelivered = Addresses.Where(x => x.Status != RouteListItemStatus.Transfered).ToList();
			foreach(RouteListItem item in addresesDelivered) {

				//Возврат залогов
				var bottleRefundDeposit = Math.Abs(item.BottleDepositsCollected);
				var equipmentRefundDeposit = Math.Abs(item.EquipmentDepositsCollected);

				var operations = item.Order.UpdateDepositOperations(UoW, equipmentRefundDeposit, bottleRefundDeposit);

				operations.ForEach(x => result.Add(x));
			}
			return result; 
		}

		public virtual List<MoneyMovementOperation> UpdateMoneyMovementOperations()
		{
			var result = new List<MoneyMovementOperation>();
			var addresesDelivered = Addresses.Where(x => x.Status != RouteListItemStatus.Transfered).ToList();
			foreach(var address in addresesDelivered) {
				var order = address.Order;
				var depositsTotal = order.OrderDepositItems.Sum(dep => dep.ActualCount ?? 0 * dep.Deposit);
				decimal? money = null;
				if(address.TotalCash != 0)
					money = address.TotalCash;
				MoneyMovementOperation moneyMovementOperation = order.MoneyMovementOperation;
				if(moneyMovementOperation == null) {
					moneyMovementOperation = new MoneyMovementOperation {
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

		public virtual string[] ManualCashOperations(ref Income cashIncome, ref Expense cashExpense, decimal casheInput)
		{
			var messages = new List<string>();
			if(casheInput > 0) {
				cashIncome = new Income {
					IncomeCategory = CategoryRepository.RouteListClosingIncomeCategory(UoW),
					TypeOperation = IncomeType.DriverReport,
					Date = DateTime.Now,
					Casher = this.Cashier,
					Employee = Driver,
					Description = $"Дополнение к МЛ №{this.Id} от {Date:d}",
					Money = Math.Round(casheInput, 0, MidpointRounding.AwayFromZero),
					RouteListClosing = this,
					RelatedToSubdivision = ClosingSubdivision
				};

				messages.Add($"Создан приходный ордер на сумму {cashIncome.Money:C0}");

			} else {
				cashExpense = new Expense {
					ExpenseCategory = CategoryRepository.RouteListClosingExpenseCategory(UoW),
					TypeOperation = ExpenseType.Expense,
					Date = DateTime.Now,
					Casher = this.Cashier,
					Employee = Driver,
					Description = $"Дополнение к МЛ #{this.Id} от {Date:d}",
					Money = Math.Round(-casheInput, 0, MidpointRounding.AwayFromZero),
					RouteListClosing = this,
					RelatedToSubdivision = ClosingSubdivision
				};
				messages.Add($"Создан расходный ордер на сумму {cashExpense.Money:C0}");
			}
			IsManualAccounting = true;
			return messages.ToArray();
		}

		public virtual string EmployeeAdvanceOperation(ref Expense cashExpense, decimal cashInput)
		{
			string message;

			cashExpense = new Expense {
				ExpenseCategory = CategoryRepository.EmployeeSalaryExpenseCategory(UoW),
				TypeOperation = ExpenseType.EmployeeAdvance,
				Date = DateTime.Now,
				Casher = this.Cashier,
				Employee = Driver,
				Description = $"Выдача аванса к МЛ #{this.Id} от {Date:d}",
				Money = Math.Round(cashInput, 0, MidpointRounding.AwayFromZero),
				RouteListClosing = this,
				RelatedToSubdivision = ClosingSubdivision
			};

			message = $"Создан расходный ордер на сумму {cashExpense.Money:C0}";
			return (message);
		}

		private void ConfirmAndClose()
		{
			if(Status != RouteListStatus.OnClosing && Status != RouteListStatus.MileageCheck) {
				throw new InvalidOperationException(String.Format("Закрыть маршрутный лист можно только если он находится в статусе {0} или  {1}", RouteListStatus.OnClosing, RouteListStatus.MileageCheck));
			}

			if(Driver != null && Driver.FirstWorkDay == null) {
				Driver.FirstWorkDay = date;
				UoW.Save(Driver);
			}

			if(Forwarder != null && Forwarder.FirstWorkDay == null) {
				Forwarder.FirstWorkDay = date;
				UoW.Save(Forwarder);
			}

			switch(Status) {
				case RouteListStatus.OnClosing:
					CloseFromOnClosing();
					break;
				case RouteListStatus.MileageCheck:
					CloseFromOnMileageCheck();
					break;
			}
		}

		/// <summary>
		/// Закрывает МЛ, либо переводит в сдается, при необходимых условиях, из статуса "Проверка километража" 
		/// </summary>
		private void CloseFromOnMileageCheck()
		{
			if(Status != RouteListStatus.MileageCheck) {
				return;
			}

			decimal cash = CashRepository.CurrentRouteListCash(UoW, this.Id);
			if(NeedMileageCheck && ConfirmedDistance > 0 && cash == Total && (!IsConsistentWithUnloadDocument() && !DifferencesConfirmed)) {
				ChangeStatus(RouteListStatus.Closed);
			} else {
				ChangeStatus(RouteListStatus.OnClosing);

			}
		}

		/// <summary>
		/// Закрывает МЛ, либо переводит в проверку км, при необходимых условиях, из статуса "Сдается" 
		/// </summary>
		private void CloseFromOnClosing()
		{
			if(Status != RouteListStatus.OnClosing) {
				return;
			}

			if(!NeedMileageCheck || (NeedMileageCheck && ConfirmedDistance > 0)) {
				ChangeStatus(RouteListStatus.Closed);
				return;
			}

			if(NeedMileageCheck && ConfirmedDistance <= 0) {
				ChangeStatus(RouteListStatus.MileageCheck);
				return;
			}
		}

		public virtual void AcceptCash()
		{
			if(Status != RouteListStatus.OnClosing) {
				return;
			}

			if(cashier == null) {
				throw new InvalidOperationException(String.Format("Должен быть заполнен кассир"));
			}

			if(!PermissionRepository.HasAccessToClosingRoutelist()) {
				return;
			}

			ConfirmAndClose();
		}

		public virtual void AcceptMileage()
		{
			if(Status != RouteListStatus.MileageCheck) {
				return;
			}

			RecalculateFuelOutlay();
			ConfirmAndClose();
		}

		public virtual void UpdateFuelOperation()
		{
			//Необходимо для того что бы случайно не пересчитать операцию расхода топлива. После массовой смены расхода.
			if(FuelOutlayedOperation != null && Date < new DateTime(2017, 6, 6)) {
				return;
			}

			foreach(var item in FuelDocuments) {
				item.UpdateDocument(UoW);
			}

			if(ConfirmedDistance == 0) {
				if(FuelOutlayedOperation != null) {
					UoW.Delete(FuelOutlayedOperation);
					FuelOutlayedOperation = null;
				}
			} else {
				if(FuelOutlayedOperation == null) {
					FuelOutlayedOperation = new FuelOperation();
				}
				decimal litresOutlayed = (decimal)Car.FuelConsumption / 100 * ConfirmedDistance;

				FuelOutlayedOperation.Driver = Car.IsCompanyHavings ? null : Driver; ;
				FuelOutlayedOperation.Car = Car.IsCompanyHavings ? Car : null;
				FuelOutlayedOperation.Fuel = Car.FuelType;
				FuelOutlayedOperation.OperationTime = Date;
				FuelOutlayedOperation.LitersOutlayed = litresOutlayed;
			}
		}

		public virtual void RecalculateFuelOutlay()
		{
			if(this.ConfirmedDistance == 0)
				return;

			if(FuelOutlayedOperation == null) {
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
			decimal driverWage = GetDriversTotalWage();

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

			decimal forwarderWage = GetForwardersTotalWage();

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
			date = DateTime.Today;
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

			return reportInfo;
		}

		public virtual IEnumerable<string> UpdateCashOperations()
		{
			var messages = new List<string>();
			//Закрываем наличку.
			Income cashIncome = null;
			Expense cashExpense = null;

			var currentRouteListCash = CashRepository.CurrentRouteListCash(UoW, this.Id);
			var different = Total - currentRouteListCash;
			if(different == 0M) {
				return messages.ToArray();
			}
			if(different > 0) {
				cashIncome = new Income {
					IncomeCategory = CategoryRepository.RouteListClosingIncomeCategory(UoW),
					TypeOperation = IncomeType.DriverReport,
					Date = DateTime.Now,
					Casher = this.Cashier,
					Employee = Driver,
					Description = $"Закрытие МЛ №{Id} от {Date:d}",
					Money = Math.Round(different, 2, MidpointRounding.AwayFromZero),
					RouteListClosing = this,
					RelatedToSubdivision = ClosingSubdivision
				};
				messages.Add($"Создан приходный ордер на сумму {cashIncome.Money}");
			} else {
				cashExpense = new Expense {
					ExpenseCategory = CategoryRepository.RouteListClosingExpenseCategory(UoW),
					TypeOperation = ExpenseType.Expense,
					Date = DateTime.Now,
					Casher = this.Cashier,
					Employee = Driver,
					Description = $"Закрытие МЛ #{Id} от {Date:d}",
					Money = Math.Round(-different, 2, MidpointRounding.AwayFromZero),
					RouteListClosing = this,
					RelatedToSubdivision = ClosingSubdivision
				};
				messages.Add($"Создан расходный ордер на сумму {cashExpense.Money}");
			}

			if(cashIncome != null) UoW.Save(cashIncome);
			if(cashExpense != null) UoW.Save(cashExpense);

			return messages;
		}

		public virtual IEnumerable<string> UpdateMovementOperations()
		{
			var result = UpdateCashOperations();
			UpdateOperations();
			return result;
		}

		public virtual void UpdateOperations()
		{
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
		}

		#region Для логистических расчетов

		public virtual TimeSpan? FirstAddressTime {
			get {
				return Addresses.FirstOrDefault()?.Order.DeliverySchedule.From;
			}
		}

		public virtual void RecalculatePlanTime(RouteGeometryCalculator sputnikCache)
		{
			TimeSpan minTime = new TimeSpan(); ;
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

				minTime += TimeSpan.FromSeconds(Addresses[ix].TimeOnPoint);
			}
			//Расчет максимального времени до которого нужно подъехать.
			TimeSpan maxTime = new TimeSpan();
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

				maxTime -= TimeSpan.FromSeconds(Addresses[ix].TimeOnPoint);

				if(maxTime < Addresses[ix].PlanTimeStart) { //Расписание испорчено, успеть нельзя. Пытаемся его более менее адекватно отобразить.
					TimeSpan beforeMin = new TimeSpan(1, 0, 0, 0);
					if(ix > 0)
						beforeMin = Addresses[ix - 1].PlanTimeStart.Value
													 + TimeSpan.FromSeconds(sputnikCache.TimeSec(Addresses[ix - 1].Order.DeliveryPoint, Addresses[ix].Order.DeliveryPoint))
													 + TimeSpan.FromSeconds(Addresses[ix - 1].TimeOnPoint);
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
									   x.FirstAddressTime.Value - TimeSpan.FromSeconds(sputnikCache.TimeFromBase(x.Addresses.First().Order.DeliveryPoint)),
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
			result.AddRange(Addresses.Where(x => x.Order.DeliveryPoint.CoordinatesExist).Select(x => CachedDistance.GetHash(x.Order.DeliveryPoint)));
			result.Add(CachedDistance.BaseHash);
			return result.ToArray();
		}

		/// <summary>
		/// Полный вес товаров и оборудования в маршрутном листе
		/// </summary>
		/// <returns>Вес в килограммах</returns>
		public virtual double GetTotalWeight() => Addresses.Where(item => item.Status != RouteListItemStatus.Transfered)
														   .Sum(item => item.Order.GetWeight());
		/// <summary>
		/// Проверка на перегруз автомобиля
		/// </summary>
		/// <returns><c>true</c>, если автомобиль "Ларгус" или "раскат" и имеется его перегруз, <c>false</c> в остальных случаях.</returns>
		public virtual bool HasOverweight() => Car != null && (Car.IsRaskat || Car.TypeOfUse == CarTypeOfUse.Largus) && Car.MaxWeight < GetTotalWeight();

		/// <summary>
		/// Перегруз в килограммах
		/// </summary>
		/// <returns>Возрат значения перегруза в килограммах.</returns>
		public virtual double Overweight() => HasOverweight() ? Math.Round(GetTotalWeight() - Car.MaxWeight, 2) : 0;

		/// <summary>
		/// Нода с номенклатурами и различными количествами после погрузки МЛ на складе
		/// </summary>
		public virtual List<RouteListControlNotLoadedNode> NotLoadedNomenclatures()
		{
			List<RouteListControlNotLoadedNode> notLoadedNomenclatures = new List<RouteListControlNotLoadedNode>();
			if(Id > 0) {
				var loadedNomenclatures = RouteListRepository.AllGoodsLoaded(UoW, this);
				var nomenclaturesToLoad = RouteListRepository.GetGoodsAndEquipsInRL(UoW, this);
				foreach(var n in nomenclaturesToLoad) {
					var loaded = loadedNomenclatures.FirstOrDefault(x => x.NomenclatureId == n.NomenclatureId);
					decimal loadedAmount = 0;
					if(loaded != null)
						loadedAmount = loaded.Amount;
					if(loadedAmount < n.Amount) {
						notLoadedNomenclatures.Add(new RouteListControlNotLoadedNode {
							NomenclatureId = n.NomenclatureId,
							CountTotal = n.Amount,
							CountNotLoaded = (int)(n.Amount - loadedAmount)
						});
					}
				}
				DomainHelper.FillPropertyByEntity<RouteListControlNotLoadedNode, Nomenclature>(
								UoW,
								notLoadedNomenclatures,
								x => x.NomenclatureId,
								(node, obj) => node.Nomenclature = obj
							);
			}
			return notLoadedNomenclatures;
		}

		#endregion
	}

	public enum RouteListStatus
	{
		[Display(Name = "Новый")]
		New,
		[Display(Name = "Подтвержден")]
		Confirmed,
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
		public RouteListStatusStringType() : base(typeof(RouteListStatus)) { }
	}

	public class RouteListControlNotLoadedNode
	{
		public int NomenclatureId { get; set; }
		public Nomenclature Nomenclature { get; set; }
		public int CountNotLoaded { get; set; }
		public int CountTotal { get; set; }
		public int CountLoaded => CountTotal - CountNotLoaded;
		public string CountLoadedString => string.Format("<span foreground=\"{0}\">{1}</span>", CountLoaded > 0 ? "Orange" : "Red", CountLoaded);
	}
}
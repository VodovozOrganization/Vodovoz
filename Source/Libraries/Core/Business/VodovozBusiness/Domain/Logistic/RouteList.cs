using Autofac;
using Gamma.Utilities;
using NHibernate.Criterion;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Osrm;
using QS.Project.Services;
using QS.Report;
using QS.Utilities.Debug;
using QS.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Controllers;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Logistics;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Documents.DriverTerminal;
using Vodovoz.Domain.Documents.DriverTerminalTransfer;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Logistic.FastDelivery;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Profitability;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.EntityRepositories.Permissions;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Factories;
using Vodovoz.Models;
using Vodovoz.Repository.Store;
using Vodovoz.Services;
using Vodovoz.Services.Logistics;
using Vodovoz.Settings.Cash;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Delivery;
using Vodovoz.Settings.Logistics;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Settings.Orders;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;
using Vodovoz.Tools.Logistic;
using VodovozBusiness.Services.Orders;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Domain.Logistic
{
	public class RouteList : RouteListEntity, IValidatableObject
	{
		public const decimal ConfirmedDistanceLimit = 99_999.99m;
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private static IGeneralSettings _generalSettingsSettingsGap;

		private IUnitOfWorkFactory _uowFactory => ScopeProvider.Scope
			.Resolve<IUnitOfWorkFactory>();
		private ISubdivisionRepository _subdivisionRepository => ScopeProvider.Scope
			.Resolve<ISubdivisionRepository>();
		private IOrganizationRepository _organizationRepository => ScopeProvider.Scope
			.Resolve<IOrganizationRepository>();
		private IRouteListRepository _routeListRepository => ScopeProvider.Scope
			.Resolve<IRouteListRepository>();
		private IRouteListItemRepository _routeListItemRepository => ScopeProvider.Scope.Resolve<IRouteListItemRepository>();
		private IDeliveryRulesSettings _deliveryRulesSettings => ScopeProvider.Scope
			.Resolve<IDeliveryRulesSettings>();
		private IDeliveryRepository _deliveryRepository => ScopeProvider.Scope
			.Resolve<IDeliveryRepository>();
		private IGeneralSettings GetGeneralSettingsSettings => ScopeProvider.Scope
			.Resolve<IGeneralSettings>();
		private IRouteListCashOrganisationDistributor routeListCashOrganisationDistributor => ScopeProvider.Scope
			.Resolve<IRouteListCashOrganisationDistributor>();
		private IExpenseCashOrganisationDistributor expenseCashOrganisationDistributor => ScopeProvider.Scope
			.Resolve<IExpenseCashOrganisationDistributor>();
		private ICarUnloadRepository _carUnloadRepository => ScopeProvider.Scope
			.Resolve<ICarUnloadRepository>();
		private ICashRepository _cashRepository => ScopeProvider.Scope
			.Resolve<ICashRepository>();
		private IEmployeeRepository _employeeRepository => ScopeProvider.Scope
			.Resolve<IEmployeeRepository>();
		private ICarLoadDocumentRepository _carLoadDocumentRepository => ScopeProvider.Scope
			.Resolve<ICarLoadDocumentRepository>();
		private IOrderRepository _orderRepository => ScopeProvider.Scope
			.Resolve<IOrderRepository>();
		private IOsrmSettings _osrmSettings => ScopeProvider.Scope
			.Resolve<IOsrmSettings>();
		private IOsrmClient _osrmClient => ScopeProvider.Scope
			.Resolve<IOsrmClient>();
		private INomenclatureSettings _nomenclatureSettings => ScopeProvider.Scope
			.Resolve<INomenclatureSettings>();
		private INomenclatureRepository _nomenclatureRepository => ScopeProvider.Scope
			.Resolve<INomenclatureRepository>();

		private IPermissionRepository _permissionRepository => ScopeProvider.Scope.Resolve<IPermissionRepository>();

		private CarVersion _carVersion;
		private Car _car;
		private RouteListProfitability _routeListProfitability;
		private GenericObservableList<DeliveryFreeBalanceOperation> _observableDeliveryFreeBalanceOperations;

		#region Свойства

		Employee _driver;

		[Display(Name = "Водитель")]
		public virtual new Employee Driver {
			get => _driver;
			set {
				Employee oldDriver = _driver;
				if(SetField(ref _driver, value, () => Driver)) {
					ChangeFuelDocumentsOnChangeDriver(oldDriver);
					if(Id == 0 || oldDriver != _driver)
						Forwarder = GetDefaultForwarder(_driver);
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
				SetField(ref forwarder, value);
			}
		}

		Employee logistician;

		[Display(Name = "Логист")]
		public virtual Employee Logistician {
			get => logistician;
			set => SetField(ref logistician, value);
		}

		[Display(Name = "Машина")]
		public virtual Car Car {
			get => _car;
			set {
				var oldCar = _car;
				if(SetField(ref _car, value, () => Car))
				{
					ChangeFuelDocumentsChangeCar(oldCar);

					if(value?.Driver != null && value.Driver.Status != EmployeeStatus.IsFired)
					{
						Driver = value.Driver;
					}

					if(Id == 0) {
						ObservableGeographicGroups.Clear();
						if(value != null)
						{
							foreach(var group in value.GeographicGroups)
							{
								ObservableGeographicGroups.Add(group);
							}
						}
					}

					if(!CanAddForwarder)
					{
						Forwarder = null;
					}
					OnPropertyChanged(nameof(CanAddForwarder));
				}
			}
		}

		DeliveryShift shift;

		[Display(Name = "Смена доставки")]
		public virtual DeliveryShift Shift {
			get => shift;
			set => SetField(ref shift, value);
		}

		Decimal confirmedDistance;

		/// <summary>
		/// Расстояние в километрах
		/// </summary>
		[Display(Name = "Подтверждённое расстояние")]
		public virtual Decimal ConfirmedDistance {
			get => confirmedDistance;
			set => SetField(ref confirmedDistance, value);
		}

		private decimal? planedDistance;

		/// <summary>
		/// Расстояние в километрах
		/// </summary>
		[Display(Name = "Планируемое расстояние")]
		public virtual decimal? PlanedDistance {
			get => planedDistance;
			protected set => SetField(ref planedDistance, value);
		}

		decimal? recalculatedDistance;

		/// <summary>
		/// Расстояние в километрах.
		/// </summary>
		[Display(Name = "Пересчитанное расстояние")]
		public virtual decimal? RecalculatedDistance {
			get => recalculatedDistance;
			set => SetField(ref recalculatedDistance, value);
		}

		RouteListStatus status;

		[Display(Name = "Статус")]
		public virtual RouteListStatus Status {
			get => status;
			set
			{
				if(SetField(ref status, value))
				{
					OnPropertyChanged(() => CanChangeStatusToDelivered);
					OnPropertyChanged(() => CanChangeStatusToDeliveredWithIgnoringAdditionalLoadingDocument);
				}
			}
		}

		DateTime? closingDate;
		[Display(Name = "Дата закрытия")]
		[HistoryDateOnly]
		public virtual DateTime? ClosingDate {
			get => closingDate;
			set => SetField(ref closingDate, value);
		}

		private DateTime? _firstClosingDate;
		[Display(Name = "Дата первого закрытия")]
		[HistoryDateOnly]
		public virtual DateTime? FirstClosingDate {
			get => _firstClosingDate;
			set => SetField(ref _firstClosingDate, value);
		}

		string closingComment;

		[Display(Name = "Комментарий")]
		public virtual string ClosingComment {
			get => closingComment;
			set => SetField(ref closingComment, value);
		}

		string logisticiansComment;
		[Display(Name = "Комментарий ЛО")]
		public virtual string LogisticiansComment {
			get => logisticiansComment;
			set => SetField(ref logisticiansComment, value);
		}

		Employee logisticiansCommentAuthor;
		[Display(Name = "Последний редактор комментария ЛО")]
		public virtual Employee LogisticiansCommentAuthor {
			get => logisticiansCommentAuthor;
			set => SetField(ref logisticiansCommentAuthor, value);
		}

		string cashierReviewComment;
		[Display(Name = "Комментарий по закрытию кассы")]
		public virtual string CashierReviewComment {
			get => cashierReviewComment;
			set => SetField(ref cashierReviewComment, value);
		}

		private bool wasAcceptedByCashier;
		[Display(Name = "Был подтверждён в диалоге закрытия МЛ")]
		public virtual bool WasAcceptedByCashier {
			get => wasAcceptedByCashier;
			set => SetField(ref wasAcceptedByCashier, value);
		}

		private bool _hasFixedShippingPrice;
		[Display(Name = "Есть фиксированная стоимость доставки?")]
		public virtual bool HasFixedShippingPrice
		{
			get => _hasFixedShippingPrice;
			set => SetField(ref _hasFixedShippingPrice, value);
		}

		private decimal _fixedShippingPrice;
		[Display(Name = "Фиксированная стоимость доставки")]
		public virtual decimal FixedShippingPrice
		{
			get => _fixedShippingPrice;
			set => SetField(ref _fixedShippingPrice, value);
		}

		Employee cashier;
		[IgnoreHistoryTrace]
		public virtual Employee Cashier {
			get => cashier;
			set => SetField(ref cashier, value);
		}

		decimal fixedDriverWage;

		[Display(Name = "Фиксированная заработанная плата водителя")]
		[IgnoreHistoryTrace]
		public virtual decimal FixedDriverWage {
			get => fixedDriverWage;
			set => SetField(ref fixedDriverWage, value);
		}

		decimal fixedForwarderWage;

		[Display(Name = "Фиксированная заработанная плата экспедитора")]
		[IgnoreHistoryTrace]
		public virtual decimal FixedForwarderWage {
			get => fixedForwarderWage;
			set => SetField(ref fixedForwarderWage, value);
		}

		Fine bottleFine;

		[Display(Name = "Штраф за бутыли")]
		public virtual Fine BottleFine {
			get => bottleFine;
			set => SetField(ref bottleFine, value);
		}

		private FuelOperation fuelOutlayedOperation;

		[Display(Name = "Операции расхода топлива")]
		[IgnoreHistoryTrace]
		public virtual FuelOperation FuelOutlayedOperation {
			get => fuelOutlayedOperation;
			set => SetField(ref fuelOutlayedOperation, value);
		}

		private bool differencesConfirmed;

		[Display(Name = "Расхождения подтверждены")]
		public virtual bool DifferencesConfirmed {
			get => differencesConfirmed;
			set => SetField(ref differencesConfirmed, value);
		}

		private DateTime? lastCallTime;

		[Display(Name = "Время последнего созвона")]
		public virtual DateTime? LastCallTime {
			get => lastCallTime;
			set => SetField(ref lastCallTime, value);
		}

		[Display(Name = "Время завершеняи доставки")]
		public virtual DateTime? DeliveredAt
		{
			get => _deliveredAt;
			set
			{
				if(_deliveredAt is null
				&& value != null
				&& value != default(DateTime)
				&& value != DateTime.MinValue)
				{
					SetField(ref _deliveredAt, value);
				}
			}
		}

		private bool closingFilled;

		/// <summary>
		/// Внутренее поле говорящее о том что первоначалная подготовка маршрутного листа к закрытию выполнена.
		/// Эта операция выполняется 1 раз при первом открытии диалога закрытия МЛ, тут оставляется пометка о том что операция выполнена.
		/// </summary>
		public virtual bool ClosingFilled {
			get => closingFilled;
			set => SetField(ref closingFilled, value);
		}

		IList<RouteListItem> addresses = new List<RouteListItem>();

		[Display(Name = "Адреса в маршрутном листе")]
		public virtual IList<RouteListItem> Addresses {
			get => addresses;
			set {
				SetField(ref addresses, value);
				SetNullToObservableAddresses();
			}
		}

		IList<RouteListFastDeliveryMaxDistance> _fastDeliveryMaxDistanceItems = new List<RouteListFastDeliveryMaxDistance>();

		[Display(Name = "Значения радиусов для быстрой доставки")]
		public virtual IList<RouteListFastDeliveryMaxDistance> FastDeliveryMaxDistanceItems
		{
			get => _fastDeliveryMaxDistanceItems;
			set
			{
				SetField(ref _fastDeliveryMaxDistanceItems, value);
			}
		}

		IList<RouteListMaxFastDeliveryOrders> _maxFastDeliveryOrdersItems = new List<RouteListMaxFastDeliveryOrders>();

		[Display(Name = "Значения макс. кол-ва заказов ДЗЧ")]
		public virtual IList<RouteListMaxFastDeliveryOrders> MaxFastDeliveryOrdersItems
		{
			get => _maxFastDeliveryOrdersItems;
			set => SetField(ref _maxFastDeliveryOrdersItems, value);
	}

		IList<DeliveryFreeBalanceOperation> _deliveryFreeBalanceOperations = new List<DeliveryFreeBalanceOperation>();

		[Display(Name = "Операции со свободными остатками")]
		public virtual IList<DeliveryFreeBalanceOperation> DeliveryFreeBalanceOperations
		{
			get => _deliveryFreeBalanceOperations;
			set => SetField(ref _deliveryFreeBalanceOperations, value);
		}

		public virtual GenericObservableList<DeliveryFreeBalanceOperation> ObservableDeliveryFreeBalanceOperations =>
			_observableDeliveryFreeBalanceOperations ?? (_observableDeliveryFreeBalanceOperations = new GenericObservableList<DeliveryFreeBalanceOperation>(DeliveryFreeBalanceOperations));

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
			set => SetField(ref fuelDocuments, value);
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
			set => SetField(ref normalWage, value);
		}

		private WagesMovementOperations driverWageOperation;

		[Display(Name = "Операция начисления зарплаты водителю")]
		[IgnoreHistoryTrace]
		public virtual WagesMovementOperations DriverWageOperation {
			get => driverWageOperation;
			set => SetField(ref driverWageOperation, value);
		}

		private WagesMovementOperations forwarderWageOperation;

		[Display(Name = "Операция начисления зарплаты экспедитору")]
		[IgnoreHistoryTrace]
		public virtual WagesMovementOperations ForwarderWageOperation {
			get => forwarderWageOperation;
			set => SetField(ref forwarderWageOperation, value);
		}

		private bool isManualAccounting;
		[Display(Name = "Расчёт наличных вручную?")]
		public virtual bool IsManualAccounting {
			get => isManualAccounting;
			set => SetField(ref isManualAccounting, value);
		}

		private TimeSpan? onLoadTimeStart;

		[Display(Name = "На погрузку в")]
		public virtual TimeSpan? OnLoadTimeStart {
			get => onLoadTimeStart;
			set => SetField(ref onLoadTimeStart, value);
		}

		private TimeSpan? onLoadTimeEnd;

		[Display(Name = "Закончить погрузку в")]
		public virtual TimeSpan? OnLoadTimeEnd {
			get => onLoadTimeEnd;
			set => SetField(ref onLoadTimeEnd, value);
		}

		private int? onLoadGate;

		[Display(Name = "Ворота на погрузку")]
		public virtual int? OnLoadGate {
			get => onLoadGate;
			set => SetField(ref onLoadGate, value);
		}

		private bool onLoadTimeFixed;

		[Display(Name = "Время погрузки установлено в ручную")]
		public virtual bool OnloadTimeFixed {
			get => onLoadTimeFixed;
			set => SetField(ref onLoadTimeFixed, value);
		}

		private bool addressesOrderWasChangedAfterPrinted;
		[Display(Name = "Был изменен порядок адресов после печати")]
		public virtual bool AddressesOrderWasChangedAfterPrinted {
			get => addressesOrderWasChangedAfterPrinted;
			set => SetField(ref addressesOrderWasChangedAfterPrinted, value);
		}

		string mileageComment;

		[Display(Name = "Комментарий к километражу")]
		public virtual string MileageComment {
			get => mileageComment;
			set => SetField(ref mileageComment, value);
		}

		bool mileageCheck;

		[Display(Name = "Проверка километража")]
		public virtual bool MileageCheck {
			get => mileageCheck;
			set => SetField(ref mileageCheck, value);
		}

		Employee closedBy;
		[Display(Name = "Закрыт сотрудником")]
		[IgnoreHistoryTrace]
		public virtual Employee ClosedBy {
			get => closedBy;
			set => SetField(ref closedBy, value);
		}

		[Display(Name = "Сдается в подразделение")]
		public virtual Subdivision ClosingSubdivision
		{
			get
			{
				var geoGroup = GeographicGroups.FirstOrDefault();
				if(geoGroup == null)
				{
					return null;
				}

				var geoGroupVersion = geoGroup.GetVersionOrNull(Date);
				if(geoGroupVersion == null)
				{
					return null;
				}
				return geoGroupVersion.CashSubdivision;
			}
		}

		IList<GeoGroup> geographicGroups = new List<GeoGroup>();
		[Display(Name = "Группа района")]
		public virtual IList<GeoGroup> GeographicGroups {
			get => geographicGroups;
			set => SetField(ref geographicGroups, value);
		}

		GenericObservableList<GeoGroup> observableGeographicGroups;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<GeoGroup> ObservableGeographicGroups {
			get {
				if(observableGeographicGroups == null)
					observableGeographicGroups = new GenericObservableList<GeoGroup>(GeographicGroups);
				return observableGeographicGroups;
			}
		}

		bool? notFullyLoaded;
		[Display(Name = "МЛ погружен не полностью")]
		public virtual bool? NotFullyLoaded {
			get => notFullyLoaded;
			set => SetField(ref notFullyLoaded, value);
		}

		private IList<DocumentPrintHistory> _printsHistory = new List<DocumentPrintHistory>();
		[Display(Name = "История печати маршрутного листа")]
		public virtual IList<DocumentPrintHistory> PrintsHistory
		{
			get => _printsHistory;
			set => SetField(ref _printsHistory, value);
		}

		private DriverTerminalCondition? _driverTerminalCondition;
		[Display(Name = "Состояние терминала")]
		public virtual DriverTerminalCondition? DriverTerminalCondition
		{
			get => _driverTerminalCondition;
			set => SetField(ref _driverTerminalCondition, value);
		}

		private AdditionalLoadingDocument _additionalLoadingDocument;
		private DateTime? _deliveredAt;
		private bool _specialConditionsAccepted;
		private DateTime? _specialConditionsAcceptedAt;

		[Display(Name = "Документ запаса")]
		public virtual AdditionalLoadingDocument AdditionalLoadingDocument
		{
			get => _additionalLoadingDocument;
			set => SetField(ref _additionalLoadingDocument, value);
		}

		[Display(Name = "Рентабельность МЛ")]
		public virtual RouteListProfitability RouteListProfitability
		{
			get => _routeListProfitability;
			set => SetField(ref _routeListProfitability, value);
		}

		[Display(Name = "Специальные условия МЛ приняты")]
		public virtual bool SpecialConditionsAccepted
		{
			get => _specialConditionsAccepted;
			set => SetField(ref _specialConditionsAccepted, value);
		}

		public virtual DateTime? SpecialConditionsAcceptedAt
		{
			get => _specialConditionsAcceptedAt;
			set => SetField(ref _specialConditionsAcceptedAt, value);
		}

		#endregion Свойства

		#region readonly Свойства

		public virtual string Title => string.Format("МЛ №{0}", Id);

		public virtual bool HasAddressesOrAdditionalLoading =>
			Addresses.Any() || AdditionalLoadingDocument != null;

		public virtual decimal UniqueAddressCount => Addresses.Where(item => item.IsDelivered())
			.Select(item => item.Order.DeliveryPoint.Id)
			.Distinct()
			.Count();

		public virtual bool NeedMileageCheck =>
			GetCarVersion.CarOwnType == CarOwnType.Company && Car.CarModel.CarTypeOfUse != CarTypeOfUse.Truck;

		public virtual decimal PhoneSum {
			get {
				if(GetCarVersion.CarOwnType == CarOwnType.Company && Car.CarModel.CarTypeOfUse == CarTypeOfUse.Truck || Driver == null || Driver.VisitingMaster)
				{
					return 0;
				}

				return Wages.GetDriverRates(Date).PhoneServiceCompensationRate * UniqueAddressCount;
			}
		}

		public virtual decimal Total => Addresses.Sum(x => x.TotalCash) - PhoneSum;

		public virtual decimal MoneyToReturn {
			get {
				decimal payedForFuel = FuelDocuments.Where(x => x.PayedForFuel.HasValue && x.FuelPaymentType.HasValue && x.FuelPaymentType == FuelPaymentType.Cash).Sum(x => x.PayedForFuel.Value);

				return Total - payedForFuel;
			}
		}

		/// <summary>
		/// Количество полных 19л бутылей в МЛ для клиентов
		/// </summary>
		/// <returns>Количество полных 19л бутылей</returns>
		public virtual int TotalFullBottlesToClient => Addresses.Sum(a => a.GetFullBottlesToDeliverCount());

		public virtual bool NeedToLoad =>
			Addresses.Any(address => address.NeedToLoad)
			|| (AdditionalLoadingDocument != null && AdditionalLoadingDocument.HasItemsNeededToLoad);

		public virtual bool HasMoneyDiscrepancy => Total != _cashRepository.CurrentRouteListCash(UoW, Id);

		public virtual bool CanChangeStatusToDelivered
		{
			get
			{
				return Status == RouteListStatus.EnRoute
					   && !Addresses.Any(a => a.Status == RouteListItemStatus.EnRoute)
					   && AdditionalLoadingDocument == null;
			}
		}

		public virtual bool CanChangeStatusToDeliveredWithIgnoringAdditionalLoadingDocument
		{
			get
			{
				return Status == RouteListStatus.EnRoute
					   && !Addresses.Any(a => a.Status == RouteListItemStatus.EnRoute);
			}
		}

		/// <summary>
		/// МЛ находится в статусе для открытия диалога закрытия
		/// </summary>
		public virtual bool CanBeOpenedInClosingDlg
			=> new[] { RouteListStatus.Delivered, RouteListStatus.MileageCheck, RouteListStatus.OnClosing, RouteListStatus.Closed }
				.Contains(Status);

		public virtual decimal CurrentFastDeliveryMaxDistanceValue => GetFastDeliveryMaxDistanceValue();

		public virtual List<RouteListItem> DeliveredRouteListAddresses =>
			Addresses.Where(a => a.IsDelivered()).ToList();

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

		public virtual CarVersion GetCarVersion => Car?.GetActiveCarVersionOnDate(Date);

		public virtual IDictionary<int, decimal> GetCashChangesForOrders()
		{
			var result = new Dictionary<int, decimal>();

			foreach(var order in Addresses
				.Where(a => a.Status != RouteListItemStatus.Transfered)
				.Select(a => a.Order)
				.Where(o => o.PaymentType == Client.PaymentType.Cash))
			{
				var change = (order?.Trifle ?? 0) - order.OrderSum;

				if(change > 0)
				{
					result.Add(order.Id, change);
				}
			}

			return result;
		}

		/// <summary>
		/// Возврат экспедитора по умолчанию для водителя <paramref name="driver"/>
		/// </summary>
		/// <returns>Экспедитор по умолчание если не уволен</returns>
		/// <param name="driver">Водитель</param>
		Employee GetDefaultForwarder(Employee driver)
		{
			if(driver?.DefaultForwarder?.Status == EmployeeStatus.IsFired) {
				//если больше не с нами,то не нужно его держать умолчальным в водителе
				driver.DefaultForwarder = null;
			} else if(driver != null && driver.DefaultForwarder != null) {
				return driver.DefaultForwarder;
			}
			return null;
		}

		public virtual void ChangeFuelDocumentsChangeCar(Car oldCar)
		{
			if(oldCar == null || Car == oldCar || !FuelDocuments.Any()) {
				return;
			}

			foreach(FuelDocument item in ObservableFuelDocuments) {
				item.Car = Car;
				item.FuelOperation.Car = Car;
			}
		}

		public virtual void ChangeFuelDocumentsOnChangeDriver(Employee oldDriver)
		{
			if(Driver == null || oldDriver == null || Driver == oldDriver || !FuelDocuments.Any())
				return;

			foreach(FuelDocument item in ObservableFuelDocuments) {
				item.Driver = Driver;
				item.FuelOperation.Driver = Driver;
			}
		}

		public virtual bool FuelOperationHaveDiscrepancy()
		{
			if(FuelOutlayedOperation == null) {
				return false;
			}
			var carDiff = FuelDocuments.Select(x => x.FuelOperation).Any(x => x.Car != null && x.Car.Id != Car.Id)
									   || (FuelOutlayedOperation.Car != null && FuelOutlayedOperation.Car.Id != Car.Id);
			var driverDiff = FuelDocuments.Select(x => x.FuelOperation).Any(x => x.Driver != null && x.Driver.Id != Driver.Id)
										  || (FuelOutlayedOperation.Driver != null && FuelOutlayedOperation.Driver.Id != Driver.Id);
			return carDiff || driverDiff;
		}

		public virtual bool TryRemoveAddress(RouteListItem address, out string msg, IRouteListItemRepository routeListItemRepository)
		{
			if(routeListItemRepository == null)
				throw new ArgumentNullException(nameof(routeListItemRepository));

			msg = string.Empty;
			if(address.WasTransfered) {
				var from = routeListItemRepository.GetTransferredFrom(UoW, address)?.RouteList?.Id;
				msg = string.Format(
					"Адрес \"{0}\" не может быть удалён, т.к. был перенесён из МЛ №{1}. Воспользуйтесь функционалом из вкладки \"Перенос адресов маршрутных листов\" для возврата этого адреса в исходный МЛ.",
					address.Order.DeliveryPoint?.ShortAddress,
					from.HasValue ? from.Value.ToString() : "???"
				);
				return false;
			}

			if(address.TransferedTo != null)
			{
				msg = $"Адрес \"{address.Order.DeliveryPoint?.ShortAddress}\" не может быть удалён, т.к. был перенесён в МЛ №{address.TransferedTo.RouteList.Id}.\n" +
					  $"Воспользуйтесь функционалом из вкладки \"Перенос адресов маршрутных листов\" для возврата этого адреса в исходный МЛ.";
				return false;
			}

			if(_routeListRepository.GetCarLoadDocuments(UoW, Id).Any())
			{
				msg = "Для маршрутного листа были созданы документы погрузки. Сначала необходимо удалить их.";
				return false;
			}
			if(address.Status == RouteListItemStatus.Transfered && address.TransferedTo != null)
			{
				var toAddress = routeListItemRepository.GetRouteListItemById(UoW, address.TransferedTo.Id);
				toAddress.SetTransferTo(null);
				toAddress.WasTransfered = false;
				toAddress.AddressTransferType = null;

				UoW.Save(toAddress);
			}
			else
			{
				address.ChangeOrderStatus(OrderStatus.Accepted);
			}

			var routeListAddressKeepingDocumentController = new RouteListAddressKeepingDocumentController(_employeeRepository, _nomenclatureRepository);
			routeListAddressKeepingDocumentController.RemoveRouteListKeepingDocument(UoW, address, true);

			ObservableAddresses.Remove(address);
			return true;
		}

		public virtual void RemoveAddress(RouteListItem address)
		{
			if(!TryRemoveAddress(address, out string message, _routeListItemRepository))
				throw new NotSupportedException(string.Format("\n\n{0}\n", message));
		}

		public virtual void CheckAddressOrder()
		{
			for(int i = 0; i < Addresses.Count; i++) {
				if(Addresses[i] == null) {
					Addresses.RemoveAt(i);
					i--;
					continue;
				}

				if(Addresses[i].IndexInRoute != i) {
					if(PrintsHistory?.Any() ?? false) {
						AddressesOrderWasChangedAfterPrinted = true;
					}
					Addresses[i].IndexInRoute = i;
				}
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

		public virtual List<Discrepancy> GetDiscrepancies()
		{
			List<Discrepancy> result = new List<Discrepancy>();

			#region Талон погрузки

			var allLoaded = _routeListRepository.AllGoodsLoaded(UoW, this);

			AddDiscrepancy(allLoaded, result, (discrepancy, amount) => discrepancy.FromWarehouse = amount);

			#endregion

			#region Талон разгрузки

			var shipmentCategories = Nomenclature.GetCategoriesForShipment().ToArray();

			var allUnloaded = _routeListRepository.GetReturnsToWarehouse(UoW, Id, shipmentCategories, new[] { _nomenclatureSettings.DefaultBottleNomenclatureId })
				.Select(x => new GoodsInRouteListResult { NomenclatureId = x.NomenclatureId, Amount = x.Amount });

			AddDiscrepancy(allUnloaded, result, (discrepancy, amount) => discrepancy.ToWarehouse = amount);

			#endregion

			#region Получено от других водителей

			var allGoodsTransferredFromDrivers = _routeListRepository.AllGoodsTransferredFromDrivers(UoW, this, Nomenclature.GetCategoriesForShipment(), AddressTransferType.FromHandToHand);
			AddDiscrepancy(allGoodsTransferredFromDrivers, result, (discrepancy, amount) => discrepancy.TransferedFromDrivers = amount);

			#endregion

			#region Передано другим водителям

			var allGoodsTransferedToAnotherDrivers = _routeListRepository.AllGoodsTransferredToAnotherDrivers(
				UoW, this, Nomenclature.GetCategoriesForShipment(), AddressTransferType.FromHandToHand);

			AddDiscrepancy(allGoodsTransferedToAnotherDrivers, result, (discrepancy, amount) => discrepancy.TransferedToAnotherDrivers = amount);

			#endregion

			#region Доставлено клиентам

			var allDelivered = _routeListRepository.GetActualGoodsForShipment(UoW, Id).ToList();

			if(_routeListRepository.GetActualEquipmentForShipment(UoW, this.Id, Direction.Deliver) is IEnumerable<GoodsInRouteListResult> equipmentActualCount)
			{
				allDelivered.AddRange(equipmentActualCount);
			}

			AddDiscrepancy(allDelivered, result, (discrepancy, amount) => discrepancy.DeliveredToClient = amount);

			#endregion

			#region Недовeзённое кол-во

			foreach(var address in Addresses) {
				foreach(var orderItem in address.Order.OrderItems) {
					if(!Nomenclature.GetCategoriesForShipment().Contains(orderItem.Nomenclature.Category)
						|| orderItem.Nomenclature.Category == NomenclatureCategory.bottle)
					{
						continue;
					}
					Discrepancy discrepancy = null;

					var isNotFromHandsToHandsTransfer = address.TransferedTo == null
						|| (address.TransferedTo.AddressTransferType != null
							&& new[] { AddressTransferType.NeedToReload, AddressTransferType.FromFreeBalance }.Contains(address.TransferedTo.AddressTransferType.Value));

					if(isNotFromHandsToHandsTransfer)
					{
						discrepancy = new Discrepancy
						{
							ClientRejected = orderItem.ReturnedCount,
							Nomenclature = orderItem.Nomenclature,
							Name = orderItem.Nomenclature.Name
						};
					}

					if(discrepancy != null && discrepancy.ClientRejected != 0) {
						AddDiscrepancy(result, discrepancy);
					}
				}
			}

			#endregion

			#region Оборудование

			foreach(var address in Addresses)
			{
				foreach(var orderEquipment in address.Order.OrderEquipments)
				{
					if(!Nomenclature.GetCategoriesForShipment().Contains(orderEquipment.Nomenclature.Category))
					{
						continue;
					}
					var discrepancy = new Discrepancy
					{
						Nomenclature = orderEquipment.Nomenclature,
						Name = orderEquipment.Nomenclature.Name
					};

					if(address.TransferedTo == null)
					{
						if(orderEquipment.Direction == Direction.Deliver)
						{
							discrepancy.ClientRejected = orderEquipment.UndeliveredCount;
						}
						else
						{
							discrepancy.PickedUpFromClient = orderEquipment.ActualCount ?? 0;
						}
						AddDiscrepancy(result, discrepancy);
					}
					else if(address.TransferedTo.AddressTransferType != null
						&& new[] { AddressTransferType.NeedToReload, AddressTransferType.FromFreeBalance }
							.Contains(address.TransferedTo.AddressTransferType.Value))
					{
						if(orderEquipment.Direction == Direction.Deliver)
						{// не обрабатываем pickup, т.к. водитель физически не был на адресе, чтобы забрать оборудование
							discrepancy.ClientRejected = orderEquipment.Count;
							AddDiscrepancy(result, discrepancy);
						}
					}
				}
			}

			#endregion

			#region Терминал для оплаты

			//Терминал для оплаты 
			//TODO: Если используются операции по водителю с терминалами, переделать на них.

			var terminalId = _nomenclatureSettings.NomenclatureIdForTerminal;
			var terminal = UoW.GetById<Nomenclature>(terminalId);
			var loadedTerminalAmount = _carLoadDocumentRepository.LoadedTerminalAmount(UoW, Id, terminalId);
			var unloadedTerminalAmount = _carUnloadRepository.UnloadedTerminalAmount(UoW, Id, terminalId);

			if(loadedTerminalAmount > 0)
			{
				var discrepancyTerminalFreeBalance = new Discrepancy
				{
					Nomenclature = terminal,
					FreeBalance = loadedTerminalAmount - unloadedTerminalAmount,
					ToWarehouse = unloadedTerminalAmount,
					Name = terminal.Name
				};

				AddDiscrepancy(result, discrepancyTerminalFreeBalance);
			}

			#endregion


			#region Свободные остатки

			var freeBalance = ObservableDeliveryFreeBalanceOperations
				.Where(o => o.Nomenclature.Id != _nomenclatureSettings.DefaultBottleNomenclatureId)
				.GroupBy(o => o.Nomenclature)
				.Select(list => new GoodsInRouteListResult
				{
					NomenclatureId = list.First().Nomenclature.Id,
					Amount = list.Sum(o => o.Amount)
				});

			AddDiscrepancy(freeBalance, result, (discrepancy, amount) => discrepancy.FreeBalance = amount);

			#endregion

			return result;
		}

		private void AddDiscrepancy(IEnumerable<GoodsInRouteListResult> goods, List<Discrepancy> discrepancies, Action<Discrepancy, decimal> setAmountAction)
		{
			var nomenclatures = UoW.Session.QueryOver<Nomenclature>()
				.Where(n => n.Id.IsIn(goods.Select(g => g.NomenclatureId).ToArray()))
				.List();

			foreach(var product in goods)
			{
				var nomenclature = nomenclatures.First(n => n.Id == product.NomenclatureId);

				var discrepancy = new Discrepancy
				{
					Name = nomenclature.Name,
					Nomenclature = nomenclature
				};

				setAmountAction(discrepancy, product.Amount);

				AddDiscrepancy(discrepancies, discrepancy);
			}
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
				existingDiscrepancy.TransferedToAnotherDrivers += item.TransferedToAnotherDrivers;
				existingDiscrepancy.TransferedFromDrivers += item.TransferedFromDrivers;
				existingDiscrepancy.DeliveredToClient += item.DeliveredToClient;
				existingDiscrepancy.FreeBalance += item.FreeBalance;
			}
		}

		public virtual bool IsConsistentWithUnloadDocument()
		{
			var bottlesReturnedToWarehouse = (int)_routeListRepository.GetReturnsToWarehouse(
				UoW,
				Id,
				_nomenclatureSettings.ReturnedBottleNomenclatureId)
			.Sum(item => item.Amount);

			var discrepancies = GetDiscrepancies();

			var hasItemsDiscrepancies = discrepancies.Any(discrepancy => discrepancy.Remainder != 0);
			bool hasFine = BottleFine != null;
			var items = Addresses.Where(item => item.IsDelivered());
			int bottlesReturnedTotal = items.Sum(item => item.BottlesReturned);
			var hasTotalBottlesDiscrepancy = bottlesReturnedToWarehouse != bottlesReturnedTotal;
			return hasFine || (!hasTotalBottlesDiscrepancy && !hasItemsDiscrepancies) || DifferencesConfirmed;
		}

		public virtual void UpdateClosedInformation()
		{
			if(Status == RouteListStatus.Closed)
			{
				ClosedBy = _employeeRepository.GetEmployeeForCurrentUser(UoW);
				ClosingDate = DateTime.Now;
				if(!FirstClosingDate.HasValue)
				{
					FirstClosingDate = DateTime.Now;
				}
			}
			else
			{
				ClosedBy = null;
				ClosingDate = null;
			}
		}



		public virtual void AddPrintHistory()
		{
			var newHistory = new DocumentPrintHistory
			{
				PrintingTime = DateTime.Now,
				DocumentType = PrintAsClosed() ? PrintedDocumentType.ClosedRouteList : PrintedDocumentType.RouteList,
				RouteList = this
			};
			_printsHistory.Add(newHistory);
		}

		/// <summary>
		/// Указывает, находится ли МЛ в таком статусе, что его надо печатать как ClosedRouteList.rdl
		/// </summary>
		public virtual bool PrintAsClosed() => CanBeOpenedInClosingDlg;

		public virtual void CreateSelfDriverTerminalTransferDocument()
		{
			if(_routeListRepository.GetLastTerminalDocumentForEmployee(UoW, Driver) is DriverAttachedTerminalGiveoutDocument)
			{
				ServicesConfig.InteractiveService.ShowMessage(
					ImportanceLevel.Warning,
					$"Терминал привязан к водителю {Driver.GetPersonNameWithInitials()}",
					"Не удалось перенести терминал");

				return;
			}

			var foundRouteLists = UoW.Session.QueryOver<RouteList>()
				.Where(x => x.Driver == this.Driver
							&& x.Date == this.Date
							&& x.Status == RouteListStatus.InLoading
							&& x.Id != this.Id)
				.List();

			if(foundRouteLists.Count == 0)
			{
				ServicesConfig.InteractiveService.ShowMessage(
					ImportanceLevel.Warning,
					$"Не найдены подходящие МЛ для переноса терминала из МЛ №{Id}. Попробуйте перенести терминал вручную.",
					"Не удалось перенести терминал");

				return;
			}

			var terminalId = _nomenclatureSettings.NomenclatureIdForTerminal;
			var loadedTerminalAmount = _carLoadDocumentRepository.LoadedTerminalAmount(UoW, Id, terminalId);
			var unloadedTerminalAmount = _carUnloadRepository.UnloadedTerminalAmount(UoW, Id, terminalId);

			if(loadedTerminalAmount - unloadedTerminalAmount <= 0)
			{
				ServicesConfig.InteractiveService.ShowMessage(
					ImportanceLevel.Warning,
					$"В МЛ №{Id} отсутствуют погруженные терминалы.");

				return;
			}

			if(foundRouteLists.Count > 1)
			{
				ServicesConfig.InteractiveService.ShowMessage(
					ImportanceLevel.Warning,
					$"Для переноса терминала из МЛ № {Id} найдено больше одного МЛ: " +
						$"{string.Join(", ", foundRouteLists.Select(x => x.Id).ToArray())}.\nПопробуйте перенести терминал вручную.",
					"Не удалось перенести терминал");

				return;
			}

			var foundRouteList = foundRouteLists.FirstOrDefault();

			var selfDriverTerminalTransferDocument = _routeListRepository.GetSelfDriverTerminalTransferDocument(UoW, foundRouteList.Driver, foundRouteList);

			if(selfDriverTerminalTransferDocument != null)
			{
				ServicesConfig.InteractiveService.ShowMessage(
					ImportanceLevel.Warning,
					$"Терминал уже был перенесён в МЛ №{selfDriverTerminalTransferDocument.RouteListTo.Id}",
					"Не удалось перенести терминал");

				return;
			}
			else
			{
				if(ServicesConfig.InteractiveService.Question($"Терминал будет перенесён из МЛ №{Id} в МЛ №{foundRouteList.Id}. Продолжить?"))
				{
					var terminalTransferDocumentForOneDriver = new SelfDriverTerminalTransferDocument()
					{
						Author = _employeeRepository.GetEmployeeForCurrentUser(UoW),
						CreateDate = DateTime.Now,
						DriverFrom = foundRouteList.Driver,
						DriverTo = foundRouteList.Driver,
						RouteListFrom = this,
						RouteListTo = foundRouteList,
					};

					using(var localUoW = _uowFactory.CreateWithoutRoot())
					{
						localUoW.Save(terminalTransferDocumentForOneDriver);
						localUoW.Commit();
					}

					ServicesConfig.InteractiveService.ShowMessage(
						ImportanceLevel.Info,
						$"Терминал перенесён в МЛ №{foundRouteList.Id}",
						"Готово");
				}
			}
		}

		public virtual bool CanAddForwarder
		{
			get
			{
				if(GetCarVersion?.CarOwnType != CarOwnType.Company)
				{
					return true;
				}

				switch(Car.CarModel.CarTypeOfUse)
				{
					case CarTypeOfUse.Largus:
						return GetGeneralSettingsSettings.GetCanAddForwardersToLargus;
					case CarTypeOfUse.Minivan:
						return GetGeneralSettingsSettings.GetCanAddForwardersToMinivan;
					default:
						return true;
				}
			}
		}

		public static void SetGeneralSettingsSettingsGap(
			IGeneralSettings generalSettingsSettingsGap)
		{
			_generalSettingsSettingsGap = generalSettingsSettingsGap;
		}

		public virtual void UpdateFastDeliveryMaxDistanceValue(decimal _fastDeliveryMaxDistanceValue)
		{
			if(FastDeliveryMaxDistanceItems.Count > 0)
			{
				var currentFastDeliveryMaxDistance = FastDeliveryMaxDistanceItems.Where(f => f.EndDate == null).FirstOrDefault();

				if(currentFastDeliveryMaxDistance != null)
				{
					if(currentFastDeliveryMaxDistance.Distance != _fastDeliveryMaxDistanceValue)
					{
						currentFastDeliveryMaxDistance.EndDate = DateTime.Now;
					}
					else
					{
						return;
					}
				}
			}

			var routeListFastDeliveryMaxDistance = new RouteListFastDeliveryMaxDistance()
			{
				RouteList = this,
				Distance = _fastDeliveryMaxDistanceValue,
				StartDate = DateTime.Now
			};

			FastDeliveryMaxDistanceItems.Add(routeListFastDeliveryMaxDistance);
		}

		public virtual void UpdateMaxFastDeliveryOrdersValue(int maxFastDeliveryOrdersValue)
		{
			if(MaxFastDeliveryOrdersItems.Count > 0)
			{
				var currentMaxFastDeliveryOrders = MaxFastDeliveryOrdersItems.Where(f => f.EndDate == null).FirstOrDefault();

				if(currentMaxFastDeliveryOrders != null)
				{
					if(currentMaxFastDeliveryOrders.MaxOrders != maxFastDeliveryOrdersValue)
					{
						currentMaxFastDeliveryOrders.EndDate = DateTime.Now;
					}
					else
					{
						return;
					}
				}
			}

			var maxFastDeliveryOrders = new RouteListMaxFastDeliveryOrders
			{
				RouteList = this,
				MaxOrders = maxFastDeliveryOrdersValue,
				StartDate = DateTime.Now
			};

			MaxFastDeliveryOrdersItems.Add(maxFastDeliveryOrders);
		}

		public virtual decimal GetFastDeliveryMaxDistanceValue(DateTime? date = null)
		{
			if(date == null)
			{
				date = DateTime.Now;
			}

			var fastDeliveryMaxDistanceItem = UoW.GetAll<RouteListFastDeliveryMaxDistance>()
				.Where(d => d.RouteList.Id == this.Id && d.StartDate <= date && (d.EndDate == null || d.EndDate > date))
				.FirstOrDefault();

			if(fastDeliveryMaxDistanceItem != null)
			{
				return fastDeliveryMaxDistanceItem.Distance;
			}

			return (decimal)_deliveryRepository.GetMaxDistanceToLatestTrackPointKmFor(date ?? DateTime.Now);
		}

		public virtual int GetMaxFastDeliveryOrdersValue(DateTime? date = null)
		{
			if(date == null)
			{
				date = DateTime.Now;
			}

			var maxFastDeliveryOrdersItem = UoW.GetAll<RouteListMaxFastDeliveryOrders>()
				.Where(d => d.RouteList.Id == Id && d.StartDate <= date && (d.EndDate == null || d.EndDate > date))
				.FirstOrDefault();

			if(maxFastDeliveryOrdersItem != null)
			{
				return maxFastDeliveryOrdersItem.MaxOrders;
			}

			return _deliveryRulesSettings.MaxFastOrdersPerSpecificTime;
		}

		public virtual bool IsDriversDebtInPermittedRangeVerification()
		{
			if(Driver != null)
			{
				var maxDriversUnclosedRouteListsCountParameter = GetGeneralSettingsSettings.DriversUnclosedRouteListsHavingDebtMaxCount;
				var maxDriversRouteListsDebtsSumParameter = GetGeneralSettingsSettings.DriversRouteListsMaxDebtSum;

				var isDriverHasActiveStopListRemoval = Driver.IsDriverHasActiveStopListRemoval(UoW);

				if(isDriverHasActiveStopListRemoval)
				{
					return true;
				}

				var unclosedRouteListsHavingDebtsCount =
					_routeListRepository.GetUnclosedRouteListsCountHavingDebtByDriver(UoW, Driver.Id, Id);
				var unclosedRouteListsDebtsSum =
					_routeListRepository.GetUnclosedRouteListsDebtsSumByDriver(UoW, Driver.Id, Id);

				if(unclosedRouteListsHavingDebtsCount > maxDriversUnclosedRouteListsCountParameter 
					|| unclosedRouteListsDebtsSum > maxDriversRouteListsDebtsSumParameter)
				{
					var messageString =
						$"Водитель {Driver.FullName} в стоп-листе, т.к. кол-во незакрытых МЛ с долгом {unclosedRouteListsHavingDebtsCount} штук " +
						$"и суммарный долг водителя по всем МЛ составляет {unclosedRouteListsDebtsSum} рублей.";

					var canEditDriversStopListParameters =
						ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_drivers_stop_list_parameters");

					if(canEditDriversStopListParameters)
					{
						messageString += "\n\nВсе равно продолжить?";
						return ServicesConfig.InteractiveService.Question(messageString, "Требуется подтверждение");
					}

					ServicesConfig.InteractiveService.ShowMessage(ImportanceLevel.Error, messageString);
					return false;
				}
			}
			return true;
		}

		#endregion

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			bool cashOrderClose = false;
			bool canSaveRouteListWithoutOrders = false;
			var routeList = validationContext.ObjectInstance as RouteList;

			if(validationContext.Items.ContainsKey("cash_order_close"))
			{
				cashOrderClose = (bool)validationContext.Items["cash_order_close"];
			}

			if(validationContext.Items.ContainsKey(Core.Domain.Permissions.LogisticPermissions.RouteList.CanCreateRouteListWithoutOrders))
			{
				canSaveRouteListWithoutOrders =
					(bool)validationContext.Items[Core.Domain.Permissions.LogisticPermissions.RouteList.CanCreateRouteListWithoutOrders];
			}

			if(validationContext.Items.ContainsKey("NewStatus"))
			{
				RouteListStatus newStatus = (RouteListStatus)validationContext.Items["NewStatus"];
				switch(newStatus)
				{
					case RouteListStatus.New:
					case RouteListStatus.Confirmed:
					case RouteListStatus.InLoading:
					case RouteListStatus.Closed: break;
					case RouteListStatus.MileageCheck:
						var orderSettings = validationContext.GetService<IOrderSettings>();
						var deliveryRulesSettings = validationContext.GetService<IDeliveryRulesSettings>();

						validationContext.Items.TryGetValue(ValidationKeyIgnoreReceiptsForOrders, out var ignoreReceiptsInOrdersParameter);

						if(!(ignoreReceiptsInOrdersParameter is List<int> ignoreReceiptsInOrders))
						{
							ignoreReceiptsInOrders = new List<int>();
						}

						foreach(var address in Addresses)
						{
							var validator = ServicesConfig.ValidationService;
							var orderValidationContext = new ValidationContext(
								address.Order,
								null,
								new Dictionary<object, object>
								{
									{ "NewStatus", OrderStatus.Closed },
									{ "cash_order_close", cashOrderClose },
									{ "AddressStatus", address.Status },
									{ Order.ValidationKeyIgnoreReceipts, ignoreReceiptsInOrders.Contains(address.Order.Id) }
								}
							);

							orderValidationContext.InitializeServiceProvider(type =>
							{
								if(type == typeof(IOrderSettings))
								{
									return orderSettings;
								}

								if(type == typeof(IDeliveryRulesSettings))
								{
									return deliveryRulesSettings;
								}

								return null;
							});

							validator.Validate(address.Order, orderValidationContext, false);

							foreach(var result in validator.Results)
							{
								yield return result;
							}
						}

						break;
					case RouteListStatus.EnRoute: break;
					case RouteListStatus.OnClosing: break;
				}
			}

			validationContext.Items.TryGetValue(nameof(IRouteListItemRepository), out var rliRepositoryObject);

			if(!(rliRepositoryObject is IRouteListItemRepository rliRepository))
			{
				rliRepository = validationContext.GetService<IRouteListItemRepository>();
			}

			if(rliRepository != null)
			{
				foreach(var address in Addresses)
				{
					if(rliRepository.AnotherRouteListItemForOrderExist(UoW, address))
					{
						yield return new ValidationResult($"Адрес {address.Order.Id} находится в другом МЛ");
					}

					if(rliRepository.CurrentRouteListHasOrderDuplicate(UoW, address, Addresses.Select(x => x.Id).ToArray()))
					{
						yield return new ValidationResult($"Адрес {address.Order.Id} дублируется в текущем МЛ");
					}

					foreach(var result in address.Validate(new ValidationContext(address)))
					{
						yield return result;
					}
				}
			}
			else
			{
				throw new ArgumentException($"Для валидации МЛ должен быть доступен {nameof(IRouteListItemRepository)}");
			}

			if(!GeographicGroups.Any())
			{
				yield return new ValidationResult(
					"Необходимо указать район",
					new[] { Gamma.Utilities.PropertyUtil.GetPropertyName(this, o => o.GeographicGroups) }
				);
			}

			if(Driver == null)
			{
				yield return new ValidationResult("Не заполнен водитель.",
					new[] { Gamma.Utilities.PropertyUtil.GetPropertyName(this, o => o.Driver) });
			}

			if(Driver != null && Driver.GetActualWageParameter(Date) == null)
			{
				yield return new ValidationResult($"Нет данных о параметрах расчета зарплаты водителя на выбранную дату доставки.",
					new[] { Gamma.Utilities.PropertyUtil.GetPropertyName(this, o => o.Driver) });
			}

			if(Forwarder != null && Forwarder.GetActualWageParameter(Date) == null)
			{
				yield return new ValidationResult($"Нет данных о параметрах расчета зарплаты экспедитора на выбранную дату доставки.",
					new[] { Gamma.Utilities.PropertyUtil.GetPropertyName(this, o => o.Forwarder) });
			}

			if(Car is null)
			{
				yield return new ValidationResult("На заполнен автомобиль.",
					new[] { nameof(Car) });
			}
			else
			{
				if(GetCarVersion == null)
				{
					yield return new ValidationResult("Нет данных о версии автомобиля на выбранную дату доставки.",
						new[] { nameof(Car.CarVersions) });
				}

				if(Car.CarModel?.CarTypeOfUse == CarTypeOfUse.Loader)
				{
					yield return new ValidationResult("Нельзя использовать погрузчик как автомобиль МЛ",
						new[] { nameof(Car) });
				}
			}

			if(MileageComment?.Length > 500)
			{
				yield return new ValidationResult($"Превышена длина комментария к километражу ({MileageComment.Length}/500)",
					new[] { nameof(MileageComment) });
			}

			if(validationContext.Items.ContainsKey(nameof(DriverTerminalCondition)) &&
			   (bool)validationContext.Items[nameof(DriverTerminalCondition)] && DriverTerminalCondition == null)
			{
				yield return new ValidationResult("Не указано состояние терминала водителя", new[] { nameof(DriverTerminalCondition) });
			}

			if(GeographicGroups.Any(x => x.GetVersionOrNull(Date) == null))
			{
				yield return new ValidationResult("Выбрана часть города без актуальных данных о координатах, кассе и складе. Сохранение невозможно.",
					new[] { nameof(GeographicGroups) });
			}

			var ignoreRouteListItemStatuses = new List<RouteListItemStatus> { RouteListItemStatus.Canceled, RouteListItemStatus.Transfered };

			var onlineOrders = Addresses
				.Where(x => !ignoreRouteListItemStatuses.Contains(x.Status) && x.Order.PaymentType != PaymentType.Terminal)
				.GroupBy(x => x.Order.OnlinePaymentNumber)
				.Where(g => g.Key != null && g.Count() > 1)
				.Select(o => o.Key);

			if(onlineOrders.Any())
			{
				yield return new ValidationResult($"В МЛ дублируются номера оплат: {string.Join(", ", onlineOrders)}", new[] { nameof(Addresses) });
			}

			if(ConfirmedDistance > ConfirmedDistanceLimit)
			{
				yield return new ValidationResult($"Подтверждённое расстояние не может быть больше {ConfirmedDistanceLimit}",
					new[] { nameof(ConfirmedDistance) });
			}

			var banStatuses = new[]
			{
				RouteListStatus.EnRoute,
				RouteListStatus.Delivered,
				RouteListStatus.Closed,
				RouteListStatus.OnClosing,
				RouteListStatus.MileageCheck
			};
			
			if(routeList != null
			   && banStatuses.Contains(routeList.Status)
			   && ObservableAddresses.Count == 0
			   && !canSaveRouteListWithoutOrders)
			{
				yield return new ValidationResult($"В маршрутном листе нет заказов. Добавьте заказы для подтверждения",
					new[] { nameof(ObservableAddresses) });
			}
		}

		public static string ValidationKeyIgnoreReceiptsForOrders => nameof(ValidationKeyIgnoreReceiptsForOrders);

		#endregion

		#region Функции относящиеся к закрытию МЛ

		/// <summary>
		/// Проверка по установленным вариантам расчета зарплаты, должен ли водитель на данном автомобилей проходить проверку километража
		/// </summary>
		public virtual bool NeedMileageCheckByWage {
			get {
				if(GetCarVersion.CarOwnType == CarOwnType.Company) {
					return true;
				}
				var actualWageParameter = Driver.GetActualWageParameter(Date);
				return actualWageParameter == null || actualWageParameter.WageParameterItem.WageParameterItemType != WageParameterItemTypes.RatesLevel;
			}
		}

		//FIXME потом метод скрыть. Должен вызываться только при переходе в статус на закрытии.
		public virtual void FirstFillClosing(IWageParameterService wageParameterService)
		{
			if(wageParameterService == null) {
				throw new ArgumentNullException(nameof(wageParameterService));
			}

			PerformanceHelper.StartMeasurement("Первоначальное заполнение");
			var addresesDelivered = Addresses.Where(x => x.Status != RouteListItemStatus.Transfered);
			foreach(var routeListItem in addresesDelivered) {
				PerformanceHelper.StartPointsGroup($"Заказ {routeListItem.Order.Id}");

				logger.Debug("Количество элементов в заказе {0}", routeListItem.Order.OrderItems.Count);
				routeListItem.FirstFillClosing(wageParameterService);
				PerformanceHelper.EndPointsGroup();
			}

			PerformanceHelper.AddTimePoint("Закончили");
			PerformanceHelper.Main.PrintAllPoints(logger);
			ClosingFilled = true;
		}

		public virtual void UpdateBottlesMovementOperation()
		{
			foreach(RouteListItem address in addresses.Where(x => x.Status != RouteListItemStatus.Transfered))
				address.Order.UpdateBottleMovementOperation(UoW, _nomenclatureSettings, returnByStock: address.BottlesReturned);
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

				operations?.ForEach(x => result.Add(x));
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

		public virtual string[] ManualCashOperations(
			ref Income cashIncome, ref Expense cashExpense, decimal casheInput, IFinancialCategoriesGroupsSettings financialCategoriesGroupsSettings)
		{
			var messages = new List<string>();

			if(Cashier?.Subdivision == null) {
				messages.Add("Создающий кассовые документы пользователь - не привязан к сотруднику!");
				return messages.ToArray();
			}

			if(casheInput > 0) {
				cashIncome = new Income {
					IncomeCategoryId = financialCategoriesGroupsSettings.RouteListClosingFinancialIncomeCategoryId,
					TypeOperation = IncomeType.DriverReport,
					Date = DateTime.Now,
					Casher = this.Cashier,
					Employee = Driver,
					Description = $"Дополнение к МЛ №{this.Id} от {Date:d}",
					Money = Math.Round(casheInput, 0, MidpointRounding.AwayFromZero),
					RouteListClosing = this,
					RelatedToSubdivision = Cashier.Subdivision
				};

				messages.Add($"Создан приходный ордер на сумму {cashIncome.Money:C0}");
				routeListCashOrganisationDistributor.DistributeIncomeCash(UoW, this, cashIncome, cashIncome.Money);
			} else {
				cashExpense = new Expense {
					ExpenseCategoryId = financialCategoriesGroupsSettings.RouteListClosingFinancialExpenseCategoryId,
					TypeOperation = ExpenseType.Expense,
					Date = DateTime.Now,
					Casher = this.Cashier,
					Employee = Driver,
					Description = $"Дополнение к МЛ #{this.Id} от {Date:d}",
					Money = Math.Round(-casheInput, 0, MidpointRounding.AwayFromZero),
					RouteListClosing = this,
					RelatedToSubdivision = Cashier.Subdivision
				};
				messages.Add($"Создан расходный ордер на сумму {cashExpense.Money:C0}");
				routeListCashOrganisationDistributor.DistributeExpenseCash(UoW, this, cashExpense, cashExpense.Money);
			}
			IsManualAccounting = true;
			return messages.ToArray();
		}

		public virtual string EmployeeAdvanceOperation(ref Expense cashExpense, decimal cashInput, IFinancialCategoriesGroupsSettings financialCategoriesGroupsSettings)
		{
			string message;
			if(Cashier?.Subdivision == null)
				return "Создающий кассовый документ пользователь - не привязан к сотруднику!";

			cashExpense = new Expense {
				ExpenseCategoryId = financialCategoriesGroupsSettings.EmployeeSalaryFinancialExpenseCategoryId,
				TypeOperation = ExpenseType.EmployeeAdvance,
				Date = DateTime.Now,
				Casher = this.Cashier,
				Employee = Driver,
				Organisation = _organizationRepository.GetCommonOrganisation(UoW),
				Description = $"Выдача аванса к МЛ #{this.Id} от {Date:d}",
				Money = Math.Round(cashInput, 0, MidpointRounding.AwayFromZero),
				RouteListClosing = this,
				RelatedToSubdivision = Cashier.Subdivision
			};

			message = $"Создан расходный ордер на сумму {cashExpense.Money:C0}";
			expenseCashOrganisationDistributor.DistributeCashForExpense(UoW, cashExpense, true);
			return (message);
		}

		public virtual bool TryValidateFuelOperation(IValidator validator)
		{
			if(FuelOutlayedOperation != null)
			{
				var fuelValidationContext =
					new ValidationContext(
						FuelOutlayedOperation,
						new Dictionary<object, object>
						{
							{ FuelOperation.DialogMessage, $"Неверный разнос километража в МЛ {Id}"},
						});
				
				if(!validator.Validate(FuelOutlayedOperation, fuelValidationContext))
				{
					return false;
				}
			}

			return true;
		}

		public virtual void UpdateFuelOperation()
		{
			//Необходимо для того что бы случайно не пересчитать операцию расхода топлива. После массовой смены расхода.
			if(FuelOutlayedOperation != null && Date < new DateTime(2017, 6, 6)) {
				return;
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

				FuelOutlayedOperation.Driver = GetCarVersion.CarOwnType == CarOwnType.Company ? null : Driver;
				FuelOutlayedOperation.Car = GetCarVersion.CarOwnType == CarOwnType.Company ? Car : null;
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
					Driver = Driver,
					Car = Car,
					Fuel = Car.FuelType
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

		public virtual void UpdateDeliveryDocuments(IUnitOfWork uow)
		{
			var controller =
				new RouteListClosingDocumentsController(
					_nomenclatureSettings, _employeeRepository, _routeListRepository);
			controller.UpdateDocuments(this, uow);
		}

		private decimal CalculateRouteListDebt()
		{
			decimal routeListDebt = 0;
			if(Id > 0)
			{
				using(var uow = _uowFactory.CreateWithoutRoot())
				{
					var totalCachAmount = DeliveredRouteListAddresses.Sum(item => item.TotalCash) - PhoneSum;
					var routeListCashAdvance = _cashRepository.GetRouteListCashExpensesSum(uow, Id);
					var routeListCashReturn = _cashRepository.GetRouteListCashReturnSum(uow, Id);
					var routeListRevenue = _cashRepository.CurrentRouteListCash(uow, Id);
					var routeListAdvances = _cashRepository.GetRouteListAdvancsReportsSum(uow, Id);

					routeListDebt = totalCachAmount + routeListCashAdvance - routeListCashReturn - routeListRevenue - routeListAdvances;
				}
			}

			return routeListDebt;
		}

		public virtual void UpdateRouteListDebt()
		{
			if(Id == 0)
			{
				return;
			}

			var debt = CalculateRouteListDebt();
			RouteListDebt routeListDebt = null;

			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				routeListDebt = uow.GetAll<RouteListDebt>()
					.Where(r => r.RouteList.Id == Id)
					.FirstOrDefault();

				if(routeListDebt?.Debt == debt)
				{
					return;
				}

				if(routeListDebt == null)
				{
					routeListDebt = new RouteListDebt { RouteList = this, Debt = debt };
				}
				else
				{
					routeListDebt.Debt = debt;
				}

				try
				{
					logger.Info($"Создание записи суммы долга для МЛ");

					uow.Save(routeListDebt);
					uow.Commit();

					logger.Info($"OK");
				}
				catch(Exception ex)
				{
					logger.Error(ex, "Ошибка при сохранении значения долга по МЛ");
					throw new Exception("Ошибка при выполнении сохранения долга по МЛ", ex);
				}
			}
		}

		public virtual decimal RouteListDebt => GetRouteListDebt();

		private decimal GetRouteListDebt()
		{
			UpdateRouteListDebt();

			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var routeListDebt = uow.GetAll<RouteListDebt>()
					.Where(r => r.RouteList.Id == Id)
					.Select(r => r.Debt)
					.FirstOrDefault();

				return routeListDebt;
			}
		}

		#endregion

		public virtual ReportInfo OrderOfAddressesRep(int id)
		{
			var reportInfofactory = ScopeProvider.Scope.Resolve<IReportInfoFactory>();
			var reportInfo = reportInfofactory.Create();
			reportInfo.Title = String.Format("Отчёт по порядку адресов в МЛ №{0}", id);
			reportInfo.Identifier = "Logistic.OrderOfAddresses";
			reportInfo.Parameters = new Dictionary<string, object> {
				{ "RouteListId",  id }
			};
			return reportInfo;
		}

		public virtual IEnumerable<string> UpdateCashOperations(IFinancialCategoriesGroupsSettings financialCategoriesGroupsSettings)
		{
			var messages = new List<string>();
			//Закрываем наличку.
			Income cashIncome = null;
			Expense cashExpense = null;

			var currentRouteListCash = _cashRepository.CurrentRouteListCash(UoW, this.Id);
			var different = Total - currentRouteListCash;
			if(different == 0M) {
				return messages.ToArray();
			}

			if(Cashier?.Subdivision == null) {
				messages.Add("Создающий кассовые документы пользователь - не привязан к сотруднику!");
				return messages.ToArray();
			}

			if(different > 0) {
				cashIncome = new Income {
					IncomeCategoryId = financialCategoriesGroupsSettings.RouteListClosingFinancialIncomeCategoryId,
					TypeOperation = IncomeType.DriverReport,
					Date = DateTime.Now,
					Casher = this.Cashier,
					Employee = Driver,
					Description = $"Закрытие МЛ №{Id} от {Date:d}",
					Money = Math.Round(different, 2, MidpointRounding.AwayFromZero),
					RouteListClosing = this,
					RelatedToSubdivision = Cashier.Subdivision
				};
				
				messages.Add($"Создан приходный ордер на сумму {cashIncome.Money}");
				routeListCashOrganisationDistributor.DistributeIncomeCash(UoW, this, cashIncome, cashIncome.Money);
			} else {
				cashExpense = new Expense {
					ExpenseCategoryId = financialCategoriesGroupsSettings.RouteListClosingFinancialExpenseCategoryId,
					TypeOperation = ExpenseType.Expense,
					Date = DateTime.Now,
					Casher = this.Cashier,
					Employee = Driver,
					Description = $"Закрытие МЛ #{Id} от {Date:d}",
					Money = Math.Round(-different, 2, MidpointRounding.AwayFromZero),
					RouteListClosing = this,
					RelatedToSubdivision = Cashier.Subdivision
				};
				
				messages.Add($"Создан расходный ордер на сумму {cashExpense.Money}");
				routeListCashOrganisationDistributor.DistributeExpenseCash(UoW, this, cashExpense, cashExpense.Money);
			}

			// Если хотя бы один fuelDocument имеет PayedForFuel то добавить пустую строку разделитель и сообщения о расходных ордерах топлива
			bool wasEmptyLineAdded = false;
			foreach(var fuelDocument in fuelDocuments) {
				if (fuelDocument.PayedForFuel != null && fuelDocument.PayedForFuel != 0 && fuelDocument.FuelPaymentType == FuelPaymentType.Cash) {
					if(!wasEmptyLineAdded) {
						messages.Add("\n");
						wasEmptyLineAdded = true;
					}
					messages.Add($"Создан расходный ордер топлива на сумму {fuelDocument.PayedForFuel}");
				}
			}

			if (cashIncome != null) UoW.Save(cashIncome);
			if (cashExpense != null) UoW.Save(cashExpense);
			
			return messages;
		}

		public virtual IEnumerable<string> UpdateMovementOperations(IFinancialCategoriesGroupsSettings financialCategoriesGroupsSettings)
		{
			var result = UpdateCashOperations(financialCategoriesGroupsSettings);
			UpdateOperations();
			return result;
		}

		public virtual void UpdateOperations()
		{
			this.UpdateFuelOperation();

			var counterpartyMovementOperations = this.UpdateCounterpartyMovementOperations();
			var moneyMovementOperations = this.UpdateMoneyMovementOperations();
			var depositsOperations = this.UpdateDepositOperations(UoW);

			counterpartyMovementOperations.ForEach(op => UoW.Save(op));
			UpdateBottlesMovementOperation();
			depositsOperations.ForEach(op => UoW.Save(op));
			moneyMovementOperations.ForEach(op => UoW.Save(op));

			UpdateWageOperation();
			var premiumRaskatSettings = ScopeProvider.Scope.Resolve<IPremiumRaskatGAZelleSettings>();
			var wageSettings = ScopeProvider.Scope.Resolve<IWageSettings>();
			var premiumRaskatGAZelleWageModel = new PremiumRaskatGAZelleWageModel(_employeeRepository, wageSettings,
				premiumRaskatSettings, this);

			// Пока отключено по просьбе Маслякова А.Д., https://vod.myalm.ru/pm/Vodovoz/I-5083
			// premiumRaskatGAZelleWageModel.UpdatePremiumRaskatGAZelle(UoW);
		}

		#region Для логистических расчетов

		public virtual TimeSpan? FirstAddressTime {
			get {
				return Addresses.FirstOrDefault()?.Order.DeliverySchedule.From;
			}
		}

		public virtual void RecalculatePlanTime(RouteGeometryCalculator sputnikCache)
		{
			TimeSpan minTime = new TimeSpan();
			//Расчет минимального времени к которому нужно\можно подъехать.
			for(int ix = 0; ix < Addresses.Count; ix++)
			{
				if(ix == 0)
				{
					minTime = Addresses[ix].Order.DeliverySchedule.From;

					var geoGroup = GeographicGroups.FirstOrDefault();
					var geoGroupVersion = geoGroup.GetVersionOrNull(Date);
					if(geoGroupVersion == null)
					{
						throw new GeoGroupVersionNotFoundException($"Невозможно рассчитать планируемое время, так как на {Date} у части города нет актуальных данных.");
					}

					var timeFromBase = TimeSpan.FromSeconds(
						sputnikCache.TimeFromBase(
							geoGroupVersion.PointCoordinates,
							Addresses[ix].Order.DeliveryPoint.PointCoordinates));
					
					var onBase = minTime - timeFromBase;
					
					if(Shift != null && onBase < Shift.StartTime)
					{
						minTime = Shift.StartTime + timeFromBase;
					}
				}
				else
				{
					minTime += TimeSpan.FromSeconds(
						sputnikCache.TimeSec(
							Addresses[ix - 1].Order.DeliveryPoint.PointCoordinates,
							Addresses[ix].Order.DeliveryPoint.PointCoordinates));
				}

				Addresses[ix].PlanTimeStart = minTime > Addresses[ix].Order.DeliverySchedule.From ? minTime : Addresses[ix].Order.DeliverySchedule.From;

				minTime += TimeSpan.FromSeconds(Addresses[ix].TimeOnPoint);
			}
			//Расчет максимального времени до которого нужно подъехать.
			TimeSpan maxTime = new TimeSpan();
			for(int ix = Addresses.Count - 1; ix >= 0; ix--) {

				if(ix == Addresses.Count - 1)
				{
					maxTime = Addresses[ix].Order.DeliverySchedule.To;

					var geoGroup = GeographicGroups.FirstOrDefault();
					var geoGroupVersion = geoGroup.GetVersionOrNull(Date);
					if(geoGroupVersion == null)
					{
						throw new GeoGroupVersionNotFoundException(
							$"Невозможно рассчитать планируемое время, так как на {Date} у части города нет актуальных данных.");
					}

					var timeToBase = TimeSpan.FromSeconds(
						sputnikCache.TimeToBase(
							Addresses[ix].Order.DeliveryPoint.PointCoordinates,
							geoGroupVersion.PointCoordinates));
					
					var onBase = maxTime + timeToBase;
					
					if(Shift != null && onBase > Shift.EndTime)
					{
						maxTime = Shift.EndTime - timeToBase;
					}
				}
				else
				{
					maxTime -= TimeSpan.FromSeconds(
						sputnikCache.TimeSec(
							Addresses[ix].Order.DeliveryPoint.PointCoordinates,
							Addresses[ix + 1].Order.DeliveryPoint.PointCoordinates));
				}

				if(maxTime > Addresses[ix].Order.DeliverySchedule.To)
				{
					maxTime = Addresses[ix].Order.DeliverySchedule.To;
				}

				maxTime -= TimeSpan.FromSeconds(Addresses[ix].TimeOnPoint);

				if(maxTime < Addresses[ix].PlanTimeStart)
				{
					//Расписание испорчено, успеть нельзя. Пытаемся его более менее адекватно отобразить.
					TimeSpan beforeMin = new TimeSpan(1, 0, 0, 0);
					if(ix > 0)
					{
						beforeMin = Addresses[ix - 1].PlanTimeStart.Value
									+ TimeSpan.FromSeconds(sputnikCache.TimeSec(Addresses[ix - 1].Order.DeliveryPoint.PointCoordinates,
										Addresses[ix].Order.DeliveryPoint.PointCoordinates))
									+ TimeSpan.FromSeconds(Addresses[ix - 1].TimeOnPoint);
					}

					if(beforeMin < Addresses[ix].Order.DeliverySchedule.From)
					{
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
				PlanedDistance = distanceCalculator.GetRouteDistance(GenerateHashPointsOfRoute()) / 1000m;
		}

		public static void RecalculateOnLoadTime(IList<RouteList> routelists, RouteGeometryCalculator sputnikCache)
		{
			

			var sorted =
				routelists
					.Where(x => x.Addresses.Any() && !x.OnloadTimeFixed)
					.Select(
						rl => {
							var geoGroup = rl.GeographicGroups.FirstOrDefault();
							var geoGroupVersion = geoGroup.GetVersionOrNull(rl.Date);
							if(geoGroupVersion == null)
							{
								throw new GeoGroupVersionNotFoundException($"Невозможно рассчитать время на погрузке, так как на {rl.Date} у части города ({geoGroup.Name}) нет актуальных данных.");
							}

							var time =
								rl.FirstAddressTime.Value - TimeSpan.FromSeconds(
									sputnikCache.TimeFromBase(
										geoGroupVersion.PointCoordinates,
										rl.Addresses.First().Order.DeliveryPoint.PointCoordinates));
							
							return new Tuple<TimeSpan, RouteList>(time, rl);
						}
					)
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

		public virtual long TimeOnLoadMinuts =>
			GetTimeOnLoadMinuts();

		private int GetTimeOnLoadMinuts()
		{
			var defaultTimeOnLoad = 30;
			var companyLargusTimeOnLoad = 15;
			var companyMinivanTimeOnLoad = 20;

			if(GetCarVersion.CarOwnType == CarOwnType.Company)
			{
				if(Car.CarModel.CarTypeOfUse == CarTypeOfUse.Largus)
				{
					return companyLargusTimeOnLoad;
				}

				if(Car.CarModel.CarTypeOfUse == CarTypeOfUse.Minivan)
				{
					return companyMinivanTimeOnLoad;
				}
			}

			return defaultTimeOnLoad;
		}

		public virtual long[] GenerateHashPointsOfRoute()
		{
			var geoGroup = GeographicGroups.FirstOrDefault();
			var geoGroupVersion = geoGroup.GetVersionOrNull(Date);
			if(geoGroupVersion == null)
			{
				throw new GeoGroupVersionNotFoundException($"Невозможно построить трек, так как на {Date} у части города ({geoGroup.Name}) нет актуальных данных.");
			}

			var hash = CachedDistance.GetHash(geoGroupVersion.PointCoordinates);
			var result = new List<long>();
			result.Add(hash);
			result.AddRange(
				Addresses.Where(x => x.Order.DeliveryPoint.CoordinatesExist)
					.Select(x => CachedDistance.GetHash(x.Order.DeliveryPoint.PointCoordinates)));
			result.Add(hash);
			return result.ToArray();
		}

		#region Вес

		/// <summary>
		/// Полный вес товаров и оборудования в маршрутном листе
		/// </summary>
		/// <returns>Вес в килограммах</returns>
		public virtual decimal GetTotalWeight()
		{
			var ordersWeight = Addresses
				.Where(item => item.Status != RouteListItemStatus.Transfered)
				.Sum(item => item.Order.FullWeight());

			var additionalLoadingWeight = AdditionalLoadingDocument?.Items.Sum(x => x.Nomenclature.Weight * x.Amount) ?? 0;
			return Math.Round(ordersWeight + additionalLoadingWeight, 3);
		}

		/// <summary>
		/// Полный вес продаваемых товаров в маршрутном листе
		/// </summary>
		/// <returns>Вес в килограммах</returns>
		public virtual decimal GetTotalSalesGoodsWeight()
		{
			var ordersWeight = Addresses
				.Where(item => item.Status != RouteListItemStatus.Transfered)
				.Sum(item => item.Order.GetSalesItemsWeight());
			return Math.Round(ordersWeight, 3);
		}

		/// <summary>
		/// Проверка на перегруз автомобиля
		/// </summary>
		/// <returns><c>true</c>, если автомобиль "Ларгус" или "раскат" и имеется его перегруз, <c>false</c> в остальных случаях.</returns>
		public virtual bool HasOverweight()
		{
			var carVersion = GetCarVersion;
			return Car != null
				&& (carVersion.CarOwnType == CarOwnType.Raskat
					|| carVersion.CarOwnType == CarOwnType.Company && Car.CarModel.CarTypeOfUse == CarTypeOfUse.Largus
					|| carVersion.CarOwnType == CarOwnType.Company && Car.CarModel.CarTypeOfUse == CarTypeOfUse.Minivan
					|| carVersion.CarOwnType == CarOwnType.Company && Car.CarModel.CarTypeOfUse == CarTypeOfUse.GAZelle)
				&& Car.CarModel.MaxWeight < GetTotalWeight();
		}

		/// <summary>
		/// Перегруз в килограммах
		/// </summary>
		/// <returns>Возрат значения перегруза в килограммах.</returns>
		public virtual decimal Overweight() => HasOverweight() ? Math.Round(GetTotalWeight() - Car.CarModel.MaxWeight, 2) : 0;
		#endregion Вес

		#region Объём
		/// <summary>
		/// Полный объём товаров и оборудования в маршрутном листе
		/// </summary>
		/// <returns>Объём в кубических метрах</returns>
		public virtual decimal GetTotalVolume() =>
			Addresses.Where(item => item.Status != RouteListItemStatus.Transfered).Sum(item => item.Order.FullVolume())
			+ (AdditionalLoadingDocument?.Items.Sum(x => (decimal)x.Nomenclature.Volume * x.Amount) ?? 0);

		/// <summary>
		/// Проверка на превышение объёма груза автомобиля
		/// </summary>
		/// <returns><c>true</c>, если имеется превышение объёма, <c>false</c> в остальных случаях.</returns>
		public virtual bool HasVolumeExecess() => Car.CarModel.MaxVolume < GetTotalVolume();

		/// <summary>
		/// Величина, на оторую превышен объём груза
		/// </summary>
		/// <returns>Возрат значения превышения объёма груза в метрах кубических.</returns>
		public virtual decimal VolumeExecess() => HasVolumeExecess() ? Math.Round(GetTotalVolume() - Car.CarModel.MaxVolume, 3) : 0;

		/// <summary>
		/// Полный объём ВОЗВРАЩАЕМОГО оборудования в маршрутном листе
		/// </summary>
		/// <returns>Объём в кубических метрах</returns>
		public virtual decimal GetTotalReverseVolume() =>
			Addresses.Where(item => item.Status != RouteListItemStatus.Transfered).Sum(item => item.Order.FullReverseVolume())
			+ (AdditionalLoadingDocument?.Items.Sum(x => (decimal)x.Nomenclature.Volume * x.Amount) ?? 0);

		/// <summary>
		/// Проверка на превышение объёма ВОЗВРАЩАЕМОГО груза автомобиля
		/// </summary>
		/// <returns><c>true</c>, если имеется превышение объёма ВОЗВРАЩАЕМОГО груза, <c>false</c> в остальных случаях.</returns>
		public virtual bool HasReverseVolumeExcess() => Car.CarModel.MaxVolume < GetTotalReverseVolume();

		/// <summary>
		/// Величина, на оторую превышен объём груза
		/// </summary>
		/// <returns>Возрат значения превышения объёма ВОЗВРАЩАЕМОГО груза в метрах кубических.</returns>
		public virtual decimal ReverseVolumeExecess() => HasReverseVolumeExcess() ? Math.Round(GetTotalReverseVolume() - Car.CarModel.MaxVolume, 3) : 0;

		#endregion Объём

		/// <summary>
		/// Нода с номенклатурами и различными количествами после погрузки МЛ на складе
		/// </summary>
		public virtual List<RouteListControlNotLoadedNode> NotLoadedNomenclatures(bool needTerminalAccounting, int? terminalId = null)
		{
			List<RouteListControlNotLoadedNode> notLoadedNomenclatures = new List<RouteListControlNotLoadedNode>();
			if(Id > 0) {
				var loadedNomenclatures = _routeListRepository.AllGoodsLoaded(UoW, this);
				var nomenclaturesToLoad = _routeListRepository.GetGoodsAndEquipsInRL(UoW, this);
				var hasSelfDriverTerminalTransferDocument = _routeListRepository.GetSelfDriverTerminalTransferDocument(UoW, this.Driver, this) != null;

				foreach(var n in nomenclaturesToLoad) {

					if(n.NomenclatureId == terminalId && (!needTerminalAccounting || hasSelfDriverTerminalTransferDocument))
					{
						continue;
					}

					var loaded = loadedNomenclatures.FirstOrDefault(x => x.NomenclatureId == n.NomenclatureId);
					decimal loadedAmount = 0;
					if(loaded != null)
					{
						loadedAmount = loaded.Amount;
					}
					if(loadedAmount < n.Amount) {
						notLoadedNomenclatures.Add(new RouteListControlNotLoadedNode {
							NomenclatureId = n.NomenclatureId,
							CountTotal = n.Amount,
							CountNotLoaded = (n.Amount - loadedAmount)
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

		public virtual bool RecountMileage()
		{
			var pointsToRecalculate = new List<PointOnEarth>();
			var pointsToBase = new List<PointOnEarth>();
			var geoGroup = GeographicGroups.FirstOrDefault();
			if(geoGroup == null)
			{
				throw new InvalidOperationException($"В маршрутном листе должна быть добавлена часть города");
			}

			var geoGroupVersion = geoGroup.GetActualVersionOrNull();
			if(geoGroupVersion == null)
			{
				throw new InvalidOperationException($"Не установлена активная версия данных в части города {geoGroup.Name}");
			}

			var baseLat = (double)geoGroupVersion.BaseLatitude.Value;
			var baseLon = (double)geoGroupVersion.BaseLongitude.Value;

			decimal totalDistanceTrack = 0;

			IEnumerable<RouteListItem> completedAddresses =
				Addresses.Where(x => x.Status == RouteListItemStatus.Completed).ToList();

			if(!completedAddresses.Any())
			{
				ServicesConfig.InteractiveService.ShowMessage(ImportanceLevel.Warning, "Для МЛ нет завершенных адресов, невозможно расчитать трек", "");
				return true;
			}

			if(completedAddresses.Count() > 1)
			{
				foreach(RouteListItem address in Addresses.OrderBy(x => x.StatusLastUpdate))
				{
					if(address.Status == RouteListItemStatus.Completed)
					{
						var deliveryPoint = address.Order.DeliveryPoint;

						if(!deliveryPoint.CoordinatesExist)
						{
							ServicesConfig.InteractiveService.ShowMessage(
								ImportanceLevel.Error,
								$"В точке доставки №{deliveryPoint.Id} {deliveryPoint.ShortAddress} необходимо указать координаты",
								"У точки доставки не указаны координаты");

							return false;
						}
						
						pointsToRecalculate.Add(new PointOnEarth((double)deliveryPoint.Latitude, (double)deliveryPoint.Longitude));
					}
				}

				var recalculatedTrackResponse = _osrmClient.GetRoute(pointsToRecalculate, false, GeometryOverview.Full, _osrmSettings.ExcludeToll);

				if(recalculatedTrackResponse.Routes is null)
				{
					recalculatedTrackResponse = _osrmClient.GetRoute(pointsToRecalculate, false, GeometryOverview.Full);
				}
				
				var recalculatedTrack = recalculatedTrackResponse.Routes.First();

				totalDistanceTrack = recalculatedTrack.TotalDistanceKm;
			}
			else
			{
				var point = Addresses.First(x => x.Status == RouteListItemStatus.Completed).Order.DeliveryPoint;
				
				if(!point.CoordinatesExist)
				{
					ServicesConfig.InteractiveService.ShowMessage(
						ImportanceLevel.Error,
						$"В точке доставки №{point.Id} {point.ShortAddress} необходимо указать координаты",
						"У точки доставки не указаны координаты");
					
					return false;
				}
				
				pointsToRecalculate.Add(new PointOnEarth((double)point.Latitude, (double)point.Longitude));
			}

			pointsToBase.Add(pointsToRecalculate.Last());
			pointsToBase.Add(new PointOnEarth(baseLat, baseLon));
			pointsToBase.Add(pointsToRecalculate.First());

			var recalculatedToBaseResponse = _osrmClient.GetRoute(pointsToBase, false, GeometryOverview.Full, _osrmSettings.ExcludeToll);
			var recalculatedToBase = recalculatedToBaseResponse.Routes.First();

			RecalculatedDistance = decimal.Round(totalDistanceTrack + recalculatedToBase.TotalDistanceKm);
			return true;
		}

		#endregion

		#region Зарплата

		private IRouteListWageCalculationService GetDriverWageCalculationService(IWageParameterService wageParameterService)
		{
			if(wageParameterService == null) {
				throw new ArgumentNullException(nameof(wageParameterService));
			}

			return wageParameterService.ActualizeWageParameterAndGetCalculationService(UoW, Driver, DriverWageCalculationSrc);
		}

		private IRouteListWageCalculationService GetForwarderWageCalculationService(IWageParameterService wageParameterService)
		{
			if(wageParameterService == null) {
				throw new ArgumentNullException(nameof(wageParameterService));
			}

			if(Forwarder == null) {
				return null;
			}

			if(Forwarder != null && Forwarder.GetActualWageParameter(Date) == null)
			{
				var exceptionMessage = $"Нет данных о параметрах расчета зарплаты экспедитора id={Forwarder.Id} на выбранную дату доставки {Date:dd.MM.yyyy}.";
				Forwarder = null;
				throw new InvalidOperationException(exceptionMessage);
			}

			return wageParameterService.ActualizeWageParameterAndGetCalculationService(UoW, Forwarder, ForwarderWageCalculationSrc);
		}


		/// <summary>
		/// Возвращает пересчитанную заново зарплату водителя (не записывает)
		/// </summary>
		public virtual decimal GetRecalculatedDriverWage(IWageParameterService wageParameterService)
		{
			var routeListWageCalculationService = GetDriverWageCalculationService(wageParameterService);
			var wageResult = routeListWageCalculationService.CalculateWage();
			return wageResult.Wage;
		}

		/// <summary>
		/// Возвращает пересчитанную заного зарплату экспедитора (не записывает)
		/// </summary>
		public virtual decimal GetRecalculatedForwarderWage(IWageParameterService wageParameterService)
		{
			if(wageParameterService == null) {
				throw new ArgumentNullException(nameof(wageParameterService));
			}

			if(Forwarder == null) {
				return 0;
			}

			var routeListWageCalculationService = GetForwarderWageCalculationService(wageParameterService);
			var wageResult = routeListWageCalculationService.CalculateWage();
			return wageResult.Wage;
		}

		/// <summary>
		/// Возвращает текущую зарплату водителя
		/// </summary>
		public virtual decimal GetDriversTotalWage()
		{
			if(FixedDriverWage > 0) {
				//Если все заказы не выполнены, то нет зарплаты
				return DriverWageCalculationSrc.HasAnyCompletedAddress ? FixedDriverWage : 0;
			}
			return Addresses.Sum(item => (item.DriverWage + item.DriverWageSurcharge));
		}

		/// <summary>
		/// Возвращает текущую зарплату экспедитора
		/// </summary>
		public virtual decimal GetForwardersTotalWage()
		{
			if(Forwarder == null)
				return 0;
			if(FixedForwarderWage > 0)
				//Если все заказы не выполнены, то нет зарплаты
				return ForwarderWageCalculationSrc.HasAnyCompletedAddress ? FixedForwarderWage : 0;
			return Addresses.Sum(item => item.ForwarderWage);
		}

		public virtual void RecalculateWagesForRouteListItem(RouteListItem address, IWageParameterService wageParameterService)
		{
			if(!Addresses.Contains(address)) {
				throw new InvalidOperationException("Расчет зарплаты возможен только для адресов текущего маршрутного листа.");
			}

			var routeListDriverWageCalculationService = GetDriverWageCalculationService(wageParameterService);
			var drvWageResult = routeListDriverWageCalculationService.CalculateWageForRouteListItem(address.DriverWageCalculationSrc);
			address.DriverWage = drvWageResult.Wage;
			address.DriverWageCalcMethodicTemporaryStore = drvWageResult.WageDistrictLevelRate;
			if(Forwarder != null) {
				var routeListForwarderWageCalculationService = GetForwarderWageCalculationService(wageParameterService);
				var fwdWageResult = routeListForwarderWageCalculationService.CalculateWageForRouteListItem(address.ForwarderWageCalculationSrc);
				address.ForwarderWage = fwdWageResult.Wage;
				address.ForwarderWageCalcMethodicTemporaryStore = fwdWageResult.WageDistrictLevelRate;
			}
		}

		/// <summary>
		/// Расчитывает и записывает зарплату
		/// </summary>
		public virtual void CalculateWages(IWageParameterService wageParameterService)
		{
			if(wageParameterService == null) {
				throw new ArgumentNullException(nameof(wageParameterService));
			}

			if (Status == RouteListStatus.New)
			{
				ClearWages();
				return;
			}

			IRouteListWageCalculationService routeListDriverWageCalculationService = null;
			if(Driver != null)
			{
				routeListDriverWageCalculationService = GetDriverWageCalculationService(wageParameterService);
				FixedDriverWage = routeListDriverWageCalculationService.CalculateWage().FixedWage;
			}

			IRouteListWageCalculationService routeListForwarderWageCalculationService = null;
			if(Driver != null && Forwarder != null) 
			{
				routeListForwarderWageCalculationService = GetForwarderWageCalculationService(wageParameterService);
				FixedForwarderWage = routeListForwarderWageCalculationService.CalculateWage().FixedWage;
			}

			foreach(var address in Addresses) 
			{
				if(routeListDriverWageCalculationService != null)
				{
					var drvWageResult = routeListDriverWageCalculationService.CalculateWageForRouteListItem(address.DriverWageCalculationSrc);
					address.DriverWage = drvWageResult.Wage;
					address.DriverWageCalcMethodicTemporaryStore = drvWageResult.WageDistrictLevelRate;
					address.IsDriverForeignDistrict = address.DriverWageCalculationSrc.IsDriverForeignDistrict;
				}
				
				if(routeListForwarderWageCalculationService != null)
				{
					var fwdWageResult = routeListForwarderWageCalculationService.CalculateWageForRouteListItem(address.ForwarderWageCalculationSrc);
					address.ForwarderWage = fwdWageResult.Wage;
					address.ForwarderWageCalcMethodicTemporaryStore = fwdWageResult.WageDistrictLevelRate;
				}
			}
		}

		/// <summary>
		/// Обнуляет зарплату в МЛ и его адресах
		/// </summary>
		public virtual void ClearWages()
		{
			FixedDriverWage = 0;
			FixedForwarderWage = 0;
			foreach(var address in Addresses)
			{
				address.DriverWage = 0;
				address.ForwarderWage = 0;
			}
		}

		public virtual void RecalculateAllWages(IWageParameterService wageParameterService)
		{
			if(wageParameterService == null) {
				throw new ArgumentNullException(nameof(wageParameterService));
			}

			if(GetCarVersion == null)
			{
				var exceptionMessage = $"Нет данных о версии автомобиля id={Car.Id} на выбранную дату доставки {Date:dd.MM.yyyy}.";
				Car = null;
				throw new InvalidOperationException(exceptionMessage);
			}

			if(Driver?.GetActualWageParameter(Date) == null)
			{
				var exceptionMessage = $"Нет данных о параметрах расчета зарплаты водителя id={Driver?.Id} на выбранную дату доставки {Date:dd.MM.yyyy}.";
				Driver = null;
				throw new InvalidOperationException(exceptionMessage);
			}

			CalculateWages(wageParameterService);
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

			foreach(var address in Addresses) {
				address.SaveWageCalculationMethodics();
			}
		}

		IRouteListWageCalculationSource DriverWageCalculationSrc => new RouteListWageCalculationSource(this, EmployeeCategory.driver);

		IRouteListWageCalculationSource ForwarderWageCalculationSrc => new RouteListWageCalculationSource(this, EmployeeCategory.forwarder);

		private string CreateWageCalculationDetailsTextForAddress(RouteListItemWageCalculationDetails addressWageDetails, RouteListItem address,
			EmployeeCategory employeeCategory, string carOwner, string wageDistrictName)
		{
			if(addressWageDetails == null)
			{
				return "";
			}

			string addressDetailsText =
				$"{ addressWageDetails.RouteListItemWageCalculationName } ({carOwner}, {wageDistrictName}), адрес №{ address.Id }, заказ №{ address.Order.Id }" +
				$", категория \"{ employeeCategory.GetEnumTitle() }\":\n";

			var wageRateTypeTitles = Enum.GetValues(typeof(WageRateTypes)).OfType<WageRateTypes>().Select(w => w.GetEnumTitle());

			addressDetailsText += string.Join("\n",
				addressWageDetails.WageCalculationDetailsList
					.Where(d => d.Count > 0 || !wageRateTypeTitles.Contains(d.Name))
					.Select(d =>
					{
						var s = $"- {d.Name}";
						if(wageRateTypeTitles.Contains(d.Name))
						{
							if(d.Name == WageRateTypes.PackOfBottles600ml.GetEnumTitle())
							{
								s += $" = { decimal.Round(d.Price, 2) } руб. * {d.Count} шт. = { Math.Truncate(d.Price * d.Count) } руб.";
							}
							else
							{
								s += $" = { decimal.Round(d.Price, 2) } руб. * {d.Count} шт. = { decimal.Round(d.Price * d.Count, 2) } руб.";
							}
						}

						return s;
					})
				);

			var adsressSum = addressWageDetails.WageCalculationDetailsList.Sum(d =>
			{
				return d.Name == WageRateTypes.PackOfBottles600ml.GetEnumTitle() ? Math.Truncate(d.Price * d.Count) : decimal.Round(d.Price * d.Count, 2);
			});

			addressDetailsText += $"\nИтого за адрес: { Math.Round(adsressSum, 2) } руб.\n\n";

			return addressDetailsText;
		}

		public virtual string GetWageCalculationDetails(IWageParameterService wageParameterService)
		{
			var routeListDriverWageCalculationService = GetDriverWageCalculationService(wageParameterService);
			var routeListForwarderWageCalculationService = GetForwarderWageCalculationService(wageParameterService);

			List<RouteListItemWageCalculationDetails> addressWageDetailsList = new List<RouteListItemWageCalculationDetails>();

			string resultTextDriver = "ЗП водителя:\n\n";
			string resultTextForwarder = "\n\nЗП экспедитора:\n\n";

			string carOwner = DriverWageCalculationSrc.CarTypeOfUse.GetEnumTitle();

			if(routeListDriverWageCalculationService is RouteListWageCalculationService service
			   && service.GetWageCalculationService is RouteListFixedWageCalculationService)
			{
				resultTextDriver +=  $"Расчёт ЗП с фиксированной суммой за МЛ ({ carOwner }) = { routeListDriverWageCalculationService.CalculateWage().FixedWage } руб.";
				return resultTextDriver;
			}

			foreach(var address in addresses)
			{
				var driverAddressWageDetails = routeListDriverWageCalculationService?
					.GetWageCalculationDetailsForRouteListItem(address.DriverWageCalculationSrc);
				if(driverAddressWageDetails != null)
				{
					addressWageDetailsList.Add(driverAddressWageDetails);
				}

				var forwarderAddressWageDetails = routeListForwarderWageCalculationService?
					.GetWageCalculationDetailsForRouteListItem(address.ForwarderWageCalculationSrc);
				if(forwarderAddressWageDetails != null)
				{
					addressWageDetailsList.Add(forwarderAddressWageDetails);
				}

				var addressWageDistrict = address.DriverWageCalculationSrc.WageDistrictOfAddress.Name;

				resultTextDriver += CreateWageCalculationDetailsTextForAddress(driverAddressWageDetails, address, EmployeeCategory.driver,
					carOwner, addressWageDistrict);
				resultTextForwarder += CreateWageCalculationDetailsTextForAddress(forwarderAddressWageDetails, address,
					EmployeeCategory.forwarder, carOwner, addressWageDistrict);
			}

			var routeListDriverWageSum = addressWageDetailsList.Where(w=>w.WageCalculationEmployeeCategory == EmployeeCategory.driver).Sum(a => a.WageCalculationDetailsList.Sum(d =>
			{
				return d.Name == WageRateTypes.PackOfBottles600ml.GetEnumTitle() ? Math.Truncate(d.Price * d.Count) : decimal.Round(d.Price * d.Count, 2);
			}));

			var routeListForwarderWageSum = addressWageDetailsList.Where(w => w.WageCalculationEmployeeCategory == EmployeeCategory.forwarder).Sum(a => a.WageCalculationDetailsList.Sum(d =>
			{
				return d.Name == WageRateTypes.PackOfBottles600ml.GetEnumTitle() ? Math.Truncate(d.Price * d.Count) : decimal.Round(d.Price * d.Count, 2);
			}));

			resultTextDriver += $"Итого ЗП водителя за МЛ: { routeListDriverWageSum } руб.";

			resultTextForwarder += $"Итого ЗП экспедитора за МЛ: { routeListForwarderWageSum } руб.";

			return $"{ resultTextDriver }\n\n{ resultTextForwarder }";
		}

		#endregion Зарплата

		public static RouteListStatus[] AvailableToSendEnRouteStatuses { get; } = { RouteListStatus.Confirmed, RouteListStatus.InLoading };

		public static RouteListStatus[] NotLoadedRouteListStatuses { get; } = { RouteListStatus.New, RouteListStatus.Confirmed, RouteListStatus.InLoading };

		public static RouteListStatus[] DeliveredRouteListStatuses { get; } = { RouteListStatus.Delivered, RouteListStatus.OnClosing, RouteListStatus.MileageCheck, RouteListStatus.Closed };
		
		public static RouteListStatus[] EnRouteAndDeliveredStatuses { get; } =
		{
			RouteListStatus.EnRoute,
			RouteListStatus.Delivered,
			RouteListStatus.OnClosing,
			RouteListStatus.MileageCheck,
			RouteListStatus.Closed
		};
	}
}

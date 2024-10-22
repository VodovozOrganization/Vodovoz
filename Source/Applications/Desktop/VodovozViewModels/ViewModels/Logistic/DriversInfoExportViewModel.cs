using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Gamma.Utilities;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.DB;
using QS.Tdi;
using QS.Utilities.Enums;
using QS.Utilities.Text;
using QS.ViewModels;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public sealed class DriversInfoExportViewModel : DialogTabViewModelBase, ITDICloseControlTab
	{
		private readonly IInteractiveService _interactiveService;
		private readonly IWageParameterService _wageParameterService;

		private EmployeeStatus? _employeeStatus;
		private DateTime? _endDate;
		private DateTime? _startDate;
		private IList<CarTypeOfUse> _restrictedCarTypesOfUse;
		private IList<CarOwnType> _restrictedCarOwnTypes;

		private string _statusMessage;
		private string _exportPath;
		private bool _dataIsLoading;
		private GenericObservableList<DriverInfoNode> _items;
		private DelegateCommand _exportCommand;
		private DelegateCommand _helpCommand;

		public DriversInfoExportViewModel(
			IWageParameterService wageParameterService,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_wageParameterService = wageParameterService ?? throw new ArgumentNullException(nameof(wageParameterService));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));

			_restrictedCarOwnTypes = EnumHelper.GetValuesList<CarOwnType>();
			_restrictedCarTypesOfUse = EnumHelper.GetValuesList<CarTypeOfUse>();
			_restrictedCarTypesOfUse.Remove(CarTypeOfUse.Truck);
			_restrictedCarTypesOfUse.Remove(CarTypeOfUse.Loader);

			TabName = "Выгрузка по водителям";
			Items = new GenericObservableList<DriverInfoNode>();

			DataIsLoading = false;
		}

		public GenericObservableList<DriverInfoNode> Items
		{
			get => _items;
			set
			{
				if(SetField(ref _items, value) && value != null)
				{
					SubscribeOnItemsListChanges();
					OnPropertyChanged(nameof(CanExport));
				}
			}
		}

		public bool CanExport => !DataIsLoading && Items.Any();
		public bool CanForm => !DataIsLoading;

		public IList<CarTypeOfUse> RestrictedCarTypesOfUse
		{
			get => _restrictedCarTypesOfUse;
			set => SetField(ref _restrictedCarTypesOfUse, value);
		}
		public IList<CarOwnType> RestrictedCarOwnTypes
		{
			get => _restrictedCarOwnTypes;
			set => SetField(ref _restrictedCarOwnTypes, value);
		}
		public DateTime? StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}
		public DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}
		public EmployeeStatus? EmployeeStatus
		{
			get => _employeeStatus;
			set => SetField(ref _employeeStatus, value);
		}
		public bool DataIsLoading
		{
			get => _dataIsLoading;
			set
			{
				if(SetField(ref _dataIsLoading, value))
				{
					OnPropertyChanged(nameof(CanExport));
					OnPropertyChanged(nameof(CanForm));
				}
			}
		}
		public string StatusMessage
		{
			get => _statusMessage;
			set => SetField(ref _statusMessage, value);
		}
		public string ExportPath
		{
			get => _exportPath;
			set => SetField(ref _exportPath, value);
		}

		public DriversInfoExportType DriversInfoExportType { get; set; }

		public DriversInfoExportPlanFactType DriversInfoExportPlanFactType { get; set; }

		public bool IsPlan => DriversInfoExportPlanFactType == DriversInfoExportPlanFactType.Plan
		                      || DriversInfoExportPlanFactType == DriversInfoExportPlanFactType.PlanFact;

		public bool IsFact => DriversInfoExportPlanFactType == DriversInfoExportPlanFactType.Fact
		                      || DriversInfoExportPlanFactType == DriversInfoExportPlanFactType.PlanFact;

		public DelegateCommand HelpCommand => _helpCommand ?? (_helpCommand = new DelegateCommand(
			() =>
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Info,
					"В отчёт попадают все МЛ, кроме тех, у которых\n" +
					"водитель является выездным мастером\n\n" +
					"Планирумая ЗП водителя считается только для незакрытых МЛ\n" +
					"Фактическая - только для закрытых\n" +
					"ЗП за период - Сумма фактической ЗП за период",
					"Информация"
				);
			},
			() => true
		));
		public DelegateCommand ExportCommand => _exportCommand ?? (_exportCommand = new DelegateCommand(
				() =>
				{
					if(string.IsNullOrWhiteSpace(ExportPath))
					{
						throw new InvalidOperationException($"Не был заполнен путь выгрузки: {nameof(ExportPath)}");
					}
					var exportString = GetCsvString(Items, DriversInfoExportType, IsPlan, IsFact);
					try
					{
						File.WriteAllText(ExportPath, exportString, Encoding.UTF8);
					}
					catch(IOException)
					{
						_interactiveService.ShowMessage(ImportanceLevel.Error,
							"Не удалось сохранить файл выгрузки. Возможно не закрыт предыдущий файл выгрузки", "Ошибка");
					}
				},
				() => CanExport)
			);

		public bool CanClose()
		{
			if(!DataIsLoading)
			{
				return true;
			}
			_interactiveService.ShowMessage(ImportanceLevel.Info, "Дождитесь окончания загрузки данных");
			return false;
		}

		public IEnumerable<DriverInfoNode> GetDriverInfoNodes()
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				var nodes = LoadNotGroupedDriverInfoNodes(uow, StartDate, EndDate?.Date.AddDays(1).AddSeconds(-1),
					EmployeeStatus, RestrictedCarTypesOfUse, RestrictedCarOwnTypes);

				return DriversInfoExportType == DriversInfoExportType.RouteListGrouping
						? GroupAndGetAdditionalDataForDriverInfoNodes(uow, nodes, _wageParameterService)
						: GroupByDriverCarAndGetAdditionalDataForDriverInfoNodes(uow, StartDate, EndDate?.Date, nodes, _wageParameterService);
			}
		}

		private static IList<DriverInfoNode> LoadNotGroupedDriverInfoNodes(IUnitOfWork uow, DateTime? startDate,
			DateTime? endDate, EmployeeStatus? employeeStatus, IList<CarTypeOfUse> carTypesOfUse, IList<CarOwnType> carOwnTypes)
		{
			#region Aliases

			DriverInfoNode resultAlias = null;
			RouteList routeListAlias = null;
			Employee driverAlias = null;
			Car carAlias = null;
			WagesMovementOperations driverWageOperationAlias = null;
			RouteListItem routeListItemAlias = null;
			Order orderAlias = null;
			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			District districtAlias = null;
			RouteList routeListAlias2 = null;
			WagesMovementOperations driverWageOperationAlias2 = null;
			CarVersion carVersionAlias = null;
			CarModel carModelAlias = null;
			
			#endregion

			#region Joins & Filters

			var query = uow.Session.QueryOver(() => routeListAlias)
				.Inner.JoinAlias(() => routeListAlias.Driver, () => driverAlias)
				.Inner.JoinAlias(() => routeListAlias.Car, () => carAlias)
				.Inner.JoinAlias(() => carAlias.CarModel, () => carModelAlias)
				.JoinEntityAlias(() => carVersionAlias,
					() => carVersionAlias.Car.Id == carAlias.Id
						&& carVersionAlias.StartDate <= routeListAlias.Date &&
						(carVersionAlias.EndDate == null || carVersionAlias.EndDate >= routeListAlias.Date))
				.JoinEntityAlias(
					() => driverWageOperationAlias,
					() => routeListAlias.Status == RouteListStatus.Closed
						&& routeListAlias.DriverWageOperation.Id == driverWageOperationAlias.Id,
					JoinType.LeftOuterJoin)
				.Left.JoinAlias(() => routeListAlias.Addresses, () => routeListItemAlias)
				.Left.JoinAlias(() => routeListItemAlias.Order, () => orderAlias)
				.Left.JoinAlias(() => orderAlias.DeliveryPoint, () => deliveryPointAlias)
				.Left.JoinAlias(() => deliveryPointAlias.District, () => districtAlias)
				.Left.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias)
				.Left.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias);

			if(startDate != null)
			{
				query.Where(() => routeListAlias.Date >= startDate);
			}
			if(endDate != null)
			{
				query.Where(() => routeListAlias.Date <= endDate);
			}
			if(employeeStatus != null)
			{
				query.Where(() => driverAlias.Status == employeeStatus);
			}

			query.WhereRestrictionOn(() => carModelAlias.CarTypeOfUse).IsInG(carTypesOfUse);
			query.WhereRestrictionOn(() => carVersionAlias.CarOwnType).IsInG(carOwnTypes);
			query.Where(() => !driverAlias.VisitingMaster);

			#endregion

			#region Subqueries

			var firstRouteListDateSubQuery = QueryOver.Of<RouteList>()
				.Where(x => x.Driver.Id == driverAlias.Id)
				.Select(x => x.Date)
				.OrderBy(x => x.Date).Asc
				.Take(1);

			var lastRouteListDateSubQuery = QueryOver.Of<RouteList>()
				.Where(x => x.Driver.Id == driverAlias.Id)
				.Select(x => x.Date)
				.OrderBy(x => x.Date).Desc
				.Take(1);

			var wagePeriodSubQuery = QueryOver.Of(() => routeListAlias2)
				.Inner.JoinAlias(() => routeListAlias2.DriverWageOperation, () => driverWageOperationAlias2)
				.Where(() => routeListAlias2.Date >= startDate)
				.And(() => routeListAlias2.Date <= endDate)
				.And(() => routeListAlias2.Driver.Id == driverAlias.Id)
				.And(() => routeListAlias2.Status == RouteListStatus.Closed)
				.Select(Projections.Sum(() => driverWageOperationAlias2.Money));

			#endregion

			#region Select & Execute

			query.SelectList(list => list
				.Select(() => routeListAlias.Id).WithAlias(() => resultAlias.RouteListId)
				.Select(() => routeListAlias.Date).WithAlias(() => resultAlias.RouteListDate)
				.Select(() => routeListAlias.Status).WithAlias(() => resultAlias.RouteListStatus)
				.Select(() => routeListAlias.PlanedDistance).WithAlias(() => resultAlias.RouteListPlanedDistance)
				.Select(() => routeListAlias.RecalculatedDistance).WithAlias(() => resultAlias.RouteListRecalculatedDistance)
				.Select(() => routeListAlias.ConfirmedDistance).WithAlias(() => resultAlias.RouteListConfirmedDistance)
				.Select(() => driverAlias.Id).WithAlias(() => resultAlias.DriverId)
				.Select(() => driverAlias.Status).WithAlias(() => resultAlias.DriverStatus)
				.Select(() => driverWageOperationAlias.Money).WithAlias(() => resultAlias.DriverRouteListWageFact)
				.Select(() => driverAlias.Name).WithAlias(() => resultAlias.DriverName)
				.Select(() => driverAlias.LastName).WithAlias(() => resultAlias.DriverLastName)
				.Select(() => driverAlias.Patronymic).WithAlias(() => resultAlias.DriverPatronymic)
				.Select(() => carVersionAlias.CarOwnType).WithAlias(() => resultAlias.CarOwnType)
				.Select(() => carAlias.RegistrationNumber).WithAlias(() => resultAlias.CarRegNumber)
				.Select(() => carModelAlias.CarTypeOfUse).WithAlias(() => resultAlias.CarTypeOfUse)
				.Select(() => orderItemAlias.Count).WithAlias(() => resultAlias.OrderItemsCount)
				.Select(() => orderItemAlias.ActualCount).WithAlias(() => resultAlias.OrderItemsActualCount)
				.Select(() => nomenclatureAlias.TareVolume).WithAlias(() => resultAlias.NomecnaltureTareVolume)
				.Select(() => nomenclatureAlias.Category).WithAlias(() => resultAlias.NomenclatureCategory)
				.Select(() => routeListItemAlias.WasTransfered).WithAlias(() => resultAlias.WasTransfered)
				.Select(() => routeListItemAlias.Status).WithAlias(() => resultAlias.RouteListItemStatus)
				.Select(() => routeListItemAlias.Id).WithAlias(() => resultAlias.RouteListItemId)
				.Select(() => routeListItemAlias.BottlesReturned).WithAlias(() => resultAlias.RouteListItemReturnedBottlesCount)
				.Select(() => districtAlias.DistrictName).WithAlias(() => resultAlias.RouteListItemDistrictName)
				.Select(Projections.SubQuery(firstRouteListDateSubQuery)).WithAlias(() => resultAlias.DriverFirstRouteListDate)
				.Select(Projections.SubQuery(lastRouteListDateSubQuery)).WithAlias(() => resultAlias.DriverLastRouteListDate)
				.Select(Projections.SubQuery(wagePeriodSubQuery)).WithAlias(() => resultAlias.DriverPeriodWage)
			);

			return query
				.SetTimeout(240)
				.TransformUsing(Transformers.AliasToBean<DriverInfoNode>())
				.List<DriverInfoNode>();

			#endregion
		}

		private static IEnumerable<DriverInfoNode> GroupAndGetAdditionalDataForDriverInfoNodes(IUnitOfWork uow,
			IEnumerable<DriverInfoNode> nodes, IWageParameterService wageParameterService)
		{
			#region Aliases

			DriverDistrictPrioritySet driverDistrictPrioritySetAlias = null;
			DriverDistrictPriority driverDistrictPriorityAlias = null;
			RouteListAssignedDistrictsNode routeListAssignedDistrictsNodeAlias = null;
			RouteList routeListAlias = null;
			Employee driverAlias = null;
			District districtAlias = null;

			#endregion

			var groupedByRouteList = nodes.GroupBy(x => x.RouteListId);
			var driverInfoNodes = new List<DriverInfoNode>();

			foreach(var groupNode in groupedByRouteList)
			{
				var firstNode = groupNode.First();

				var node = new DriverInfoNode
				{
					RouteListId = groupNode.Key,
					RouteListDateString = firstNode.RouteListDate.ToString("d"),
					RouteListStatus = firstNode.RouteListStatus,
					RouteListPlanedDistance = firstNode.RouteListPlanedDistance,
					RouteListRecalculatedDistance = firstNode.RouteListRecalculatedDistance,
					RouteListConfirmedDistance = firstNode.RouteListConfirmedDistance,
					DriverId = firstNode.DriverId,
					DriverStatus = firstNode.DriverStatus,
					DriverRouteListWageFact = firstNode.DriverRouteListWageFact,
					DriverPeriodWage = firstNode.DriverPeriodWage,
					DriverFullName = PersonHelper.PersonNameWithInitials(
						firstNode.DriverLastName, firstNode.DriverName, firstNode.DriverPatronymic),
					CarRegNumber = firstNode.CarRegNumber,
					CarOwnType = firstNode.CarOwnType,
					CarTypeOfUse = firstNode.CarTypeOfUse,
					DriverFirstRouteListDateString = firstNode.DriverFirstRouteListDate.ToString("d"),
					DriverLastRouteListDateString = firstNode.DriverLastRouteListDate.ToString("d")
				};

				#region Bottles & Equipments Calculation

				var vol19LActualCount = firstNode.RouteListStatus == RouteListStatus.Closed
					? (int)groupNode.Where(x =>
							x.RouteListItemStatus == RouteListItemStatus.Completed
							&& x.NomenclatureCategory == NomenclatureCategory.water
							&& x.NomecnaltureTareVolume == TareVolume.Vol19L)
						.Sum(x => x.OrderItemsActualCount ?? 0)
					: 0;
				node.Vol19LBottlesActualCount = vol19LActualCount == 0 ? "" : vol19LActualCount.ToString();

				var vol19LCount = (int)groupNode.Where(x =>
						x.NomenclatureCategory == NomenclatureCategory.water
						&& x.NomecnaltureTareVolume == TareVolume.Vol19L)
					.Sum(x => x.OrderItemsCount);
				node.Vol19LBottlesCount = vol19LCount == 0 ? "" : vol19LCount.ToString();

				var vol6LActualCount = firstNode.RouteListStatus == RouteListStatus.Closed
					? (int)groupNode.Where(x =>
							x.RouteListItemStatus == RouteListItemStatus.Completed
							&& x.NomenclatureCategory == NomenclatureCategory.water
							&& x.NomecnaltureTareVolume == TareVolume.Vol6L)
						.Sum(x => x.OrderItemsActualCount ?? 0)
					: 0;
				node.Vol6LBottlesActualCount = vol6LActualCount == 0 ? "" : vol6LActualCount.ToString();

				var vol6LCount = (int)groupNode
					.Where(x =>
						x.NomenclatureCategory == NomenclatureCategory.water
						&& x.NomecnaltureTareVolume == TareVolume.Vol6L)
					.Sum(x => x.OrderItemsCount);
				node.Vol6LBottlesCount = vol6LCount == 0 ? "" : vol6LCount.ToString();

				var vol1500MlActualCount = firstNode.RouteListStatus == RouteListStatus.Closed
					? (int)groupNode
						.Where(x =>
							x.RouteListItemStatus == RouteListItemStatus.Completed
							&& x.NomenclatureCategory == NomenclatureCategory.water
							&& x.NomecnaltureTareVolume == TareVolume.Vol1500ml)
						.Sum(x => x.OrderItemsActualCount ?? 0)
					: 0;
				node.Vol1500MlBottlesActualCount = vol1500MlActualCount == 0 ? "" : vol1500MlActualCount.ToString();

				var vol1500MlCount = (int)groupNode
					.Where(x =>
						x.NomenclatureCategory == NomenclatureCategory.water
						&& x.NomecnaltureTareVolume == TareVolume.Vol1500ml)
					.Sum(x => x.OrderItemsCount);
				node.Vol1500MlBottlesCount = vol1500MlCount == 0 ? "" : vol1500MlCount.ToString();

				var vol600MlActualCount = firstNode.RouteListStatus == RouteListStatus.Closed
					? (int)groupNode
						.Where(x =>
							x.RouteListItemStatus == RouteListItemStatus.Completed
							&& x.NomenclatureCategory == NomenclatureCategory.water
							&& x.NomecnaltureTareVolume == TareVolume.Vol600ml)
						.Sum(x => x.OrderItemsActualCount ?? 0)
					: 0;
				node.Vol600MlBottlesActualCount = vol600MlActualCount == 0 ? "" : vol600MlActualCount.ToString();

				var vol600MlCount = (int)groupNode
					.Where(x =>
						x.NomenclatureCategory == NomenclatureCategory.water
						&& x.NomecnaltureTareVolume == TareVolume.Vol600ml)
					.Sum(x => x.OrderItemsCount);
				node.Vol600MlBottlesCount = vol600MlCount == 0 ? "" : vol600MlCount.ToString();

				var equipmentActualCount = firstNode.RouteListStatus == RouteListStatus.Closed
					? (int)groupNode.Where(x =>
							x.RouteListItemStatus == RouteListItemStatus.Completed
							&& x.NomenclatureCategory == NomenclatureCategory.equipment)
						.Sum(x => x.OrderItemsActualCount ?? 0)
					: 0;
				node.EquipmentActualCount = equipmentActualCount == 0 ? "" : equipmentActualCount.ToString();

				var equipmentCount = (int)groupNode
					.Where(x => x.NomenclatureCategory == NomenclatureCategory.equipment)
					.Sum(x => x.OrderItemsCount);
				node.EquipmentCount = equipmentCount == 0 ? "" : equipmentCount.ToString();

				var vol19Undelivered = (int)groupNode
					.Where(x =>
						(x.RouteListItemStatus == RouteListItemStatus.Canceled || x.RouteListItemStatus == RouteListItemStatus.Overdue)
						&& x.NomenclatureCategory == NomenclatureCategory.water && x.NomecnaltureTareVolume == TareVolume.Vol19L)
					.Sum(x => x.OrderItemsCount);
				node.Vol19LUndelivered = vol19Undelivered == 0 ? "" : vol19Undelivered.ToString();

				var routeListReturnedBottlesCount = groupNode
					.GroupBy(x => x.RouteListItemId)
					.Select(x => x.First())
					.Sum(x => x.RouteListItemReturnedBottlesCount ?? 0);
				node.RouteListReturnedBottlesCount = routeListReturnedBottlesCount == 0 ? "" : routeListReturnedBottlesCount.ToString();

				#endregion

				#region Districts & RouteListItemCounts Calculation

				node.DriverPlannedDistricts = String.Join(", ",
					groupNode.GroupBy(x => x.RouteListItemId)
						.Select(x => x.First())
						.Where(x => !x.WasTransfered)
						.Select(x => x.RouteListItemDistrictName)
						.Distinct()
				);

				node.DriverFactDistricts = firstNode.RouteListStatus == RouteListStatus.Closed
					? String.Join(", ",
						groupNode.GroupBy(x => x.RouteListItemId)
							.Select(x => x.First())
							.Where(x => x.RouteListItemStatus == RouteListItemStatus.Completed)
							.Select(x => x.RouteListItemDistrictName)
							.Distinct())
					: "";

				node.RouteListItemCountPlanned = groupNode.Where(x => !x.WasTransfered).GroupBy(x => x.RouteListItemId).Count();

				node.RouteListItemCountFact =
					firstNode.RouteListStatus == RouteListStatus.Closed
						? groupNode.Where(x => x.RouteListItemStatus == RouteListItemStatus.Completed)
							.GroupBy(x => x.RouteListItemId)
							.Count()
							.ToString()
						: "0";

				#endregion

				driverInfoNodes.Add(node);
			}

			#region WorkDays & AssignedDistricts & PlannedWage Calculation

			//Рабочие дни
			foreach(var driverInfoNode in driverInfoNodes.GroupBy(x => x.DriverId).Select(g => g.First()))
			{
				var driverNodes = driverInfoNodes
					.Where(x => x.DriverId == driverInfoNode.DriverId)
					.ToList();
				driverInfoNode.DriverDaysWorkedCount = driverNodes.GroupBy(x => x.RouteListDateString).Count();
				foreach(var driverNode in driverNodes.Where(x => x.DriverDaysWorkedCount == 0))
				{
					driverNode.DriverDaysWorkedCount = driverInfoNode.DriverDaysWorkedCount;
				}
			}

			//Привязанные районы
			var routeListNodes = uow.Session.QueryOver(() => routeListAlias)
				.Inner.JoinAlias(() => routeListAlias.Driver, () => driverAlias)
				.Inner.JoinAlias(() => driverAlias.DriverDistrictPrioritySets, () => driverDistrictPrioritySetAlias)
				.Inner.JoinAlias(() => driverDistrictPrioritySetAlias.DriverDistrictPriorities, () => driverDistrictPriorityAlias)
				.Inner.JoinAlias(() => driverDistrictPriorityAlias.District, () => districtAlias)
				.WhereRestrictionOn(() => routeListAlias.Id).IsIn(driverInfoNodes.Select(x => x.RouteListId).ToArray())
				.And(() => driverDistrictPrioritySetAlias.DateActivated <= routeListAlias.Date)
				.And(Restrictions.Disjunction()
					.Add(() => driverDistrictPrioritySetAlias.DateDeactivated == null)
					.Add(() => driverDistrictPrioritySetAlias.DateDeactivated >= routeListAlias.Date))
				.SelectList(list => list
					.SelectGroup(() => routeListAlias.Id).WithAlias(() => routeListAssignedDistrictsNodeAlias.RouteListId)
					.Select(CustomProjections.GroupConcat(() => districtAlias.DistrictName,
						orderByExpression: () => districtAlias.DistrictName, separator: ", "))
					.WithAlias(() => routeListAssignedDistrictsNodeAlias.AssignedDistricts))
				.TransformUsing(Transformers.AliasToBean<RouteListAssignedDistrictsNode>())
				.List<RouteListAssignedDistrictsNode>();

			foreach(var routeListNode in routeListNodes)
			{
				driverInfoNodes
					.First(x => x.RouteListId == routeListNode.RouteListId).DriverAssignedDistricts = routeListNode.AssignedDistricts;
			}

			var notClosedRouteListIds = driverInfoNodes
				.Where(x => x.RouteListStatus != RouteListStatus.Closed)
				.Select(x => x.RouteListId)
				.ToArray();

			//Fetching. Как это работает: https://github.com/nhibernate/nhibernate-core/pull/1599
			var routeListsFuture = uow.Session.QueryOver(() => routeListAlias)
				.WhereRestrictionOn(x => x.Id).IsIn(notClosedRouteListIds)
				.Future();

			uow.Session.QueryOver(() => routeListAlias)
				.Fetch(SelectMode.ChildFetch, () => routeListAlias)
				.Fetch(SelectMode.Fetch, () => routeListAlias.Addresses)
				.WhereRestrictionOn(x => x.Id).IsIn(notClosedRouteListIds)
				.Future();

			uow.Session.QueryOver(() => routeListAlias)
				.Fetch(SelectMode.ChildFetch, () => routeListAlias, () => routeListAlias.Addresses)
				.Fetch(SelectMode.Fetch, () => routeListAlias.Addresses[0].Order)
				.WhereRestrictionOn(x => x.Id).IsIn(notClosedRouteListIds)
				.Future();

			uow.Session.QueryOver(() => routeListAlias)
				.Fetch(SelectMode.Fetch, x => x.Driver)
				.WhereRestrictionOn(x => x.Id).IsIn(notClosedRouteListIds)
				.Future();

			uow.Session.QueryOver(() => routeListAlias)
				.Fetch(SelectMode.ChildFetch, () => routeListAlias, () => routeListAlias.Driver)
				.Fetch(SelectMode.Fetch, () => routeListAlias.Driver.WageParameters)
				.WhereRestrictionOn(x => x.Id).IsIn(notClosedRouteListIds)
				.Future();

			//Планируемая ЗП
			foreach(var routeList in routeListsFuture.ToList())
			{
				routeList.RecalculateAllWages(wageParameterService);
				var driverWage = routeList.GetDriversTotalWage();
				driverInfoNodes.First(x => x.RouteListId == routeList.Id).DriverRouteListWagePlanned = driverWage;
			}

			#endregion

			return driverInfoNodes.OrderBy(x => x.DriverId).ThenBy(x => x.RouteListDate).ToList();
		}

		private static IEnumerable<DriverInfoNode> GroupByDriverCarAndGetAdditionalDataForDriverInfoNodes(IUnitOfWork uow, DateTime? startDate, DateTime? endDate,
			IEnumerable<DriverInfoNode> nodes, IWageParameterService wageParameterService)
		{
			#region Aliases

			DriverDistrictPrioritySet driverDistrictPrioritySetAlias = null;
			DriverDistrictPriority driverDistrictPriorityAlias = null;
			RouteListAssignedDistrictsNode routeListAssignedDistrictsNodeAlias = null;
			RouteList routeListAlias = null;
			Employee driverAlias = null;
			District districtAlias = null;

			#endregion

			var groupedByDriver = nodes.GroupBy(x => new { x.DriverId, x.CarRegNumber, x.RouteListDate });

			var driverInfoNodes = new List<DriverInfoNode>();

			foreach(var groupNode in groupedByDriver)
			{
				var firstNode = groupNode.First();

				var groupedByRouteListId = groupNode.GroupBy(x => x.RouteListId).ToList();

				var node = new DriverInfoNode
				{
					InnerNodes = groupedByRouteListId.Select(x => x.First()),
					RouteListDateString = firstNode.RouteListDate.ToString("d"),
					RouteListDate = firstNode.RouteListDate,
					DriverId = firstNode.DriverId,
					DriverStatus = firstNode.DriverStatus,
					DriverRouteListWageFact = groupedByRouteListId.Sum(x => x.First().DriverRouteListWageFact),
					DriverPeriodWage = firstNode.DriverPeriodWage,
					DriverFullName = PersonHelper.PersonNameWithInitials(firstNode.DriverLastName, firstNode.DriverName, firstNode.DriverPatronymic),
					CarRegNumber = firstNode.CarRegNumber,
					CarOwnType = firstNode.CarOwnType,
					CarTypeOfUse = firstNode.CarTypeOfUse,
					DriverFirstRouteListDateString = firstNode.DriverFirstRouteListDate.ToString("d"),
					DriverLastRouteListDateString = firstNode.DriverLastRouteListDate.ToString("d")
				};

				#region Bottles & Equipments Calculation

				var vol19LActualCount = (int)groupNode
					.Where(x => 
						x.RouteListStatus == RouteListStatus.Closed
						&& x.RouteListItemStatus == RouteListItemStatus.Completed
						&& x.NomenclatureCategory == NomenclatureCategory.water
						&& x.NomecnaltureTareVolume == TareVolume.Vol19L)
					.Sum(x => x.OrderItemsActualCount ?? 0);
				node.Vol19LBottlesActualCount = vol19LActualCount == 0 ? "" : vol19LActualCount.ToString();

				var vol19LCount = (int)groupNode
					.Where(x =>
						x.NomenclatureCategory == NomenclatureCategory.water
						&& x.NomecnaltureTareVolume == TareVolume.Vol19L)
					.Sum(x => x.OrderItemsCount);
				node.Vol19LBottlesCount = vol19LCount == 0 ? "" : vol19LCount.ToString();

				var vol6LActualCount = (int)groupNode
					.Where(x =>
						x.RouteListStatus == RouteListStatus.Closed
						&& x.RouteListItemStatus == RouteListItemStatus.Completed
						&& x.NomenclatureCategory == NomenclatureCategory.water
						&& x.NomecnaltureTareVolume == TareVolume.Vol6L)
					.Sum(x => x.OrderItemsActualCount ?? 0);
				node.Vol6LBottlesActualCount = vol6LActualCount == 0 ? "" : vol6LActualCount.ToString();

				var vol6LCount = (int)groupNode
					.Where(x =>
						x.NomenclatureCategory == NomenclatureCategory.water
						&& x.NomecnaltureTareVolume == TareVolume.Vol6L)
					.Sum(x => x.OrderItemsCount);
				node.Vol6LBottlesCount = vol6LCount == 0 ? "" : vol6LCount.ToString();

				var vol1500MlActualCount = (int)groupNode
					.Where(x =>
						x.RouteListStatus == RouteListStatus.Closed
						&& x.RouteListItemStatus == RouteListItemStatus.Completed
						&& x.NomenclatureCategory == NomenclatureCategory.water
						&& x.NomecnaltureTareVolume == TareVolume.Vol1500ml)
					.Sum(x => x.OrderItemsActualCount ?? 0);
				node.Vol1500MlBottlesActualCount = vol1500MlActualCount == 0 ? "" : vol1500MlActualCount.ToString();

				var vol1500MlCount = (int)groupNode
					.Where(x =>
						x.NomenclatureCategory == NomenclatureCategory.water
						&& x.NomecnaltureTareVolume == TareVolume.Vol1500ml)
					.Sum(x => x.OrderItemsCount);
				node.Vol1500MlBottlesCount = vol1500MlCount == 0 ? "" : vol1500MlCount.ToString();

				var vol600MlActualCount = (int)groupNode
					.Where(x =>
						x.RouteListStatus == RouteListStatus.Closed
						&& x.RouteListItemStatus == RouteListItemStatus.Completed
						&& x.NomenclatureCategory == NomenclatureCategory.water
						&& x.NomecnaltureTareVolume == TareVolume.Vol600ml)
					.Sum(x => x.OrderItemsActualCount ?? 0);
				node.Vol600MlBottlesActualCount = vol600MlActualCount == 0 ? "" : vol600MlActualCount.ToString();

				var vol600MlCount = (int)groupNode
					.Where(x =>
						x.NomenclatureCategory == NomenclatureCategory.water
						&& x.NomecnaltureTareVolume == TareVolume.Vol600ml)
					.Sum(x => x.OrderItemsCount);
				node.Vol600MlBottlesCount = vol600MlCount == 0 ? "" : vol600MlCount.ToString();

				var equipmentActualCount = (int)groupNode
					.Where(x =>
						x.RouteListStatus == RouteListStatus.Closed
						&& x.RouteListItemStatus == RouteListItemStatus.Completed
						&& x.NomenclatureCategory == NomenclatureCategory.equipment)
					.Sum(x => x.OrderItemsActualCount ?? 0);
				node.EquipmentActualCount = equipmentActualCount == 0 ? "" : equipmentActualCount.ToString();

				var equipmentCount = (int)groupNode
					.Where(x => x.NomenclatureCategory == NomenclatureCategory.equipment)
					.Sum(x => x.OrderItemsCount);
				node.EquipmentCount = equipmentCount == 0 ? "" : equipmentCount.ToString();

				var vol19Undelivered = (int)groupNode
					.Where(x =>
						(x.RouteListItemStatus == RouteListItemStatus.Canceled || x.RouteListItemStatus == RouteListItemStatus.Overdue)
						&& x.NomenclatureCategory == NomenclatureCategory.water && x.NomecnaltureTareVolume == TareVolume.Vol19L)
					.Sum(x => x.OrderItemsCount);
				node.Vol19LUndelivered = vol19Undelivered == 0 ? "" : vol19Undelivered.ToString();

				var routeListReturnedBottlesCount = groupNode
					.GroupBy(x => x.RouteListItemId)
					.Select(x => x.First())
					.Sum(x => x.RouteListItemReturnedBottlesCount ?? 0);
				node.RouteListReturnedBottlesCount = routeListReturnedBottlesCount == 0 ? "" : routeListReturnedBottlesCount.ToString();

				#endregion

				#region Districts & RouteListItemCounts Calculation

				node.DriverPlannedDistricts = String.Join(", ",
					groupNode.GroupBy(x => x.RouteListItemId)
						.Select(x => x.First())
						.Where(x => !x.WasTransfered)
						.Select(x => x.RouteListItemDistrictName)
						.Distinct()
				);

				node.DriverFactDistricts = String.Join(", ", 
					groupNode.GroupBy(x => x.RouteListItemId)
						.Select(x => x.First())
						.Where(x => x.RouteListItemStatus == RouteListItemStatus.Completed
									&& x.RouteListStatus == RouteListStatus.Closed)
						.Select(x => x.RouteListItemDistrictName)
						.Distinct());

				node.RouteListItemCountPlanned = groupNode.Where(x => !x.WasTransfered).GroupBy(x => x.RouteListItemId).Count();

				node.RouteListItemCountFact = groupNode
					.Where(x => x.RouteListItemStatus == RouteListItemStatus.Completed 
					            && x.RouteListStatus == RouteListStatus.Closed)
					.GroupBy(x => x.RouteListItemId)
					.Count()
					.ToString();

				#endregion

				driverInfoNodes.Add(node);
			}

			#region WorkDays & AssignedDistricts & PlannedWage Calculation

			//Рабочие дни
			foreach(var driverInfoNode in driverInfoNodes.GroupBy(x => x.DriverId).Select(g => g.First()))
			{
				var driverNodes = driverInfoNodes
					.Where(x => x.DriverId == driverInfoNode.DriverId)
					.ToList();
				driverInfoNode.DriverDaysWorkedCount = driverNodes.GroupBy(x => x.RouteListDateString).Count();
				foreach(var driverNode in driverNodes.Where(x => x.DriverDaysWorkedCount == 0))
				{
					driverNode.DriverDaysWorkedCount = driverInfoNode.DriverDaysWorkedCount;
				}
			}

			//Привязанные районы
			var driverListNodes = uow.Session.QueryOver(() => driverAlias)
				.Inner.JoinAlias(() => driverAlias.DriverDistrictPrioritySets, () => driverDistrictPrioritySetAlias)
				.Inner.JoinAlias(() => driverDistrictPrioritySetAlias.DriverDistrictPriorities, () => driverDistrictPriorityAlias)
				.Inner.JoinAlias(() => driverDistrictPriorityAlias.District, () => districtAlias)
				.WhereRestrictionOn(() => driverAlias.Id).IsIn(driverInfoNodes.Select(x => x.DriverId).ToArray())
				.And(() => driverDistrictPrioritySetAlias.DateActivated <= endDate)
				.And(Restrictions.Disjunction()
					.Add(() => driverDistrictPrioritySetAlias.DateDeactivated == null)
					.Add(() => driverDistrictPrioritySetAlias.DateDeactivated >= startDate))
				.SelectList(list => list
					.SelectGroup(() => driverAlias.Id).WithAlias(() => routeListAssignedDistrictsNodeAlias.DriverId)
					.Select(CustomProjections.GroupConcat(() => districtAlias.DistrictName, true,
						orderByExpression: () => districtAlias.DistrictName, separator: ", "))
					.WithAlias(() => routeListAssignedDistrictsNodeAlias.AssignedDistricts))
				.TransformUsing(Transformers.AliasToBean<RouteListAssignedDistrictsNode>())
				.List<RouteListAssignedDistrictsNode>();

			foreach(var driverListNode in driverListNodes)
			{
				driverInfoNodes.First(x => x.DriverId == driverListNode.DriverId).DriverAssignedDistricts = driverListNode.AssignedDistricts;
			}

			var notClosedRouteListIds = driverInfoNodes
				.SelectMany(x => x.InnerNodes)
				.Where(x => x.RouteListStatus != RouteListStatus.Closed)
				.Select(x => x.RouteListId)
				.ToArray();

			//Fetching. Как это работает: https://github.com/nhibernate/nhibernate-core/pull/1599
			var routeListsFuture = uow.Session.QueryOver(() => routeListAlias)
				.WhereRestrictionOn(x => x.Id).IsIn(notClosedRouteListIds)
				.Future();

			uow.Session.QueryOver(() => routeListAlias)
				.Fetch(SelectMode.ChildFetch, () => routeListAlias)
				.Fetch(SelectMode.Fetch, () => routeListAlias.Addresses)
				.WhereRestrictionOn(x => x.Id).IsIn(notClosedRouteListIds)
				.Future();

			uow.Session.QueryOver(() => routeListAlias)
				.Fetch(SelectMode.ChildFetch, () => routeListAlias, () => routeListAlias.Addresses)
				.Fetch(SelectMode.Fetch, () => routeListAlias.Addresses[0].Order)
				.WhereRestrictionOn(x => x.Id).IsIn(notClosedRouteListIds)
				.Future();

			uow.Session.QueryOver(() => routeListAlias)
				.Fetch(SelectMode.Fetch, x => x.Driver)
				.WhereRestrictionOn(x => x.Id).IsIn(notClosedRouteListIds)
				.Future();

			uow.Session.QueryOver(() => routeListAlias)
				.Fetch(SelectMode.ChildFetch, () => routeListAlias, () => routeListAlias.Driver)
				.Fetch(SelectMode.Fetch, () => routeListAlias.Driver.WageParameters)
				.WhereRestrictionOn(x => x.Id).IsIn(notClosedRouteListIds)
				.Future();

			//Планируемая ЗП
			foreach(var routeList in routeListsFuture)
			{
				routeList.RecalculateAllWages(wageParameterService);
			}

			foreach(var driverInfoNode in driverInfoNodes)
			{
				driverInfoNode.DriverRouteListWagePlanned = routeListsFuture
					.Where(x => 
						x.Driver.Id == driverInfoNode.DriverId
						&& x.Date == driverInfoNode.RouteListDate
						&& x.Car.RegistrationNumber == driverInfoNode.CarRegNumber)
					.Sum(x => x.GetDriversTotalWage());
			}

			#endregion

			return driverInfoNodes.OrderBy(x => x.DriverId).ThenBy(x => x.RouteListDate).ToList();
		}

		private void SubscribeOnItemsListChanges()
		{
			Items.ElementAdded += (list, idx) => { OnPropertyChanged(nameof(CanExport)); };
			Items.ElementRemoved += (list, idx, aObject) => { OnPropertyChanged(nameof(CanExport)); };
		}

		private static string GetCsvString(IList<DriverInfoNode> exportData, DriversInfoExportType driversInfoExportType, bool isPlan, bool isFact)
		{
			return driversInfoExportType == DriversInfoExportType.RouteListGrouping
				? GetRouteListGroupingCsvString(exportData, isPlan, isFact)
				: GetDriverCarGroupingCsvString(exportData, isPlan, isFact);
		}

		private static string GetRouteListGroupingCsvString(IList<DriverInfoNode> exportData, bool isPlan, bool isFact)
		{
			var sb = new StringBuilder();
			sb.Append("Код МЛ;Дата МЛ;Статус МЛ;Код водителя;Водитель;Статус водителя;Планируемое расстояние;Рассчитанное расстояние;" +
				"Подтвержденное расстояние;ЗП водителя за МЛ (план+факт);ЗП водителя за период;Гос. номер авто;Принадлежность авто;Тип авто;" +
				"Кол-во отраб. дней за период;");

			if(isPlan)
			{
				sb.Append("Адреса план;");
			}

			if(isFact)
			{
				sb.Append("Адреса факт;");
			}

			sb.Append("19л от клиента;");

			if(isPlan)
			{
				sb.Append("19л план;");
			}

			if(isFact)
			{
				sb.Append("19л факт;");
			}

			if(isPlan)
			{
				sb.Append("6л план;");
			}

			if(isFact)
			{
				sb.Append("6л факт;");
			}

			if(isPlan)
			{
				sb.Append("1.5л план;");
			}

			if(isFact)
			{
				sb.Append("1.5л факт;");
			}

			if(isPlan)
			{
				sb.Append("0.6л план;");
			}

			if(isFact)
			{
				sb.Append("0.6л факт;");
			}

			if(isPlan)
			{
				sb.Append("обор. план;");
			}

			if(isFact)
			{
				sb.Append("обор. факт;");
			}

			sb.Append("19л недовозы;Дата первого МЛ;Дата последнего МЛ;Закреплённые районы;Планируемые районы;Фактические районы");
			
			sb.AppendLine();

			foreach(var item in exportData)
			{
				var lines = new List<string>
				{
					item.RouteListId.ToString(), item.RouteListDateString, item.RouteListStatus.GetEnumTitle(), item.DriverId.ToString(),
					item.DriverFullName, item.DriverStatus.GetEnumTitle(),
					item.RouteListPlanedDistance.HasValue ? $"{item.RouteListPlanedDistance:N2}" : $"{default(decimal)}",
					item.RouteListRecalculatedDistance.HasValue ? $"{item.RouteListRecalculatedDistance:N2}" : $"{default(decimal)}",
					item.RouteListConfirmedDistance.ToString("N2"), item.DriverRouteListWageForRouteListGroupingString,
					item.DriverPeriodWageString, item.CarRegNumber, item.CarOwnType.GetEnumTitle(),
					item.CarTypeOfUse.GetEnumTitle(), item.DriverDaysWorkedCount.ToString()
				};

				if(isPlan)
				{
					lines.Add(item.RouteListItemCountPlanned.ToString());
				}

				if(isFact)
				{
					lines.Add(item.RouteListItemCountFact);
				}

				lines.Add(item.RouteListReturnedBottlesCount);

				if(isPlan)
				{
					lines.Add(item.Vol19LBottlesCount);
				}

				if(isFact)
				{
					lines.Add(item.Vol19LBottlesActualCount);
				}

				if(isPlan)
				{
					lines.Add(item.Vol6LBottlesCount);
				}

				if(isFact)
				{
					lines.Add(item.Vol6LBottlesActualCount);
				}

				if(isPlan)
				{
					lines.Add(item.Vol1500MlBottlesCount);
				}

				if(isFact)
				{
					lines.Add(item.Vol1500MlBottlesActualCount);
				}

				if(isPlan)
				{
					lines.Add(item.Vol600MlBottlesCount);
				}

				if(isFact)
				{
					lines.Add(item.Vol600MlBottlesActualCount);
				}

				if(isPlan)
				{
					lines.Add(item.EquipmentCount);
				}

				if(isFact)
				{
					lines.Add(item.EquipmentActualCount);
				}

				lines.AddRange(new List<string>
				{
					item.Vol19LUndelivered, item.DriverFirstRouteListDateString, item.DriverLastRouteListDateString,
					item.DriverAssignedDistricts, item.DriverPlannedDistricts, item.DriverFactDistricts
				});
				
				foreach(var line in lines)
				{
					sb.Append(line?.Replace(';', ',').Replace("\r\n", " ").Replace('\n', ' ') + ';');
				}
				sb.Remove(sb.Length - 1, 1);
				sb.AppendLine();
			}
			sb.Remove(sb.Length - 1, 1);
			return sb.ToString();
		}

		private static string GetDriverCarGroupingCsvString(IList<DriverInfoNode> exportData, bool isPlan, bool isFact)
		{
			var sb = new StringBuilder();
			sb.Append("Дата МЛ;Принадлежность авто;Тип авто;Гос. номер авто;Водитель;");

			if(isPlan)
			{
				sb.Append("Адреса план;");
			}

			if(isFact)
			{
				sb.Append("Адреса факт;");
			}

			if(isPlan)
			{
				sb.Append("19л план;");
			}

			if(isFact)
			{
				sb.Append("19л факт;");
			}

			sb.Append("19л от клиента;");

			if(isPlan)
			{
				sb.Append("6л план;");
			}

			if(isFact)
			{
				sb.Append("6л факт;");
			}

			if(isPlan)
			{
				sb.Append("1.5л план;");
			}

			if(isFact)
			{
				sb.Append("1.5л факт;");
			}

			if(isPlan)
			{
				sb.Append("0.6л план;");
			}

			if(isFact)
			{
				sb.Append("0.6л факт;");
			}

			if(isPlan)
			{
				sb.Append("обор. план;");
			}

			if(isFact)
			{
				sb.Append("обор. факт;");
			}
			sb.Append("19л недовозы;ЗП водителя за МЛ (факт+план);Закреплённые районы;Планируемые районы;Фактические районы;Код водителя;Статус водителя;" +
					  "ЗП водителя за период;Кол-во отраб. дней за период;Дата первого МЛ;Дата последнего МЛ;");

			sb.AppendLine();

			foreach(var item in exportData)
			{
				var lines = new List<string>
				{
					item.RouteListDateString,
					item.CarOwnType.GetEnumTitle(),
					item.CarTypeOfUse.GetEnumTitle(),
					item.CarRegNumber,
					item.DriverFullName
				};

				if(isPlan)
				{
					lines.Add(item.RouteListItemCountPlanned.ToString());
				}

				if(isFact)
				{
					lines.Add(item.RouteListItemCountFact);
				}

				if(isPlan)
				{
					lines.Add(item.Vol19LBottlesCount);
				}

				if(isFact)
				{
					lines.Add(item.Vol19LBottlesActualCount);
				}

				lines.Add(item.RouteListReturnedBottlesCount);

				if(isPlan)
				{
					lines.Add(item.Vol6LBottlesCount);
				}

				if(isFact)
				{
					lines.Add(item.Vol6LBottlesActualCount);
				}

				if(isPlan)
				{
					lines.Add(item.Vol1500MlBottlesCount);
				}

				if(isFact)
				{
					lines.Add(item.Vol1500MlBottlesActualCount);
				}

				if(isPlan)
				{
					lines.Add(item.Vol600MlBottlesCount);
				}

				if(isFact)
				{
					lines.Add(item.Vol600MlBottlesActualCount);
				}

				if(isPlan)
				{
					lines.Add(item.EquipmentCount);
				}

				if(isFact)
				{
					lines.Add(item.EquipmentActualCount);
				}

				lines.AddRange(new List<string>
					{
						item.Vol19LUndelivered,
						item.DriverRouteListWageForDriverCarGroupingString,
						item.DriverAssignedDistricts,
						item.DriverPlannedDistricts,
						item.DriverFactDistricts,
						item.DriverId.ToString(),
						item.DriverStatus.GetEnumTitle(),
						item.DriverPeriodWageString,
						item.DriverDaysWorkedCount.ToString(),
						item.DriverFirstRouteListDateString,
						item.DriverLastRouteListDateString
					}
				);

				foreach(var line in lines)
				{
					sb.Append(line?.Replace(';', ',').Replace("\r\n", " ").Replace('\n', ' ') + ';');
				}
				sb.Remove(sb.Length - 1, 1);
				sb.AppendLine();
			}
			sb.Remove(sb.Length - 1, 1);
			return sb.ToString();
		}

		private class RouteListAssignedDistrictsNode
		{
			public int RouteListId { get; set; }
			public string AssignedDistricts { get; set; }
			public int DriverId { get; set; }
		}
	}

	public class DriverInfoNode
	{
		#region RouteList

		public int RouteListId { get; set; }
		public DateTime RouteListDate { get; set; }
		public string RouteListDateString { get; set; }

		public RouteListStatus RouteListStatus { get; set; }
		public string RouteListReturnedBottlesCount { get; set; }
		
		public decimal? RouteListPlanedDistance { get; set; }
		public decimal? RouteListRecalculatedDistance { get; set; }
		public decimal RouteListConfirmedDistance { get; set; }

		#endregion

		#region RouteListItem

		public int RouteListItemId { get; set; }
		public bool WasTransfered { get; set; }
		public RouteListItemStatus RouteListItemStatus { get; set; }
		public string RouteListItemDistrictName { get; set; }
		public int RouteListItemCountPlanned { get; set; }
		public string RouteListItemCountFact { get; set; }
		public int? RouteListItemReturnedBottlesCount { get; set; }

		#endregion

		#region Driver

		public int DriverId { get; set; }
		public EmployeeStatus DriverStatus { get; set; }

		public decimal? DriverRouteListWageFact { get; set; }
		public string DriverRouteListWageFactString =>
			DriverRouteListWageFact.HasValue ? DriverRouteListWageFact.Value.ToString(CultureInfo.CurrentCulture) : "";

		public decimal DriverRouteListWagePlanned { get; set; }
		public string DriverRouteListWagePlannedString =>
			DriverRouteListWagePlanned > 0 ? DriverRouteListWagePlanned.ToString(CultureInfo.CurrentCulture) : "";

		public string DriverRouteListWageForRouteListGroupingString => 
			RouteListStatus == RouteListStatus.Closed ? DriverRouteListWageFactString : DriverRouteListWagePlannedString;

		public string DriverRouteListWageForDriverCarGroupingString => (
			(DriverRouteListWageFact.HasValue ? DriverRouteListWageFact.Value : 0)
			+ DriverRouteListWagePlanned)
			.ToString(CultureInfo.CurrentCulture);

		public decimal? DriverPeriodWage { get; set; }
		public string DriverPeriodWageString =>
			DriverPeriodWage.HasValue ? DriverPeriodWage.Value.ToString(CultureInfo.CurrentCulture) : "";

		public int DriverDaysWorkedCount { get; set; }
		public string DriverLastName { get; set; }
		public string DriverName { get; set; }
		public string DriverPatronymic { get; set; }
		public string DriverFullName { get; set; }
		public DateTime DriverFirstRouteListDate { get; set; }
		public DateTime DriverLastRouteListDate { get; set; }
		public string DriverFirstRouteListDateString { get; set; }
		public string DriverLastRouteListDateString { get; set; }
		public string DriverAssignedDistricts { get; set; }
		public string DriverPlannedDistricts { get; set; }
		public string DriverFactDistricts { get; set; }

		#endregion

		#region Car

		public string CarRegNumber { get; set; }

		public CarOwnType CarOwnType { get; set; }

		public CarTypeOfUse CarTypeOfUse { get; set; }

		#endregion

		#region OrderItems & OrderEquipment

		public NomenclatureCategory NomenclatureCategory { get; set; }
		public TareVolume NomecnaltureTareVolume { get; set; }

		public decimal OrderItemsCount { get; set; }
		public decimal? OrderItemsActualCount { get; set; }

		public string Vol19LBottlesCount { get; set; }
		public string Vol19LBottlesActualCount { get; set; }

		public string Vol6LBottlesCount { get; set; }
		public string Vol6LBottlesActualCount { get; set; }
		public string Vol1500MlBottlesCount { get; set; }
		public string Vol1500MlBottlesActualCount { get; set; }

		public string Vol600MlBottlesCount { get; set; }
		public string Vol600MlBottlesActualCount { get; set; }

		public string EquipmentCount { get; set; }
		public string EquipmentActualCount { get; set; }

		public string Vol19LUndelivered { get; set; }
		public IEnumerable<DriverInfoNode> InnerNodes { get; set; }

		#endregion
	}

	public enum DriversInfoExportType
	{
		[Display(Name = "Группировка по МЛ")]
		RouteListGrouping,
		[Display(Name = "Группировка по водителю и авто")]
		DriverCarGrouping
	}

	public enum DriversInfoExportPlanFactType
	{
		[Display(Name = "Факт и План")]
		PlanFact,
		[Display(Name = "Факт")]
		Fact,
		[Display(Name = "План")]
		Plan
	}
}

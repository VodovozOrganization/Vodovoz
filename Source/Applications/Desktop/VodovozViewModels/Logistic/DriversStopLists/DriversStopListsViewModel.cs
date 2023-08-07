using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Utilities.Text;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Parameters;

namespace Vodovoz.ViewModels.Logistic.DriversStopLists
{
	public class DriversStopListsViewModel : DialogTabViewModelBase
	{
		private EmployeeStatus? _filterEmployeeStatus = EmployeeStatus.IsWorking;
		private CarTypeOfUse? _filterCarTypeOfUse;
		private CarOwnType? _filterCarOwnType;

		private int _driversUnclosedRouteListsMaxCountParameter;
		private int _driversRouteListsDebtsMaxSumParameter;
		private bool _filterVisibility = true;

		public DriversStopListsViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IGeneralSettingsParametersProvider generalSettingsParametersProvider
			) : base(unitOfWorkFactory, interactiveService, navigation)
		{
			if(generalSettingsParametersProvider is null)
			{
				throw new ArgumentNullException(nameof(generalSettingsParametersProvider));
			}

			Title = "Снятие стоп-листов";

			_driversUnclosedRouteListsMaxCountParameter = generalSettingsParametersProvider.DriversUnclosedRouteListsHavingDebtMaxCount;
			_driversRouteListsDebtsMaxSumParameter = generalSettingsParametersProvider.DriversRouteListsMaxDebtSum;
		}

		#region Свойства

		[PropertyChangedAlso(nameof(CurrentDriversList), nameof(StopListsRemovalHistory))]
		public EmployeeStatus? FilterEmployeeStatus
		{
			get => _filterEmployeeStatus; 
			set => SetField(ref _filterEmployeeStatus, value);
		}

		[PropertyChangedAlso(nameof(CurrentDriversList), nameof(StopListsRemovalHistory))]
		public CarTypeOfUse? FilterCarTypeOfUse
		{
			get => _filterCarTypeOfUse;
			set => SetField(ref _filterCarTypeOfUse, value);
		}

		[PropertyChangedAlso(nameof(CurrentDriversList), nameof(StopListsRemovalHistory))]
		public CarOwnType? FilterCarOwnType
		{
			get => _filterCarOwnType;
			set => SetField(ref _filterCarOwnType, value);
		}

		public bool FilterVisibility 
		{ 
			get => _filterVisibility;
			set => SetField(ref _filterVisibility, value);
		}

		#endregion

		public List<DriverStopListRemoval> StopListsRemovalHistory => 
			UoW.GetAll<DriverStopListRemoval>()
			.OrderByDescending(x => x.Id)
			.Take(100)
			.ToList();

		public List<DriverNode> CurrentDriversList => GetCurrentDriversList();

		private List<DriverNode> GetCurrentDriversList()
		{
			Employee driverAlias = null;
			Car carAlias = null;
			CarModel carModelAlias = null;
			CarVersion carVersionAlias = null;
			DriverNode driverNodeAlias = null;
			RouteList routeListAlias = null;
			RouteListDebt routeListDebtAlias = null;
			DriverStopListRemoval driverStopListRemovalAlias = null;

			var query = UoW.Session.QueryOver(() => driverAlias)
				.JoinEntityAlias(
					() => carAlias, 
					() => carAlias.Driver.Id == driverAlias.Id 
						&& driverAlias.Category == EmployeeCategory.driver
						&& !carAlias.IsArchive, 
					JoinType.LeftOuterJoin)
				.Left.JoinAlias(() => carAlias.CarModel, () => carModelAlias)
				.Left.JoinAlias(() => carAlias.CarVersions, () => carVersionAlias)
				.Where(() =>
					carVersionAlias.StartDate <= DateTime.Now
					&& (carVersionAlias.EndDate == null || carVersionAlias.EndDate > DateTime.Now));

			if(FilterEmployeeStatus != null)
			{
				query.Where(() => driverAlias.Status == FilterEmployeeStatus);
			}

			if(FilterCarTypeOfUse != null)
			{
				query.Where(() => carModelAlias.CarTypeOfUse == FilterCarTypeOfUse);
			}

			if(FilterCarOwnType != null)
			{
				query.Where(() => carVersionAlias.CarOwnType == FilterCarOwnType);
			}

			var unclosedRouteListsDebtsSumSubquery = QueryOver.Of(() => routeListDebtAlias)
				.JoinAlias(() => routeListDebtAlias.RouteList, () => routeListAlias)
				.Where(() =>
					routeListAlias.Driver.Id == driverAlias.Id
					&& routeListAlias.Status != RouteListStatus.Closed
					&& routeListDebtAlias.Debt > 0)
				.Select(Projections.Sum(() => routeListDebtAlias.Debt));

			var unclosedRouteListsHavingDebtCountSubquery = QueryOver.Of(() => routeListDebtAlias)
				.JoinAlias(() => routeListDebtAlias.RouteList, () => routeListAlias)
				.Where(() =>
					routeListAlias.Driver.Id == driverAlias.Id
					&& routeListAlias.Status != RouteListStatus.Closed
					&& routeListDebtAlias.Debt > 0)
				.Select(Projections.Count(() => routeListDebtAlias.RouteList));

			var isStopListRemovedSubquery = QueryOver.Of(() => driverStopListRemovalAlias)
				.Where(() => driverStopListRemovalAlias.Driver.Id == driverAlias.Id
					&& driverStopListRemovalAlias.DateFrom <= DateTime.Now
					&& driverStopListRemovalAlias.DateTo > DateTime.Now)
				.Select(Projections.Conditional(
					Restrictions.Gt(Projections.Count(() => driverStopListRemovalAlias.Id), 0),
					Projections.Constant(true),
					Projections.Constant(false)
					));

			var drivers = query.SelectList(list => list
					.Select(() => driverAlias.Id).WithAlias(() => driverNodeAlias.DriverId)
					.Select(() => driverAlias.Name).WithAlias(() => driverNodeAlias.DriverName)
					.Select(() => driverAlias.LastName).WithAlias(() => driverNodeAlias.DriverLastName)
					.Select(() => driverAlias.Patronymic).WithAlias(() => driverNodeAlias.DriverPatronymic)
					.Select(() => carAlias.RegistrationNumber).WithAlias(() => driverNodeAlias.CarRegistrationNumber)
					.SelectSubQuery(unclosedRouteListsDebtsSumSubquery).WithAlias(() => driverNodeAlias.RouteListsDebtsSum)
					.SelectSubQuery(unclosedRouteListsHavingDebtCountSubquery).WithAlias(() => driverNodeAlias.UnclosedRouteListsWithDebtCount)
					.SelectSubQuery(isStopListRemovedSubquery).WithAlias(() => driverNodeAlias.IsStopListRemoved)
					.Select(Projections.Constant(_driversRouteListsDebtsMaxSumParameter).WithAlias(() => driverNodeAlias.DriversRouteListsDebtsMaxSum))
					.Select(Projections.Constant(_driversUnclosedRouteListsMaxCountParameter).WithAlias(() => driverNodeAlias.DriversUnclosedRouteListsMaxCount))
					)
				.TransformUsing(Transformers.AliasToBean<DriverNode>())
				.List<DriverNode>();

			return  drivers
				.OrderByDescending(d => d.IsDriverInStopList)
				.ThenBy(d => d.DriverLastName)
				.ToList();
		}

		#region Команды

		#region RemoveStopList
		private DelegateCommand _removeStopListCommand;
		public DelegateCommand RemoveStopListCommand
		{
			get
			{
				if(_removeStopListCommand == null)
				{
					_removeStopListCommand = new DelegateCommand(RemoveStopList, () => CanRemoveStopList);
					_removeStopListCommand.CanExecuteChangedWith(this, x => x.CanRemoveStopList);
				}
				return _removeStopListCommand;
			}
		}

		public bool CanRemoveStopList => true;

		private void RemoveStopList()
		{
			NavigationManager.OpenViewModel<DriverStopListRemovalViewModel, int>(null, 68);
		}
		#endregion

		#region Update
		private DelegateCommand _updateCommand;
		public DelegateCommand UpdateCommand
		{
			get
			{
				if(_updateCommand == null)
				{
					_updateCommand = new DelegateCommand(Update, () => CanUpdate);
					_updateCommand.CanExecuteChangedWith(this, x => x.CanUpdate);
				}
				return _updateCommand;
			}
		}

		public bool CanUpdate => true;

		private void Update()
		{

		}
		#endregion#

		#region CloseFilter
		private DelegateCommand _closeFilterCommand;
		public DelegateCommand CloseFilterCommand
		{
			get
			{
				if(_closeFilterCommand == null)
				{
					_closeFilterCommand = new DelegateCommand(CloseFilter, () => CanCloseFilter);
					_closeFilterCommand.CanExecuteChangedWith(this, x => x.CanCloseFilter);
				}
				return _closeFilterCommand;
			}
		}

		public bool CanCloseFilter => true;

		private void CloseFilter()
		{
			FilterVisibility = !FilterVisibility;
		}
		#endregion

		#endregion

		public sealed class DriverNode
		{
			public int DriverId { get; set; }
			public string DriverName { get; set; }
			public string DriverLastName { get; set; }
			public string DriverPatronymic { get; set; }
			public string CarRegistrationNumber { get; set; }
			public decimal RouteListsDebtsSum { get; set; }
			public int UnclosedRouteListsWithDebtCount { get; set; }
			public bool IsStopListRemoved { get; set; }
			public int DriversUnclosedRouteListsMaxCount { get; set; }
			public int DriversRouteListsDebtsMaxSum { get; set; }
			public string DriverFullName => PersonHelper.PersonNameWithInitials(DriverLastName, DriverName, DriverPatronymic);
			public bool IsDriverInStopList =>
				!IsStopListRemoved
				&& ((DriversRouteListsDebtsMaxSum > 0 && RouteListsDebtsSum >= DriversRouteListsDebtsMaxSum)
					|| (DriversUnclosedRouteListsMaxCount > 0 && UnclosedRouteListsWithDebtCount >= DriversUnclosedRouteListsMaxCount));
		}
	}
}

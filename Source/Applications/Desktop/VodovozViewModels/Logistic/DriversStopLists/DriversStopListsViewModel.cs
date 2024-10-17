using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.Settings.Common;
using Vodovoz.ViewModels.ViewModels.Organizations;

namespace Vodovoz.ViewModels.Logistic.DriversStopLists
{
	public partial class DriversStopListsViewModel : DialogTabViewModelBase
	{
		private readonly ICommonServices _commonServices;
		private readonly ViewModelEEVMBuilder<Subdivision> _subdivisionViewModelEEVMBuilder;
		private readonly IPermissionResult _currentUserRouteListRemovalPermissions;
		private readonly int _driversUnclosedRouteListsMaxCountParameter;
		private readonly int _driversRouteListsDebtsMaxSumParameter;

		private EmployeeStatus? _filterEmployeeStatus = EmployeeStatus.IsWorking;
		private CarTypeOfUse? _filterCarTypeOfUse;
		private CarOwnType? _filterCarOwnType;
		private Subdivision _filterSubdivision;
		private DriversSortOrder _currentDriversListSortOrder;
		private bool _isExcludeVisitingMasters;
		private DelegateCommand _closeFilterCommand;

		private bool _filterVisibility = true;
		private DriverNode _selectedDriverNode;

		public DriversStopListsViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			ICommonServices commonServices,
			IGeneralSettings generalSettingsSettings,
			ViewModelEEVMBuilder<Subdivision> subdivisionViewModelEEVMBuilder
			) : base(unitOfWorkFactory, interactiveService, navigation)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_subdivisionViewModelEEVMBuilder = subdivisionViewModelEEVMBuilder ?? throw new ArgumentNullException(nameof(subdivisionViewModelEEVMBuilder));

			if(generalSettingsSettings is null)
			{
				throw new ArgumentNullException(nameof(generalSettingsSettings));
			}

			Title = "Снятие стоп-листов";

			FilterSubdivisionEntityEntryViewModel = CreateSubdivisionEntityEntryViewModel();

			_currentUserRouteListRemovalPermissions =
				_commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(DriverStopListRemoval));

			_driversUnclosedRouteListsMaxCountParameter =
				generalSettingsSettings.DriversUnclosedRouteListsHavingDebtMaxCount;

			_driversRouteListsDebtsMaxSumParameter =
				generalSettingsSettings.DriversRouteListsMaxDebtSum;

			NotifyConfiguration.Instance.BatchSubscribeOnEntity<DriverStopListRemoval>((s) => UpdateCommand?.Execute());
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

		[PropertyChangedAlso(nameof(CurrentDriversList), nameof(StopListsRemovalHistory))]
		public Subdivision FilterSubdivision
		{
			get => _filterSubdivision;
			set => SetField(ref _filterSubdivision, value);
		}

		[PropertyChangedAlso(nameof(CurrentDriversList), nameof(StopListsRemovalHistory))]
		public bool IsExcludeVisitingMasters
		{
			get => _isExcludeVisitingMasters;
			set => SetField(ref _isExcludeVisitingMasters, value);
		}

		[PropertyChangedAlso(nameof(CurrentDriversList))]
		public DriversSortOrder CurrentDriversListSortOrder
		{
			get => _currentDriversListSortOrder;
			set => SetField(ref _currentDriversListSortOrder, value);
		}

		public bool FilterVisibility
		{
			get => _filterVisibility;
			set => SetField(ref _filterVisibility, value);
		}

		[PropertyChangedAlso(nameof(CanCreateStopListRemoval))]
		public DriverNode SelectedDriverNode
		{
			get => _selectedDriverNode;
			set => SetField(ref _selectedDriverNode, value);
		}

		public bool CanCreateStopListRemoval =>
			_currentUserRouteListRemovalPermissions.CanCreate
			&& SelectedDriverNode != null
			&& !SelectedDriverNode.IsStopListRemoved;

		public bool DialogVisibility =>
			_currentUserRouteListRemovalPermissions.CanRead
			|| _currentUserRouteListRemovalPermissions.CanCreate;

		public IEntityEntryViewModel FilterSubdivisionEntityEntryViewModel { get; }

		#endregion

		public List<DriverStopListRemoval> StopListsRemovalHistory => GetStopListsRemovalHistory();

		public List<DriverNode> CurrentDriversList => GetCurrentDriversList();

		private List<DriverNode> GetCurrentDriversList()
		{
			Employee driverAlias = null;
			Car carAlias = null;
			CarModel carModelAlias = null;
			DriverNode driverNodeAlias = null;
			RouteList routeListAlias = null;
			RouteListDebt routeListDebtAlias = null;
			DriverStopListRemoval driverStopListRemovalAlias = null;

			var query = UoW.Session.QueryOver(() => driverAlias)
				.JoinEntityAlias(
					() => carAlias,
					() => carAlias.Driver.Id == driverAlias.Id,
					JoinType.LeftOuterJoin)
				.Left.JoinAlias(() => carAlias.CarModel, () => carModelAlias)
				.Where(() => driverAlias.Category == EmployeeCategory.driver);

			if(FilterEmployeeStatus != null)
			{
				query.Where(() => driverAlias.Status == FilterEmployeeStatus);
			}

			if(FilterCarTypeOfUse != null)
			{
				query.Where(() => driverAlias.DriverOfCarTypeOfUse == FilterCarTypeOfUse);
			}

			if(FilterCarOwnType != null)
			{
				query.Where(() => driverAlias.DriverOfCarOwnType == FilterCarOwnType);
			}

			if(IsExcludeVisitingMasters)
			{
				query.Where(() => !driverAlias.VisitingMaster);
			}

			if(FilterSubdivision != null)
			{
				query.Where(() => driverAlias.Subdivision.Id == FilterSubdivision.Id);
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
				.List<DriverNode>()
				.ToList();

			if(CurrentDriversListSortOrder == DriversSortOrder.ByStopList)
			{
				drivers = drivers
					.OrderByDescending(d => d.IsDriverInStopList)
					.ThenBy(d => d.DriverLastName)
					.ToList();
			}
			else if(CurrentDriversListSortOrder == DriversSortOrder.ByUnclosedRouteListsCount)
			{
				drivers = drivers
					.OrderByDescending(d => d.UnclosedRouteListsWithDebtCount)
					.ThenBy(d => d.DriverLastName)
					.ToList();
			}
			else
			{
				throw new NotImplementedException("Порядок сортировки не поддерживается");
			}

			return drivers;
		}

		private List<DriverStopListRemoval> GetStopListsRemovalHistory()
		{
			DriverStopListRemoval driverStopListRemovalAlias = null;
			Employee driverAlias = null;

			var query = UoW.Session.QueryOver(() => driverStopListRemovalAlias)
				.Left.JoinAlias(() => driverStopListRemovalAlias.Driver, () => driverAlias);

			if(FilterEmployeeStatus != null)
			{
				query.Where(() => driverAlias.Status == FilterEmployeeStatus);
			}

			if(FilterCarTypeOfUse != null)
			{
				query.Where(() => driverAlias.DriverOfCarTypeOfUse == FilterCarTypeOfUse);
			}

			if(FilterCarOwnType != null)
			{
				query.Where(() => driverAlias.DriverOfCarOwnType == FilterCarOwnType);
			}

			if(IsExcludeVisitingMasters)
			{
				query.Where(() => !driverAlias.VisitingMaster);
			}

			if(FilterSubdivision != null)
			{
				query.Where(() => driverAlias.Subdivision.Id == FilterSubdivision.Id);
			}

			var driversStopListsRemovals = query
				.OrderBy(() => driverStopListRemovalAlias.Id).Desc()
				.Take(100)
				.List<DriverStopListRemoval>()
				.ToList();

			return driversStopListsRemovals;
		}

		private IEntityEntryViewModel CreateSubdivisionEntityEntryViewModel()
		{
			var viewModel = _subdivisionViewModelEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(this, x => x.FilterSubdivision)
				.UseViewModelJournalAndAutocompleter<SubdivisionsJournalViewModel>()
				.UseViewModelDialog<SubdivisionViewModel>()
				.Finish();

			viewModel.CanViewEntity = _commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Subdivision)).CanUpdate;

			return viewModel;
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
			if(!CanCreateStopListRemoval)
			{
				return;
			}

			NavigationManager.OpenViewModel<DriverStopListRemovalViewModel, int>(null, SelectedDriverNode.DriverId);
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
			OnPropertyChanged(nameof(CurrentDriversList));
			OnPropertyChanged(nameof(StopListsRemovalHistory));
		}
		#endregion#

		#region CloseFilter

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

		public override void Dispose()
		{
			NotifyConfiguration.Instance.UnsubscribeAll(this);
			base.Dispose();
		}
	}
}

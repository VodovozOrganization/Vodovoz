using NetTopologySuite.Geometries;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Utilities.Text;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using Vodovoz.Core.Domain;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Logistic.FastDelivery;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Sale;
using Vodovoz.NHibernateProjections.Logistics;
using Vodovoz.Settings.Delivery;
using Vodovoz.Settings.Employee;
using Vodovoz.Settings.Logistics;
using Vodovoz.SidePanel;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class CarsMonitoringViewModel : DialogTabViewModelBase, ICustomWidthInfoProvider
	{
		private readonly ITrackRepository _trackRepository;
		private readonly IRouteListRepository _routeListRepository;
		private readonly IScheduleRestrictionRepository _scheduleRestrictionRepository;
		private readonly IDeliveryRulesSettings _deliveryRulesSettings;
		private readonly IDeliveryRepository _deliveryRepository;
		private readonly IGtkTabsOpener _gtkTabsOpener;
		private readonly IGeographicGroupRepository _geographicGroupRepository;
		private readonly IGeographicGroupSettings _geographicGroupSettings;

		private bool _showCarCirclesOverlay = false;
		private bool _showDistrictsOverlay = false;
		private bool _showFastDeliveryOnly = false;
		private bool _showActualFastDeliveryOnly;
		private bool _showAddresses = false;
		private bool _separateVindowOpened = false;
		private bool _canShowAddresses = true;

		private readonly Color[] _availableTrackColors = new Color[]
		{
			Color.Red,
			Color.Green,
			Color.Blue,
			Color.Coral,
			Color.DarkOrange,
			Color.DarkRed,
			Color.DeepPink,
			Color.HotPink,
			Color.GreenYellow,
			Color.Gold,
		};

		private readonly Color _districtFillColor = Color.Transparent;
		private readonly Color _fastDeliveryCircleFillColor = Color.OrangeRed;
		private TimeSpan _fastDeliveryTime;

		private bool _canOpenKeepingTab;
		private bool _canEditRouteListFastDeliveryMaxDistance;
		private bool _canEditRouteListMaxFastDeliveryOrders;
		private bool _showHistory;
		private DateTime _historyDate;
		private TimeSpan _historyHour;

		private int _fastDeliveryDistrictsLastVersionId = -1;
		private IList<District> _cachedFastDeliveryDistricts;
		private IList<GeoGroup> _geogroups;
		private GeoGroup _selectedGeoGroup;
		private bool _hideTrucks;

		public CarsMonitoringViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			ITrackRepository trackRepository,
			IRouteListRepository routeListRepository,
			IScheduleRestrictionRepository scheduleRestrictionRepository,
			IDeliveryRulesSettings deliveryRulesSettings,
			IDeliveryRepository deliveryRepository,
			IGtkTabsOpener gtkTabsOpener,
			IGeographicGroupRepository geographicGroupRepository,
			IGeographicGroupSettings geographicGroupSettings,
			IEmployeeSettings employeeSettings)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_trackRepository = trackRepository ?? throw new ArgumentNullException(nameof(trackRepository));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_scheduleRestrictionRepository = scheduleRestrictionRepository ?? throw new ArgumentNullException(nameof(scheduleRestrictionRepository));
			_deliveryRulesSettings = deliveryRulesSettings ?? throw new ArgumentNullException(nameof(deliveryRulesSettings));
			_deliveryRepository = deliveryRepository ?? throw new ArgumentNullException(nameof(deliveryRepository));
			_gtkTabsOpener = gtkTabsOpener ?? throw new ArgumentNullException(nameof(gtkTabsOpener));
			_geographicGroupRepository = geographicGroupRepository ?? throw new ArgumentNullException(nameof(geographicGroupRepository));
			_geographicGroupSettings = geographicGroupSettings;

			TabName = "Мониторинг";

			MaxDaysForNewbieDriver = employeeSettings.MaxDaysForNewbieDriver;

			CarsOverlayId = "cars";
			TracksOverlayId = "tracks";
			FastDeliveryOverlayId = "fast delivery";
			FastDeliveryDistrictsOverlayId = "districts";

			UoW.Session.DefaultReadOnly = true;

			CarRefreshInterval = TimeSpan.FromSeconds(_deliveryRulesSettings.CarsMonitoringResfreshInSeconds);

			DefaultMapCenterPosition = new Coordinate(59.93900, 30.31646);
			DriverDisconnectedTimespan = TimeSpan.FromMinutes(-(int)_deliveryRulesSettings.MaxTimeOffsetForLatestTrackPoint.TotalMinutes);

			var timespanRange = new List<TimeSpan>();

			for(int i = 0; i < 24; i++)
			{
				timespanRange.Add(TimeSpan.FromHours(i));
			}

			HistoryHours = timespanRange;

			_historyDate = DateTime.Today;
			_historyHour = TimeSpan.FromHours(9);

			_fastDeliveryTime = _deliveryRulesSettings.MaxTimeForFastDelivery;

			FastDeliveryDistricts = new ObservableCollection<District>();
			RouteListAddresses = new ObservableCollection<RouteListAddressNode>();
			WorkingDrivers = new ObservableCollection<WorkingDriverNode>();
			SelectedWorkingDrivers = new ObservableCollection<WorkingDriverNode>();
			LastDriverPositions = new ObservableCollection<DriverPositionWithFastDeliveryRadius>();

			OpenKeepingDialogCommand = new DelegateCommand<int>(OpenRouteListKeepingTab);
			ChangeRouteListFastDeliveryMaxDistanceCommand = new DelegateCommand<int>(OpenChangeRouteListFastDeliveryMaxDistanceDlg);
			ChangeRouteListMaxFastDeliveryOrdersCommand = new DelegateCommand<int>(OpenChangeRouteListMaxFastDeliveryOrders);
			OpenTrackPointsJournalTabCommand = new DelegateCommand(OpenTrackPointJournalTab);
			RefreshWorkingDriversCommand = new DelegateCommand(RefreshWorkingDrivers);
			RefreshRouteListAddressesCommand = new DelegateCommand<int>(RefreshRouteListAddresses);
			RefreshFastDeliveryDistrictsCommand = new DelegateCommand(RefreshFastDeliveryDistricts);
			RefreshLastDriverPositionsCommand = new DelegateCommand(RefreshLastDriverPositions);
			RefreshAllCommand = new DelegateCommand(RefreshAll);

			NotifyConfiguration.Instance.BatchSubscribeOnEntity<RouteList>(RoutelistEntityConfigEntityUpdated);
		}

		#region Full properties

		public bool ShowDistrictsOverlay
		{
			get => _showDistrictsOverlay;
			set
			{
				if(SetField(ref _showDistrictsOverlay, value))
				{
					RefreshFastDeliveryDistrictsCommand?.Execute();
				}
			}
		}

		public bool ShowCarCirclesOverlay
		{
			get => _showCarCirclesOverlay;
			set
			{
				SetField(ref _showCarCirclesOverlay, value);
				ShowDistrictsOverlay = value;
			}
		}

		[PropertyChangedAlso(nameof(CoveragePercentBeforeText))]
		public bool ShowActualFastDeliveryOnly
		{
			get => _showActualFastDeliveryOnly;
			set
			{
				if(SetField(ref _showActualFastDeliveryOnly, value))
				{
					RefreshWorkingDriversCommand?.Execute();
					if(ShowDistrictsOverlay)
					{
						RefreshFastDeliveryDistrictsCommand?.Execute();
					}
				}
			}

		}

		public bool ShowFastDeliveryOnly
		{
			get => _showFastDeliveryOnly;
			set
			{
				SetField(ref _showFastDeliveryOnly, value);
				if(!value)
				{
					ShowCarCirclesOverlay = false;
					ShowHistory = false;
					ShowActualFastDeliveryOnly = false;
				}
				RefreshWorkingDriversCommand?.Execute();
			}
		}

		public bool ShowAddresses
		{
			get => _showAddresses;
			set => SetField(ref _showAddresses, value);
		}

		public bool SeparateVindowOpened
		{
			get => _separateVindowOpened;
			set
			{
				SetField(ref _separateVindowOpened, value);
				CanShowAddresses = !value;
			}
		}

		public bool CanShowAddresses
		{
			get => _canShowAddresses;
			private set
			{
				SetField(ref _canShowAddresses, value);
				if(!value)
				{
					ShowAddresses = false;
				}
			}
		}

		public bool CanOpenKeepingTab
		{
			get => _canOpenKeepingTab;
			set => SetField(ref _canOpenKeepingTab, value);
		}

		public bool CanEditRouteListFastDeliveryMaxDistance
		{
			get => _canEditRouteListFastDeliveryMaxDistance;
			set => SetField(ref _canEditRouteListFastDeliveryMaxDistance, value);
		}

		public bool CanEditRouteListMaxFastDeliveryOrders
		{
			get => _canEditRouteListMaxFastDeliveryOrders;
			set => SetField(ref _canEditRouteListMaxFastDeliveryOrders, value);
		}

		public bool ShowHistory
		{
			get => _showHistory;
			set
			{
				SetField(ref _showHistory, value);
				RefreshWorkingDriversCommand?.Execute();
				if(ShowDistrictsOverlay)
				{
					RefreshFastDeliveryDistrictsCommand?.Execute();
				}
			}
		}

		public bool HideTrucks
		{
			get => _hideTrucks;
			set
			{
				SetField(ref _hideTrucks, value);
				RefreshWorkingDriversCommand?.Execute();
			}
		}

		public DateTime HistoryDate
		{
			get => _historyDate;
			set
			{
				SetField(ref _historyDate, value);
				if(ShowHistory)
				{
					RefreshWorkingDriversCommand?.Execute();
					if(ShowDistrictsOverlay)
					{
						RefreshFastDeliveryDistrictsCommand?.Execute();
					}
				}
			}
		}

		public TimeSpan HistoryHour
		{
			get => _historyHour;
			set
			{
				SetField(ref _historyHour, value);
				if(ShowHistory)
				{
					RefreshWorkingDriversCommand?.Execute();
					if(ShowDistrictsOverlay)
					{
						RefreshFastDeliveryDistrictsCommand?.Execute();
					}
				}
			}
		}

		public IList<GeoGroup> GeoGroups => _geogroups 
		    ?? (_geogroups = _geographicGroupRepository.GeographicGroupsWithoutEast(UoW, _geographicGroupSettings));

		public GeoGroup SelectedGeoGroup
		{
			get => _selectedGeoGroup;
			set
			{
				if(SetField(ref _selectedGeoGroup, value))
				{
					RefreshWorkingDriversCommand?.Execute();
				}
			}
		}

		#endregion

		#region Readoly Properties
		public override bool HasChanges => false;

		public DateTime HistoryDateTime => HistoryDate.Add(HistoryHour);

		public TimeSpan DriverDisconnectedTimespan { get; }

		public TimeSpan CarRefreshInterval { get; }

		public string CarsOverlayId { get; }

		public string TracksOverlayId { get; }

		public string FastDeliveryOverlayId { get; }

		public string FastDeliveryDistrictsOverlayId { get; }

		public Coordinate DefaultMapCenterPosition { get; }

		public IEnumerable<TimeSpan> HistoryHours { get; }

		public TimeSpan FastDeliveryTime => _fastDeliveryTime;

		public Color DistrictFillColor => _districtFillColor;

		public Color FastDeliveryCircleFillColor => _fastDeliveryCircleFillColor;

		public Color[] AvailableTrackColors => _availableTrackColors;

		public string CoveragePercentString => $"{CoveragePercent:P}";

		public string CoveragePercentBeforeText => ShowActualFastDeliveryOnly ? "Доступно для заказа" : "Процент покрытия";

		[PropertyChangedAlso(nameof(CoveragePercentString))]
		public double CoveragePercent => DistanceCalculator.CalculateCoveragePercent(
			FastDeliveryDistricts.Select(fdd => fdd.DistrictBorder).ToList(),
			LastDriverPositions.ToList());

		#endregion

		#region Observable Collections
		public ObservableCollection<District> FastDeliveryDistricts { get; }

		public ObservableCollection<WorkingDriverNode> SelectedWorkingDrivers { get; }

		public ObservableCollection<WorkingDriverNode> WorkingDrivers { get; }

		public ObservableCollection<RouteListAddressNode> RouteListAddresses { get; }

		public ObservableCollection<DriverPositionWithFastDeliveryRadius> LastDriverPositions { get; }
		#endregion

		#region Commands
		public DelegateCommand<int> OpenKeepingDialogCommand { get; }

		public DelegateCommand<int> ChangeRouteListFastDeliveryMaxDistanceCommand { get; }

		public DelegateCommand<int> ChangeRouteListMaxFastDeliveryOrdersCommand { get; }

		public DelegateCommand OpenTrackPointsJournalTabCommand { get; }

		public DelegateCommand RefreshWorkingDriversCommand { get; }

		public DelegateCommand<int> RefreshRouteListAddressesCommand { get; }

		public DelegateCommand RefreshFastDeliveryDistrictsCommand { get; }

		public DelegateCommand RefreshLastDriverPositionsCommand { get; }

		public DelegateCommand RefreshAllCommand { get; }

		#endregion

		public PanelViewType[] InfoWidgets => new[] { PanelViewType.CarsMonitoringInfoPanelView };

		public int? WidthRequest => 420;

		#region Events
		public event Action FastDeliveryDistrictChanged;

		public event Action WorkingDriversChanged;

		public event Action RouteListAddressesChanged;

		public event EventHandler<CurrentObjectChangedArgs> CurrentObjectChanged;
		#endregion

		#region Command Handlers
		private void OpenRouteListKeepingTab(int routeListId)
		{
			NavigationManager.OpenViewModel<RouteListKeepingViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(routeListId));
		}

		private void OpenChangeRouteListFastDeliveryMaxDistanceDlg(int routeListId)
		{
			NavigationManager.OpenViewModel<RouteListFastDeliveryMaxDistanceViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(routeListId));
		}

		private void OpenChangeRouteListMaxFastDeliveryOrders(int routeListId)
		{
			NavigationManager.OpenViewModel<RouteListMaxFastDeliveryOrdersViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(routeListId));
		}

		private void OpenTrackPointJournalTab()
		{
			NavigationManager.OpenViewModel<TrackPointJournalViewModel>(this);
		}

		public void RefreshWorkingDrivers()
		{
			SelectedWorkingDrivers.Clear();

			WorkingDriverNode resultAlias = null;
			Employee driverAlias = null;
			RouteList routeListAlias = null;
			RouteListItem routeListItemAlias = null;
			Car carAlias = null;
			CarVersion carVersionAlias = null;
			CarModel carModelAlias = null;
			Track trackAlias = null;

			Domain.Orders.Order orderAlias = null;
			OrderItem ordItemsAlias = null;
			Nomenclature nomenclatureAlias = null;

			var completedSubquery = QueryOver.Of<RouteListItem>()
				.Where(i => i.RouteList.Id == routeListAlias.Id)
				.Where(i => i.Status != RouteListItemStatus.EnRoute)
				.Where(i => i.Status != RouteListItemStatus.Transfered);

			if(ShowHistory)
			{
				completedSubquery.JoinAlias(rli => rli.Order, () => orderAlias)
					.Where(Restrictions.Le(Projections.Property(() => orderAlias.TimeDelivered), HistoryDateTime));
			}

			completedSubquery.Select(Projections.RowCount());

			var addressesSubquery = QueryOver.Of<RouteListItem>()
				.Where(i => i.RouteList.Id == routeListAlias.Id)
				.Where(i => i.Status != RouteListItemStatus.Transfered);

			if(ShowHistory)
			{
				addressesSubquery.And(ua => ua.CreationDate <= HistoryDateTime);
			}

			addressesSubquery.Select(Projections.RowCount());

			var uncompletedBottlesSubquery = QueryOver.Of<RouteListItem>(() => routeListItemAlias)  // Запрашивает количество ещё не доставленных бутылей.
				.Where(i => i.RouteList.Id == routeListAlias.Id);

			if(ShowHistory)
			{
				uncompletedBottlesSubquery.Where(Restrictions.Or(
						Restrictions.Ge(Projections.Property(() => orderAlias.TimeDelivered), HistoryDateTime),
						Restrictions.Eq(Projections.Property(() => routeListItemAlias.Status), RouteListItemStatus.EnRoute)))
					.And(ua => ua.CreationDate <= HistoryDateTime);
			}
			else
			{
				uncompletedBottlesSubquery.Where(i => i.Status == RouteListItemStatus.EnRoute);
			}

			uncompletedBottlesSubquery.JoinAlias(rli => rli.Order, () => orderAlias)
				.JoinAlias(() => orderAlias.OrderItems, () => ordItemsAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(() => ordItemsAlias.Nomenclature, () => nomenclatureAlias)
			   	.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water && nomenclatureAlias.TareVolume == TareVolume.Vol19L)
				.Select(Projections.Sum(() => ordItemsAlias.Count));

			IProjection isCompanyCarProjection = CarProjections.GetIsCompanyCarProjection();

			DateTime selectedDateTime = ShowHistory ? HistoryDateTime : DateTime.Now;

			var water19LSubquery = QueryOver.Of<DeliveryFreeBalanceOperation>()
				.Where(o => o.RouteList.Id == routeListAlias.Id)
				.And(o => o.OperationTime <= selectedDateTime)
				.JoinQueryOver(o => o.Nomenclature)
				.Where(n => n.Category == NomenclatureCategory.water
				            && n.TareVolume == TareVolume.Vol19L)
				.Select(Projections.Sum<DeliveryFreeBalanceOperation>(o => o.Amount));

			var query = UoW.Session.QueryOver<RouteList>(() => routeListAlias)
				.JoinEntityAlias(() => trackAlias, () => routeListAlias.Id == trackAlias.RouteList.Id, NHibernate.SqlCommand.JoinType.LeftOuterJoin);

			var lastTrackPoint = QueryOver.Of<TrackPoint>()
				.Left.JoinAlias(x => x.Track, () => trackAlias)
				.Where(() => trackAlias.RouteList.Id == routeListAlias.Id)
				.OrderBy(x => x.TimeStamp).Desc
				.Select(x => x.TimeStamp)
				.Take(1);

			if(ShowFastDeliveryOnly)
			{
				query.Where(() => routeListAlias.AdditionalLoadingDocument != null);
			}

			if(HideTrucks)
			{
				query.JoinAlias(() => carAlias.CarModel, () => carModelAlias)
					.Where(() => carModelAlias.CarTypeOfUse != CarTypeOfUse.Truck);
			}

			var dateForRouteListMaxFastDeliveryOrders = ShowHistory ? HistoryDateTime : DateTime.Now;

			var routeListMaxFastDeliveryOrdersSubquery = QueryOver.Of<RouteListMaxFastDeliveryOrders>()
				.Where(
					m => m.RouteList.Id == routeListAlias.Id 
					     && m.StartDate <= dateForRouteListMaxFastDeliveryOrders 
					     && (m.EndDate == null || m.EndDate > dateForRouteListMaxFastDeliveryOrders))
				.Select(m => m.MaxOrders)
				.OrderBy(m => m.StartDate).Desc
				.Take(1);

			var routeListMaxFastDeliveryOrdersProjection = Projections.Conditional(Restrictions.IsNull(Projections.SubQuery(routeListMaxFastDeliveryOrdersSubquery)),
				Projections.Constant(_deliveryRulesSettings.MaxFastOrdersPerSpecificTime),
				Projections.SubQuery(routeListMaxFastDeliveryOrdersSubquery));

			if(ShowActualFastDeliveryOnly)
			{
				var specificTimeForFastOrdersCount = (int)_deliveryRulesSettings.SpecificTimeForMaxFastOrdersCount.TotalMinutes;

				TrackPoint trackPointAlias = null;

				var addressCountSubquery = QueryOver.Of(() => routeListItemAlias)
					.Inner.JoinAlias(() => routeListItemAlias.Order, () => orderAlias)
					.Where(() => routeListItemAlias.RouteList.Id == routeListAlias.Id)
					.And(() => orderAlias.IsFastDelivery);

				var trackPointSubQuery = QueryOver.Of(() => trackPointAlias)
					.JoinAlias(() => trackPointAlias.Track, () => trackAlias)
					.Where(() => trackAlias.RouteList.Id == routeListAlias.Id);

				if(ShowHistory)
				{
					addressCountSubquery.Where(Restrictions.Or(
							Restrictions.Ge(Projections.Property(() => orderAlias.TimeDelivered), HistoryDateTime),
							Restrictions.Eq(Projections.Property(() => routeListItemAlias.Status), RouteListItemStatus.EnRoute)))
						.And(() => routeListItemAlias.CreationDate <= HistoryDateTime);
				}
				else
				{
					addressCountSubquery.Where(() => routeListItemAlias.Status == RouteListItemStatus.EnRoute)
						.And(Restrictions.GtProperty(
							Projections.Property(() => routeListItemAlias.CreationDate),
							Projections.SqlFunction(
								new SQLFunctionTemplate(NHibernateUtil.DateTime,
									$"TIMESTAMPADD(MINUTE, -{specificTimeForFastOrdersCount}, CURRENT_TIMESTAMP)"),
								NHibernateUtil.DateTime)));
				}

				addressCountSubquery.Select(Projections.Count(() => routeListItemAlias.Id));

				query.Where(Restrictions.GtProperty(routeListMaxFastDeliveryOrdersProjection, Projections.SubQuery(addressCountSubquery)));

				trackPointSubQuery.Where(Restrictions.Le(Projections.Property(() => trackPointAlias.ReceiveTimeStamp), selectedDateTime))
					.Where(Restrictions.Ge(Projections.Property(() => trackPointAlias.TimeStamp), selectedDateTime.Add(DriverDisconnectedTimespan)))
					.Select(Projections.Property(() => trackPointAlias.ReceiveTimeStamp))
					.Take(1);

				query.WithSubquery.WhereValue(selectedDateTime.Add(DriverDisconnectedTimespan)).Le(trackPointSubQuery);
			}

			query.JoinAlias(rl => rl.Driver, () => driverAlias)
				.JoinAlias(rl => rl.Car, () => carAlias)
				.JoinEntityAlias(() => carVersionAlias,
					() => carVersionAlias.Car.Id == carAlias.Id
						&& carVersionAlias.StartDate <= routeListAlias.Date
						&& (carVersionAlias.EndDate == null || carVersionAlias.EndDate >= routeListAlias.Date))
				.Where(rl => rl.Driver != null)
				.Where(rl => rl.Car != null);


			var dateForRouteListFastDeliveryMaxDistanceSubquery = DateTime.Now;

			if(ShowHistory)
			{
				var routeListHistoryStatuses = new RouteListStatus[]
				{
					RouteListStatus.EnRoute,
					RouteListStatus.Delivered,
					RouteListStatus.OnClosing,
					RouteListStatus.MileageCheck,
					RouteListStatus.Closed
				};

				query.Where(
					Restrictions.And(
						Restrictions.Or(
							Restrictions.Ge(Projections.Property(() => routeListAlias.DeliveredAt), HistoryDateTime),
							Restrictions.IsNull(Projections.Property(() => routeListAlias.DeliveredAt))),
						Restrictions.In(Projections.Property(() => routeListAlias.Status), routeListHistoryStatuses)))
					.And(() => routeListAlias.Date == HistoryDateTime.Date);

				dateForRouteListFastDeliveryMaxDistanceSubquery = HistoryDateTime;
			}
			else
			{
				query.Where(rl => rl.Status == RouteListStatus.EnRoute);
			}

			var routeListFastDeliveryMaxDistanceSubquery = QueryOver.Of<RouteListFastDeliveryMaxDistance>()
				.Where(
					d => d.RouteList.Id == routeListAlias.Id 
					&& d.StartDate <= dateForRouteListFastDeliveryMaxDistanceSubquery 
					&& (d.EndDate == null || d.EndDate > dateForRouteListFastDeliveryMaxDistanceSubquery))
				.Select(d => d.Distance)
				.OrderBy(d => d.StartDate).Desc
				.Take(1);

			if(SelectedGeoGroup != null)
			{
				GeoGroup geographicGroupAlias = null;

				query.Inner.JoinAlias(() => routeListAlias.GeographicGroups, () => geographicGroupAlias, () => geographicGroupAlias.Id == SelectedGeoGroup.Id);
			}

			var result = query.SelectList(list => list
					.Select(() => driverAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => driverAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => driverAlias.LastName).WithAlias(() => resultAlias.LastName)
					.Select(() => driverAlias.Patronymic).WithAlias(() => resultAlias.Patronymic)
					.Select(() => driverAlias.FirstWorkDay).WithAlias(() => resultAlias.FirstWorkDay)
					.Select(() => carAlias.RegistrationNumber).WithAlias(() => resultAlias.CarNumber)
					.Select(isCompanyCarProjection).WithAlias(() => resultAlias.IsVodovozAuto)
					.Select(() => routeListAlias.Id).WithAlias(() => resultAlias.RouteListNumber)
					.SelectSubQuery(routeListFastDeliveryMaxDistanceSubquery).WithAlias(() => resultAlias.FastDeliveryMaxDistance)
					.Select(routeListMaxFastDeliveryOrdersProjection).WithAlias(() => resultAlias.MaxFastDeliveryOrders)
					.SelectSubQuery(addressesSubquery).WithAlias(() => resultAlias.AddressesAll)
					.SelectSubQuery(completedSubquery).WithAlias(() => resultAlias.AddressesCompleted)
					.Select(() => trackAlias.Id).WithAlias(() => resultAlias.TrackId)
					.SelectSubQuery(lastTrackPoint).WithAlias(() => resultAlias.LastTrackPointTime)
					.SelectSubQuery(uncompletedBottlesSubquery).WithAlias(() => resultAlias.BottlesLeft)
					.SelectSubQuery(water19LSubquery).WithAlias(() => resultAlias.Water19LReserve))
				.TransformUsing(Transformers.AliasToBean<WorkingDriverNode>())
				.SetTimeout(180)
				.List<WorkingDriverNode>();

			WorkingDrivers.Clear();

			int rowNum = 0;

			WorkingDriverNode savedRow;

			var driversNodes = result.GroupBy(x => x.Id).OrderBy(x => x.First().ShortName).ToList();

			var disconnectedDateTime = (ShowHistory ? HistoryDateTime : DateTime.Now).Add(DriverDisconnectedTimespan);

			for(var i = 0; i < driversNodes.Count; i++)
			{
				savedRow = driversNodes[i].First();

				savedRow.RouteListsIds = driversNodes[i].ToDictionary(x => x.RouteListNumber, x => x.TrackId);
				savedRow.RouteListsOnlineState = driversNodes[i]
					.Select(x => (x.RouteListNumber, Online: x.TrackId != null
						&& x.LastTrackPointTime >= disconnectedDateTime))
					.ToDictionary(x => x.RouteListNumber, x => x.Online);
				savedRow.AddressesAll = driversNodes[i].Sum(x => x.AddressesAll);
				savedRow.AddressesCompleted = driversNodes[i].Sum(x => x.AddressesCompleted);
				savedRow.Water19LReserve = driversNodes[i].Sum(x => x.Water19LReserve);
				savedRow.RowNumber = ++rowNum;
				if (savedRow.FastDeliveryMaxDistance == null)
				{
					savedRow.FastDeliveryMaxDistance = (decimal)_deliveryRepository.GetMaxDistanceToLatestTrackPointKmFor(ShowHistory ? HistoryDateTime : DateTime.Now);
				}

				savedRow.MaxFastDeliveryOrders = driversNodes[i].Max(x => x.MaxFastDeliveryOrders);

				WorkingDrivers.Add(savedRow);
			}
			WorkingDriversChanged?.Invoke();
			CanOpenKeepingTab = SelectedWorkingDrivers.Any();
		}

		public void RefreshRouteListAddresses(int driverId)
		{
			RouteListAddresses.Clear();

			RouteListAddressNode resultAlias = null;
			Employee driverAlias = null;
			RouteList routeListAlias = null;
			RouteListItem routeListItemAlias = null;
			Domain.Orders.Order orderAlias = null;

			var routeListAddresses = UoW.Session.QueryOver<RouteListItem>(() => routeListItemAlias)
				.JoinAlias(rli => rli.RouteList, () => routeListAlias)
				.JoinAlias(rli => rli.RouteList.Driver, () => driverAlias)
				.JoinAlias(rli => rli.Order, () => orderAlias)
				.Where(() => routeListAlias.Status == RouteListStatus.EnRoute)
				.Where(() => routeListAlias.Driver.Id == driverId)
				.SelectList(list => list
					.Select(() => routeListItemAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(Projections.Entity(() => orderAlias)).WithAlias(() => resultAlias.Order)
					.Select(Projections.Entity(() => routeListItemAlias)).WithAlias(() => resultAlias.RouteListItem)
					.Select(() => routeListAlias.Id).WithAlias(() => resultAlias.RouteListNumber)
					.Select(() => routeListItemAlias.Status).WithAlias(() => resultAlias.Status)
					.Select(() => orderAlias.DeliverySchedule).WithAlias(() => resultAlias.Time)
					.Select(() => orderAlias.DeliveryPoint).WithAlias(() => resultAlias.DeliveryPoint))
				.TransformUsing(Transformers.AliasToBean<RouteListAddressNode>())
				.List<RouteListAddressNode>();

			foreach(var routeListAddress in routeListAddresses)
			{
				RouteListAddresses.Add(routeListAddress);
			}
			
			RouteListAddressesChanged?.Invoke();
		}

		private void RefreshFastDeliveryDistricts()
		{
			FastDeliveryDistricts.Clear();

			if(ShowDistrictsOverlay)
			{
				IList<District> districts;
				if(ShowHistory)
				{
					var historyDistrictsVersionId = _scheduleRestrictionRepository
						.GetDistrictsForFastDeliveryHistoryVersionId(UoW, HistoryDateTime);

					if(_fastDeliveryDistrictsLastVersionId == historyDistrictsVersionId)
					{
						districts = _cachedFastDeliveryDistricts;
					}
					else
					{
						districts = _scheduleRestrictionRepository
							.GetDistrictsWithBorderForFastDeliveryAtDateTime(UoW, HistoryDateTime);

						_cachedFastDeliveryDistricts = districts;
						_fastDeliveryDistrictsLastVersionId = historyDistrictsVersionId;
					}
				}
				else
				{
					var currentDistrictsVersionId = _scheduleRestrictionRepository
						.GetDistrictsForFastDeliveryCurrentVersionId(UoW);

					if(_fastDeliveryDistrictsLastVersionId == currentDistrictsVersionId)
					{
						districts = _cachedFastDeliveryDistricts;
					}
					else
					{
						districts = _scheduleRestrictionRepository.GetDistrictsWithBorderForFastDelivery(UoW);

						_cachedFastDeliveryDistricts = districts;
						_fastDeliveryDistrictsLastVersionId = currentDistrictsVersionId;
					}
				}

				for(var i = 0; i < districts.Count; i++)
				{
					FastDeliveryDistricts.Add(districts[i]);
				}

				OnPropertyChanged(nameof(CoveragePercent));
			}

			FastDeliveryDistrictChanged?.Invoke();
		}

		private void RefreshLastDriverPositions()
		{
			var routeListIds = WorkingDrivers.SelectMany(x => x.RouteListsIds.Keys).ToArray();

			IList<DriverPositionWithFastDeliveryRadius> lastPoints;

			if(ShowFastDeliveryOnly)
			{
				if(ShowHistory)
				{
					lastPoints = _trackRepository.GetLastRouteListFastDeliveryTrackPointsWithRadius(UoW, routeListIds, DriverDisconnectedTimespan, HistoryDateTime);
				}
				else
				{
					lastPoints = _trackRepository.GetLastRouteListFastDeliveryTrackPointsWithRadius(UoW, routeListIds, DriverDisconnectedTimespan);
				}
			}
			else
			{
				lastPoints = _trackRepository.GetLastPointForRouteListsWithRadius(UoW, routeListIds);
			}

			LastDriverPositions.Clear();

			foreach(var driverPosition in lastPoints)
			{
				LastDriverPositions.Add(driverPosition);
			}

			OnPropertyChanged(nameof(CoveragePercent));

			CurrentObjectChanged?.Invoke(this, CurrentObjectChangedArgs.Empty);
		}

		private void RoutelistEntityConfigEntityUpdated(EntityChangeEvent[] changeEvents)
		{
			RefreshWorkingDrivers();
		}

		private void RefreshAll()
		{
			RefreshWorkingDriversCommand.Execute();
			RefreshFastDeliveryDistrictsCommand.Execute();
		}

		#endregion

		public int[] GetDriversWithAdditionalLoadingFrom(int[] ids)
		{
			return _routeListRepository.GetDriversWithAdditionalLoading(UoW, ids)
				.Select(x => x.Id).ToArray();
		}

		public IList<DriverPosition> GetLastRouteListTrackPoints(int[] ids)
		{
			return _trackRepository.GetLastPointForRouteLists(UoW, ids);
		}

		public IList<DriverPosition> GetLastRouteListTrackPoints(int[] routeListsIds, DateTime disconnectedDateTime)
		{
			return _trackRepository.GetLastPointForRouteLists(UoW, routeListsIds, disconnectedDateTime);
		}

		public IList<TrackPoint> GetRouteListTrackPoints(int id)
		{
			return _trackRepository.GetPointsForRouteList(UoW, id);
		}

		private void OnDataChanged(object sender, EventArgs e)
		{
			CurrentObjectChanged?.Invoke(this, CurrentObjectChangedArgs.Empty);
		}

		public int MaxDaysForNewbieDriver { get; }

		#region IDisposable
		public override void Dispose()
		{
			FastDeliveryDistricts.Clear();
			WorkingDrivers.Clear();
			SelectedWorkingDrivers.Clear();
			NotifyConfiguration.Instance.UnsubscribeAll(this);
			base.Dispose();
		}
		#endregion
	}

	#region Nodes
	public class WorkingDriverNode
	{
		private int _routeListNumber;

		public int? TrackId;

		public int Id { get; set; }

		public int RowNumber { get; set; }

		public string Name { get; set; }

		public string LastName { get; set; }

		public string Patronymic { get; set; }

		public DateTime? FirstWorkDay { get; set; }

		public string CarNumber { get; set; }

		public bool IsVodovozAuto { get; set; }

		public Dictionary<int, int?> RouteListsIds;

		public Dictionary<int, bool> RouteListsOnlineState;

		public int AddressesCompleted { get; set; }

		public int AddressesAll { get; set; }

		public decimal BottlesLeft { get; set; }

		public decimal Water19LReserve { get; set; }

		public int RouteListNumber
		{
			get => _routeListNumber;
			set
			{
				_routeListNumber = value;
			}
		}

		public decimal? FastDeliveryMaxDistance { get; set; }

		public int? MaxFastDeliveryOrders { get; set; }

		public string FastDeliveryMaxDistanceString => FastDeliveryMaxDistance.HasValue ? $"{FastDeliveryMaxDistance.Value:N1}" : "-";

		public string MaxFastDeliveryOrdersString => MaxFastDeliveryOrders.HasValue ? $"{MaxFastDeliveryOrders.Value}" : "-";

		public int CompletedPercent => AddressesAll == 0 ? 100 : (int)(((double)AddressesCompleted / AddressesAll) * 100);

		public string CompletedText => string.Format("{0}/{1}", AddressesCompleted, AddressesAll);

		public string CarText => IsVodovozAuto ? string.Format("<b>{0}</b>", CarNumber) : CarNumber;

		public string ShortName => PersonHelper.PersonNameWithInitials(LastName, Name, Patronymic);

		public DateTime? LastTrackPointTime { get; set; }

		public int TotalWorkDays => (int)(FirstWorkDay.HasValue ? (DateTime.Now - FirstWorkDay.Value).TotalDays : 0);
	}

	public class RouteListAddressNode
	{
		public int Id { get; set; }

		public Domain.Orders.Order Order { get; set; }

		public RouteListItem RouteListItem { get; set; }

		public DeliveryPoint DeliveryPoint { get; set; }

		public DeliverySchedule Time { get; set; }

		public RouteListItemStatus Status { get; set; }

		public int RouteListNumber { get; set; }
	}
	#endregion
}

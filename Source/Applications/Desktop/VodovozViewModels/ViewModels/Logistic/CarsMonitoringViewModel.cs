using DateTimeHelpers;
using NetTopologySuite.Geometries;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Utilities.Text;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Sale;
using Vodovoz.NHibernateProjections.Logistics;
using Vodovoz.NHibernateProjections.Orders;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class CarsMonitoringViewModel : DialogTabViewModelBase
	{
		private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly ITrackRepository _trackRepository;
		private readonly IRouteListRepository _routeListRepository;
		private readonly IScheduleRestrictionRepository _scheduleRestrictionRepository;

		private readonly IGtkTabsOpener _gtkTabsOpener;

		private bool _showCarCirclesOverlay = false;
		private bool _showDistrictsOverlay = false;
		private bool _showFastDeliveryOnly = false;
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

		private IUnitOfWork _unitOfWork;
		private bool _canOpenKeepingTab;
		private bool _showHistory;
		private DateTime _historyDate;
		private TimeSpan _historyHour;
		private readonly TimeSpan _fastDeliveryTime;

		private readonly double _fastDeliveryMaxDistance;

		public CarsMonitoringViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			ITrackRepository trackRepository,
			IRouteListRepository routeListRepository,
			IScheduleRestrictionRepository scheduleRestrictionRepository,
			IDeliveryRulesParametersProvider deliveryRulesParametersProvider,
			IGtkTabsOpener gtkTabsOpener)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_trackRepository = trackRepository ?? throw new ArgumentNullException(nameof(trackRepository));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_scheduleRestrictionRepository = scheduleRestrictionRepository ?? throw new ArgumentNullException(nameof(scheduleRestrictionRepository));

			TabName = "Мониторинг";

			CarsOverlayId = "cars";
			TracksOverlayId = "tracks";
			FastDeliveryOverlayId = "fast delivery";
			FastDeliveryDistrictsOverlayId = "districts";

			_unitOfWork = unitOfWorkFactory.CreateWithoutRoot();
			_unitOfWork.Session.DefaultReadOnly = true;

			CarRefreshInterval = TimeSpan.FromSeconds(10);

			DefaultMapCenterPosition = new Coordinate(59.93900, 30.31646);
			DriverDisconnectedTimespan = TimeSpan.FromMinutes(-20);

			var timespanRange = new List<TimeSpan>();

			for(int i = 0; i < 24; i++)
			{
				timespanRange.Add(TimeSpan.FromHours(i));
			}

			HistoryHours = timespanRange;

			_historyDate = DateTime.Today;
			_historyHour = TimeSpan.FromHours(9);

			if(deliveryRulesParametersProvider is null)
			{
				throw new ArgumentNullException(nameof(deliveryRulesParametersProvider));
			}

			_fastDeliveryTime = deliveryRulesParametersProvider.MaxTimeForFastDelivery;
			_fastDeliveryMaxDistance = deliveryRulesParametersProvider.MaxDistanceToLatestTrackPointKm;

			_gtkTabsOpener = gtkTabsOpener ?? throw new ArgumentNullException(nameof(gtkTabsOpener));

			FastDeliveryDistricts = new ObservableCollection<District>();
			RouteListAddresses = new ObservableCollection<RouteListAddressNode>();
			WorkingDrivers = new ObservableCollection<WorkingDriverNode>();
			SelectedWorkingDrivers = new ObservableCollection<WorkingDriverNode>();

			OpenKeepingDialogCommand = new DelegateCommand<int>(OpenRouteListKeepingTab);
			OpenTrackPointsJournalTabCommand = new DelegateCommand(OpenTrackPointJournalTab);
			RefreshWorkingDriversCommand = new DelegateCommand(RefreshWorkingDrivers);
			RefreshRouteListAddressesCommand = new DelegateCommand<int>(RefreshRouteListAddresses);
			RefreshFastDeliveryDistrictsCommand = new DelegateCommand(RefreshFastDeliveryDistricts);

			SelectedWorkingDrivers.CollectionChanged += SelectedWorkingDriversCollectionChanged;
		}

		#region Full properties

		public bool ShowDistrictsOverlay
		{
			get => _showDistrictsOverlay;
			set
			{
				SetField(ref _showDistrictsOverlay, value);
				if(value)
				{
					RefreshFastDeliveryDistrictsCommand?.Execute();
				}
				else
				{
					FastDeliveryDistricts.Clear();
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

		public bool ShowFastDeliveryOnly
		{
			get => _showFastDeliveryOnly;
			set
			{
				SetField(ref _showFastDeliveryOnly, value);
				if(!value)
				{
					ShowCarCirclesOverlay = false;
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

		public bool ShowHistory
		{
			get => _showHistory;
			set
			{
				SetField(ref _showHistory, value);
				RefreshWorkingDriversCommand?.Execute();
				if(ShowDistrictsOverlay)
				{
					RefreshFastDeliveryDistrictsCommand.Execute();
				}
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
						RefreshFastDeliveryDistrictsCommand.Execute();
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
						RefreshFastDeliveryDistrictsCommand.Execute();
					}
				}
			}
		}
		#endregion

		#region Readoly Properties
		public override bool HasChanges => false;

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

		public double FastDeliveryMaxDistance => _fastDeliveryMaxDistance;

		public Color[] AvailableTrackColors => _availableTrackColors;
		#endregion

		#region Observable Collections
		public ObservableCollection<District> FastDeliveryDistricts { get; }

		public ObservableCollection<WorkingDriverNode> SelectedWorkingDrivers { get; }

		public ObservableCollection<WorkingDriverNode> WorkingDrivers { get; }

		public ObservableCollection<RouteListAddressNode> RouteListAddresses { get; }
		#endregion

		#region Commands
		public DelegateCommand<int> OpenKeepingDialogCommand { get; }

		public DelegateCommand OpenTrackPointsJournalTabCommand { get; }

		public DelegateCommand RefreshWorkingDriversCommand { get; }

		public DelegateCommand<int> RefreshRouteListAddressesCommand { get; }

		public DelegateCommand RefreshFastDeliveryDistrictsCommand { get; }
		#endregion

		private void SelectedWorkingDriversCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			CanOpenKeepingTab = SelectedWorkingDrivers.Any();
		}

		#region Command Handlers
		private void OpenRouteListKeepingTab(int routeListId)
		{
			_gtkTabsOpener.OpenRouteListKeepingDlg(this, routeListId);
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
			Car carAlias = null;
			CarVersion carVersionAlias = null;

			Domain.Orders.Order orderAlias = null;
			OrderItem ordItemsAlias = null;
			Nomenclature nomenclatureAlias = null;

			var completedSubquery = QueryOver.Of<RouteListItem>()
				.Where(i => i.RouteList.Id == routeListAlias.Id)
				.Where(i => i.Status != RouteListItemStatus.EnRoute)
				.Select(Projections.RowCount());

			var addressesSubquery = QueryOver.Of<RouteListItem>()
				.Where(i => i.RouteList.Id == routeListAlias.Id)
				.Select(Projections.RowCount());

			var uncompletedBottlesSubquery = QueryOver.Of<RouteListItem>()  // Запрашивает количество ещё не доставленных бутылей.
				.Where(i => i.RouteList.Id == routeListAlias.Id)
				.Where(i => i.Status == RouteListItemStatus.EnRoute)
				.JoinAlias(rli => rli.Order, () => orderAlias)
				.JoinAlias(() => orderAlias.OrderItems, () => ordItemsAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(() => ordItemsAlias.Nomenclature, () => nomenclatureAlias)
			   	.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water && nomenclatureAlias.TareVolume == TareVolume.Vol19L)
				.Select(Projections.Sum(() => ordItemsAlias.Count));

			var trackSubquery = QueryOver.Of<Track>()
				.Where(x => x.RouteList.Id == routeListAlias.Id)
				.Select(x => x.Id);

			IProjection isCompanyCarProjection = CarProjections.GetIsCompanyCarProjection();

			IProjection water19LReserveProjection = GetWater19LReserveProjection();

			var query = UoW.Session.QueryOver<RouteList>(() => routeListAlias);

			if(ShowFastDeliveryOnly)
			{
				query.Where(() => routeListAlias.AdditionalLoadingDocument != null);
			}

			query.JoinAlias(rl => rl.Driver, () => driverAlias)
				.JoinAlias(rl => rl.Car, () => carAlias)
				.JoinEntityAlias(() => carVersionAlias,
					() => carVersionAlias.Car.Id == carAlias.Id
						&& carVersionAlias.StartDate <= routeListAlias.Date &&
						(carVersionAlias.EndDate == null || carVersionAlias.EndDate >= routeListAlias.Date))
				.Where(rl => rl.Driver != null)
				.Where(rl => rl.Car != null);

			if(ShowHistory)
			{
				query.Where(Restrictions.Between(
					Projections.Property(() => routeListAlias.Date),
					HistoryDate,
					HistoryDate.LatestDayTime()));
			}
			else
			{
				query.Where(rl => rl.Status == RouteListStatus.EnRoute);
			}

			var result = query.SelectList(list => list
					.Select(() => driverAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => driverAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => driverAlias.LastName).WithAlias(() => resultAlias.LastName)
					.Select(() => driverAlias.Patronymic).WithAlias(() => resultAlias.Patronymic)
					.Select(() => carAlias.RegistrationNumber).WithAlias(() => resultAlias.CarNumber)
					.Select(isCompanyCarProjection).WithAlias(() => resultAlias.IsVodovozAuto)
					.Select(() => routeListAlias.Id).WithAlias(() => resultAlias.RouteListNumber)
					.SelectSubQuery(addressesSubquery).WithAlias(() => resultAlias.AddressesAll)
					.SelectSubQuery(completedSubquery).WithAlias(() => resultAlias.AddressesCompleted)
					.SelectSubQuery(trackSubquery).WithAlias(() => resultAlias.TrackId)
					.SelectSubQuery(uncompletedBottlesSubquery).WithAlias(() => resultAlias.BottlesLeft)
					.Select(water19LReserveProjection).WithAlias(() => resultAlias.Water19LReserve))
				.TransformUsing(Transformers.AliasToBean<WorkingDriverNode>())
				.List<WorkingDriverNode>();

			WorkingDrivers.Clear();

			int rowNum = 0;
			foreach(var driver in result.GroupBy(x => x.Id).OrderBy(x => x.First().ShortName))
			{
				var savedRow = driver.First();
				savedRow.RouteListsText = string.Join("; ", driver.Select(x => x.TrackId != null
						? $"<span foreground=\"green\"><b>{x.RouteListNumber}</b></span>"
						: x.RouteListNumber.ToString()));
				savedRow.RouteListsIds = driver.ToDictionary(x => x.RouteListNumber, x => x.TrackId);
				savedRow.AddressesAll = driver.Sum(x => x.AddressesAll);
				savedRow.AddressesCompleted = driver.Sum(x => x.AddressesCompleted);
				savedRow.Water19LReserve = driver.Sum(x => x.Water19LReserve);
				savedRow.RowNumber = ++rowNum;
				WorkingDrivers.Add(savedRow);
			}
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
		}

		private void RefreshFastDeliveryDistricts()
		{
			FastDeliveryDistricts.Clear();
			IList<District> districts;
			if(ShowHistory)
			{
				districts = _scheduleRestrictionRepository
					.GetDistrictsWithBorderForFastDeliveryAtDateTime(_unitOfWork, HistoryDate.Add(HistoryHour));
			}
			else
			{
				districts = _scheduleRestrictionRepository.GetDistrictsWithBorderForFastDelivery(_unitOfWork);
			}
			foreach(var district in districts)
			{
				FastDeliveryDistricts.Add(district);
			}
		}
		#endregion

		public int[] GetDriversWithAdditionalLoadingFrom(int[] ids)
		{
			return _routeListRepository.GetDriversWithAdditionalLoading(_unitOfWork, ids)
				.Select(x => x.Id).ToArray();
		}

		public IList<DriverPosition> GetLastRouteListTrackPoints(int[] ids)
		{
			return _trackRepository.GetLastPointForRouteLists(_unitOfWork, ids);
		}

		public IList<DriverPosition> GetLastRouteListTrackPoints(int[] routeListsIds, DateTime disconnectedDateTime)
		{
			return _trackRepository.GetLastPointForRouteLists(_unitOfWork, routeListsIds, disconnectedDateTime);
		}

		public IList<TrackPoint> GetRouteListTrackPoints(int id)
		{
			return _trackRepository.GetPointsForRouteList(_unitOfWork, id);
		}

		#region Query Methods
		private static QueryOver<RouteListItem, RouteListItem> CreateOwnOrdersSubquery()
		{
			RouteListItem routeListItemAlias = null;
			OrderItem orderItemAlias = null;
			RouteList routeListAlias = null;
			Domain.Orders.Order orderAlias = null;
			Nomenclature nomenclatureAlias = null;

			return QueryOver.Of<RouteListItem>(() => routeListItemAlias)
							.JoinAlias(() => routeListItemAlias.Order, () => orderAlias)
							.JoinEntityAlias(() => orderItemAlias, () => orderItemAlias.Order.Id == orderAlias.Id)
							.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
							.Where(() => !orderAlias.IsFastDelivery && !routeListItemAlias.WasTransfered)
							.And(() => nomenclatureAlias.Category == NomenclatureCategory.water
								&& nomenclatureAlias.TareVolume == TareVolume.Vol19L)
							.And(() => routeListItemAlias.RouteList.Id == routeListAlias.Id)
							.Select(Projections.Sum(() => orderItemAlias.Count));
		}

		private QueryOver<AdditionalLoadingDocumentItem, AdditionalLoadingDocumentItem> CreateAdditionalBalanceSuqquery()
		{
			RouteList routeListAlias = null;
			Nomenclature nomenclatureAlias = null;
			AdditionalLoadingDocumentItem additionalLoadingDocumentItemAlias = null;
			AdditionalLoadingDocument additionalLoadingDocumentAlias = null;

			var subquery = QueryOver.Of<AdditionalLoadingDocumentItem>(() => additionalLoadingDocumentItemAlias)
				.JoinAlias(() => additionalLoadingDocumentItemAlias.Nomenclature, () => nomenclatureAlias)
				.JoinAlias(() => additionalLoadingDocumentItemAlias.AdditionalLoadingDocument, () => additionalLoadingDocumentAlias)
				.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water
					&& nomenclatureAlias.TareVolume == TareVolume.Vol19L)
				.And(() => routeListAlias.AdditionalLoadingDocument.Id == additionalLoadingDocumentAlias.Id)
				.Select(Projections.Sum(() => additionalLoadingDocumentItemAlias.Amount));

			if(ShowHistory)
			{
				subquery.And(Restrictions.Between(
					Projections.Property(() => additionalLoadingDocumentAlias.CreationDate),
						HistoryDate,
						HistoryDate.Add(HistoryHour)));
			}

			return subquery;
		}

		private static QueryOver<RouteListItem, RouteListItem> CreateDeliveredOrdersSubquery()
		{
			RouteList routeListAlias = null;
			Domain.Orders.Order orderAlias = null;
			Nomenclature nomenclatureAlias = null;
			RouteListItem routeListItemAlias = null;
			RouteListItem transferedToAlias = null;
			OrderItem orderItemAlias = null;

			var deliveredOrdersSubquery = QueryOver.Of<RouteListItem>(() => routeListItemAlias)
				.JoinAlias(() => routeListItemAlias.Order, () => orderAlias)
				.JoinEntityAlias(() => orderItemAlias, () => orderItemAlias.Order.Id == orderAlias.Id)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => routeListItemAlias.TransferedTo, () => transferedToAlias)
				.Where(() =>
					//не отменённые и не недовозы
					routeListItemAlias.Status != RouteListItemStatus.Canceled
					&& routeListItemAlias.Status != RouteListItemStatus.Overdue
					// и не перенесённые к водителю; либо перенесённые с погрузкой; либо перенесённые и это экспресс-доставка (всегда без погрузки)
					&& (!routeListItemAlias.WasTransfered || routeListItemAlias.NeedToReload || orderAlias.IsFastDelivery)
					// и не перенесённые от водителя; либо перенесённые и не нужна погрузка и не экспресс-доставка (остатки по экспресс-доставке не переносятся)
					&& (routeListItemAlias.Status != RouteListItemStatus.Transfered
						|| (!transferedToAlias.NeedToReload && !orderAlias.IsFastDelivery)))
				.And(() => nomenclatureAlias.Category == NomenclatureCategory.water &&
					nomenclatureAlias.TareVolume == TareVolume.Vol19L)
				.And(() => routeListItemAlias.RouteList.Id == routeListAlias.Id)
				.Select(OrderProjections.GetOrderItemCountSumProjection());
			return deliveredOrdersSubquery;
		}

		private IProjection GetWater19LReserveProjection()
		{
			QueryOver<RouteListItem, RouteListItem> ownOrdersSubquery = CreateOwnOrdersSubquery();

			QueryOver<AdditionalLoadingDocumentItem, AdditionalLoadingDocumentItem> additionalBalanceSubquery = CreateAdditionalBalanceSuqquery();

			QueryOver<RouteListItem, RouteListItem> deliveredOrdersSubquery = CreateDeliveredOrdersSubquery();

			RouteList routeListAlias = null;

			IProjection water19LReserveProjection;

			if(ShowHistory)
			{
				water19LReserveProjection = Projections.Conditional(
					Restrictions.Eq(Projections.Property(() => routeListAlias.AdditionalLoadingDocument), null),
					Projections.Constant(0m),
					Projections.SqlFunction(
						new SQLFunctionTemplate(NHibernateUtil.Decimal, "IFNULL(?1, 0) + IFNULL(?2, 0) - IFNULL(?3, 0)"),
						NHibernateUtil.Decimal,
						Projections.SubQuery(ownOrdersSubquery),
						Projections.SubQuery(additionalBalanceSubquery),
						Projections.SubQuery(deliveredOrdersSubquery)));
			}
			else
			{
				water19LReserveProjection = Projections.Conditional(
					Restrictions.Eq(Projections.Property(() => routeListAlias.AdditionalLoadingDocument), null),
					Projections.Constant(0m),
					Projections.SqlFunction(
						new SQLFunctionTemplate(NHibernateUtil.Decimal, "IFNULL(?1, 0) + IFNULL(?2, 0) - IFNULL(?3, 0)"),
						NHibernateUtil.Decimal,
						Projections.SubQuery(ownOrdersSubquery),
						Projections.SubQuery(additionalBalanceSubquery),
						Projections.SubQuery(deliveredOrdersSubquery)));
			}

			return water19LReserveProjection;
		}
		#endregion

		#region IDisposable
		public override void Dispose()
		{
			FastDeliveryDistricts.Clear();
			WorkingDrivers.Clear();
			SelectedWorkingDrivers.Clear();
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

		public string CarNumber { get; set; }

		public bool IsVodovozAuto { get; set; }

		//RouteListId, TrackId
		public Dictionary<int, int?> RouteListsIds;

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
				RouteListsText = value.ToString();
			}
		}

		public string RouteListsText { get; set; }

		public int CompletedPercent => AddressesAll == 0 ? 100 : (int)(((double)AddressesCompleted / AddressesAll) * 100);

		public string CompletedText => string.Format("{0}/{1}", AddressesCompleted, AddressesAll);

		public string CarText => IsVodovozAuto ? string.Format("<b>{0}</b>", CarNumber) : CarNumber;

		public string ShortName => PersonHelper.PersonNameWithInitials(LastName, Name, Patronymic);
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

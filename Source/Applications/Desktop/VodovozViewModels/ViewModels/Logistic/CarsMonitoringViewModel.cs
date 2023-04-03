﻿using NetTopologySuite.Geometries;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
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
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Sale;
using Vodovoz.Extensions;
using Vodovoz.NHibernateProjections.Logistics;
using Vodovoz.NHibernateProjections.Orders;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.Tools.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class CarsMonitoringViewModel : DialogTabViewModelBase
	{
		private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly ITrackRepository _trackRepository;
		private readonly IRouteListRepository _routeListRepository;
		private readonly IScheduleRestrictionRepository _scheduleRestrictionRepository;
		private readonly IDeliveryRulesParametersProvider _deliveryRulesParametersProvider;

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
		private TimeSpan _fastDeliveryTime;

		private IUnitOfWork _unitOfWork;
		private bool _canOpenKeepingTab;
		private bool _showHistory;
		private DateTime _historyDate;
		private TimeSpan _historyHour;

		private double _fastDeliveryMaxDistance;

		private int _fastDeliveryDistrictsLastVersionId;
		private IList<District> _cachedFastDeliveryDistricts;

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
			_deliveryRulesParametersProvider = deliveryRulesParametersProvider ?? throw new ArgumentNullException(nameof(deliveryRulesParametersProvider));
			_gtkTabsOpener = gtkTabsOpener ?? throw new ArgumentNullException(nameof(gtkTabsOpener));

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

			_fastDeliveryTime = _deliveryRulesParametersProvider.MaxTimeForFastDelivery;

			FastDeliveryDistricts = new ObservableCollection<District>();
			RouteListAddresses = new ObservableCollection<RouteListAddressNode>();
			WorkingDrivers = new ObservableCollection<WorkingDriverNode>();
			SelectedWorkingDrivers = new ObservableCollection<WorkingDriverNode>();
			LastDriverPositions = new ObservableCollection<DriverPosition>();

			OpenKeepingDialogCommand = new DelegateCommand<int>(OpenRouteListKeepingTab);
			OpenTrackPointsJournalTabCommand = new DelegateCommand(OpenTrackPointJournalTab);
			RefreshWorkingDriversCommand = new DelegateCommand(RefreshWorkingDrivers);
			RefreshRouteListAddressesCommand = new DelegateCommand<int>(RefreshRouteListAddresses);
			RefreshFastDeliveryDistrictsCommand = new DelegateCommand(RefreshFastDeliveryDistricts);
			RefreshFastDeliveryMaxKmValueCommand = new DelegateCommand(RefreshFastDeliveryMaxKmValue);
			RefreshLastDriverPositionsCommand = new DelegateCommand(RefreshLastDriverPositions);

			RefreshFastDeliveryMaxKmValueCommand?.Execute();
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
					RefreshFastDeliveryDistrictsCommand?.Execute();
					RefreshFastDeliveryMaxKmValueCommand?.Execute();
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
						RefreshFastDeliveryDistrictsCommand?.Execute();
						RefreshFastDeliveryMaxKmValueCommand?.Execute();
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
						RefreshFastDeliveryMaxKmValueCommand?.Execute();
					}
				}
			}
		}

		[PropertyChangedAlso(nameof(CoveragePercent))]
		public double FastDeliveryMaxDistance
		{
			get => _fastDeliveryMaxDistance;
			set => SetField(ref _fastDeliveryMaxDistance, value);
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

		[PropertyChangedAlso(nameof(CoveragePercentString))]
		public double CoveragePercent => DistanceCalculator.CalculateCoveragePercent(
			FastDeliveryDistricts.Select(fdd => fdd.DistrictBorder).ToList(),
			LastDriverPositions.Select(pos => pos.ToCoordinate()).ToList(),
			FastDeliveryMaxDistance);

		#endregion

		#region Observable Collections
		public ObservableCollection<District> FastDeliveryDistricts { get; }

		public ObservableCollection<WorkingDriverNode> SelectedWorkingDrivers { get; }

		public ObservableCollection<WorkingDriverNode> WorkingDrivers { get; }

		public ObservableCollection<RouteListAddressNode> RouteListAddresses { get; }

		public ObservableCollection<DriverPosition> LastDriverPositions { get; }
		#endregion

		#region Commands
		public DelegateCommand<int> OpenKeepingDialogCommand { get; }

		public DelegateCommand OpenTrackPointsJournalTabCommand { get; }

		public DelegateCommand RefreshWorkingDriversCommand { get; }

		public DelegateCommand<int> RefreshRouteListAddressesCommand { get; }

		public DelegateCommand RefreshFastDeliveryDistrictsCommand { get; }

		public DelegateCommand RefreshFastDeliveryMaxKmValueCommand { get; }

		public DelegateCommand RefreshLastDriverPositionsCommand { get; }
		#endregion

		#region Events
		public event Action FastDeliveryDistrictChanged;

		public event Action WorkingDriversChanged;

		public event Action RouteListAddressesChanged;
		#endregion

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
			RouteListItem routeListItemAlias = null;
			Car carAlias = null;
			CarVersion carVersionAlias = null;
			Track trackAlias = null;

			Domain.Orders.Order orderAlias = null;
			OrderItem ordItemsAlias = null;
			Nomenclature nomenclatureAlias = null;
			RouteListFastDeliveryMaxDistance routeListFastDeliveryMaxDistanceAlias = null;

			var completedSubquery = QueryOver.Of<RouteListItem>()
				.Where(i => i.RouteList.Id == routeListAlias.Id)
				.Where(i => i.Status != RouteListItemStatus.EnRoute);

			if(ShowHistory)
			{
				completedSubquery.JoinAlias(rli => rli.Order, () => orderAlias)
					.Where(Restrictions.Le(Projections.Property(() => orderAlias.TimeDelivered), HistoryDateTime));
			}

			completedSubquery.Select(Projections.RowCount());

			var addressesSubquery = QueryOver.Of<RouteListItem>()
				.Where(i => i.RouteList.Id == routeListAlias.Id);

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

			var water19LSubquery = QueryOver.Of<DeliveryFreeBalanceOperation>()
				.Where(o => o.RouteList.Id == routeListAlias.Id)
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

			query.JoinAlias(rl => rl.Driver, () => driverAlias)
				.JoinAlias(rl => rl.Car, () => carAlias)
				.JoinEntityAlias(() => carVersionAlias,
					() => carVersionAlias.Car.Id == carAlias.Id
						&& carVersionAlias.StartDate <= routeListAlias.Date
						&& (carVersionAlias.EndDate == null || carVersionAlias.EndDate >= routeListAlias.Date))
				.Where(rl => rl.Driver != null)
				.Where(rl => rl.Car != null);

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
			}
			else
			{
				query.Where(rl => rl.Status == RouteListStatus.EnRoute);
			}

			var routeListFastDeliveryMaxDistanceSubquery = QueryOver.Of<RouteListFastDeliveryMaxDistance>()
				.Where(d => d.RouteList.Id == routeListAlias.Id && d.StartDate < DateTime.Now && (d.EndDate == null || d.EndDate > DateTime.Now))
				.Select(d => d.Distance)
				.OrderBy(d => d.StartDate).Desc
				.Take(1);

			var result = query.SelectList(list => list
					.Select(() => driverAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => driverAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => driverAlias.LastName).WithAlias(() => resultAlias.LastName)
					.Select(() => driverAlias.Patronymic).WithAlias(() => resultAlias.Patronymic)
					.Select(() => carAlias.RegistrationNumber).WithAlias(() => resultAlias.CarNumber)
					.Select(isCompanyCarProjection).WithAlias(() => resultAlias.IsVodovozAuto)
					.Select(() => routeListAlias.Id).WithAlias(() => resultAlias.RouteListNumber)
					.SelectSubQuery(routeListFastDeliveryMaxDistanceSubquery).WithAlias(() => resultAlias.FastDeliveryMaxDistance)
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
				savedRow.RouteListsText =
					string.Join("; ", driversNodes[i].Select(x => x.TrackId != null
						? x.LastTrackPointTime >= disconnectedDateTime
							? $"<span foreground=\"green\"><b>{x.RouteListNumber}</b></span>"
							: $"<span foreground=\"blue\"><b>{x.RouteListNumber}</b></span>"
						: x.RouteListNumber.ToString()));
				savedRow.RouteListsIds = driversNodes[i].ToDictionary(x => x.RouteListNumber, x => x.TrackId);
				savedRow.AddressesAll = driversNodes[i].Sum(x => x.AddressesAll);
				savedRow.AddressesCompleted = driversNodes[i].Sum(x => x.AddressesCompleted);
				savedRow.Water19LReserve = driversNodes[i].Sum(x => x.Water19LReserve);
				savedRow.RowNumber = ++rowNum;

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
						.GetDistrictsForFastDeliveryHistoryVersionId(_unitOfWork, HistoryDateTime);

					if(_fastDeliveryDistrictsLastVersionId == historyDistrictsVersionId)
					{
						districts = _cachedFastDeliveryDistricts;
					}
					else
					{
						districts = _scheduleRestrictionRepository
							.GetDistrictsWithBorderForFastDeliveryAtDateTime(_unitOfWork, HistoryDateTime);

						_cachedFastDeliveryDistricts = districts;
						_fastDeliveryDistrictsLastVersionId = historyDistrictsVersionId;
					}
				}
				else
				{
					var currentDistrictsVersionId = _scheduleRestrictionRepository
						.GetDistrictsForFastDeliveryCurrentVersionId(_unitOfWork);

					if(_fastDeliveryDistrictsLastVersionId == currentDistrictsVersionId)
					{
						districts = _cachedFastDeliveryDistricts;
					}
					else
					{
						districts = _scheduleRestrictionRepository.GetDistrictsWithBorderForFastDelivery(_unitOfWork);

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

		public void RefreshFastDeliveryMaxKmValue()
		{
			if(ShowHistory)
			{
				FastDeliveryMaxDistance = _deliveryRulesParametersProvider.GetMaxDistanceToLatestTrackPointKmFor(HistoryDateTime);
			}
			else
			{
				FastDeliveryMaxDistance = _deliveryRulesParametersProvider.MaxDistanceToLatestTrackPointKm;
			}
		}

		private void RefreshLastDriverPositions()
		{
			var routeListIds = WorkingDrivers.SelectMany(x => x.RouteListsIds.Keys).ToArray();

			IList<DriverPosition> lastPoints;
			if(ShowFastDeliveryOnly)
			{
				if(ShowHistory)
				{
					lastPoints = _trackRepository.GetLastRouteListFastDeliveryTrackPoints(UoW, routeListIds, DriverDisconnectedTimespan, HistoryDateTime);
				}
				else
				{
					lastPoints = _trackRepository.GetLastRouteListFastDeliveryTrackPoints(UoW, routeListIds, DriverDisconnectedTimespan);
				}
			}
			else
			{
				lastPoints = GetLastRouteListTrackPoints(routeListIds);
			}

			LastDriverPositions.Clear();

			foreach(var driverPosition in lastPoints)
			{
				LastDriverPositions.Add(driverPosition);
			}

			OnPropertyChanged(nameof(CoveragePercent));
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

		public decimal FastDeliveryMaxDistance { get; set; }
		public string FastDeliveryMaxDistanceString => $"{FastDeliveryMaxDistance:N1}";

		public int CompletedPercent => AddressesAll == 0 ? 100 : (int)(((double)AddressesCompleted / AddressesAll) * 100);

		public string CompletedText => string.Format("{0}/{1}", AddressesCompleted, AddressesAll);

		public string CarText => IsVodovozAuto ? string.Format("<b>{0}</b>", CarNumber) : CarNumber;

		public string ShortName => PersonHelper.PersonNameWithInitials(LastName, Name, Patronymic);

		public DateTime? LastTrackPointTime { get; set; }
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

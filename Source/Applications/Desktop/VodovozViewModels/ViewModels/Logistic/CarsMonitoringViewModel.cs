using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.ViewModels;
using System;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Chats;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Sale;
using Vodovoz.Services;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class CarsMonitoringViewModel : DialogTabViewModelBase
	{
		private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly IEmployeeRepository _employeeRepository;
		private readonly IChatRepository _chatRepository;
		private readonly ITrackRepository _trackRepository;
		private readonly IRouteListRepository _routeListRepository;
		private readonly IScheduleRestrictionRepository _scheduleRestrictionRepository;

		private IUnitOfWork _unitOfWork;
		private readonly Employee _currentEmployee;
		private const uint _carRefreshInterval = 10000;
		private uint _timerId;

		//private readonly GMapOverlay carsOverlay = new GMapOverlay("cars");
		//private readonly GMapOverlay tracksOverlay = new GMapOverlay("tracks");
		//private readonly GMapOverlay _fastDeliveryOverlay = new GMapOverlay("fast delivery");
		//private readonly GMapOverlay _districtsOverlay = new GMapOverlay("districts") { IsVisibile = false };
		//private Dictionary<int, CarMarker> carMarkers;
		//private Dictionary<int, CarMarkerType> lastSelectedDrivers = new Dictionary<int, CarMarkerType>();
		//private Gtk.Window mapWindow;
		//private List<DistanceTextInfo> tracksDistance = new List<DistanceTextInfo>();
		private readonly TimeSpan _fastDeliveryTime;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ICommonServices _commonServices;
		private readonly double _fastDeliveryMaxDistanceKm;

		public CarsMonitoringViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IEmployeeRepository employeeRepository,
			IChatRepository chatRepository,
			ITrackRepository trackRepository,
			IRouteListRepository routeListRepository,
			IScheduleRestrictionRepository scheduleRestrictionRepository,
			IDeliveryRulesParametersProvider deliveryRulesParametersProvider,
			ICommonServices commonServices)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_unitOfWork = unitOfWorkFactory.CreateWithoutRoot();

			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_chatRepository = chatRepository ?? throw new ArgumentNullException(nameof(chatRepository));
			_trackRepository = trackRepository ?? throw new ArgumentNullException(nameof(trackRepository));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_scheduleRestrictionRepository = scheduleRestrictionRepository ?? throw new ArgumentNullException(nameof(scheduleRestrictionRepository));
			_fastDeliveryTime =
				(deliveryRulesParametersProvider ?? throw new ArgumentNullException(nameof(deliveryRulesParametersProvider)))
				.MaxTimeForFastDelivery;
			_fastDeliveryMaxDistanceKm = deliveryRulesParametersProvider.MaxDistanceToLatestTrackPointKm;
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));

			TabName = "Мониторинг";

			_currentEmployee = employeeRepository.GetEmployeeForCurrentUser(_unitOfWork);

			if(_currentEmployee == null)
			{
				interactiveService.ShowMessage(ImportanceLevel.Error ,"Ваш пользователь не привязан к сотруднику. Чат не будет работать.");
			}
		}

		//private void LoadTracksForDriver(int driverId)
		//{
		//	tracksOverlay.Clear();
		//	tracksDistance.Clear();
		//	//Load tracks
		//	var driverRow = (yTreeViewDrivers.RepresentationModel.ItemsList as IList<Vodovoz.ViewModel.WorkingDriverVMNode>).FirstOrDefault(x => x.Id == driverId);
		//	int colorIter = 0;
		//	foreach(var routeId in driverRow.RouteListsIds)
		//	{
		//		var pointList = _trackRepository.GetPointsForRouteList(uow, routeId.Key);
		//		if(pointList.Count == 0)
		//			continue;

		//		var points = pointList.Select(p => new PointLatLng(p.Latitude, p.Longitude));

		//		var route = new GMapRoute(points, routeId.ToString());

		//		route.Stroke = new System.Drawing.Pen(GetTrackColor(colorIter));
		//		colorIter++;
		//		route.Stroke.Width = 4;
		//		route.Stroke.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;

		//		tracksDistance.Add(MakeDistanceLayout(route));
		//		tracksOverlay.Routes.Add(route);
		//	}

		//	//LoadAddresses
		//	foreach(var point in (IList<DriverRouteListAddressVMNode>)yTreeAddresses.RepresentationModel.ItemsList)
		//	{
		//		if(point.DeliveryPoint == null)
		//		{
		//			logger.Warn("Для заказа №{0}, отсутствует точка доставки. Поэтому добавление маркера пропущено.", point.Order.Id);
		//			continue;
		//		}
		//		if(point.DeliveryPoint.Latitude.HasValue && point.DeliveryPoint.Longitude.HasValue)
		//		{
		//			GMarkerGoogleType type;
		//			switch(point.Status)
		//			{
		//				case RouteListItemStatus.Completed:
		//					type = GMarkerGoogleType.green_small;
		//					break;
		//				case RouteListItemStatus.EnRoute:
		//					if(point.Order != null && point.Order.IsFastDelivery)
		//					{
		//						type = GMarkerGoogleType.yellow_small;
		//					}
		//					else
		//					{
		//						type = GMarkerGoogleType.gray_small;
		//					}
		//					break;
		//				case RouteListItemStatus.Canceled:
		//					type = GMarkerGoogleType.purple_small;
		//					break;
		//				case RouteListItemStatus.Overdue:
		//					type = GMarkerGoogleType.red_small;
		//					break;
		//				default:
		//					type = GMarkerGoogleType.none;
		//					break;
		//			}
		//			var addressMarker = new GMarkerGoogle(new PointLatLng((double)point.DeliveryPoint.Latitude, (double)point.DeliveryPoint.Longitude), type);
		//			addressMarker.Tag = point;
		//			addressMarker.ToolTipText = $"{point.DeliveryPoint.ShortAddress}\nВремя доставки: {point.Time?.Name ?? "Не назначено"}";
		//			tracksOverlay.Markers.Add(addressMarker);
		//		}
		//	}
		//	buttonCleanTrack.Sensitive = true;
		//}

	}
}

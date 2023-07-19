using System;
using System.Globalization;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalNodes;
using VodovozOrder = Vodovoz.Domain.Orders.Order;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using QS.Project.Journal.DataLoader;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.Parameters;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.Services;

namespace Vodovoz.JournalViewModels
{
	public class OrderForRouteListJournalViewModel : FilterableSingleEntityJournalViewModelBase<VodovozOrder, OrderDlg, OrderForRouteListJournalNode, OrderJournalFilterViewModel>
	{
		private readonly IOrderSelectorFactory _orderSelectorFactory;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly ICounterpartyJournalFactory _counterpartyJournalFactory;
		private readonly IDeliveryPointJournalFactory _deliveryPointJournalFactory;
		private readonly ISubdivisionJournalFactory _subdivisionJournalFactory;
		private readonly IGtkTabsOpener _gtkDialogsOpener;
		private readonly IUndeliveredOrdersJournalOpener _undeliveredOrdersJournalOpener;
		private readonly IEmployeeService _employeeService;
		private readonly IUndeliveredOrdersRepository _undeliveredOrdersRepository;
		private readonly ISubdivisionParametersProvider _subdivisionParametersProvider;
		private readonly int _closingDocumentDeliveryScheduleId;

		public OrderForRouteListJournalViewModel(
			OrderJournalFilterViewModel filterViewModel, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			ICommonServices commonServices,
			IOrderSelectorFactory orderSelectorFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IDeliveryPointJournalFactory deliveryPointJournalFactory,
			ISubdivisionJournalFactory subdivisionJournalFactory,
			IGtkTabsOpener gtkDialogsOpener,
			IUndeliveredOrdersJournalOpener undeliveredOrdersJournalOpener,
			IEmployeeService employeeService,
			IUndeliveredOrdersRepository undeliveredOrdersRepository,
			ISubdivisionParametersProvider subdivisionParametersProvider,
			IDeliveryScheduleParametersProvider deliveryScheduleParametersProvider) : base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			_orderSelectorFactory = orderSelectorFactory ?? throw new ArgumentNullException(nameof(orderSelectorFactory));
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_counterpartyJournalFactory = counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory));
			_deliveryPointJournalFactory = deliveryPointJournalFactory ?? throw new ArgumentNullException(nameof(deliveryPointJournalFactory));
			_subdivisionJournalFactory = subdivisionJournalFactory ?? throw new ArgumentNullException(nameof(subdivisionJournalFactory));
			_gtkDialogsOpener = gtkDialogsOpener ?? throw new ArgumentNullException(nameof(gtkDialogsOpener));
			_undeliveredOrdersJournalOpener =
				undeliveredOrdersJournalOpener ?? throw new ArgumentNullException(nameof(undeliveredOrdersJournalOpener));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_undeliveredOrdersRepository =
				undeliveredOrdersRepository ?? throw new ArgumentNullException(nameof(undeliveredOrdersRepository));
			_subdivisionParametersProvider = subdivisionParametersProvider ?? throw new ArgumentNullException(nameof(subdivisionParametersProvider));
			_closingDocumentDeliveryScheduleId =
				(deliveryScheduleParametersProvider ?? throw new ArgumentNullException(nameof(deliveryScheduleParametersProvider)))
				.ClosingDocumentDeliveryScheduleId;
				
			TabName = "Журнал заказов";

			var threadLoader = DataLoader as ThreadDataLoader<OrderForRouteListJournalNode>;
			threadLoader.MergeInOrderBy(x => x.CreateDate, true);

			FinishJournalConfiguration();

			UpdateOnChanges(
				typeof(VodovozOrder),
				typeof(OrderWithoutShipmentForDebt),
				typeof(OrderWithoutShipmentForPayment),
				typeof(OrderWithoutShipmentForAdvancePayment),
				typeof(OrderWithoutShipmentForPaymentItem),
				typeof(OrderWithoutShipmentForAdvancePaymentItem),
				typeof(OrderItem)
			);
		}

		private IQueryOver<VodovozOrder> GetOrdersQuery(IUnitOfWork uow)
		{
			OrderForRouteListJournalNode resultAlias = null;
			VodovozOrder orderAlias = null;
			Nomenclature nomenclatureAlias = null;
			OrderItem orderItemAlias = null;
			Counterparty counterpartyAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			DeliverySchedule deliveryScheduleAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;
			District districtAlias = null;

			var sanitizationNomenclatureIds = 
				new NomenclatureParametersProvider(new ParametersProvider()).GetSanitisationNomenclature(uow);

			var query = uow.Session.QueryOver<VodovozOrder>(() => orderAlias);

			if (FilterViewModel.ViewTypes != ViewTypes.Order && FilterViewModel.ViewTypes != ViewTypes.All)
			{
				query.Where(o => o.Id == -1);
			}
			
			if(FilterViewModel.RestrictStatus != null) {
				query.Where(o => o.OrderStatus == FilterViewModel.RestrictStatus);
			}

			if(FilterViewModel.RestrictPaymentType != null) {
				query.Where(o => o.PaymentType == FilterViewModel.RestrictPaymentType);
			}

			if(FilterViewModel.HideStatuses != null) {
				query.WhereRestrictionOn(o => o.OrderStatus).Not.IsIn(FilterViewModel.HideStatuses);
			}

			if(FilterViewModel.RestrictOnlySelfDelivery != null) {
				query.Where(o => o.SelfDelivery == FilterViewModel.RestrictOnlySelfDelivery);
			}

			if(FilterViewModel.RestrictWithoutSelfDelivery != null) {
				query.Where(o => o.SelfDelivery != FilterViewModel.RestrictWithoutSelfDelivery);
			}

			if(FilterViewModel.RestrictCounterparty != null) {
				query.Where(o => o.Client == FilterViewModel.RestrictCounterparty);
			}

			if(FilterViewModel.DeliveryPoint != null) {
				query.Where(o => o.DeliveryPoint == FilterViewModel.DeliveryPoint);
			}

			if(FilterViewModel.StartDate != null) {
				query.Where(o => o.DeliveryDate >= FilterViewModel.StartDate);
			}

			if(FilterViewModel.EndDate != null) {
				query.Where(o => o.DeliveryDate <= FilterViewModel.EndDate.Value.AddDays(1).AddTicks(-1));
			}

			if(FilterViewModel.RestrictLessThreeHours == true) {
				query.Where(Restrictions
							.GtProperty(Projections.SqlFunction(
											new SQLFunctionTemplate(NHibernateUtil.Time, "ADDTIME(?1, ?2)"),
											NHibernateUtil.Time,
											Projections.Property(() => deliveryScheduleAlias.From),
											Projections.Constant("3:0:0")),
											Projections.Property(() => deliveryScheduleAlias.To)));
			}

			if(FilterViewModel.RestrictHideService != null) 
			{
				if(FilterViewModel.RestrictHideService.Value)
				{
					query.Where(o => o.OrderAddressType != OrderAddressType.Service);
				}
				else
				{
					query.Where(o => o.OrderAddressType == OrderAddressType.Service);
				}
			}

			if(FilterViewModel.RestrictOnlyService != null) 
			{
				if(FilterViewModel.RestrictOnlyService.Value)
				{
					query.Where(o => o.OrderAddressType == OrderAddressType.Service);
				}
				else
				{
					query.Where(o => o.OrderAddressType != OrderAddressType.Service);
				}
			}
			
			if(FilterViewModel.OrderPaymentStatus != null) {
				query.Where(o => o.OrderPaymentStatus == FilterViewModel.OrderPaymentStatus);
			}

			if(FilterViewModel.ExcludeClosingDocumentDeliverySchedule)
			{
				query.Where(o => o.DeliverySchedule.Id == null || o.DeliverySchedule.Id != _closingDocumentDeliveryScheduleId);
			}

			var bottleCountSubquery = QueryOver.Of<OrderItem>(() => orderItemAlias)
				.Where(() => orderAlias.Id == orderItemAlias.Order.Id)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water && nomenclatureAlias.TareVolume == TareVolume.Vol19L)
				.Select(Projections.Sum(() => orderItemAlias.Count));

			var sanitisationCountSubquery = QueryOver.Of<OrderItem>(() => orderItemAlias)
													 .Where(() => orderAlias.Id == orderItemAlias.Order.Id)
													 .Where(Restrictions.In(Projections.Property(() => orderItemAlias.Nomenclature.Id), sanitizationNomenclatureIds))
													 .Select(Projections.Sum(() => orderItemAlias.Count));

			var orderSumSubquery = QueryOver.Of<OrderItem>(() => orderItemAlias)
											.Where(() => orderItemAlias.Order.Id == orderAlias.Id)
											.Select(
												Projections.Sum(
													Projections.SqlFunction(
														new SQLFunctionTemplate(NHibernateUtil.Decimal, "?1 * ?2 - IF(?3 IS NULL OR ?3 = 0, IFNULL(?4, 0), ?3)"),
														NHibernateUtil.Decimal,
														Projections.Property<OrderItem>(x => x.Count),
														Projections.Property<OrderItem>(x => x.Price),
														Projections.Property<OrderItem>(x => x.DiscountMoney),
														Projections.Property<OrderItem>(x => x.OriginalDiscountMoney)
													   )
												   )
											   );

			query.Left.JoinAlias(o => o.DeliveryPoint, () => deliveryPointAlias)
				 .Left.JoinAlias(o => o.DeliverySchedule, () => deliveryScheduleAlias)
				 .Left.JoinAlias(o => o.Client, () => counterpartyAlias)
				 .Left.JoinAlias(o => o.Author, () => authorAlias)
				 .Left.JoinAlias(o => o.LastEditor, () => lastEditorAlias)
				 .Left.JoinAlias(() => deliveryPointAlias.District, () => districtAlias);

			query.Where(GetSearchCriterion(
				() => orderAlias.Id,
				() => counterpartyAlias.Name,
				() => deliveryPointAlias.CompiledAddress,
				() => authorAlias.LastName,
				() => orderAlias.DriverCallId,
				() => orderAlias.OnlineOrder,
				() => orderAlias.EShopOrder,
				() => orderAlias.OrderPaymentStatus
			));
			
			if(FilterViewModel.IncludeDistrictsIds != null && FilterViewModel.IncludeDistrictsIds.Any())
				query = query.Where(() => deliveryPointAlias.District.Id.IsIn(FilterViewModel.IncludeDistrictsIds));
			
			// Для того чтобы уже добавленные в МЛ заказы больше не появлялись 
			if(FilterViewModel.ExceptIds != null && FilterViewModel.ExceptIds.Any())
				query.Where(o => !RestrictionExtensions.IsIn(o.Id, FilterViewModel.ExceptIds));
			
			var resultQuery = query
				.SelectList(list => list
				   .Select(() => orderAlias.Id).WithAlias(() => resultAlias.Id)
				   .Select(() => orderAlias.SelfDelivery).WithAlias(() => resultAlias.IsSelfDelivery)
				   .Select(() => deliveryScheduleAlias.Name).WithAlias(() => resultAlias.DeliveryTime)
				   .Select(() => orderAlias.OrderStatus).WithAlias(() => resultAlias.StatusEnum)
				   .Select(() => orderAlias.Address1c).WithAlias(() => resultAlias.Address1c)
				   .Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorLastName)
				   .Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
				   .Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
				   .Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.Counterparty)
				   .Select(() => districtAlias.DistrictName).WithAlias(() => resultAlias.DistrictName)
				   .Select(() => deliveryPointAlias.CompiledAddress).WithAlias(() => resultAlias.CompilledAddress)
				   .Select(() => deliveryPointAlias.City).WithAlias(() => resultAlias.City)
				   .Select(() => deliveryPointAlias.Street).WithAlias(() => resultAlias.Street)
				   .Select(() => deliveryPointAlias.Building).WithAlias(() => resultAlias.Building)
				   .SelectSubQuery(orderSumSubquery).WithAlias(() => resultAlias.Sum)
				   .SelectSubQuery(bottleCountSubquery).WithAlias(() => resultAlias.BottleAmount)
				   .SelectSubQuery(sanitisationCountSubquery).WithAlias(() => resultAlias.SanitisationAmount)
				)
				.OrderBy(x => x.CreateDate).Desc
				.TransformUsing(Transformers.AliasToBean<OrderForRouteListJournalNode<VodovozOrder>>());

			return resultQuery;
		}

		protected override void CreatePopupActions()
		{
			bool IsOrder(object[] objs) 
			{
				var selectedNodes = objs.Cast<OrderForRouteListJournalNode>();
				if(selectedNodes.Count() != 1)
					return false;

				return selectedNodes.FirstOrDefault().EntityType == typeof(VodovozOrder);
			}

			PopupActionsList.Add(
				new JournalAction(
					"Перейти в маршрутный лист",
					selectedItems => selectedItems.Any(
						x => CheckAccessRouteListKeeping((x as OrderForRouteListJournalNode).Id)) && IsOrder(selectedItems),
					selectedItems => true,
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<OrderForRouteListJournalNode>();
						var addresses = UoW.Session.QueryOver<RouteListItem>()
							.Where(x => x.Order.Id.IsIn(selectedNodes.Select(n => n.Id).ToArray())).List();

						var routes = addresses.GroupBy(x => x.RouteList.Id);

						var tdiMain = Startup.MainWin.TdiMain;

						foreach(var route in routes) {
							tdiMain.OpenTab(
								DialogHelper.GenerateDialogHashName<RouteList>(route.Key),
								() => new RouteListKeepingDlg(route.Key, route.Select(x => x.Order.Id).ToArray())
							);
						}
					}
				)
			);
			PopupActionsList.Add(
				new JournalAction(
					"Перейти в недовоз",
					(selectedItems) => selectedItems.Any(
						o => _undeliveredOrdersRepository.GetListOfUndeliveriesForOrder(
							UoW, (o as OrderForRouteListJournalNode).Id).Any()) && IsOrder(selectedItems),
					selectedItems => true,
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<OrderForRouteListJournalNode>();
						var order = UoW.GetById<VodovozOrder>(selectedNodes.FirstOrDefault().Id);

						var undeliveredOrdersFilter = new UndeliveredOrdersFilterViewModel(
							commonServices,
							_orderSelectorFactory,
							_employeeJournalFactory,
							_counterpartyJournalFactory,
							_deliveryPointJournalFactory,
							_subdivisionJournalFactory)
						{
							HidenByDefault = true,
							RestrictOldOrder = order,
							RestrictOldOrderStartDate = order.DeliveryDate,
							RestrictOldOrderEndDate = order.DeliveryDate
						};

						var dlg = new UndeliveredOrdersJournalViewModel(
							undeliveredOrdersFilter,
							UnitOfWorkFactory,
							commonServices,
							_gtkDialogsOpener,
							_employeeJournalFactory,
							_employeeService,
							_undeliveredOrdersJournalOpener,
							_orderSelectorFactory,
							_undeliveredOrdersRepository,
							new EmployeeSettings(new ParametersProvider()),
							_subdivisionParametersProvider
							);

						Startup.MainWin.TdiMain.AddTab(dlg);
					}
				)
			);
			PopupActionsList.Add(
				new JournalAction(
					"Открыть диалог закрытия",
					(selectedItems) => selectedItems.Any(
						x => CheckAccessToRouteListClosing((x as OrderForRouteListJournalNode).Id)) && IsOrder(selectedItems),
					selectedItems => true,
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<OrderForRouteListJournalNode>();
						var routeListIds = selectedNodes.Select(x => x.Id).ToArray();
						var addresses = UoW.Session.QueryOver<RouteListItem>()
							.Where(x => x.Order.Id.IsIn(routeListIds)).List();

						var routes = addresses.GroupBy(x => x.RouteList.Id);
						var tdiMain = Startup.MainWin.TdiMain;

						foreach(var rl in routes) {
							tdiMain.OpenTab(
								DialogHelper.GenerateDialogHashName<RouteList>(rl.Key),
								() => new RouteListClosingDlg(rl.Key)
							);
						}
					}
				)
			);
			PopupActionsList.Add(
				new JournalAction(
					"Открыть на Yandex картах(координаты)",
					IsOrder,
					selectedItems => true,
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<OrderForRouteListJournalNode>();
						foreach(var sel in selectedNodes) {
							var order = UoW.GetById<VodovozOrder>(sel.Id);
							if(order.DeliveryPoint == null || order.DeliveryPoint.Latitude == null || order.DeliveryPoint.Longitude == null)
								continue;

							System.Diagnostics.Process.Start(
								string.Format(
									CultureInfo.InvariantCulture,
									"https://maps.yandex.ru/?ll={0},{1}&z=17",
									order.DeliveryPoint.Longitude,
									order.DeliveryPoint.Latitude
								)
							);
						}
					}
				)
			);
			PopupActionsList.Add(
				new JournalAction(
					"Открыть на Yandex картах(адрес)",
					IsOrder,
					selectedItems => true,
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<OrderForRouteListJournalNode>();
						foreach(var sel in selectedNodes) {
							var order = UoW.GetById<VodovozOrder>(sel.Id);
							if(order.DeliveryPoint == null)
								continue;

							System.Diagnostics.Process.Start(
								string.Format(CultureInfo.InvariantCulture,
									"https://maps.yandex.ru/?text={0} {1} {2}",
									order.DeliveryPoint.City,
									order.DeliveryPoint.Street,
									order.DeliveryPoint.Building
								)
							);
						}
					}
				)
			);
			PopupActionsList.Add(
				new JournalAction(
					"Открыть на карте OSM",
					IsOrder,
					selectedItems => true,
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<OrderForRouteListJournalNode>();
						foreach(var sel in selectedNodes) {
							var order = UoW.GetById<VodovozOrder>(sel.Id);
							if(order.DeliveryPoint == null || order.DeliveryPoint.Latitude == null || order.DeliveryPoint.Longitude == null)
								continue;

							System.Diagnostics.Process.Start(string.Format(CultureInfo.InvariantCulture, "http://www.openstreetmap.org/#map=17/{1}/{0}", order.DeliveryPoint.Longitude, order.DeliveryPoint.Latitude));
						}
					}
				)
			);
		}

		bool CheckAccessToRouteListClosing(int orderId)
		{
			var orderIdArr = new[] { orderId };
			var routeListItems = UoW.Session.QueryOver<RouteListItem>()
						.Where(x => x.Order.Id.IsIn(orderIdArr)).List();

			if(routeListItems.Any()) {
				var validStates = new RouteListStatus[] {
											RouteListStatus.OnClosing,
											RouteListStatus.MileageCheck,
											RouteListStatus.Closed
								  };
				return validStates.Contains(routeListItems.First().RouteList.Status);
			}
			return false;
		}

		bool CheckAccessRouteListKeeping(int orderId)
		{
			var orderIdArr = new[] { orderId };
			var routeListItems = UoW.Session.QueryOver<RouteListItem>()
						.Where(x => x.Order.Id.IsIn(orderIdArr)).List();

			if(routeListItems.Any()) {
				return true;
			}
			return false;
		}

		protected override Func<IUnitOfWork, IQueryOver<VodovozOrder>> ItemsSourceQueryFunction => GetOrdersQuery;
		protected override Func<OrderDlg> CreateDialogFunction => () => new OrderDlg ();
		protected override Func<OrderForRouteListJournalNode, OrderDlg> OpenDialogFunction => node => new OrderDlg(node.Id);
	}
}

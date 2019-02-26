using System;
using System.Globalization;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using NHibernate.Util;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QS.Utilities.Text;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using QSProjectsLib;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.JournalViewers;
using Vodovoz.Repositories;
using Vodovoz.Repository;

namespace Vodovoz.ViewModel
{
	public class OrdersVM : RepresentationModelEntityBase<Vodovoz.Domain.Orders.Order, OrdersVMNode>
	{
		public OrdersFilter Filter {
			get => RepresentationFilter as OrdersFilter;
			set => RepresentationFilter = value as IRepresentationFilter;
		}
		public bool CanToggleVisibilityOfColumns { get; set; }

		#region IRepresentationModel implementation

		Nomenclature sanitizationNomenclature = null;

		public override void UpdateNodes()
		{
			OrdersVMNode resultAlias = null;
			Vodovoz.Domain.Orders.Order orderAlias = null;
			Nomenclature nomenclatureAlias = null;
			OrderItem orderItemAlias = null;
			Counterparty counterpartyAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			DeliverySchedule deliveryScheduleAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;
			ScheduleRestrictedDistrict districtAlias = null;

			var query = UoW.Session.QueryOver<Vodovoz.Domain.Orders.Order>(() => orderAlias);

			if(Filter.RestrictStatus != null) {
				query.Where(o => o.OrderStatus == Filter.RestrictStatus);
			}

			if(Filter.RestrictPaymentType != null) {
				query.Where(o => o.PaymentType == Filter.RestrictPaymentType);
			}

			if(Filter.HideStatuses != null) {
				query.WhereRestrictionOn(o => o.OrderStatus).Not.IsIn(Filter.HideStatuses);
			}

			if(Filter.RestrictSelfDelivery != null) {
				query.Where(o => o.SelfDelivery == Filter.RestrictSelfDelivery);
			}

			if(Filter.RestrictWithoutSelfDelivery != null) {
				query.Where(o => o.SelfDelivery != Filter.RestrictWithoutSelfDelivery);
			}

			if(Filter.RestrictCounterparty != null) {
				query.Where(o => o.Client == Filter.RestrictCounterparty);
			}

			if(Filter.RestrictDeliveryPoint != null) {
				query.Where(o => o.DeliveryPoint == Filter.RestrictDeliveryPoint);
			}

			if(Filter.RestrictStartDate != null) {
				query.Where(o => o.DeliveryDate >= Filter.RestrictStartDate);
			}

			if(Filter.RestrictEndDate != null) {
				query.Where(o => o.DeliveryDate <= Filter.RestrictEndDate.Value.AddDays(1).AddTicks(-1));
			}

			if(Filter.RestrictOnlyWithoutCoodinates) {
				query.Where(() => deliveryPointAlias.Longitude == null && deliveryPointAlias.Latitude == null);
			}

			if(Filter.RestrictLessThreeHours == true) {
				query.Where(Restrictions
							.GtProperty(Projections.SqlFunction(
											new SQLFunctionTemplate(NHibernateUtil.Time, "ADDTIME(?1, ?2)"),
											NHibernateUtil.Time,
											Projections.Property(() => deliveryScheduleAlias.From),
											Projections.Constant("3:0:0")),
											Projections.Property(() => deliveryScheduleAlias.To)));
			}

			if(Filter.RestrictHideService != null) {
				query.Where(o => o.IsService != Filter.RestrictHideService);
			}

			if(Filter.RestrictOnlyService != null) {
				query.Where(o => o.IsService == Filter.RestrictOnlyService);
			}

			if(Filter.ExceptIds != null && Filter.ExceptIds.Any())
				query.Where(o => !RestrictionExtensions.IsIn(o.Id, Filter.ExceptIds));

			var bottleCountSubquery = QueryOver.Of<OrderItem>(() => orderItemAlias)
				.Where(() => orderAlias.Id == orderItemAlias.Order.Id)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water && nomenclatureAlias.TareVolume == TareVolume.Vol19L)
				.Select(Projections.Sum(() => orderItemAlias.Count));

			var sanitisationCountSubquery = QueryOver.Of<OrderItem>(() => orderItemAlias)
													 .Where(() => orderAlias.Id == orderItemAlias.Order.Id)
													 .Where(() => orderItemAlias.Nomenclature.Id == sanitizationNomenclature.Id)
													 .Select(Projections.Sum(() => orderItemAlias.Count));

			var orderSumSubquery = QueryOver.Of<OrderItem>(() => orderItemAlias)
											.Where(() => orderItemAlias.Order.Id == orderAlias.Id)
											.Select(
												Projections.Sum(
													Projections.SqlFunction(
														new SQLFunctionTemplate(NHibernateUtil.Decimal, "?1 * ?2 - ?3"),
														NHibernateUtil.Decimal,
														Projections.Property<OrderItem>(x => x.Count),
														Projections.Property<OrderItem>(x => x.Price),
														Projections.Property<OrderItem>(x => x.DiscountMoney)
													   )
												   )
											   );

			query.JoinAlias(o => o.DeliveryPoint, () => deliveryPointAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				 .JoinAlias(o => o.DeliverySchedule, () => deliveryScheduleAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				 .JoinAlias(o => o.Client, () => counterpartyAlias)
				 .JoinAlias(o => o.Author, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				 .JoinAlias(o => o.LastEditor, () => lastEditorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				 .Left.JoinAlias(() => deliveryPointAlias.District, () => districtAlias);

			if(Filter.IncludeDistrictsIds != null && Filter.IncludeDistrictsIds.Any())
				query = query.Where(() => deliveryPointAlias.District.Id.IsIn(Filter.IncludeDistrictsIds));

			var result = query
				.SelectList(list => list
				   .Select(() => orderAlias.Id).WithAlias(() => resultAlias.Id)
				   .Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.Date)
				   .Select(() => deliveryScheduleAlias.Name).WithAlias(() => resultAlias.DeliveryTime)
				   .Select(() => orderAlias.OrderStatus).WithAlias(() => resultAlias.StatusEnum)
				   .Select(() => orderAlias.Address1c).WithAlias(() => resultAlias.Address1c)
				   .Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorLastName)
				   .Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
				   .Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
				   .Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorLastName)
				   .Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
				   .Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
				   .Select(() => orderAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime)
				   .Select(() => orderAlias.DriverCallId).WithAlias(() => resultAlias.DriverCallId)
				   .Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.Counterparty)
				   .Select(() => districtAlias.DistrictName).WithAlias(() => resultAlias.DistrictName)
				   .Select(() => deliveryPointAlias.City).WithAlias(() => resultAlias.City)
				   .Select(() => deliveryPointAlias.Street).WithAlias(() => resultAlias.Street)
				   .Select(() => deliveryPointAlias.Building).WithAlias(() => resultAlias.Building)
				   .Select(() => deliveryPointAlias.Latitude).WithAlias(() => resultAlias.Latitude)
				   .Select(() => deliveryPointAlias.Longitude).WithAlias(() => resultAlias.Longitude)
				   .SelectSubQuery(orderSumSubquery).WithAlias(() => resultAlias.Sum)
				   .SelectSubQuery(bottleCountSubquery).WithAlias(() => resultAlias.BottleAmount)
				   .SelectSubQuery(sanitisationCountSubquery).WithAlias(() => resultAlias.SanitisationAmount)
				).OrderBy(x => x.DeliveryDate).Desc
				.TransformUsing(Transformers.AliasToBean<OrdersVMNode>())
				.List<OrdersVMNode>();

			if(CanToggleVisibilityOfColumns)
				ShowColumns(Filter.RestrictOnlyService);

			SetItemsSource(result);
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig<OrdersVMNode>.Create()
			.AddColumn("Номер").SetDataProperty(node => node.Id.ToString())
			.AddColumn("Дата").SetDataProperty(node => node.Date.ToString("d"))
			.AddColumn("Автор").SetDataProperty(node => node.Author)
			.AddColumn("Время").SetDataProperty(node => node.DeliveryTime)
			.AddColumn("Статус").SetDataProperty(node => node.StatusEnum.GetEnumTitle())
			.AddColumn("Бутыли").AddTextRenderer(node => node.BottleAmount.ToString())
			.AddColumn("Кол-во с/о")
				.SetTag("Hidden")
				.AddTextRenderer(node => node.SanitisationAmount.ToString())
			.AddColumn("Клиент").SetDataProperty(node => node.Counterparty)
			.AddColumn("Сумма").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.Sum))
			.AddColumn("Коор.").AddTextRenderer(x => x.Latitude.HasValue && x.Longitude.HasValue ? "Есть" : String.Empty)
			.AddColumn("Район доставки").SetDataProperty(node => node.DistrictName)
			.AddColumn("Адрес").SetDataProperty(node => node.Address)
			.AddColumn("Изменил").SetDataProperty(node => node.LastEditor)
			.AddColumn("Послед. изменения").AddTextRenderer(node => node.LastEditedTime != default(DateTime) ? node.LastEditedTime.ToString() : String.Empty)
			.AddColumn("Номер звонка").SetDataProperty(node => node.DriverCallId)
			.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = n.RowColor)
			.Finish();

		public override IColumnsConfig ColumnsConfig => columnsConfig;
		public override bool PopupMenuExist => true;

		public void ShowColumns(bool? val = null)
		{
			foreach(var c in columnsConfig.GetColumnsByTag("Hidden"))
				c.Visible = val.HasValue && val.Value;
		}

		public override Gtk.Menu GetPopupMenu(RepresentationSelectResult[] selected)
		{
			lastMenuSelected = selected;

			Menu popupMenu = new Gtk.Menu();
			Gtk.MenuItem menuItemRouteList = new MenuItem("Перейти в маршрутный лист");
			menuItemRouteList.Activated += MenuItemRouteList_Activated;
			menuItemRouteList.Sensitive = selected.Any(x => {
				var availableStatuses = new OrderStatus[] { OrderStatus.OnTheWay, OrderStatus.DeliveryCanceled, OrderStatus.Shipped, OrderStatus.UnloadingOnStock, OrderStatus.NotDelivered };
				return availableStatuses.Contains(((OrdersVMNode)x.VMNode).StatusEnum);
			});
			popupMenu.Add(menuItemRouteList);

			Gtk.MenuItem menuItemUndelivery = new MenuItem("Перейти в недовоз");
			menuItemUndelivery.Activated += MenuItemUndelivery_Activated;
			menuItemUndelivery.Sensitive = selected.Any(o => UndeliveredOrdersRepository.GetListOfUndeliveriesForOrder(UoW, o.EntityId).Any());
			popupMenu.Add(menuItemUndelivery);

			Gtk.MenuItem menuItemRouteListClosingDlg = new Gtk.MenuItem("Открыть диалог закрытия");
			menuItemRouteListClosingDlg.Activated += MenuItemRouteListClosingDlg_Activated;
			menuItemRouteListClosingDlg.Sensitive = selected.Any(x => IsOrderInRouteListStatusEnRouted(x.EntityId));
			popupMenu.Add(menuItemRouteListClosingDlg);

			popupMenu.Add(new SeparatorMenuItem());

			Gtk.MenuItem menuItemYandex = new MenuItem("Открыть на Yandex картах(координаты)");
			menuItemYandex.Activated += MenuItemYandex_Activated;
			popupMenu.Add(menuItemYandex);
			Gtk.MenuItem menuItemYandexAddress = new MenuItem("Открыть на Yandex картах(адрес)");
			menuItemYandexAddress.Activated += MenuItemYandexAddress_Activated;
			popupMenu.Add(menuItemYandexAddress);
			Gtk.MenuItem menuItemOSM = new MenuItem("Открыть на карте OSM");
			menuItemOSM.Activated += MenuItemOSM_Activated;
			popupMenu.Add(menuItemOSM);
			return popupMenu;
		}

		#endregion

		private RepresentationSelectResult[] lastMenuSelected;

		void MenuItemOSM_Activated(object sender, EventArgs e)
		{
			foreach(var sel in lastMenuSelected) {
				var order = UoW.GetById<Vodovoz.Domain.Orders.Order>(sel.EntityId);
				if(order.DeliveryPoint == null || order.DeliveryPoint.Latitude == null || order.DeliveryPoint.Longitude == null)
					continue;

				System.Diagnostics.Process.Start(String.Format(CultureInfo.InvariantCulture, "http://www.openstreetmap.org/#map=17/{1}/{0}", order.DeliveryPoint.Longitude, order.DeliveryPoint.Latitude));
			}
		}

		bool IsOrderInRouteListStatusEnRouted(int orderId)
		{
			var orderIdArr = new[] { orderId };
			var routeListItems = UoW.Session.QueryOver<RouteListItem>()
						.Where(x => x.Order.Id.IsIn(orderIdArr)).List();

			if(routeListItems.Count != 0) {
				foreach(var routeListItem in routeListItems) {
					if(routeListItem.RouteList.Status >= RouteListStatus.EnRoute)
						return true;
					return false;
				}
			}
			return false;
		}

		void MenuItemRouteList_Activated(object sender, EventArgs e)
		{
			var ordersIds = lastMenuSelected.Select(x => x.EntityId).ToArray();
			var addresses = UoW.Session.QueryOver<RouteListItem>()
				.Where(x => x.Order.Id.IsIn(ordersIds)).List();

			var routes = addresses.GroupBy(x => x.RouteList.Id);

			var tdiMain = MainClass.MainWin.TdiMain;

			foreach(var route in routes) {
				tdiMain.OpenTab(
					DialogHelper.GenerateDialogHashName<RouteList>(route.Key),
					() => new RouteListKeepingDlg(route.Key, route.Select(x => x.Order.Id).ToArray())
				);
			}
		}

		void MenuItemUndelivery_Activated(object sender, EventArgs e)
		{
			var order = UoW.GetById<Domain.Orders.Order>(lastMenuSelected.FirstOrDefault().EntityId);
			UndeliveriesView dlg = new UndeliveriesView();
			dlg.HideFilterAndControls();
			dlg.GetUndeliveryFilter.SetAndRefilterAtOnce(
				x => x.ResetFilter(),
				x => x.RestrictOldOrder = order,
				x => x.RestrictOldOrderStartDate = order.DeliveryDate,
				x => x.RestrictOldOrderEndDate = order.DeliveryDate
			);
			MainClass.MainWin.TdiMain.AddTab(dlg);
		}


		void MenuItemRouteListClosingDlg_Activated(object sender, EventArgs e)
		{
			var routeListIds = lastMenuSelected.Select(x => x.EntityId).ToArray();
			var addresses = UoW.Session.QueryOver<RouteListItem>()
				.Where(x => x.Order.Id.IsIn(routeListIds)).List();

			var routes = addresses.GroupBy(x => x.RouteList.Id);
			var tdiMain = MainClass.MainWin.TdiMain;

			foreach(var rl in routes) {
				tdiMain.OpenTab(
					DialogHelper.GenerateDialogHashName<RouteList>(rl.Key),
					() => new RouteListClosingDlg(rl.Key)
				);
			}
		}

		void MenuItemYandexAddress_Activated(object sender, EventArgs e)
		{
			foreach(var sel in lastMenuSelected) {
				var order = UoW.GetById<Vodovoz.Domain.Orders.Order>(sel.EntityId);
				if(order.DeliveryPoint == null)
					continue;

				System.Diagnostics.Process.Start(
					String.Format(CultureInfo.InvariantCulture,
						"https://maps.yandex.ru/?text={0} {1} {2}",
						order.DeliveryPoint.City,
						order.DeliveryPoint.Street,
						order.DeliveryPoint.Building
					));
			}
		}

		void MenuItemYandex_Activated(object sender, EventArgs e)
		{
			foreach(var sel in lastMenuSelected) {
				var order = UoW.GetById<Vodovoz.Domain.Orders.Order>(sel.EntityId);
				if(order.DeliveryPoint == null || order.DeliveryPoint.Latitude == null || order.DeliveryPoint.Longitude == null)
					continue;

				System.Diagnostics.Process.Start(String.Format(CultureInfo.InvariantCulture, "https://maps.yandex.ru/?ll={0},{1}&z=17", order.DeliveryPoint.Longitude, order.DeliveryPoint.Latitude));
			}
		}

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc(Vodovoz.Domain.Orders.Order updatedSubject) => true;

		#endregion

		public OrdersVM(OrdersFilter filter) : this(filter.UoW)
		{
			Filter = filter;
		}

		public OrdersVM() : this(UnitOfWorkFactory.CreateWithoutRoot())
		{
			CreateRepresentationFilter = () => new OrdersFilter(UoW);
		}

		public OrdersVM(IUnitOfWork uow) : base()
		{
			this.UoW = uow;
			sanitizationNomenclature = NomenclatureRepository.GetSanitisationNomenclature(UoW);
			ShowColumns(false);
		}
	}

	public class OrdersVMNode
	{
		[UseForSearch]
		[SearchHighlight]
		public int Id { get; set; }

		public OrderStatus StatusEnum { get; set; }

		public DateTime Date { get; set; }
		public string DeliveryTime { get; set; }
		public int BottleAmount { get; set; }
		public int SanitisationAmount { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string Counterparty { get; set; }

		public decimal Sum { get; set; }

		public string DistrictName { get; set; }
		public string City { get; set; }
		public string Street { get; set; }
		public string Building { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string Address1c { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string Address => String.Format("{0}, {1} д.{2}", City, Street, Building);

		public string AuthorLastName { get; set; }
		public string AuthorName { get; set; }
		public string AuthorPatronymic { get; set; }

		public string LastEditorLastName { get; set; }
		public string LastEditorName { get; set; }
		public string LastEditorPatronymic { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string Author => PersonHelper.PersonNameWithInitials(AuthorLastName, AuthorName, AuthorPatronymic);

		[UseForSearch]
		[SearchHighlight]
		public string LastEditor => PersonHelper.PersonNameWithInitials(LastEditorLastName, LastEditorName, LastEditorPatronymic);

		[UseForSearch]
		[SearchHighlight]
		public DateTime LastEditedTime { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public int DriverCallId { get; set; }

		public decimal? Latitude { get; set; }
		public decimal? Longitude { get; set; }

		public string RowColor {
			get {
				if(StatusEnum == OrderStatus.Canceled || StatusEnum == OrderStatus.DeliveryCanceled)
					return "grey";
				if(StatusEnum == OrderStatus.Closed)
					return "green";
				if(StatusEnum == OrderStatus.NotDelivered)
					return "blue";
				return "black";
			}
		}
	}
}
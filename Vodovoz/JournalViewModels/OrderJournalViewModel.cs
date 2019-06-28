using System;
using System.Globalization;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.Dialog.Gtk;
using QS.DomainModel.Config;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalNodes;
using Vodovoz.JournalViewers;
using Vodovoz.Repositories;
using VodovozOrder = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.JournalViewModels
{
	public class OrderJournalViewModel : FilterableSingleEntityJournalViewModelBase<VodovozOrder, OrderDlg, OrderJournalNode, OrderJournalFilterViewModel>
	{
		public OrderJournalViewModel(OrderJournalFilterViewModel filterViewModel, IEntityConfigurationProvider entityConfigurationProvider, ICommonServices commonServices) : base(filterViewModel, entityConfigurationProvider, commonServices)
		{
			TabName = "Журнал заказов";
			SetOrder<OrderJournalNode>(x => x.CreateDate, true);

			RegisterAliasPropertiesToSearch(
				() => counterpartyAlias.Name,
				() => deliveryPointAlias.City,
				() => deliveryPointAlias.Street,
				() => deliveryPointAlias.Building,
				() => authorAlias.Name,
				() => authorAlias.LastName,
				() => authorAlias.Patronymic,
				() => lastEditorAlias.Name,
				() => lastEditorAlias.LastName,
				() => lastEditorAlias.Patronymic,
				() => orderAlias.LastEditedTime,
				() => orderAlias.DriverCallId,
				() => orderAlias.OnlineOrder,
				() => orderAlias.Id
			);
			UpdateOnChanges(
				typeof(VodovozOrder),
				typeof(OrderItem)
			);
		}

		OrderJournalNode resultAlias = null;
		VodovozOrder orderAlias = null;
		Nomenclature nomenclatureAlias = null;
		OrderItem orderItemAlias = null;
		Counterparty counterpartyAlias = null;
		DeliveryPoint deliveryPointAlias = null;
		DeliverySchedule deliveryScheduleAlias = null;
		Employee authorAlias = null;
		Employee lastEditorAlias = null;
		ScheduleRestrictedDistrict districtAlias = null;

		protected override Func<IQueryOver<VodovozOrder>> ItemsSourceQueryFunction => () => {

			Nomenclature sanitizationNomenclature = new NomenclatureRepository().GetSanitisationNomenclature(UoW);

			var query = UoW.Session.QueryOver<VodovozOrder>(() => orderAlias);

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

			if(FilterViewModel.RestrictDeliveryPoint != null) {
				query.Where(o => o.DeliveryPoint == FilterViewModel.RestrictDeliveryPoint);
			}

			if(FilterViewModel.RestrictStartDate != null) {
				query.Where(o => o.DeliveryDate >= FilterViewModel.RestrictStartDate);
			}

			if(FilterViewModel.RestrictEndDate != null) {
				query.Where(o => o.DeliveryDate <= FilterViewModel.RestrictEndDate.Value.AddDays(1).AddTicks(-1));
			}

			if(FilterViewModel.RestrictOnlyWithoutCoodinates) {
				query.Where(() => deliveryPointAlias.Longitude == null && deliveryPointAlias.Latitude == null);
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

			if(FilterViewModel.RestrictHideService != null) {
				query.Where(o => o.IsService != FilterViewModel.RestrictHideService);
			}

			if(FilterViewModel.RestrictOnlyService != null) {
				query.Where(o => o.IsService == FilterViewModel.RestrictOnlyService);
			}

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
				 .JoinAlias(o => o.Client, () => counterpartyAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				 .JoinAlias(o => o.Author, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				 .JoinAlias(o => o.LastEditor, () => lastEditorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				 .Left.JoinAlias(() => deliveryPointAlias.District, () => districtAlias);

			var resultQuery = query
				.SelectList(list => list
				   .Select(() => orderAlias.Id).WithAlias(() => resultAlias.Id)
				   .Select(() => orderAlias.SelfDelivery).WithAlias(() => resultAlias.IsSelfDelivery)
				   .Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.Date)
				   .Select(() => orderAlias.CreateDate).WithAlias(() => resultAlias.CreateDate)
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
				   .Select(() => orderAlias.OnlineOrder).WithAlias(() => resultAlias.OnlineOrder)
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
				)
				.OrderBy(x => x.CreateDate).Desc
				.TransformUsing(Transformers.AliasToBean<OrderJournalNode>());

			return resultQuery;
		};

		protected override Func<OrderDlg> CreateDialogFunction => () => new OrderDlg();

		protected override Func<OrderJournalNode, OrderDlg> OpenDialogFunction => node => new OrderDlg(node.Id);

		protected override void CreatePopupActions()
		{
			PopupActionsList.Add(
				new JournalAction(
					"Перейти в маршрутный лист",
					selectedItems => selectedItems.Any(
						x => (x as OrderJournalNode).StatusEnum != OrderStatus.Accepted && (x as OrderJournalNode).StatusEnum != OrderStatus.NewOrder
					),
					selectedItems => true,
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<OrderJournalNode>();
						var addresses = UoW.Session.QueryOver<RouteListItem>()
							.Where(x => x.Order.Id.IsIn(selectedNodes.Select(n => n.Id).ToArray())).List();

						var routes = addresses.GroupBy(x => x.RouteList.Id);

						var tdiMain = MainClass.MainWin.TdiMain;

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
					(selectedItems) => selectedItems.Any(o => UndeliveredOrdersRepository.GetListOfUndeliveriesForOrder(UoW, (o as OrderJournalNode).Id).Any()),
					selectedItems => true,
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<OrderJournalNode>();
						var order = UoW.GetById<VodovozOrder>(selectedNodes.FirstOrDefault().Id);
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
				)
			);
			PopupActionsList.Add(
				new JournalAction(
					"Открыть диалог закрытия",
					(selectedItems) => selectedItems.Any(x => IsOrderInRouteListStatusEnRouted((x as OrderJournalNode).Id)),
					selectedItems => true,
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<OrderJournalNode>();
						var routeListIds = selectedNodes.Select(x => x.Id).ToArray();
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
				)
			);
			PopupActionsList.Add(
				new JournalAction(
					"Открыть на Yandex картах(координаты)",
					selectedItems => true,
					selectedItems => true,
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<OrderJournalNode>();
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
					selectedItems => true,
					selectedItems => true,
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<OrderJournalNode>();
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
					selectedItems => true,
					selectedItems => true,
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<OrderJournalNode>();
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

		bool IsOrderInRouteListStatusEnRouted(int orderId)
		{
			var orderIdArr = new[] { orderId };
			var routeListItems = UoW.Session.QueryOver<RouteListItem>()
						.Where(x => x.Order.Id.IsIn(orderIdArr)).List();

			if(routeListItems.Any()) {
				foreach(var routeListItem in routeListItems) {
					if(routeListItem.RouteList.Status >= RouteListStatus.EnRoute)
						return true;
					return false;
				}
			}
			return false;
		}

	}
}

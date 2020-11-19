using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.JournalFilters;
using Vodovoz.Repositories;
using Vodovoz.SidePanel.InfoProviders;

namespace Vodovoz.SidePanel.InfoViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UndeliveredOrdersPanelView : Gtk.Bin, IPanelView
	{
		public UndeliveredOrdersPanelView()
		{
			this.Build();

			Gdk.Color wh = new Gdk.Color(255, 255, 255);
			Gdk.Color gr = new Gdk.Color(223, 223, 223);
			yTreeView.ColumnsConfig = ColumnsConfigFactory.Create<object[]>()
				.AddColumn("Виновный")
					.AddTextRenderer(n => n[0].ToString())
					.WrapWidth(150).WrapMode(Pango.WrapMode.WordChar)
				.AddColumn("Кол-во")
					.AddTextRenderer(n => n[1].ToString())
					.WrapWidth(50).WrapMode(Pango.WrapMode.WordChar)
				.RowCells()
					.AddSetter<CellRenderer>((c, n) => c.CellBackgroundGdk = (int)n[2] % 2 == 0 ? wh : gr)
				.Finish();
		}

		List<object[]> guilties = new List<object[]>();

		#region IPanelView implementation

		public IInfoProvider InfoProvider { get; set; }

		public bool VisibleOnPanel => true;

		public void OnCurrentObjectChanged(object changedObject) => Refresh();

		public void Refresh()
		{
			var undeliveredOrdersFilter = (InfoProvider as IUndeliveredOrdersInfoProvider).UndeliveredOrdersFilter;

			guilties = new List<object[]>(GetGuilties(undeliveredOrdersFilter));

			Application.Invoke((s, args) => DrawRefreshed(undeliveredOrdersFilter));
		}

		#endregion

		private void DrawRefreshed(UndeliveredOrdersFilter undeliveredOrdersFilter)
		{
			lblCaption.Markup = "<u><b>Сводка по недовозам\nСписок виновных:</b></u>";

			yTreeView.ItemsDataSource = guilties;

			lblTotalUdeliveredBottles.Markup = 
				$"Воды 19л: <b>{guilties.Sum(g => (decimal) g[3]):N0}</b> бут.";

			lblTotalUndeliveredOrders.Markup = 
				$"Заказов: <b>{guilties.Sum(o => (int) o[1])}</b> шт.";
		}

		#region Queries

		public IList<object[]> GetGuilties(UndeliveredOrdersFilter filter)
		{
			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;

			UndeliveredOrder undeliveredOrderAlias = null;
			Domain.Orders.Order oldOrderAlias = null;
			Domain.Orders.Order newOrderAlias = null;
			Employee oldOrderAuthorAlias = null;
			Counterparty counterpartyAlias = null;
			DeliveryPoint undeliveredOrderDeliveryPointAlias = null;
			Subdivision subdivisionAlias = null;
			GuiltyInUndelivery guiltyInUndeliveryAlias = null;

			var subquery19LWatterQty = QueryOver.Of<OrderItem>(() => orderItemAlias)
												.Where(() => orderItemAlias.Order.Id == oldOrderAlias.Id)
												.Left.JoinQueryOver(i => i.Nomenclature, () => nomenclatureAlias)
												.Where(n => n.Category == NomenclatureCategory.water && n.TareVolume == TareVolume.Vol19L)
												.Select(Projections.Sum(() => orderItemAlias.Count));

			var query = InfoProvider.UoW.Session.QueryOver<UndeliveredOrder>(() => undeliveredOrderAlias)
						   .Left.JoinAlias(u => u.OldOrder, () => oldOrderAlias)
						   .Left.JoinAlias(u => u.NewOrder, () => newOrderAlias)
						   .Left.JoinAlias(() => oldOrderAlias.Client, () => counterpartyAlias)
						   .Left.JoinAlias(() => oldOrderAlias.Author, () => oldOrderAuthorAlias)
						   .Left.JoinAlias(() => oldOrderAlias.DeliveryPoint, () => undeliveredOrderDeliveryPointAlias)
						   .Left.JoinAlias(() => undeliveredOrderAlias.GuiltyInUndelivery, () => guiltyInUndeliveryAlias)
						   .Left.JoinAlias(() => guiltyInUndeliveryAlias.GuiltyDepartment, () => subdivisionAlias);

			if(filter?.RestrictDriver != null) {
				var oldOrderIds = UndeliveredOrdersRepository.GetListOfUndeliveryIdsForDriver(InfoProvider.UoW, filter.RestrictDriver);
				query.Where(() => oldOrderAlias.Id.IsIn(oldOrderIds.ToArray()));
			}

			if(filter?.RestrictOldOrder != null)
				query.Where(() => oldOrderAlias.Id == filter.RestrictOldOrder.Id);

			if(filter?.RestrictClient != null)
				query.Where(() => counterpartyAlias.Id == filter.RestrictClient.Id);

			if(filter?.RestrictAddress != null)
				query.Where(() => undeliveredOrderDeliveryPointAlias.Id == filter.RestrictAddress.Id);

			if(filter?.RestrictOldOrderAuthor != null)
				query.Where(() => oldOrderAuthorAlias.Id == filter.RestrictOldOrderAuthor.Id);

			if(filter?.RestrictOldOrderStartDate != null)
				query.Where(() => oldOrderAlias.DeliveryDate >= filter.RestrictOldOrderStartDate);

			if(filter?.RestrictOldOrderEndDate != null)
				query.Where(() => oldOrderAlias.DeliveryDate <= filter.RestrictOldOrderEndDate.Value.AddDays(1).AddTicks(-1));

			if(filter?.RestrictNewOrderStartDate != null)
				query.Where(() => newOrderAlias.DeliveryDate >= filter.RestrictNewOrderStartDate);

			if(filter?.RestrictNewOrderEndDate != null)
				query.Where(() => newOrderAlias.DeliveryDate <= filter.RestrictNewOrderEndDate.Value.AddDays(1).AddTicks(-1));

			if(filter?.RestrictGuiltySide != null)
				query.Where(() => guiltyInUndeliveryAlias.GuiltySide == filter.RestrictGuiltySide);

			if(filter != null && filter.IsProblematicCasesChkActive)
				query.Where(() => !guiltyInUndeliveryAlias.GuiltySide.IsIn(filter.ExcludingGuiltiesForProblematicCases));

			if(filter?.RestrictGuiltyDepartment != null)
				query.Where(() => subdivisionAlias.Id == filter.RestrictGuiltyDepartment.Id);

			if(filter?.RestrictInProcessAtDepartment != null)
				query.Where(u => u.InProcessAtDepartment.Id == filter.RestrictInProcessAtDepartment.Id);

			if(filter?.NewInvoiceCreated != null) {
				if(filter.NewInvoiceCreated.Value)
					query.Where(u => u.NewOrder != null);
				else
					query.Where(u => u.NewOrder == null);
			}

			if(filter?.RestrictUndeliveryStatus != null)
				query.Where(u => u.UndeliveryStatus == filter.RestrictUndeliveryStatus);

			if(filter?.RestrictUndeliveryAuthor != null)
				query.Where(u => u.Author == filter.RestrictUndeliveryAuthor);

			int position = 0;
			var result = query.SelectList(list => list
										.SelectGroup(u => u.Id)
										.Select(
											  Projections.SqlFunction(
												  new SQLFunctionTemplate(
													  NHibernateUtil.String,
													  "GROUP_CONCAT(CASE ?1 WHEN 'Department' THEN IFNULL(CONCAT('Отд: ', ?2), 'Отдел ВВ') WHEN 'Client' THEN 'Клиент' WHEN 'Driver' THEN 'Водитель' WHEN 'ServiceMan' THEN 'Мастер СЦ' WHEN 'None' THEN 'Нет (не недовоз)' WHEN 'Unknown' THEN 'Неизвестно' ELSE ?1 END ORDER BY ?1 ASC SEPARATOR '\n')"
													 ),
												  NHibernateUtil.String,
												  Projections.Property(() => guiltyInUndeliveryAlias.GuiltySide),
												  Projections.Property(() => subdivisionAlias.ShortName)
												 )
											 )
										 .SelectSubQuery(subquery19LWatterQty)
										 )
							  .List<object[]>()
							  .GroupBy(x => x[1])
							  .Select(r => new[] { r.Key, r.Count(), position++, r.Sum(x => x[2] == null ? 0 : (decimal)x[2]) })
							  .ToList();
			return result;
		}

		#endregion
	}
}

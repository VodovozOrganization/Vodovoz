using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using Gtk;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using QS.DomainModel.UoW;
using QS.Project.Services;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.Infrastructure;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;
using IUndeliveredOrdersInfoProvider = Vodovoz.ViewModels.Infrastructure.InfoProviders.IUndeliveredOrdersInfoProvider;

namespace Vodovoz.SidePanel.InfoViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UndeliveredOrdersPanelView : Gtk.Bin, IPanelView
	{
		private readonly IUnitOfWork _uow;
		private readonly IUndeliveredOrdersRepository _undeliveredOrdersRepository;
		
		public UndeliveredOrdersPanelView(IUndeliveredOrdersRepository undeliveredOrdersRepository)
		{
			_undeliveredOrdersRepository = undeliveredOrdersRepository
				?? throw new ArgumentNullException(nameof(undeliveredOrdersRepository));

			Build();

			yTreeView.ColumnsConfig = ColumnsConfigFactory.Create<object[]>()
				.AddColumn("Ответственный")
					.AddTextRenderer(n => n[0] != null ? n[0].ToString() : "")
					.WrapWidth(150).WrapMode(Pango.WrapMode.WordChar)
				.AddColumn("Кол-во")
					.AddTextRenderer(n => n[1].ToString())
					.WrapWidth(50).WrapMode(Pango.WrapMode.WordChar)
				.RowCells()
					.AddSetter<CellRenderer>((c, n) => c.CellBackgroundGdk = (int)n[2] % 2 == 0 ? GdkColors.PrimaryBase : GdkColors.InsensitiveBase)
				.Finish();
			
			_uow = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
		}

		List<object[]> guilties = new List<object[]>();

		#region IPanelView implementation

		public IInfoProvider InfoProvider { get; set; }

		public bool VisibleOnPanel => true;

		public void OnCurrentObjectChanged(object changedObject) => Refresh();

		public void Refresh()
		{
			var undeliveredOrdersFilter = (InfoProvider as IUndeliveredOrdersInfoProvider)?.UndeliveredOrdersFilterViewModel;

			guilties = new List<object[]>(GetGuilties(undeliveredOrdersFilter));

			Gtk.Application.Invoke((s, args) => DrawRefreshed(undeliveredOrdersFilter));
		}

		#endregion

		private void DrawRefreshed(UndeliveredOrdersFilterViewModel undeliveredOrdersFilter)
		{
			lblCaption.Markup = "<u><b>Сводка по недовозам\nСписок ответственных:</b></u>";

			yTreeView.ItemsDataSource = guilties;

			lblTotalUdeliveredBottles.Markup = 
				$"Воды 19л: <b>{guilties.Sum(g => (decimal) g[3]):N0}</b> бут.";

			lblTotalUndeliveredOrders.Markup = 
				$"Заказов: <b>{guilties.Sum(o => (int) o[1])}</b> шт.";
		}

		#region Queries

		public IList<object[]> GetGuilties(UndeliveredOrdersFilterViewModel filter)
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
			Employee authorAlias = null;

			var subquery19LWatterQty = QueryOver.Of<OrderItem>(() => orderItemAlias)
				.Where(() => orderItemAlias.Order.Id == oldOrderAlias.Id)
				.Left.JoinQueryOver(i => i.Nomenclature, () => nomenclatureAlias)
				.Where(n => n.Category == NomenclatureCategory.water && n.TareVolume == TareVolume.Vol19L)
				.Select(Projections.Sum(() => orderItemAlias.Count));

			var query = _uow.Session.QueryOver<UndeliveredOrder>(() => undeliveredOrderAlias)
				.Left.JoinAlias(u => u.OldOrder, () => oldOrderAlias)
				.Left.JoinAlias(u => u.NewOrder, () => newOrderAlias)
				.Left.JoinAlias(() => oldOrderAlias.Client, () => counterpartyAlias)
				.Left.JoinAlias(() => oldOrderAlias.Author, () => oldOrderAuthorAlias)
				.Left.JoinAlias(() => oldOrderAlias.DeliveryPoint, () => undeliveredOrderDeliveryPointAlias)
				.Left.JoinAlias(() => undeliveredOrderAlias.GuiltyInUndelivery, () => guiltyInUndeliveryAlias)
				.Left.JoinAlias(() => guiltyInUndeliveryAlias.GuiltyDepartment, () => subdivisionAlias)
				.Left.JoinAlias(u => u.Author, () => authorAlias);

			if(filter?.RestrictDriver != null) {
				var oldOrderIds = _undeliveredOrdersRepository.GetListOfUndeliveryIdsForDriver(_uow, filter.RestrictDriver);
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

			if(filter?.OldOrderStatus != null)
				query.Where(() => undeliveredOrderAlias.OldOrderStatus == filter.OldOrderStatus);

			if(filter != null && filter.RestrictIsProblematicCases)
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


			if(filter?.RestrictAuthorSubdivision != null)
			{
				query.Where(() => authorAlias.Subdivision.Id == filter.RestrictAuthorSubdivision.Id);
			}

			int position = 0;
			var result = 
				query.SelectList(list => list
					.SelectGroup(u => u.Id)
					.Select(Projections.SqlFunction(
						new SQLFunctionTemplate(
							NHibernateUtil.String,
							"GROUP_CONCAT(" +
							"CASE ?1 " +
							$"WHEN '{nameof(GuiltyTypes.Department)}' THEN IFNULL(CONCAT('Отд: ', ?2), '{GuiltyTypes.Department.GetEnumTitle()}') " +
							$"WHEN '{nameof(GuiltyTypes.Client)}' THEN '{GuiltyTypes.Client.GetEnumTitle()}' " +
							$"WHEN '{nameof(GuiltyTypes.Driver)}' THEN '{GuiltyTypes.Driver.GetEnumTitle()}' " +
							$"WHEN '{nameof(GuiltyTypes.ServiceMan)}' THEN '{GuiltyTypes.ServiceMan.GetEnumTitle()}' " +
							$"WHEN '{nameof(GuiltyTypes.ForceMajor)}' THEN '{GuiltyTypes.ForceMajor.GetEnumTitle()}' " +
							$"WHEN '{nameof(GuiltyTypes.DirectorLO)}' THEN '{GuiltyTypes.DirectorLO.GetEnumTitle()}' " +
							$"WHEN '{nameof(GuiltyTypes.DirectorLOCurrentDayDelivery)}' THEN '{GuiltyTypes.DirectorLOCurrentDayDelivery.GetEnumTitle()}' " +
							$"WHEN '{nameof(GuiltyTypes.AutoСancelAutoTransfer)}' THEN '{GuiltyTypes.AutoСancelAutoTransfer.GetEnumTitle()}' " +
							$"WHEN '{nameof(GuiltyTypes.None)}' THEN '{GuiltyTypes.None.GetEnumTitle()}' " +
							"ELSE ?1 " +
							"END ORDER BY ?1 ASC SEPARATOR '\n')"
						 ),
						NHibernateUtil.String,
						Projections.Property(() => guiltyInUndeliveryAlias.GuiltySide),
						Projections.Property(() => subdivisionAlias.ShortName)))
					.SelectSubQuery(subquery19LWatterQty))
				.List<object[]>()
				.GroupBy(x => x[1])
				.Select(r => new[] { r.Key, r.Count(), position++, r.Sum(x => x[2] == null ? 0 : (decimal)x[2]) })
				.ToList();
			return result;
		}

		#endregion

		public override void Destroy()
		{
			_uow?.Dispose();
			base.Destroy();
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.RepresentationModel.GtkUI;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.JournalFilters;
using Vodovoz.Repositories.HumanResources;

namespace Vodovoz.Representations
{
	public class BottleDebtorsVM : QSOrmProject.RepresentationModel.RepresentationModelEntityBase<DeliveryPoint, BottleDebtorJournalNode>
	{
		public BottleDebtorsFilter Filter {
			get { return RepresentationFilter as BottleDebtorsFilter; }
			set { RepresentationFilter = value as QSOrmProject.RepresentationModel.IRepresentationFilter; }
		}

		public override void UpdateNodes()
		{
			DeliveryPoint deliveryPointAlias = null;
			Counterparty counterpartyAlias = null;
			BottlesMovementOperation bottleMovementOperationAlias = null;
			BottlesMovementOperation bottlesMovementAlias = null;
			BottleDebtorJournalNode resultAlias = null;
			Residue residueAlias = null;
			CallTask taskAlias = null;
			Domain.Orders.Order orderAlias = null;
			Domain.Orders.Order lastOrderAlias = null;
			OrderItem orderItemAlias = null;
			DiscountReason discountReasonAlias = null;
			Nomenclature nomenclatureAlias = null;

			var ordersQuery = UoW.Session.QueryOver(() => orderAlias);

			var bottleDebtByAddressQuery = QueryOver.Of(() => bottlesMovementAlias)
			.Where(() => bottlesMovementAlias.DeliveryPoint.Id == deliveryPointAlias.Id)
			.Select(
				Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Int32, "( ?2 - ?1 )"),
					NHibernateUtil.Int32, new IProjection[] {
					Projections.Sum(() => bottlesMovementAlias.Returned),
					Projections.Sum(() => bottlesMovementAlias.Delivered)}
				));

			var residueQuery = QueryOver.Of(() => residueAlias)
			.Where(() => residueAlias.DeliveryPoint.Id == deliveryPointAlias.Id)
			.Select(Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "IF(?1 IS NOT NULL,'есть', 'нет')"),
				NHibernateUtil.String,
				Projections.Property(() => residueAlias.Id)))
			.Take(1);

			var bottleDebtByClientQuery = QueryOver.Of(() => bottlesMovementAlias)
			.Where(() => bottlesMovementAlias.Counterparty.Id == counterpartyAlias.Id)
			.Select(
				Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Int32, "( ?2 - ?1 )"),
					NHibernateUtil.Int32, new IProjection[] {
								Projections.Sum(() => bottlesMovementAlias.Returned),
								Projections.Sum(() => bottlesMovementAlias.Delivered)}
				));


			var TaskExistQuery = QueryOver.Of(() => taskAlias)
				.Where(x => x.DeliveryPoint.Id == deliveryPointAlias.Id)
				.And(() => taskAlias.IsTaskComplete == false)
				.Select(Projections.SqlFunction(
					new SQLFunctionTemplate(NHibernateUtil.String, "IF(?1 IS NOT NULL,'grey', 'black')"),
					NHibernateUtil.String,
					Projections.Property(() => taskAlias.Id)))
				.Take(1);

			#region LastOrder

			var LastOrderIdQuery = QueryOver.Of(() => lastOrderAlias)
				.Where(() => lastOrderAlias.DeliveryPoint.Id == deliveryPointAlias.Id)
				.And((x) => x.OrderStatus == OrderStatus.Closed)
				.Select(Projections.Property<Domain.Orders.Order>(p => p.Id))
				.OrderByAlias(() => orderAlias.DeliveryDate).Desc
				.Take(1);

			var LastOrderNomenclatures = QueryOver.Of(() => orderItemAlias)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.Select(Projections.Property(() => nomenclatureAlias.Id))
				.Where(() => orderItemAlias.Order.Id == orderAlias.Id)
				.And(() => Filter.LastOrderNomenclature.Id == nomenclatureAlias.Id);

			var LastOrderDiscount = QueryOver.Of(() => orderItemAlias)
				.JoinAlias(() => orderItemAlias.DiscountReason, () => discountReasonAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.Select(Projections.Property(() => discountReasonAlias.Id))
				.Where(() => orderItemAlias.Order.Id == orderAlias.Id)
				.And(() => Filter.DiscountReason.Id == discountReasonAlias.Id);

			#endregion LastOrder

			#region Filter

			if(Filter.Client != null)
				ordersQuery = ordersQuery.Where((arg) => arg.Client.Id == Filter.Client.Id);
			if(Filter.Address != null)
				ordersQuery = ordersQuery.Where((arg) => arg.Id == Filter.Address.Id);
			if(Filter.OPF != null)
				ordersQuery = ordersQuery.Where( () => counterpartyAlias.PersonType == Filter.OPF.Value);
			if(Filter.LastOrderNomenclature != null)
				ordersQuery = ordersQuery.WithSubquery.WhereExists(LastOrderNomenclatures);
			if(Filter.DiscountReason != null)
				ordersQuery = ordersQuery.WithSubquery.WhereExists(LastOrderDiscount);
			if(Filter.LastOrderBottlesFrom != null)
				ordersQuery = ordersQuery.Where(() => bottleMovementOperationAlias.Delivered >= Filter.LastOrderBottlesFrom.Value);
			if(Filter.LastOrderBottlesTo != null)
				ordersQuery = ordersQuery.Where(() => bottleMovementOperationAlias.Delivered <= Filter.LastOrderBottlesTo.Value);
			if(Filter.StartDate != null)
				ordersQuery = ordersQuery.Where(() => orderAlias.DeliveryDate >= Filter.StartDate);
			if(Filter.StartDate != null)
				ordersQuery = ordersQuery.Where(() => orderAlias.DeliveryDate >= Filter.EndDate);
			if(Filter.DebtBottlesFrom != null)
				ordersQuery = ordersQuery.WithSubquery.WhereValue(Filter.DebtBottlesFrom.Value).Le(bottleDebtByAddressQuery);
			if(Filter.DebtBottlesTo != null)
				ordersQuery = ordersQuery.WithSubquery.WhereValue(Filter.DebtBottlesTo.Value).Gt(bottleDebtByAddressQuery);


			#endregion Filter

			var debtorslist = ordersQuery
				.JoinAlias(c => c.DeliveryPoint, () => deliveryPointAlias, NHibernate.SqlCommand.JoinType.RightOuterJoin)
				.JoinAlias(c => c.Client, () => counterpartyAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(c => c.BottlesMovementOperation, () => bottleMovementOperationAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.SelectList(list => list
				   .Select(() => deliveryPointAlias.Id).WithAlias(() => resultAlias.AddressId)
				   .Select(() => counterpartyAlias.Id).WithAlias(() => resultAlias.ClientId)
				   .Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.ClientName)
				   .Select(() => deliveryPointAlias.ShortAddress).WithAlias(() => resultAlias.AddressName)
				   .Select(() => deliveryPointAlias.BottleReserv).WithAlias(() => resultAlias.Reserve)
				   .Select(() => counterpartyAlias.PersonType).WithAlias(() => resultAlias.OPF)
				   .Select(() => bottleMovementOperationAlias.Delivered).WithAlias(() => resultAlias.LastOrderBottles)
				   .Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.LastOrderDate)
				   .SelectSubQuery(residueQuery).WithAlias(() => resultAlias.IsResidueExist)
				   .SelectSubQuery(bottleDebtByAddressQuery).WithAlias(() => resultAlias.DebtByAddress)
				   .SelectSubQuery(bottleDebtByClientQuery).WithAlias(() => resultAlias.DebtByClient)
				   .SelectSubQuery(TaskExistQuery).WithAlias(() => resultAlias.RowColor)
					 )
				.WithSubquery.WhereProperty(p => p.Id).Eq(LastOrderIdQuery)
				.TransformUsing(Transformers.AliasToBean<BottleDebtorJournalNode>())
				.List<BottleDebtorJournalNode>();

			SetItemsSource(debtorslist);
		}

		readonly IColumnsConfig columnsConfig = FluentColumnsConfig<BottleDebtorJournalNode>.Create()
			.AddColumn("Номер").AddNumericRenderer(x => x.AddressId)
			.AddColumn("Клиент").AddTextRenderer(node => node.ClientName)
			.AddColumn("Адрес").AddTextRenderer(node => node.AddressName)
			.AddColumn("ОПФ").AddTextRenderer(node => node.OPF.GetEnumTitle())
			.AddColumn("Последний заказ по адресу").AddTextRenderer(node => node.LastOrderDate != null ? node.LastOrderDate.Value.ToString("dd / MM / yyyy") : String.Empty)
			.AddColumn("Кол-во отгруженных в последнюю реализацию бутылей").AddNumericRenderer(node => node.LastOrderBottles)
			.AddColumn("Долг по таре (по адресу)").AddNumericRenderer(node => node.DebtByAddress)
			.AddColumn("Долг по таре (по клиенту)").AddNumericRenderer(node => node.DebtByClient)
			.AddColumn("Ввод остат.").AddTextRenderer(node => node.IsResidueExist)
			.AddColumn("Резерв").AddNumericRenderer(node => node.Reserve)
			.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = n.RowColor)
			.Finish();

		public override IColumnsConfig ColumnsConfig {
			get { return columnsConfig; }
		}

		protected override bool NeedUpdateFunc(DeliveryPoint updatedSubject) => true;

		public BottleDebtorsVM()
		{
			this.UoW = UnitOfWorkFactory.CreateWithoutRoot();
		}

		public BottleDebtorsVM(IUnitOfWork uow)
		{
			this.UoW = uow;
		}

		public BottleDebtorsVM(BottleDebtorsFilter filter) : this(filter.UoW)
		{
			Filter = filter;
		}

		public override IEnumerable<IJournalPopupItem> PopupItems {
			get {
				var result = new List<IJournalPopupItem>();

				result.Add(JournalPopupItemFactory.CreateNewAlwaysSensitiveAndVisible("Акт по бутылям и залогам(по клиенту)",
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<BottleDebtorJournalNode>();
						var selectedNode = selectedNodes.FirstOrDefault();
						if(selectedNode != null) {
							OpenReport(selectedNode.ClientId);
						}
					}
				));

				result.Add(JournalPopupItemFactory.CreateNewAlwaysSensitiveAndVisible("Акт по бутылям и залогам(по точке доставки)",
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<BottleDebtorJournalNode>();
						var selectedNode = selectedNodes.FirstOrDefault();
						if(selectedNode != null) {
							OpenReport(selectedNode.ClientId, selectedNode.AddressId);
						}
					}
				));

				result.Add(JournalPopupItemFactory.CreateNewAlwaysSensitiveAndVisible("Создать задачу",
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<BottleDebtorJournalNode>();
						var selectedNode = selectedNodes.FirstOrDefault();
						if(selectedNode != null) {
							CreateTask(selectedNodes.ToArray());
						}
					}
				));

				return result;
			}
		}

		public void OpenReport(int counterpartyId, int deliveryPointId = -1)
		{
			var reportInfo = new QS.Report.ReportInfo {
				Title = "Акт по бутылям-залогам",
				Identifier = "Client.SummaryBottlesAndDeposits",
				Parameters = new Dictionary<string, object>
				{
					{ "startDate", null },
					{ "endDate", null },
					{ "client_id", counterpartyId},
					{ "delivery_point_id", deliveryPointId}
				}
			};

			MainClass.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName(reportInfo),
				() => new QSReport.ReportViewDlg(reportInfo));
		}

		public int CreateTask(BottleDebtorJournalNode[] bottleDebtors)
		{
			int newTaskCount = 0;
			foreach(var item in bottleDebtors) {
				if(item == null)
					continue;
				CallTask task = new CallTask {
					TaskCreator = EmployeeRepository.GetEmployeeForCurrentUser(UoW),
					DeliveryPoint = UoW.GetById<DeliveryPoint>(item.AddressId),
					Counterparty = UoW.GetById<Counterparty>(item.ClientId),
					CreationDate = DateTime.Now,
					EndActivePeriod = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59)
				};
				newTaskCount++;
				UoW.Save(task);
			}
			UoW.Commit();
			return newTaskCount;
		}
	}

	public class BottleDebtorJournalNode
	{
		[UseForSearch]
		[SearchHighlight]
		public int AddressId { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string AddressName { get; set; }

		[UseForSearch]
		public int ClientId { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string ClientName { get; set; }

		public PersonType OPF { get; set; }

		public int DebtByAddress { get; set; }

		public int DebtByClient { get; set; }

		public int Reserve { get; set; }

		public string RowColor { get; set; } = "black";

		public DateTime? LastOrderDate { get; set; }

		public int? LastOrderBottles { get; set; }

		public string IsResidueExist { get; set; } = "нет";
	}
}
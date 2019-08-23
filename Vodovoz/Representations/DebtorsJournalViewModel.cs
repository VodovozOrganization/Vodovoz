using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.Config;
using QS.Project.Journal;
using QS.RepresentationModel.GtkUI;
using QS.Services;
using QSReport;
using Vodovoz.Dialogs;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.Repositories.HumanResources;

namespace Vodovoz.Representations
{
	public class DebtorsJournalViewModel : FilterableSingleEntityJournalViewModelBase<Domain.Orders.Order, CallTaskDlg, DebtorJournalNode, DebtorsJournalFilterViewModel>
	{

		public DebtorsJournalViewModel(DebtorsJournalFilterViewModel filterViewModel, IEntityConfigurationProvider entityConfigurationProvider, ICommonServices commonServices) : base(filterViewModel, entityConfigurationProvider, commonServices)
		{
			TabName = "Журнал задолженности";
			SelectionMode = JournalSelectionMode.Multiple;
		}

		protected override Func<IQueryOver<Domain.Orders.Order>> ItemsSourceQueryFunction => () => {
			DeliveryPoint deliveryPointAlias = null;
			Counterparty counterpartyAlias = null;
			BottlesMovementOperation bottleMovementOperationAlias = null;
			BottlesMovementOperation bottlesMovementAlias = null;
			DebtorJournalNode resultAlias = null;
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
				.And(() => FilterViewModel.LastOrderNomenclature.Id == nomenclatureAlias.Id);

			var LastOrderDiscount = QueryOver.Of(() => orderItemAlias)
				.JoinAlias(() => orderItemAlias.DiscountReason, () => discountReasonAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.Select(Projections.Property(() => discountReasonAlias.Id))
				.Where(() => orderItemAlias.Order.Id == orderAlias.Id)
				.And(() => FilterViewModel.DiscountReason.Id == discountReasonAlias.Id);

			#endregion LastOrder

			#region Filter

			if(FilterViewModel != null) 
			{
				if(FilterViewModel.Client != null)
					ordersQuery = ordersQuery.Where((arg) => arg.Client.Id == FilterViewModel.Client.Id);
				if(FilterViewModel.Address != null)
					ordersQuery = ordersQuery.Where((arg) => arg.Id == FilterViewModel.Address.Id);
				if(FilterViewModel.OPF != null)
					ordersQuery = ordersQuery.Where(() => counterpartyAlias.PersonType == FilterViewModel.OPF.Value);
				if(FilterViewModel.LastOrderNomenclature != null)
					ordersQuery = ordersQuery.WithSubquery.WhereExists(LastOrderNomenclatures);
				if(FilterViewModel.DiscountReason != null)
					ordersQuery = ordersQuery.WithSubquery.WhereExists(LastOrderDiscount);
				if(FilterViewModel.LastOrderBottlesFrom != null)
					ordersQuery = ordersQuery.Where(() => bottleMovementOperationAlias.Delivered >= FilterViewModel.LastOrderBottlesFrom.Value);
				if(FilterViewModel.LastOrderBottlesTo != null)
					ordersQuery = ordersQuery.Where(() => bottleMovementOperationAlias.Delivered <= FilterViewModel.LastOrderBottlesTo.Value);
				if(FilterViewModel.StartDate != null)
					ordersQuery = ordersQuery.Where(() => orderAlias.DeliveryDate >= FilterViewModel.StartDate.Value);
				if(FilterViewModel.EndDate != null)
					ordersQuery = ordersQuery.Where(() => orderAlias.DeliveryDate <= FilterViewModel.EndDate.Value);
				if(FilterViewModel.DebtBottlesFrom != null)
					ordersQuery = ordersQuery.WithSubquery.WhereValue(FilterViewModel.DebtBottlesFrom.Value).Le(bottleDebtByAddressQuery);
				if(FilterViewModel.DebtBottlesTo != null)
					ordersQuery = ordersQuery.WithSubquery.WhereValue(FilterViewModel.DebtBottlesTo.Value).Gt(bottleDebtByAddressQuery);
			}

			#endregion Filter

			ordersQuery.Where(GetSearchCriterion(
					() => deliveryPointAlias.Id,
					() => deliveryPointAlias.CompiledAddress,
					() => counterpartyAlias.Id,
					() => counterpartyAlias.Name
			));


			var resultQuery = ordersQuery
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
				.TransformUsing(Transformers.AliasToBean<DebtorJournalNode>());

			return resultQuery;
		};

		protected override void CreatePopupActions()
		{
			PopupActionsList.Add(new JournalAction("Акт по бутылям и залогам(по клиенту)", x => true, x => true, selectedItems => {
				var selectedNodes = selectedItems.Cast<DebtorJournalNode>();
				var selectedNode = selectedNodes.FirstOrDefault();
				if(selectedNode != null) {
					OpenReport(selectedNode.ClientId);
				}
			}));

			PopupActionsList.Add(new JournalAction("Акт по бутылям и залогам(по точке доставки)", x => true, x => true, selectedItems => {
				var selectedNodes = selectedItems.Cast<DebtorJournalNode>();
				var selectedNode = selectedNodes.FirstOrDefault();
				if(selectedNode != null) {
					OpenReport(selectedNode.ClientId, selectedNode.AddressId);
				}
			}));


			PopupActionsList.Add(new JournalAction("Создать задачу", x => true, x => true, selectedItems => {
				var selectedNodes = selectedItems.Cast<DebtorJournalNode>();
				var selectedNode = selectedNodes.FirstOrDefault();
				if(selectedNode != null)
					CreateTask(selectedNodes.ToArray());
			}
			));

		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();

			NodeActionsList.Add(new JournalAction("Создать задачи", x => true, x => true, selectedItems => CreateTask(selectedItems.AsEnumerable().OfType<DebtorJournalNode>().ToArray())));

			NodeActionsList.Add(new JournalAction("Акт по бутылям и залогам", x => true, x => true, selectedItems => {
				var selectedNodes = selectedItems.Cast<DebtorJournalNode>();
				var selectedNode = selectedNodes.FirstOrDefault();
				if(selectedNode != null) {
					OpenReport(selectedNode.ClientId, selectedNode.AddressId);
				}
			}));
		}

		protected override Func<CallTaskDlg> CreateDialogFunction => () => new CallTaskDlg();

		protected override Func<DebtorJournalNode, CallTaskDlg> OpenDialogFunction => (node) =>
		{
			return new CallTaskDlg(node.ClientId, node.AddressId);
		};

		public void OpenReport(int counterpartyId, int deliveryPointId = -1)
		{
			var dlg = CreateReportDlg(counterpartyId, deliveryPointId);
			TabParent.AddTab(dlg, this);
		}

		private ReportViewDlg CreateReportDlg(int counterpartyId, int deliveryPointId = -1) 
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
			return new ReportViewDlg(reportInfo);
		}

		public int CreateTask(DebtorJournalNode[] bottleDebtors)
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

	public class DebtorJournalNode : JournalEntityNodeBase<Domain.Orders.Order>
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
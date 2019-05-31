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
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Operations;
using Vodovoz.JournalFilters;
using Vodovoz.Repositories.HumanResources;

namespace Vodovoz.Representations
{
	public class BottleDebtorsVM : QSOrmProject.RepresentationModel.RepresentationModelEntityBase<DeliveryPoint, BottleDebtorsVMNode>
	{
		public BottleDebtorsFilter Filter {
			get { return RepresentationFilter as BottleDebtorsFilter; }
			set { RepresentationFilter = value as QSOrmProject.RepresentationModel.IRepresentationFilter; }
		}

		public override void UpdateNodes()
		{
			DeliveryPoint deliveryPointAlias = null;
			DeliveryPoint deliveryPointOrderAlias = null;
			Counterparty counterpartyAlias = null;
			BottlesMovementOperation bottlesMovementAlias = null;
			BottleDebtorsVMNode resultAlias = null;
			Residue residueAlias = null;
			CallTask taskAlias = null;
			Domain.Orders.Order orderAlias = null;

			var pointsQuery = UoW.Session.QueryOver(() => deliveryPointAlias)
			.Where(() => deliveryPointAlias.IsActive == true);

			var bottleDebtByAddressQuery = UoW.Session.QueryOver(() => bottlesMovementAlias)
			.Where(() => bottlesMovementAlias.DeliveryPoint.Id == deliveryPointAlias.Id)
			.Select(
				Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Int32, "( ?2 - ?1 )"),
					NHibernateUtil.Int32, new IProjection[] {
					Projections.Sum(() => bottlesMovementAlias.Returned),
					Projections.Sum(() => bottlesMovementAlias.Delivered)}
				));

			var residueQuery = UoW.Session.QueryOver(() => residueAlias)
			.Where(() => residueAlias.DeliveryPoint.Id == deliveryPointAlias.Id)
			.Select((res) => res.Id)
			.Take(1);

			var bottleDebtByClientQuery = UoW.Session.QueryOver(() => bottlesMovementAlias)
			.Where(() => bottlesMovementAlias.Counterparty.Id == counterpartyAlias.Id)
			.Select(
				Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Int32, "( ?2 - ?1 )"),
					NHibernateUtil.Int32, new IProjection[] {
								Projections.Sum(() => bottlesMovementAlias.Returned),
								Projections.Sum(() => bottlesMovementAlias.Delivered)}
				));

			var LastOrderQuery = UoW.Session.QueryOver(() => orderAlias)
				.JoinAlias(c => c.DeliveryPoint, () => deliveryPointOrderAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.Where((x) => x.DeliveryPoint.Id == deliveryPointAlias.Id)
				.Select((x) => x.DeliveryDate)
				.OrderBy(() => orderAlias.Id).Desc
				.Take(1);

			var TaskExistQuery = UoW.Session.QueryOver(() => taskAlias)
				.Where(x => x.DeliveryPoint.Id == deliveryPointAlias.Id)
				.And(() => taskAlias.IsTaskComplete == false)
				.Select(x => x.Id)
				.Take(1);

			if(Filter.Client != null)
				pointsQuery = pointsQuery.Where((arg) => arg.Counterparty.Id == Filter.Client.Id);
			if(Filter.Address != null)
				pointsQuery = pointsQuery.Where((arg) => arg.Id == Filter.Address.Id);
			if(Filter.OPF != null)
				pointsQuery = pointsQuery.Where( () => counterpartyAlias.PersonType == Filter.OPF.Value);

			var debtorslist = pointsQuery
				.JoinAlias(c => c.Counterparty, () => counterpartyAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.SelectList(list => list
				   .Select(() => deliveryPointAlias.Id).WithAlias(() => resultAlias.AddressId)
				   .Select(() => counterpartyAlias.Id).WithAlias(() => resultAlias.ClientId)
				   .Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.ClientName)
				   .Select(() => deliveryPointAlias.ShortAddress).WithAlias(() => resultAlias.AddressName)
				   .Select(() => deliveryPointAlias.BottleReserv).WithAlias(() => resultAlias.Reserve)
				   .Select(() => counterpartyAlias.PersonType).WithAlias(() => resultAlias.OPF)
				   .SelectSubQuery((QueryOver<Domain.Orders.Order>)LastOrderQuery).WithAlias(() => resultAlias.LastOrderDate)
				   .SelectSubQuery((QueryOver<Residue>)residueQuery).WithAlias(() => resultAlias.Residue)
				   .SelectSubQuery((QueryOver<BottlesMovementOperation>)bottleDebtByAddressQuery).WithAlias(() => resultAlias.DebtByAddress)
				   .SelectSubQuery((QueryOver<BottlesMovementOperation>)bottleDebtByClientQuery).WithAlias(() => resultAlias.DebtByClient)
				   .SelectSubQuery((QueryOver<CallTask>)TaskExistQuery).WithAlias(() => resultAlias.ExistTask)
					 )
				.TransformUsing(Transformers.AliasToBean<BottleDebtorsVMNode>())
				.OrderBy(x => x.Counterparty.Id).Desc
				.List<BottleDebtorsVMNode>();


			if(Filter.StartDate != null && Filter.EndDate != null)
				debtorslist = debtorslist.Where((arg) => Filter.StartDate.Value <= arg.LastOrderDate && arg.LastOrderDate <= Filter.EndDate.Value).ToList();
			if(Filter.DebtFrom != null && Filter.DebtBy != null)
				debtorslist = debtorslist.Where((arg) => Filter.DebtFrom.Value <= arg.DebtByAddress && arg.DebtByAddress <= Filter.DebtBy.Value).ToList();

			SetItemsSource(debtorslist);
		}

		readonly IColumnsConfig columnsConfig = FluentColumnsConfig<BottleDebtorsVMNode>.Create()
			.AddColumn("Номер").AddTextRenderer(x => x.AddressId.ToString())
			.AddColumn("Клиент").AddTextRenderer(node => node.ClientName)
			.AddColumn("Адрес").AddTextRenderer(node => node.AddressName)
			.AddColumn("ОПФ").AddTextRenderer(node => node.OPF.GetEnumTitle())
			.AddColumn("Последний заказ по адресу").AddTextRenderer(node => node.LastOrderDate != null ? node.LastOrderDate.Value.ToString("dd / MM / yyyy") : String.Empty)
			.AddColumn("Долг по таре (по адресу)").AddTextRenderer(node => node.DebtByAddress.ToString())
			.AddColumn("Долг по таре (по клиенту)").AddTextRenderer(node => node.DebtByClient.ToString())
			.AddColumn("Ввод остат.").AddTextRenderer(node => node.IsResidue ? "есть" : "нет")
			.AddColumn("Резерв").AddTextRenderer(node => node.Reserve.ToString())
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
						var selectedNodes = selectedItems.Cast<BottleDebtorsVMNode>();
						var selectedNode = selectedNodes.FirstOrDefault();
						if(selectedNode != null) {
							OpenReport(selectedNode.ClientId);
						}
					}
				));

				result.Add(JournalPopupItemFactory.CreateNewAlwaysSensitiveAndVisible("Акт по бутылям и залогам(по точке доставки)",
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<BottleDebtorsVMNode>();
						var selectedNode = selectedNodes.FirstOrDefault();
						if(selectedNode != null) {
							OpenReport(selectedNode.AddressId);
						}
					}
				));

				result.Add(JournalPopupItemFactory.CreateNewAlwaysSensitiveAndVisible("Создать задачу",
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<BottleDebtorsVMNode>();
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

		public void CreateTask(BottleDebtorsVMNode[] bottleDebtors)
		{
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
				UoW.Save(task);
			}
			UoW.Commit();
		}
	}

	public class BottleDebtorsVMNode
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

		public DateTime? LastOrderDate { get; set; }

		public int DebtByAddress { get; set; }

		public int DebtByClient { get; set; }

		public int Reserve { get; set; }

		public string RowColor { get { return IsTaskExist ? "grey" : "black"; } }

		public bool IsResidue { get { return Residue != null; } }

		public int? Residue { get; set; }

		public bool IsTaskExist { get { return ExistTask != null; } }

		public int? ExistTask { get; set; }
	}
}
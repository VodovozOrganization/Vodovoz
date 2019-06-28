using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.Config;
using QS.Project.Journal;
using QS.Services;
using QSProjectsLib;
using Vodovoz.Dialogs.Cash;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalNodes;
using VodovozOrder = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Representations
{
	public class SelfDeliveriesJournalViewModel : SingleEntityJournalViewModelBase<VodovozOrder, OrderDlg, SelfDeliveryJournalNode>
	{
		private OrderJournalFilterViewModel filterViewModel;
		public OrderJournalFilterViewModel FilterViewModel {
			get { return filterViewModel; }
			set {
				filterViewModel = value;
				Filter = filterViewModel;
			}
		}

		public SelfDeliveriesJournalViewModel(OrderJournalFilterViewModel filterViewModel, IEntityConfigurationProvider entityConfigurationProvider, ICommonServices commonServices) : base(entityConfigurationProvider, commonServices)
		{
			TabName = "Журнал самовывозов";
			SetOrder<SelfDeliveryJournalNode>(x => x.Date, true);

			RegisterAliasPropertiesToSearch(
				() => counterpartyAlias.Name,
				() => authorAlias.Name,
				() => authorAlias.LastName,
				() => authorAlias.Patronymic,
				() => orderAlias.Id
			);
			FilterViewModel = filterViewModel;
		}

		SelfDeliveryJournalNode resultAlias = null;
		VodovozOrder orderAlias = null;
		Nomenclature nomenclatureAlias = null;
		OrderItem orderItemAlias = null;
		OrderDepositItem orderDepositItemAlias = null;
		Income incomeAlias = null;
		Expense expenseAlias = null;
		Counterparty counterpartyAlias = null;
		DeliveryPoint deliveryPointAlias = null;
		Employee authorAlias = null;

		IQueryOver<VodovozOrder, VodovozOrder> BaseQuery(Func<IQueryOver<VodovozOrder, VodovozOrder>, IQueryOver<VodovozOrder, VodovozOrder>> func)
		{
			var query = UoW.Session.QueryOver<VodovozOrder>(() => orderAlias)
								   .Where(() => orderAlias.SelfDelivery)
								   .Where(() => !orderAlias.IsService);

			if(FilterViewModel.RestrictStatus != null)
				query.Where(o => o.OrderStatus == FilterViewModel.RestrictStatus);
			else if(FilterViewModel.AllowStatuses != null && FilterViewModel.AllowStatuses.Any())
				query.WhereRestrictionOn(o => o.OrderStatus).IsIn(FilterViewModel.AllowStatuses);

			if(FilterViewModel.RestrictPaymentType != null)
				query.Where(o => o.PaymentType == FilterViewModel.RestrictPaymentType);
			else if(FilterViewModel.AllowPaymentTypes != null && FilterViewModel.AllowPaymentTypes.Any())
				query.WhereRestrictionOn(o => o.PaymentType).IsIn(FilterViewModel.AllowPaymentTypes);

			if(FilterViewModel.RestrictCounterparty != null)
				query.Where(o => o.Client == FilterViewModel.RestrictCounterparty);

			if(FilterViewModel.RestrictDeliveryPoint != null)
				query.Where(o => o.DeliveryPoint == FilterViewModel.RestrictDeliveryPoint);

			if(FilterViewModel.RestrictStartDate != null)
				query.Where(o => o.DeliveryDate >= FilterViewModel.RestrictStartDate);

			if(FilterViewModel.RestrictEndDate != null)
				query.Where(o => o.DeliveryDate <= FilterViewModel.RestrictEndDate.Value.AddDays(1).AddTicks(-1));

			var result = query
				.Left.JoinAlias(o => o.DeliveryPoint, () => deliveryPointAlias)
				.Left.JoinAlias(o => o.Client, () => counterpartyAlias)
				.Left.JoinAlias(o => o.Author, () => authorAlias)
				.Left.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias)
				.Left.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => orderAlias.OrderDepositItems, () => orderDepositItemAlias)
				.JoinEntityAlias(() => incomeAlias, () => orderAlias.Id == incomeAlias.Order.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => expenseAlias, () => orderAlias.Id == expenseAlias.Order.Id, JoinType.LeftOuterJoin);

			if(func == null)
				return result;
			return func(result);
		}

		IQueryOver<VodovozOrder, VodovozOrder> SelectNodes(IQueryOver<VodovozOrder, VodovozOrder> query)
		{
			var result = query.SelectList(list => list
										   .SelectGroup(() => orderAlias.Id).WithAlias(() => resultAlias.Id)
										   .Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.Date)
										   .Select(() => orderAlias.OrderStatus).WithAlias(() => resultAlias.StatusEnum)
										   .Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorLastName)
										   .Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
										   .Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
										   .Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.Counterparty)
										   .Select(() => orderAlias.PayAfterShipment).WithAlias(() => resultAlias.PayAfterLoad)
										   .Select(() => orderAlias.PaymentType).WithAlias(() => resultAlias.PaymentTypeEnum)
										   .Select(Projections.Sum(
												   Projections.Conditional(
													   Restrictions.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water && nomenclatureAlias.TareVolume == TareVolume.Vol19L),
													   Projections.Property(() => orderItemAlias.Count),
													Projections.Constant(0, NHibernateUtil.Int32)
												)
											)).WithAlias(() => resultAlias.BottleAmount)
										   .Select(Projections.Sum(
												Projections.SqlFunction(
													new SQLFunctionTemplate(NHibernateUtil.Decimal, "IFNULL(?2, ?1) * ?3 - ?4"),
													NHibernateUtil.Decimal,
													Projections.Property(() => orderItemAlias.Count),
													Projections.Property(() => orderItemAlias.ActualCount),
													Projections.Property(() => orderItemAlias.Price),
													Projections.Property(() => orderItemAlias.DiscountMoney)
												   )
											)).WithAlias(() => resultAlias.OrderSum)
										   .Select(Projections.Sum(
												Projections.SqlFunction(
													new SQLFunctionTemplate(NHibernateUtil.Decimal, "IFNULL(?2, ?1) * ?3"),
													NHibernateUtil.Decimal,
													Projections.Property(() => orderDepositItemAlias.Count),
													Projections.Property(() => orderItemAlias.ActualCount),
													Projections.Property(() => orderDepositItemAlias.Deposit)
												   )
											)).WithAlias(() => resultAlias.OrderReturnSum)
										   .Select(Projections.Property(() => incomeAlias.Money)).WithAlias(() => resultAlias.CashPaid)
										   .Select(Projections.Property(() => expenseAlias.Money)).WithAlias(() => resultAlias.CashReturn)
										)
										.OrderBy(x => x.DeliveryDate).Desc.ThenBy(x => x.Id).Desc
										.TransformUsing(Transformers.AliasToBean<SelfDeliveryJournalNode>())
										;

			return result;
		}

		protected override Func<IQueryOver<VodovozOrder>> ItemsSourceQueryFunction => () => {
			var result = BaseQuery(SelectNodes);
			return result;
		};

		public override IEnumerable<IJournalAction> NodeActions => new List<IJournalAction>();

		protected override Func<OrderDlg> CreateDialogFunction => () => throw new ApplicationException();

		protected override Func<SelfDeliveryJournalNode, OrderDlg> OpenDialogFunction => node => new OrderDlg(node.Id);

		public override string FooterInfo {
			get {
				var lst = BaseQuery(SelectNodes).List<SelfDeliveryJournalNode>();
				StringBuilder sb = new StringBuilder();
				sb.Append("Сумма БН: <b>").Append(lst.Sum(n => n.OrderCashlessSumTotal).ToShortCurrencyString()).Append("</b>\t|\t");
				sb.Append("Сумма нал: <b>").Append(lst.Sum(n => n.OrderCashSumTotal).ToShortCurrencyString()).Append("</b>\t|\t");
				sb.Append("Из них возврат: <b>").Append(lst.Sum(n => n.OrderReturnSum).ToShortCurrencyString()).Append("</b>\t|\t");
				sb.Append("Приход: <b>").Append(lst.Sum(n => n.CashPaid).ToShortCurrencyString()).Append("</b>\t|\t");
				sb.Append("Возврат: <b>").Append(lst.Sum(n => n.CashReturn).ToShortCurrencyString()).Append("</b>\t|\t");
				sb.Append("Итог: <b>").Append(lst.Sum(n => n.CashTotal).ToShortCurrencyString()).Append("</b>\t|\t");
				var difference = lst.Sum(n => n.TotalCashDiff);
				if(difference == 0)
					sb.Append("Расх.нал: <b>").Append(difference.ToShortCurrencyString()).Append("</b>\t\t");
				else
					sb.Append("Расх.нал: <span foreground=\"Red\"><b>").Append(difference.ToShortCurrencyString()).Append("</b></span>\t\t");
				sb.Append("<span foreground=\"Grey\"><b>").Append(base.FooterInfo).Append("</b></span>");

				return sb.ToString();
			}
		}

		protected override void CreatePopupActions()
		{
			PopupActionsList.Add(
				new JournalAction(
					"Открыть заказ",
					selectedItems => {
						var selectedNodes = selectedItems.Cast<SelfDeliveryJournalNode>();
						return selectedNodes.Count() == 1;
					},
					selectedItems => true,
					selectedItems => {
						var selectedNodes = selectedItems.Cast<SelfDeliveryJournalNode>();
						var selectedNode = selectedNodes.FirstOrDefault();
						MainClass.MainWin.TdiMain.OpenTab(
							DialogHelper.GenerateDialogHashName<VodovozOrder>(selectedNode.Id),
							() => new OrderDlg(selectedNode.Id)
						);
					}
				)
			);
			PopupActionsList.Add(
				new JournalAction(
					"Создать кассовые ордера",
					selectedItems => true,
					selectedItems => {
						var selectedNodes = selectedItems.Cast<SelfDeliveryJournalNode>();
						return selectedNodes.Count() == 1 && selectedNodes.First().StatusEnum == OrderStatus.WaitForPayment;
					},
					selectedItems => {
						var selectedNodes = selectedItems.Cast<SelfDeliveryJournalNode>();
						var selectedNode = selectedNodes.FirstOrDefault();
						if(selectedNode != null)
							CreateSelfDeliveryCashInvoices(selectedNode.Id);
					}
				)
			);
		}

		//FIXME отделить от GTK
		void CreateSelfDeliveryCashInvoices(int orderId)
		{
			var order = UoW.GetById<VodovozOrder>(orderId);

			if(order.SelfDeliveryIsFullyPaid()) {
				MessageDialogHelper.RunInfoDialog("Заказ уже оплачен полностью");
				return;
			}

			if(order.OrderSum > 0 && !order.SelfDeliveryIsFullyIncomePaid()) {
				MainClass.MainWin.TdiMain.OpenTab(
					"selfDelivery_" + DialogHelper.GenerateDialogHashName<Income>(orderId),
					() => new CashIncomeSelfDeliveryDlg(order)
				);
			}

			if(order.OrderSumReturn > 0 && !order.SelfDeliveryIsFullyExpenseReturned()) {
				MainClass.MainWin.TdiMain.OpenTab(
					"selfDelivery_" + DialogHelper.GenerateDialogHashName<Expense>(orderId),
					() => new CashExpenseSelfDeliveryDlg(order)
				);
			}
		}
	}
}
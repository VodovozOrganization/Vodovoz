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
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QSProjectsLib;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.Extensions;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Infrastructure;
using Vodovoz.JournalNodes;
using Vodovoz.Services;
using Vodovoz.Tools.CallTasks;
using Vodovoz.ViewModels.Cash;
using VodovozOrder = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Representations
{
	public class SelfDeliveriesJournalViewModel : FilterableSingleEntityJournalViewModelBase<VodovozOrder, OrderDlg, SelfDeliveryJournalNode, OrderJournalFilterViewModel>
	{
		private readonly ICallTaskWorker _callTaskWorker;
		private readonly Employee _currentEmployee;
		private readonly IOrderPaymentSettings _orderPaymentSettings;
		private readonly IOrderParametersProvider _orderParametersProvider;
		private readonly IDeliveryRulesParametersProvider _deliveryRulesParametersProvider;
		private readonly bool _userCanChangePayTypeToByCard;

		public SelfDeliveriesJournalViewModel(
			OrderJournalFilterViewModel filterViewModel, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			ICommonServices commonServices, 
			ICallTaskWorker callTaskWorker,
			IOrderPaymentSettings orderPaymentSettings,
			IOrderParametersProvider orderParametersProvider,
			IDeliveryRulesParametersProvider deliveryRulesParametersProvider,
			IEmployeeService employeeService,
			INavigationManager navigationManager,
			Action<OrderJournalFilterViewModel> filterConfig = null) 
			: base(filterViewModel, unitOfWorkFactory, commonServices, navigation: navigationManager)
		{
			_callTaskWorker = callTaskWorker ?? throw new ArgumentNullException(nameof(callTaskWorker));
			_orderPaymentSettings = orderPaymentSettings ?? throw new ArgumentNullException(nameof(orderPaymentSettings));
			_orderParametersProvider = orderParametersProvider ?? throw new ArgumentNullException(nameof(orderParametersProvider));
			_deliveryRulesParametersProvider = deliveryRulesParametersProvider ?? throw new ArgumentNullException(nameof(deliveryRulesParametersProvider));
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_currentEmployee =
				(employeeService ?? throw new ArgumentNullException(nameof(employeeService))).GetEmployeeForUser(
					UoW,
					commonServices.UserService.CurrentUserId);

			TabName = "Журнал самовывозов";

			filterViewModel.Journal = this;

			if(filterConfig != null)
			{
				filterViewModel.ConfigureWithoutFiltering(filterConfig);
			}

			SetOrder(x => x.Date, true);
			
			UpdateOnChanges(
				typeof(VodovozOrder),
				typeof(OrderItem));

			_userCanChangePayTypeToByCard = commonServices.CurrentPermissionService.ValidatePresetPermission("allow_load_selfdelivery");
		}

		protected override Func<IUnitOfWork, IQueryOver<VodovozOrder>> ItemsSourceQueryFunction => (uow) => {
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
			CounterpartyContract contractAlias = null;

			var depositReturnQuery = QueryOver.Of(() => orderDepositItemAlias)
				.Select(Projections.Sum(
						Projections.SqlFunction(
							new SQLFunctionTemplate(NHibernateUtil.Decimal, "IFNULL(?2, ?1) * ?3"),
							NHibernateUtil.Decimal,
							Projections.Property(() => orderDepositItemAlias.Count),
							Projections.Property(() => orderDepositItemAlias.ActualCount),
							Projections.Property(() => orderDepositItemAlias.Deposit)
						   )
					))
				.Where(() => orderDepositItemAlias.Order.Id == orderAlias.Id);

			var query = uow.Session.QueryOver<VodovozOrder>(() => orderAlias)
								   .Where(() => orderAlias.SelfDelivery)
								   .Where(() => orderAlias.OrderAddressType != OrderAddressType.Service);

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

			if(FilterViewModel.DeliveryPoint != null)
				query.Where(o => o.DeliveryPoint == FilterViewModel.DeliveryPoint);

			if(FilterViewModel.StartDate != null)
				query.Where(o => o.DeliveryDate >= FilterViewModel.StartDate);

			if(FilterViewModel.EndDate != null)
				query.Where(o => o.DeliveryDate <= FilterViewModel.EndDate.Value.AddDays(1).AddTicks(-1));

			if(FilterViewModel.PaymentOrder != null) {
				bool paymentAfterShipment = false || FilterViewModel.PaymentOrder == PaymentOrder.AfterShipment;
				query.Where(o => o.PayAfterShipment == paymentAfterShipment);
			}
			
			if (FilterViewModel.Organisation != null) {
				query.Where(() => contractAlias.Organization.Id == FilterViewModel.Organisation.Id);
			}
			
			if (FilterViewModel.PaymentByCardFrom != null) {
				query.Where(o => o.PaymentByCardFrom.Id == FilterViewModel.PaymentByCardFrom.Id);
			}

			query
				.Left.JoinAlias(o => o.DeliveryPoint, () => deliveryPointAlias)
				.Left.JoinAlias(o => o.Client, () => counterpartyAlias)
				.Left.JoinAlias(o => o.Author, () => authorAlias)
				.Left.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias)
				.Left.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => orderAlias.OrderDepositItems, () => orderDepositItemAlias)
				.Left.JoinAlias(o => o.Contract, () => contractAlias)
				.JoinEntityAlias(() => incomeAlias, () => orderAlias.Id == incomeAlias.Order.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => expenseAlias, () => orderAlias.Id == expenseAlias.Order.Id, JoinType.LeftOuterJoin);

			query.Where(GetSearchCriterion(
				() => counterpartyAlias.Name,
				() => authorAlias.Name,
				() => authorAlias.LastName,
				() => authorAlias.Patronymic,
				() => orderAlias.Id
			));

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
							Projections.Constant(0, NHibernateUtil.Decimal)
						)
					)).WithAlias(() => resultAlias.BottleAmount)
					.Select(Projections.Sum(
						Projections.SqlFunction(
							new SQLFunctionTemplate(NHibernateUtil.Decimal, "ROUND(IFNULL(?2, ?1) * ?3 - ?4, 2)"),
							NHibernateUtil.Decimal,
							Projections.Property(() => orderItemAlias.Count),
							Projections.Property(() => orderItemAlias.ActualCount),
							Projections.Property(() => orderItemAlias.Price),
							Projections.Property(() => orderItemAlias.DiscountMoney)
						   )
					)).WithAlias(() => resultAlias.OrderSum)
					.Select(Projections.Property(() => incomeAlias.Money)).WithAlias(() => resultAlias.CashPaid)
					.Select(Projections.Property(() => expenseAlias.Money)).WithAlias(() => resultAlias.CashReturn)
					.SelectSubQuery(depositReturnQuery).WithAlias(() => resultAlias.OrderReturnSum)
				)
				.OrderBy(x => x.DeliveryDate).Desc.ThenBy(x => x.Id).Desc
				.TransformUsing(Transformers.AliasToBean<SelfDeliveryJournalNode>());

			return result;
		};

		public override IEnumerable<IJournalAction> NodeActions => new List<IJournalAction>();	
		
		//Действие при дабл клике
		protected override Func<OrderDlg> CreateDialogFunction => () => throw new ApplicationException();

		//FIXME отделить от GTK
		protected override Func<SelfDeliveryJournalNode, OrderDlg> OpenDialogFunction => node => new OrderDlg(node.Id);

		public override string FooterInfo {
			get {
				StringBuilder sb = new StringBuilder();
				using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
					var lst = ItemsSourceQueryFunction(uow).List<SelfDeliveryJournalNode>();
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
						sb.Append($"Расх.нал: <span foreground=\"{GdkColors.DangerText.ToHtmlColor()}\"><b>").Append(difference.ToShortCurrencyString()).Append("</b></span>\t\t");
					sb.Append($"<span foreground=\"{GdkColors.InsensitiveText.ToHtmlColor()}\"><b>").Append(base.FooterInfo).Append("</b></span>");
				}
				return sb.ToString();
			}
		}

		public INavigationManager NavigationManager { get; }

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
						Startup.MainWin.TdiMain.OpenTab(
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
			PopupActionsList.Add(
				new JournalAction(
					"Оплата по карте",
					selectedItems => {
						var selectedNodes = selectedItems.Cast<SelfDeliveryJournalNode>().ToList();
						var selectedNode = selectedNodes.FirstOrDefault();
						return selectedNodes.Count == 1 && (selectedNode.PaymentTypeEnum == PaymentType.Cash || (selectedNode.PaymentTypeEnum == PaymentType.Terminal && selectedNode.OrderCashSumTotal != 0)) && selectedNode.StatusEnum != OrderStatus.Closed;
					},
					selectedItems => _userCanChangePayTypeToByCard,
					selectedItems => {
						var selectedNodes = selectedItems.Cast<SelfDeliveryJournalNode>();
						var selectedNode  = selectedNodes.FirstOrDefault();
						if (selectedNode != null)
							TabParent.AddTab(
								new PaymentByCardViewModel(
									EntityUoWBuilder.ForOpen(selectedNode.Id),
									UnitOfWorkFactory,
									commonServices,
									_callTaskWorker,
									_orderPaymentSettings,
									_orderParametersProvider,
									_deliveryRulesParametersProvider,
									_currentEmployee), 
								this
							);
					}
					
				)
			);
			PopupActionsList.Add(
				new JournalAction(
					"Онлайн оплата",
					selectedItems => {
						var selectedNodes = selectedItems.Cast<SelfDeliveryJournalNode>().ToList();
						var selectedNode = selectedNodes.FirstOrDefault();
						return selectedNodes.Count == 1 && (selectedNode.PaymentTypeEnum == PaymentType.Cash || (selectedNode.PaymentTypeEnum == PaymentType.Terminal && selectedNode.OrderCashSumTotal != 0)) && selectedNode.StatusEnum != OrderStatus.Closed;
					},
					selectedItems => _userCanChangePayTypeToByCard,
					selectedItems => {
						var selectedNodes = selectedItems.Cast<SelfDeliveryJournalNode>();
						var selectedNode = selectedNodes.FirstOrDefault();
						if(selectedNode != null)
							TabParent.AddTab(
								new PaymentOnlineViewModel(
									EntityUoWBuilder.ForOpen(selectedNode.Id),
									UnitOfWorkFactory,
									commonServices,
									_callTaskWorker,
									_orderPaymentSettings,
									_orderParametersProvider,
									_deliveryRulesParametersProvider,
									_currentEmployee),
								this
							);
					}

				)
			);

		}
		

		//FIXME отделить от GTK
		void CreateSelfDeliveryCashInvoices(int orderId)
		{
			var order = UoW.GetById<VodovozOrder>(orderId);

			if(order.SelfDeliveryIsFullyPaid(new CashRepository())) {
				MessageDialogHelper.RunInfoDialog("Заказ уже оплачен полностью");
				return;
			}

			if(order.OrderPositiveSum > 0 && !order.SelfDeliveryIsFullyIncomePaid()) {
				var page = NavigationManager.OpenViewModel<IncomeSelfDeliveryViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate());
				page.ViewModel.SetOrderById(orderId);
			}

			if(order.OrderNegativeSum > 0 && !order.SelfDeliveryIsFullyExpenseReturned()) {
				var page = NavigationManager.OpenViewModel<ExpenseSelfDeliveryViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate());
				page.ViewModel.SetOrderById(orderId);
			}
		}
	}
}

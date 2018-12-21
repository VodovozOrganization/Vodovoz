using System;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using QSProjectsLib;
using Vodovoz.Dialogs.Cash;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Repository;
using VodovozOrder = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Representations
{
	public class SelfDeliveriesVM : RepresentationModelEntityBase<VodovozOrder, UnclosedSelfDeliveriesVMNode>
	{
		public OrdersFilter Filter {
			get { return RepresentationFilter as OrdersFilter; }
			set { RepresentationFilter = value as IRepresentationFilter; }
		}

		public SelfDeliveriesVM(OrdersFilter filter) : this(filter.UoW)
		{
			Filter = filter;
		}

		public SelfDeliveriesVM() : this(UnitOfWorkFactory.CreateWithoutRoot())
		{
			CreateRepresentationFilter = () => new OrdersFilter(UoW);
		}

		public SelfDeliveriesVM(IUnitOfWork uow) : base()
		{
			this.UoW = uow;
		}

		public override bool PopupMenuExist => true;

		IColumnsConfig columnsConfig = FluentColumnsConfig<UnclosedSelfDeliveriesVMNode>.Create()
			.AddColumn("Номер").SetDataProperty(node => node.Id.ToString())
			.AddColumn("Дата").SetDataProperty(node => node.Date.ToString("d"))
			.AddColumn("Автор").SetDataProperty(node => node.Author)
			.AddColumn("Статус").SetDataProperty(node => node.StatusEnum.GetEnumTitle())
			.AddColumn("Бутыли").AddTextRenderer(node => node.BottleAmount.ToString())
			.AddColumn("Клиент").SetDataProperty(node => node.Counterparty)
			.AddColumn("Вариант оплаты").SetDataProperty(node => node.PayOption)
			.AddColumn("Сумма товаров").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.OrderSum))
			.AddColumn("Сумма возврат").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.OrderReturnSum))
			.AddColumn("Сумма итог").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.OrderSumTotal))
			.AddColumn("Нал приход").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.CashPaid))
			.AddColumn("Нал возврат").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.CashReturn))
			.AddColumn("Нал итог").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.CashTotal))
			.AddColumn("Итого разница").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.TotalDiff))
			.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = n.RowColor)
			.Finish();

		public override IColumnsConfig ColumnsConfig => columnsConfig;

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc(VodovozOrder updatedSubject) => true;

		public override void UpdateNodes()
		{
			UnclosedSelfDeliveriesVMNode resultAlias = null;
			VodovozOrder orderAlias = null;
			Nomenclature nomenclatureAlias = null;
			OrderItem orderItemAlias = null;
			OrderDepositItem orderDepositItemAlias = null;
			Income incomeAlias = null;
			Expense expenseAlias = null;
			Counterparty counterpartyAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			Employee authorAlias = null;

			var query = UoW.Session.QueryOver<VodovozOrder>(() => orderAlias)
				.Where(() => orderAlias.SelfDelivery)
				.Where(() => !orderAlias.IsService);

			if(Filter.RestrictStatus != null) {
				query.Where(o => o.OrderStatus == Filter.RestrictStatus);
			} else if(Filter.AllowStatuses.Any()) {
				query.WhereRestrictionOn(o => o.OrderStatus).IsIn(Filter.AllowStatuses);
			}

			if(Filter.RestrictPaymentType != null) {
				query.Where(o => o.PaymentType == Filter.RestrictPaymentType);
			} else if(Filter.AllowPaymentTypes.Any()) {
				query.WhereRestrictionOn(o => o.PaymentType).IsIn(Filter.AllowPaymentTypes);
			}

			if(Filter.RestrictCounterparty != null) {
				query.Where(o => o.Client == Filter.RestrictCounterparty);
			}

			if(Filter.RestrictDeliveryPoint != null) {
				query.Where(o => o.DeliveryPoint == Filter.RestrictDeliveryPoint);
			}

			var bottleCountSubquery = QueryOver.Of<OrderItem>(() => orderItemAlias)
				.Where(() => orderAlias.Id == orderItemAlias.Order.Id)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water && nomenclatureAlias.TareVolume == TareVolume.Vol19L)
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
			var orderSumReturnSubquery = QueryOver.Of<OrderDepositItem>(() => orderDepositItemAlias)
											.Where(() => orderDepositItemAlias.Order.Id == orderAlias.Id)
											.Select(
												Projections.Sum(
													Projections.SqlFunction(
														new SQLFunctionTemplate(NHibernateUtil.Decimal, "?1 * ?2"),
														NHibernateUtil.Decimal,
														Projections.Property<OrderDepositItem>(x => x.Count),
														Projections.Property<OrderDepositItem>(x => x.Deposit)
													   )
												   )
											   );
			var incomeCashSumSubquery = QueryOver.Of<Income>(() => incomeAlias)
											.Where(() => incomeAlias.Order.Id == orderAlias.Id)
											.Select(Projections.Sum(Projections.Property<Income>(x => x.Money)));

			var expenseCashSumSubquery = QueryOver.Of<Expense>(() => expenseAlias)
											.Where(() => expenseAlias.Order.Id == orderAlias.Id)
											.Select(Projections.Sum(Projections.Property<Expense>(x => x.Money)));

			var result = query
				.JoinAlias(o => o.DeliveryPoint, () => deliveryPointAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(o => o.Client, () => counterpartyAlias)
				.JoinAlias(o => o.Author, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.SelectList(list => list
				   .Select(() => orderAlias.Id).WithAlias(() => resultAlias.Id)
				   .Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.Date)
				   .Select(() => orderAlias.OrderStatus).WithAlias(() => resultAlias.StatusEnum)
				   .Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorLastName)
				   .Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
				   .Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
				   .Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.Counterparty)
				   .Select(() => orderAlias.PayAfterShipment).WithAlias(() => resultAlias.PayAfterLoad)
				   .SelectSubQuery(orderSumSubquery).WithAlias(() => resultAlias.OrderSum)
				   .SelectSubQuery(orderSumReturnSubquery).WithAlias(() => resultAlias.CashReturn)
				   .SelectSubQuery(bottleCountSubquery).WithAlias(() => resultAlias.BottleAmount)
				   .SelectSubQuery(incomeCashSumSubquery).WithAlias(() => resultAlias.CashPaid)
				   .SelectSubQuery(expenseCashSumSubquery).WithAlias(() => resultAlias.CashReturn)
				).OrderBy(x => x.DeliveryDate).Desc.ThenBy(x => x.Id).Desc
				.TransformUsing(Transformers.AliasToBean<UnclosedSelfDeliveriesVMNode>())
				.List<UnclosedSelfDeliveriesVMNode>()
				//отображение закрытых заказов с расхождением в оплате
				//TODO необходимо разделение на расходники старого и нового типа  
				.Where(x => !(x.StatusEnum == OrderStatus.Closed && !x.HaveDiff))
				.ToList();

			SetItemsSource(result);
		}

		#endregion

		public override Menu GetPopupMenu(RepresentationSelectResult[] selected)
		{
			var selectedNode = selected.FirstOrDefault().VMNode as UnclosedSelfDeliveriesVMNode;
			var isOneSelected = selected.Count() == 1 && selectedNode != null;
			var selectedOrder = UoW.GetById<VodovozOrder>(selectedNode.Id);
			Menu popupMenu = new Menu();
			MenuItem menuItemOrder = new MenuItem("Открыть заказ");
			menuItemOrder.Activated += (sender, e) => {
				MainClass.MainWin.TdiMain.OpenTab(
					DialogHelper.GenerateDialogHashName<VodovozOrder>(selectedNode.Id),
					() => new OrderDlg(selectedNode.Id)
				);
			};
			menuItemOrder.Sensitive = isOneSelected;
			popupMenu.Add(menuItemOrder);

			MenuItem menuItemInvoices = new MenuItem("Создать кассовые ордера");
			menuItemInvoices.Activated += (sender, e) => {
				CreateSelfDeliveryCashInvoices(selectedNode.Id);
			};
			menuItemInvoices.Sensitive = isOneSelected && selectedOrder.OrderStatus == OrderStatus.WaitForPayment;
			popupMenu.Add(menuItemInvoices);

			MenuItem menuItemEditOrder = new MenuItem("Изменение товаров в заказе");
			menuItemEditOrder.Activated += (sender, e) => {
				MainClass.MainWin.TdiMain.OpenTab(
					"edit_selfdelivery_" + DialogHelper.GenerateDialogHashName<VodovozOrder>(selectedNode.Id),
					() => new SelfDeliveryOrderEditDlg(selectedNode.Id)
				);
			};
			//Закрыт до уточнения работы кассы по самовывозу
			menuItemEditOrder.Visible = isOneSelected && false;
			popupMenu.Add(menuItemEditOrder);

			return popupMenu;
		}

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

			if(order.OrderSumReturn < 0 && !order.SelfDeliveryIsFullyExpenseReturned()) {
				MainClass.MainWin.TdiMain.OpenTab(
					"selfDelivery_" + DialogHelper.GenerateDialogHashName<Expense>(orderId),
					() => new CashExpenseSelfDeliveryDlg(order)
				);
			}

		}
	}

	public class UnclosedSelfDeliveriesVMNode
	{
		[UseForSearch]
		[SearchHighlight]
		public int Id { get; set; }

		public OrderStatus StatusEnum { get; set; }

		public DateTime Date { get; set; }
		public int BottleAmount { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string Counterparty { get; set; }

		public bool PayAfterLoad { get; set; }
		public string PayOption => PayAfterLoad ? "После погрузки" : "До погрузки";

		//заказ
		public decimal OrderSum { get; set; }
		public decimal OrderReturnSum { get; set; }
		public decimal OrderSumTotal => OrderSum - OrderReturnSum;

		//наличные по кассе
		public decimal CashPaid { get; set; }
		public decimal CashReturn { get; set; }
		public decimal CashTotal => CashPaid - CashReturn;

		public decimal TotalDiff => OrderSumTotal - CashTotal;

		public bool HaveDiff {
			get {
				return OrderSumTotal != CashTotal;
			}
		}

		public string AuthorLastName { get; set; }
		public string AuthorName { get; set; }
		public string AuthorPatronymic { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string Author { get { return StringWorks.PersonNameWithInitials(AuthorLastName, AuthorName, AuthorPatronymic); } }

		public string RowColor {
			get {
				if(CashPaid > 0 && HaveDiff) {
					//light red
					return "#f97777";
				}
				if(StatusEnum == OrderStatus.Closed && HaveDiff) {
					//red
					return "#ee0000";
				}
				return "black";

			}
		}
	}
}

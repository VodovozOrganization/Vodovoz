using System;
using System.Linq;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using Gtk;
using QS.DomainModel.UoW;
using QS.Osm;
using QS.Osm.Loaders;
using QS.Project.Domain;
using QS.Project.Services;
using QS.Tdi;
using QSProjectsLib;
using Vodovoz.Dialogs.OrderWidgets;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.EntityFactories;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Factories;
using Vodovoz.Parameters;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Counterparty;
using Vodovoz.ViewWidgets.Mango;
using IDeliveryPointInfoProvider = Vodovoz.ViewModels.Infrastructure.InfoProviders.IDeliveryPointInfoProvider;

namespace Vodovoz.SidePanel.InfoViews
{
	public partial class DeliveryPointPanelView : Gtk.Bin, IPanelView
	{
		private readonly IDeliveryPointRepository _deliveryPointRepository = new DeliveryPointRepository();
		private readonly IBottlesRepository _bottlesRepository = new BottlesRepository();
		private readonly IDepositRepository _depositRepository = new DepositRepository();
		private readonly IOrderRepository _orderRepository = new OrderRepository();
		private readonly IDeliveryPointViewModelFactory _deliveryPointViewModelFactory = new DeliveryPointViewModelFactory();
		DeliveryPoint DeliveryPoint { get; set; }

		public DeliveryPointPanelView()
		{
			this.Build();
			Configure();
		}

		void Configure()
		{
			label5.Visible = labelDeposits.Visible = false;
			labelAddress.LineWrapMode = Pango.WrapMode.WordChar;
			labelLastOrders.LineWrapMode = Pango.WrapMode.WordChar;

			ytreeLastOrders.RowActivated += OnOrdersRowActivated;
			ytreeLastOrders.Selection.Changed += OnOrdersSelectionChanged;

			ytreeLastOrders.ColumnsConfig = ColumnsConfigFactory.Create<Order>()
				.AddColumn("Дата")
				.AddTextRenderer(node => node.DeliveryDate.HasValue ? node.DeliveryDate.Value.ToString("dd.MM.yy") : String.Empty)
				.AddColumn("Тип оплаты")
				.AddTextRenderer(node => GetDisplayShortName(node.PaymentType))
				.AddColumn("Бутылей")
				.AddNumericRenderer(node => node.Total19LBottlesToDeliver).Editing(false)
				.Finish();
		}

		#region IPanelView implementation
		public IInfoProvider InfoProvider { get; set; }

		public void Refresh()
		{
			DeliveryPoint = (InfoProvider as IDeliveryPointInfoProvider)?.DeliveryPoint;
			if(DeliveryPoint == null) {
				buttonSaveComment.Sensitive = false;
				return;
			}
			buttonSaveComment.Sensitive = true;
			labelAddress.Text = DeliveryPoint.CompiledAddress;

			foreach(var child in PhonesTable.Children) {
				PhonesTable.Remove(child);
				child.Destroy();
			}

			uint rowsCount = Convert.ToUInt32(DeliveryPoint.Phones.Count) + 1;
			PhonesTable.Resize(rowsCount, 2);
			for(uint row = 0; row < rowsCount - 1; row++) {
				Label label = new Label();
				label.Selectable = true;
				label.Markup = $"{DeliveryPoint.Phones[Convert.ToInt32(row)].LongText}";

				HandsetView handsetView = new HandsetView(DeliveryPoint.Phones[Convert.ToInt32(row)].DigitsNumber);

				PhonesTable.Attach(label, 0, 1, row, row + 1);
				PhonesTable.Attach(handsetView, 1, 2, row, row + 1);
			}

			Label labelAddPhone = new Label() { LabelProp = "Щёлкните чтоб\n добавить телефон-->" };
			PhonesTable.Attach(labelAddPhone, 0, 1, rowsCount - 1, rowsCount);

			Image addIcon = new Image();
			addIcon.Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-add", IconSize.Menu);
			Button btn = new Button();
			btn.Image = addIcon;
			btn.Clicked += OnBtnAddPhoneClicked;
			PhonesTable.Attach(btn, 1, 2, rowsCount - 1, rowsCount);
			PhonesTable.ShowAll();

			var bottlesAtDeliveryPoint = _bottlesRepository.GetBottlesAtDeliveryPoint(InfoProvider.UoW, DeliveryPoint);
			var bottlesAvgDeliveryPoint = _deliveryPointRepository.GetAvgBottlesOrdered(InfoProvider.UoW, DeliveryPoint, 5);
			lblBottlesQty.LabelProp = $"{bottlesAtDeliveryPoint} шт. (сред. зак.: {bottlesAvgDeliveryPoint:G3})";
			var bottlesAtCounterparty = _bottlesRepository.GetBottlesAtCounterparty(InfoProvider.UoW, DeliveryPoint.Counterparty);
			debtByClientLabel.LabelProp = $"{bottlesAtCounterparty} шт.";
			var depositsAtDeliveryPoint = _depositRepository.GetDepositsAtDeliveryPoint(InfoProvider.UoW, DeliveryPoint, null);
			labelDeposits.LabelProp = CurrencyWorks.GetShortCurrencyString(depositsAtDeliveryPoint);
			textviewComment.Buffer.Text = DeliveryPoint.Comment;

			var currentOrders = _orderRepository.GetLatestOrdersForDeliveryPoint(InfoProvider.UoW, DeliveryPoint, 5);
			ytreeLastOrders.SetItemsSource<Order>(currentOrders);
			vboxLastOrders.Visible = currentOrders.Any();

			table2.ShowAll();
		}

		public bool VisibleOnPanel {
			get {
				return DeliveryPoint != null;
			}
		}

		public void OnCurrentObjectChanged(object changedObject)
		{
			if(changedObject is DeliveryPoint deliveryPoint) {
				DeliveryPoint = deliveryPoint;
				Refresh();
			}
		}
		#endregion

		string GetDisplayShortName(Enum enumerator)
		{
			var attr = enumerator.GetAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>();
			return attr == null ? "" : attr.ShortName;
		}

		void OnOrdersRowActivated(object sender, RowActivatedArgs args)
		{
			var order = ytreeLastOrders.GetSelectedObject() as Order;
			if(InfoProvider is OrderDlg) {
				(InfoProvider as OrderDlg)?.FillOrderItems(order);
			}
		}

		void OnOrdersSelectionChanged(object sender, EventArgs args)
		{
			var order = ytreeLastOrders.GetSelectedObject() as Order;
			GenerateTooltip(order);
		}

		private void GenerateTooltip(Order order)
		{
			ytreeLastOrders.HasTooltip = false;

			if(order == null) {
				return;
			}
			string tooltip = "Заказ №" + order.Id + ":";

			foreach(OrderItem orderItem in order.OrderItems) {
				tooltip += "\n"
		           + orderItem.Nomenclature.Name + ": "
		           + orderItem.Count.ToString(orderItem.Nomenclature.Unit != null ? $"N{orderItem.Nomenclature.Unit.Digits}" : "G29");
			}

			ytreeLastOrders.TooltipText = tooltip;
			ytreeLastOrders.HasTooltip = true;
		}

		protected void OnButtonSaveCommentClicked(object sender, EventArgs e)
		{
			using(var uow = UnitOfWorkFactory.CreateForRoot<DeliveryPoint>(DeliveryPoint.Id, "Кнопка «Cохранить комментарий» на панели точки доставки")) {
				uow.Root.Comment = textviewComment.Buffer.Text;
				uow.Save();
			}
		}

		protected void OnBtnAddPhoneClicked(object sender, EventArgs e)
		{
			var dpViewModel = _deliveryPointViewModelFactory.GetForOpenDeliveryPointViewModel(DeliveryPoint.Id);
			TDIMain.MainNotebook.OpenTab(() => dpViewModel);
		}
	}
}

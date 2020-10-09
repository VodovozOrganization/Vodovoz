using System;
using System.Linq;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using Gtk;
using NHibernate.Util;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QS.Tdi;
using QS.Tdi.Gtk;
using QSProjectsLib;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Repositories.Orders;
using Vodovoz.Repository.Client;
using Vodovoz.Repository.Operations;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.ViewWidgets.Mango;

namespace Vodovoz.SidePanel.InfoViews
{
	public partial class DeliveryPointPanelView : Gtk.Bin, IPanelView
	{
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
				.AddNumericRenderer(node => node.TotalWaterBottles).Editing(false)
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

			if(DeliveryPoint.Phones.Count > 0) {
				uint rowsCount = Convert.ToUInt32(DeliveryPoint.Phones.Count) + 1;
				PhonesTable.Resize(rowsCount, 2);
				for(uint row = 0; row < rowsCount - 1; row++) {
					Label label = new Label();
					label.Markup = $"<b>+{DeliveryPoint.Phones[Convert.ToInt32(row)].Number}</b>";

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
			}

			PhonesTable.ShowAll();

			var bottlesAtDeliveryPoint = BottlesRepository.GetBottlesAtDeliveryPoint(InfoProvider.UoW, DeliveryPoint);
			var bottlesAvgDeliveryPoint = DeliveryPointRepository.GetAvgBottlesOrdered(InfoProvider.UoW, DeliveryPoint, 5);
			lblBottlesQty.Text = String.Format("{0} шт. (сред. зак.: {1:G3})", bottlesAtDeliveryPoint, bottlesAvgDeliveryPoint);
			var bottlesAtCounterparty = BottlesRepository.GetBottlesAtCounterparty(InfoProvider.UoW, DeliveryPoint.Counterparty);
			debtByClientLabel.Text = String.Format("{0} шт.", bottlesAtCounterparty);
			var depositsAtDeliveryPoint = DepositRepository.GetDepositsAtDeliveryPoint(InfoProvider.UoW, DeliveryPoint, null);
			labelDeposits.Text = CurrencyWorks.GetShortCurrencyString(depositsAtDeliveryPoint);
			textviewComment.Buffer.Text = DeliveryPoint.Comment;

			var currentOrders = OrderRepository.GetLatestOrdersForDeliveryPoint(InfoProvider.UoW, DeliveryPoint, 5);
			ytreeLastOrders.SetItemsSource<Order>(currentOrders);
			vboxLastOrders.Visible = currentOrders.Any();
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
				tooltip += "\n" + orderItem.Nomenclature.Name + ": " + orderItem.Count;
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
			TDIMain.MainNotebook.OpenTab(
				DialogHelper.GenerateDialogHashName<DeliveryPoint>(DeliveryPoint.Id),
				() => new DeliveryPointDlg(DeliveryPoint.Id)
			);
		}
	}
}


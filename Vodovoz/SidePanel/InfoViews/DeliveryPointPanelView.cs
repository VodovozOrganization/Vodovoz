using System;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using QSOrmProject;
using QSProjectsLib;
using QSTDI;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Repository;
using Vodovoz.Repository.Client;
using Vodovoz.Repository.Operations;
using Vodovoz.SidePanel.InfoProviders;
using NHibernate.Util;

namespace Vodovoz.SidePanel.InfoViews
{
	[System.ComponentModel.ToolboxItem(true)]
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
			labelPhone.LabelProp = String.Join(";\n", DeliveryPoint.Phones.Select(ph => ph.LongText));
			if(DeliveryPoint.Phones.Count <= 0)
				labelPhone.Text = "[+] чтоб добавить -->";

			var bottlesAtDeliveryPoint = BottlesRepository.GetBottlesAtDeliveryPoint(InfoProvider.UoW, DeliveryPoint);
			var bottlesAvgDeliveryPoint = DeliveryPointRepository.GetAvgBottlesOrdered(InfoProvider.UoW, DeliveryPoint, 5);
			labelBottles.Text = String.Format("{0} шт. (сред. зак.: {1:G3})", bottlesAtDeliveryPoint, bottlesAvgDeliveryPoint);
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
			var deliveryPoint = changedObject as DeliveryPoint;
			if(deliveryPoint != null) {
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
			using(var uow = UnitOfWorkFactory.CreateForRoot<DeliveryPoint>(DeliveryPoint.Id)) {
				uow.Root.Comment = textviewComment.Buffer.Text;
				uow.Save();
			}
		}

		protected void OnBtnAddPhoneClicked(object sender, EventArgs e)
		{
			TDIMain.MainNotebook.OpenTab(
				OrmMain.GenerateDialogHashName<DeliveryPoint>(DeliveryPoint.Id),
				() => new DeliveryPointDlg(DeliveryPoint.Id)
			);
		}
	}
}


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using QS.Dialog.Gtk;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Tdi;
using QSOrmProject;
using QSProjectsLib;
using QSValidation;
using Vodovoz.Dialogs;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.JournalFilters;
using Vodovoz.Repository;

namespace Vodovoz
{
	public partial class OrderReturnsView : QS.Dialog.Gtk.TdiTabBase, ITDICloseControlTab
	{
		class OrderNode : PropertyChangedBase
		{
			public enum ChangedType
			{
				None,
				DeliveryPoint,
				Both
			}

			Counterparty client;
			public Counterparty Client {
				get {
					return client;
				}
				set {
					SetField(ref client, value, () => Client);
				}
			}

			DeliveryPoint deliveryPoint;
			public DeliveryPoint DeliveryPoint {
				get {
					return deliveryPoint;
				}
				set {
					SetField(ref deliveryPoint, value, () => DeliveryPoint);
				}
			}

			private Order BaseOrder { get; set; }

			public OrderNode(Order order)
			{
				DeliveryPoint = order.DeliveryPoint;
				Client = order.Client;
				BaseOrder = order;
			}

			public ChangedType CompletedChange {
				get {
					if(Client == null || DeliveryPoint == null) {
						return ChangedType.None;
					}
					if(Client.Id == BaseOrder.Client.Id && DeliveryPoint.Id != BaseOrder.DeliveryPoint.Id) {
						return ChangedType.DeliveryPoint;
					}
					if(Client.Id != BaseOrder.Client.Id && DeliveryPoint.Id != BaseOrder.DeliveryPoint.Id) {
						return ChangedType.Both;
					}
					return ChangedType.None;
				}
			}
		}
		List<OrderItemReturnsNode> equipmentFromClient;
		List<OrderItemReturnsNode> itemsToClient;

		IUnitOfWork uow;

		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {
				uow = value;
				depositrefunditemsview1.Configure(uow, routeListItem.Order, true);
			}
		}
		OrderNode orderNode;
		RouteListItem routeListItem;

		public event PropertyChangedEventHandler PropertyChanged;

		public OrderReturnsView(RouteListItem routeListItem, IUnitOfWork uow)
		{
			this.Build();
			this.routeListItem = routeListItem;
			this.TabName = "Изменение заказа №" + routeListItem.Order.Id;

			UoW = uow;

			UpdateListsSentivity();
			entryTotal.Sensitive = yenumcomboOrderPayment.Sensitive =
				routeListItem.Status != RouteListItemStatus.Transfered;


			orderEquipmentItemsView.OnDeleteEquipment += OrderEquipmentItemsView_OnDeleteEquipment;
			Configure();
			UpdateItemsList();
			UpdateButtonsState();
		}

		private void UpdateListsSentivity()
		{
			ytreeToClient.Sensitive = 
			orderEquipmentItemsView.Sensitive =
			depositrefunditemsview1.Sensitive = routeListItem.IsDelivered();
		}

		private void UpdateItemsList()
		{
			itemsToClient = new List<OrderItemReturnsNode>();
			var nomenclatures = routeListItem.Order.OrderItems
				.Where(item => !item.Nomenclature.IsSerial).ToList();
			foreach(var item in nomenclatures) {
				itemsToClient.Add(new OrderItemReturnsNode(item));
				item.PropertyChanged += OnOrderChanged;
			}
			entryTotal.Text = CurrencyWorks.GetShortCurrencyString(routeListItem.Order.ActualGoodsTotalSum);

			ytreeToClient.ItemsDataSource = itemsToClient;
		}

		private void OpenSelectNomenclatureDlg()
		{
			var nomenclatureFilter = new NomenclatureRepFilter(UoW);
			nomenclatureFilter.SetAndRefilterAtOnce(
				x => x.AvailableCategories = Nomenclature.GetCategoriesForEditOrderFromRL(),
				x => x.DefaultSelectedCategory = NomenclatureCategory.deposit,
				x => x.DefaultSelectedSubCategory = SubtypeOfEquipmentCategory.forSale
			);
			ReferenceRepresentation SelectDialog = new ReferenceRepresentation(new ViewModel.NomenclatureForSaleVM(nomenclatureFilter));
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.TabName = "Номенклатура на продажу";
			SelectDialog.ObjectSelected += NomenclatureSelected;
			SelectDialog.ShowFilter = true;
			TabParent.AddSlaveTab(this, SelectDialog);
		}

		private void NomenclatureSelected(object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			Nomenclature nomenclature = UoW.Session.Get<Nomenclature>(e.ObjectId);
			CounterpartyContract contract = routeListItem.Order.Contract;
			WaterSalesAgreement wsa = null;
			if(routeListItem.Order.IsLoadedFrom1C || nomenclature == null || contract == null) {
				return;
			}

			if(routeListItem.Order.OrderItems.Any(x => !Nomenclature.GetCategoriesForMaster().Contains(x.Nomenclature.Category))
			   && nomenclature.Category == NomenclatureCategory.master) {
				MessageDialogWorks.RunInfoDialog("В не сервисный заказ нельзя добавить сервисную услугу");
				return;
			}

			if(routeListItem.Order.OrderItems.Any(x => x.Nomenclature.Category == NomenclatureCategory.master)
			   && !Nomenclature.GetCategoriesForMaster().Contains(nomenclature.Category)) {
				MessageDialogWorks.RunInfoDialog("В сервисный заказ нельзя добавить не сервисную услугу");
				return;
			}
			switch(nomenclature.Category) {
				case NomenclatureCategory.water:
				case NomenclatureCategory.disposableBottleWater:
					wsa = contract.GetWaterSalesAgreement(routeListItem.Order.DeliveryPoint);
					if(wsa == null) {
						MessageDialogWorks.RunErrorDialog("Невозможно добавить воду, потому что нет дополнительного соглашения о продаже воды");
					}
					routeListItem.Order.AddWaterForSale(nomenclature, wsa, 0);
					break;
				case NomenclatureCategory.master:
					routeListItem.Order.AddMasterNomenclature(nomenclature, 0);
					break;
				default:
					routeListItem.Order.AddAnyGoodsNomenclatureForSale(nomenclature, true);
					break;
			}
			UpdateItemsList();
		}

		public void OnOrderChanged(object sender, PropertyChangedEventArgs args)
		{
			entryTotal.Text = CurrencyWorks.GetShortCurrencyString(routeListItem.Order.ActualGoodsTotalSum);
		}

		protected void Configure()
		{
			orderNode = new OrderNode(routeListItem.Order);
			var counterpartyFilter = new CounterpartyFilter(UoW);
			counterpartyFilter.SetAndRefilterAtOnce(x => x.RestrictIncludeArhive = false);
			referenceClient.RepresentationModel = new ViewModel.CounterpartyVM(counterpartyFilter);
			referenceClient.Binding.AddBinding(orderNode, s => s.Client, w => w.Subject).InitializeFromSource();
			referenceClient.CanEditReference = false;
			orderEquipmentItemsView.Configure(UoW, routeListItem.Order);
			ConfigureDeliveryPointRefference(orderNode.Client);

			ytreeToClient.ColumnsConfig = ColumnsConfigFactory.Create<OrderItemReturnsNode>()
				.AddColumn("Название")
					.AddTextRenderer(node => node.Name)
				.AddColumn("Кол-во")
					.AddNumericRenderer(node => node.Count)
						.AddSetter((c, node) => c.Digits = node.Nomenclature.Unit == null ? 0 : (uint)node.Nomenclature.Unit.Digits)
					.AddTextRenderer(node => node.Nomenclature.Unit == null ? String.Empty : node.Nomenclature.Unit.Name, false)
				.AddColumn("Кол-во по факту")
					.AddToggleRenderer(node => node.IsDelivered, false)
						.AddSetter((cell, node) => cell.Visible = node.IsSerialEquipment)
					.AddNumericRenderer(node => node.ActualCount, false)
				.AddSetter((cell, node) => {
					if(node.Nomenclature.Category == NomenclatureCategory.rent
					   || node.Nomenclature.Category == NomenclatureCategory.deposit) {
						cell.Editable = false;
					} else {
						cell.Editable = true;
					}
				})
						.Adjustment(new Gtk.Adjustment(0, 0, 9999, 1, 1, 0))
						.AddSetter((cell, node) => cell.Adjustment = new Gtk.Adjustment(0, 0, GetMaxCount(node), 1, 1, 0))
						.AddSetter((cell, node) => cell.Editable = !node.IsEquipment)
					.AddTextRenderer(node => node.Nomenclature.Unit == null ? String.Empty : node.Nomenclature.Unit.Name, false)
				.AddColumn("Цена")
					.AddNumericRenderer(node => node.Price).Digits(2).WidthChars(10)
						.Adjustment(new Gtk.Adjustment(0, 0, 99999, 1, 100, 0))
						.AddSetter((cell, node) => cell.Editable = node.HasPrice)
					.AddTextRenderer(node => CurrencyWorks.CurrencyShortName, false)
				.AddColumn("Скидка")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.DiscountForPrewiev).Editing(true)
					.AddSetter(
						(c, n) => c.Adjustment = n.IsDiscountInMoney
									? new Adjustment(0, 0, (double)n.Price * n.ActualCount, 1, 100, 1)
									: new Adjustment(0, 0, 100, 1, 100, 1)
					)
					.Digits(2)
					.WidthChars(10)
					.AddTextRenderer(n => n.IsDiscountInMoney ? CurrencyWorks.CurrencyShortName : "%", false)
				.AddColumn("Скидка \nв рублях?").AddToggleRenderer(x => x.IsDiscountInMoney)
					.Editing()
				.AddColumn("Основание скидки")
					.HeaderAlignment(0.5f)
					.AddComboRenderer(node => node.DiscountReason)
					.SetDisplayFunc(x => x.Name)
					.FillItems(OrderRepository.GetDiscountReasons(UoW))
				.AddSetter((c, n) => c.Editable = n.Discount > 0)
				.AddSetter(
					(c, n) => c.BackgroundGdk = n.Discount > 0 && n.DiscountReason == null
					? new Gdk.Color(0xff, 0x66, 0x66)
					: new Gdk.Color(0xff, 0xff, 0xff)
				)
				.AddColumn("Стоимость")
					.AddNumericRenderer(node => node.Sum).Digits(2)
					.AddTextRenderer(node => CurrencyWorks.CurrencyShortName)
				.AddColumn("")
				.Finish();

			yenumcomboOrderPayment.ItemsEnum = typeof(PaymentType);
			yenumcomboOrderPayment.Binding.AddBinding(routeListItem.Order, o => o.PaymentType, w => w.SelectedItem).InitializeFromSource();

			entryOnlineOrder.ValidationMode = QSWidgetLib.ValidationType.numeric;
			entryOnlineOrder.Binding.AddBinding(routeListItem.Order, e => e.OnlineOrder, w => w.Text, new IntToStringConverter()).InitializeFromSource();
			OnlineOrderVisible();
		}

		int GetMaxCount(OrderItemReturnsNode node){
			var count = (node.Nomenclature.Category == NomenclatureCategory.service
			             || node.Nomenclature.Category == NomenclatureCategory.master
			             || node.Nomenclature.Category == NomenclatureCategory.deposit) ? 1 : 9999;
			return count;
		}

		private void ConfigureDeliveryPointRefference(Counterparty client = null)
		{
			var deliveryPointFilter = new DeliveryPointFilter(UoW);
			deliveryPointFilter.Client = client;
			referenceDeliveryPoint.RepresentationModel = new ViewModel.DeliveryPointsVM(deliveryPointFilter);
			referenceDeliveryPoint.Binding.AddBinding(orderNode, s => s.DeliveryPoint, w => w.Subject).InitializeFromSource();
			referenceDeliveryPoint.CanEditReference = false;
		}

		protected void OnButtonNotDeliveredClicked(object sender, EventArgs e)
		{
			UndeliveryOnOrderCloseDlg dlg = new UndeliveryOnOrderCloseDlg(routeListItem.Order, UoW);
			TabParent.AddSlaveTab(this, dlg);
			dlg.DlgSaved += (s, ea) => {
				routeListItem.UpdateStatus(UoW, RouteListItemStatus.Overdue);
				routeListItem.FillCountsOnCanceled();
				UpdateButtonsState();
				this.OnCloseTab(false);
			};
		}

		protected void OnButtonDeliveryCanseledClicked(object sender, EventArgs e)
		{
			UndeliveryOnOrderCloseDlg dlg = new UndeliveryOnOrderCloseDlg(routeListItem.Order, UoW);
			TabParent.AddSlaveTab(this, dlg);
			dlg.DlgSaved += (s, ea) => {
				routeListItem.UpdateStatus(UoW, RouteListItemStatus.Canceled);
				routeListItem.FillCountsOnCanceled();
				UpdateButtonsState();
				this.OnCloseTab(false);
			};
		}

		protected void OnButtonDeliveredClicked(object sender, EventArgs e)
		{
			routeListItem.UpdateStatus(UoW, RouteListItemStatus.Completed);
			routeListItem.FirstFillClosing(UoW);
			UpdateListsSentivity();
			UpdateButtonsState();
		}

		void UpdateButtonsState()
		{
			bool isTransfered = routeListItem.Status == RouteListItemStatus.Transfered;
			buttonDeliveryCanceled.Sensitive = !isTransfered && routeListItem.Status != RouteListItemStatus.Canceled;
			buttonNotDelivered.Sensitive = !isTransfered && routeListItem.Status != RouteListItemStatus.Overdue;
			buttonDelivered.Sensitive = !isTransfered && routeListItem.Status != RouteListItemStatus.Completed && routeListItem.Status != RouteListItemStatus.EnRoute;
		}

		protected void OnYenumcomboOrderPaymentChangedByUser(object sender, EventArgs e)
		{
			routeListItem.Order.ChangeOrderContract();
			routeListItem.RecalculateTotalCash();
		}

		private void AcceptOrderChange()
		{
			if(orderNode.CompletedChange == OrderNode.ChangedType.None) {
				orderNode = new OrderNode(routeListItem.Order);
				return;
			}

			if(orderNode.CompletedChange == OrderNode.ChangedType.DeliveryPoint) {
				routeListItem.Order.DeliveryPoint = orderNode.DeliveryPoint;
			}

			if(orderNode.CompletedChange == OrderNode.ChangedType.Both) {
				//Сначала ставим точку доставки чтобы при установке клиента она была доступна, 
				//иначе при записи клиента убирается не его точка доставки и будет ошибка при 
				//изменении документов которые должны меняться при смене клиента потомучто точка 
				//доставки будет пустая
				routeListItem.Order.DeliveryPoint = orderNode.DeliveryPoint;
				routeListItem.Order.Client = orderNode.Client;
			}
		}

		protected void OnReferenceClientChangedByUser(object sender, EventArgs e)
		{
			ConfigureDeliveryPointRefference(orderNode.Client);
			referenceDeliveryPoint.OpenSelectDialog();
		}

		protected void OnReferenceDeliveryPointChangedByUser(object sender, EventArgs e)
		{
			AcceptOrderChange();
		}

		protected void OnButtonAddOrderItemClicked(object sender, EventArgs e)
		{
			OpenSelectNomenclatureDlg();
		}

		protected void OnButtonDeleteOrderItemClicked(object sender, EventArgs e)
		{
			OrderItemReturnsNode selectedItemNode = ytreeToClient.GetSelectedObject() as OrderItemReturnsNode;
			if(selectedItemNode == null || selectedItemNode.OrderItem == null) {
				return;
			}
			routeListItem.Order.RemoveAloneItem(selectedItemNode.OrderItem);
			UpdateItemsList();
		}

		void OrderEquipmentItemsView_OnDeleteEquipment(object sender, OrderEquipment e)
		{
			//Если оборудование добавлено в изменении заказа то базовое количество равно 0,
			//значит такое оборудование можно удалять из изменения заказа
			if(e.OrderItem == null && e.Count == 0) {
				routeListItem.Order.RemoveEquipment(e);
			}
		}

		public bool CanClose()
		{
			var orderValidator = new QSValidator<Order>(routeListItem.Order);
			routeListItem.AddressIsValid = orderValidator.IsValid;
			orderValidator.RunDlgIfNotValid((Gtk.Window)this.Toplevel);
			routeListItem.Order.CheckAndSetOrderIsService();
			orderEquipmentItemsView.UnsubscribeOnEquipmentAdd();
			//Не блокируем закрытие вкладки
			return true;
		}

		protected void OnYenumcomboOrderPaymentChanged(object sender, EventArgs e)
		{
			OnlineOrderVisible();
		}

		private void OnlineOrderVisible()
		{
			labelOnlineOrder.Visible = entryOnlineOrder.Visible = (routeListItem.Order.PaymentType == PaymentType.ByCard);
		}
	}

	public class OrderItemReturnsNode
	{
		OrderItem orderItem;
		OrderEquipment orderEquipment;

		public OrderItem OrderItem => orderItem;

		public OrderItemReturnsNode(OrderItem item)
		{
			orderItem = item;
		}

		public OrderItemReturnsNode(OrderEquipment equipment)
		{
			orderEquipment = equipment;
		}

		public bool IsEquipment => orderEquipment != null;

		public bool IsSerialEquipment {
			get {
				return
					IsEquipment
					&& orderEquipment.Equipment != null
					&& orderEquipment.Equipment.Nomenclature.IsSerial;
			}
		}

		public bool IsDelivered {
			get => ActualCount > 0;
			set {
				if(IsEquipment && IsSerialEquipment) {
					ActualCount = value ? 1 : 0;
				}
			}
		}
		public int ActualCount {
			get {
				if(IsEquipment) {
					if(IsSerialEquipment) {
						return orderEquipment.Confirmed ? 1 : 0;
					}
					return orderEquipment.ActualCount;
				} else {
					return orderItem.ActualCount;
				}
			}
			set {
				if(IsEquipment) {
					if(IsSerialEquipment) {
						orderEquipment.ActualCount = value > 0 ? 1 : 0;
					}
					orderEquipment.ActualCount = value;
				} else {
					orderItem.ActualCount = value;
				}

			}
		}
		public Nomenclature Nomenclature {
			get {
				if(IsEquipment) {
					if(IsSerialEquipment) {
						return orderEquipment.Equipment.Nomenclature;
					}
					return orderEquipment.Nomenclature;
				} else {
					return orderItem.Nomenclature;
				}
			}
		}
		public int Count => IsEquipment ? 1 : orderItem.Count;

		public string Name => IsEquipment ? orderEquipment.NameString : orderItem.NomenclatureString;

		public bool HasPrice => !IsEquipment || orderEquipment.OrderItem != null;

		public string ConfirmedComments {
			get => IsEquipment ? orderEquipment.ConfirmedComment : null;
			set {
				if(IsEquipment)
					orderEquipment.ConfirmedComment = value;
			}
		}

		public decimal Price {
			get {
				if(IsEquipment) {
					return orderEquipment.OrderItem != null ? orderEquipment.OrderItem.Price : 0;
				} else
					return orderItem.Price;
			}
			set {
				if(IsEquipment) {
					if(orderEquipment.OrderItem != null)
						orderEquipment.OrderItem.Price = value;
				} else
					orderItem.Price = value;
			}
		}

		public bool IsDiscountInMoney{
			get {
				if(IsEquipment)
					return orderEquipment.OrderItem != null && orderEquipment.OrderItem.IsDiscountInMoney;
				return orderItem.IsDiscountInMoney;
			}

			set {
				if(IsEquipment)
					orderEquipment.OrderItem.IsDiscountInMoney = orderEquipment.OrderItem != null && value;
				else
					orderItem.IsDiscountInMoney = value;
			}
		}

		public Decimal DiscountForPrewiev{
			get{
				if(IsEquipment)
					return orderEquipment.OrderItem != null ? orderEquipment.OrderItem.DiscountForPreview : 0;
				return orderItem.DiscountForPreview;
			}

			set{
				if(IsEquipment) {
					if(orderEquipment.OrderItem != null)
						orderEquipment.OrderItem.DiscountForPreview = value;
				} else
					orderItem.DiscountForPreview = value;
			}
		}

		public Decimal Discount {
			get { 
				if(IsEquipment)
					return orderEquipment.OrderItem != null ? orderEquipment.OrderItem.Discount : 0m;
				return orderItem.Discount;
			}
			set {
				if(IsEquipment) {
					if(orderEquipment.OrderItem != null)
						orderEquipment.OrderItem.Discount = value;
				} else
					orderItem.Discount = value;
			}
		}

		public Decimal DiscountMoney {
			get {
				if(IsEquipment)
					return orderEquipment.OrderItem != null ? orderEquipment.OrderItem.DiscountMoney : 0m;
				return orderItem.DiscountMoney;
			}
			set {
				if(IsEquipment) {
					if(orderEquipment.OrderItem != null)
						orderEquipment.OrderItem.DiscountMoney = value;
				} else
					orderItem.DiscountMoney = value;
			}
		}

		public DiscountReason DiscountReason{
			get {
				if(IsEquipment)
					return orderEquipment.OrderItem != null ? orderEquipment.OrderItem.DiscountReason : null;
				else
					return orderItem.DiscountReason;
			}
			set {
				if(IsEquipment) {
					if(orderEquipment.OrderItem != null)
						orderEquipment.OrderItem.DiscountReason = value;
				} else
					orderItem.DiscountReason = value;
			}
		}

		public decimal Sum => Price * ActualCount - DiscountMoney;
	}
}
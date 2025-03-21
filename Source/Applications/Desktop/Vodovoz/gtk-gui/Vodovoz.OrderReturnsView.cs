
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz
{
	public partial class OrderReturnsView
	{
		private global::Gtk.VBox vbox1;

		private global::Gtk.Table table1;

		private global::QS.Views.Control.EntityEntry clientEntry;

		private global::QS.Widgets.GtkUI.EntityViewModelEntry entityVMEntryDeliveryPoint;

		private global::Gtk.HBox hbox3;

		private global::Gtk.Label labelOnlineOrder;

		private global::Gamma.Widgets.yValidatedEntry entryOnlineOrder;

		private global::Gtk.HBox hbox4;

		private global::Gtk.Entry entryTotal;

		private global::Gamma.Widgets.yEnumComboBox yenumcomboOrderPayment;

		private global::Gamma.Widgets.ySpecComboBox ySpecPaymentFrom;

		private global::Gamma.Widgets.yEnumComboBox yenumcomboboxTerminalSubtype;

		private global::Gtk.HBox hbox5;

		private global::Gamma.GtkWidgets.yButton buttonDelivered;

		private global::Gamma.GtkWidgets.yButton buttonNotDelivered;

		private global::Gamma.GtkWidgets.yButton buttonDeliveryCanceled;

		private global::Gtk.Label label1;

		private global::Gtk.Label label2;

		private global::Gtk.Label label3;

		private global::Gamma.GtkWidgets.yHBox yhboxBottles;

		private global::Gtk.HBox hboxBottlesByStock;

		private global::Gtk.Label labelBottlesTyStockCount;

		private global::Gamma.GtkWidgets.ySpinButton yspinbuttonBottlesByStockCount;

		private global::Gtk.Label labelBottlesTyStockActualCount;

		private global::Gamma.GtkWidgets.ySpinButton yspinbuttonBottlesByStockActualCount;

		private global::Gtk.Frame frame1;

		private global::Gtk.Alignment GtkAlignment6;

		private global::Gtk.VBox vbox5;

		private global::Gtk.ScrolledWindow GtkScrolledWindow1;

		private global::Gamma.GtkWidgets.yTreeView ytreeToClient;

		private global::Gtk.HBox hbox6;

		private global::Gamma.GtkWidgets.yButton buttonAddOrderItem;

		private global::Gamma.GtkWidgets.yButton buttonDeleteOrderItem;

		private global::Gtk.Label GtkLabel11;

		private global::Gtk.HBox hbox7;

		private global::Gtk.Frame frame2;

		private global::Gtk.Alignment GtkAlignment9;

		private global::Vodovoz.ViewWidgets.OrderEquipmentItemsView orderEquipmentItemsView;

		private global::Gtk.Label GtkLabel15;

		private global::Gtk.Frame frame3;

		private global::Gtk.Alignment GtkAlignment13;

		private global::Gtk.VBox vbox3;

		private global::Vodovoz.ViewWidgets.DepositRefundItemsView depositrefunditemsview1;

		private global::Gtk.Label GtkLabel19;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.OrderReturnsView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.OrderReturnsView";
			// Container child Vodovoz.OrderReturnsView.Gtk.Container+ContainerChild
			this.vbox1 = new global::Gtk.VBox();
			this.vbox1.Name = "vbox1";
			this.vbox1.Spacing = 6;
			// Container child vbox1.Gtk.Box+BoxChild
			this.table1 = new global::Gtk.Table(((uint)(4)), ((uint)(3)), false);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(6));
			// Container child table1.Gtk.Table+TableChild
			this.clientEntry = new global::QS.Views.Control.EntityEntry();
			this.clientEntry.Events = ((global::Gdk.EventMask)(256));
			this.clientEntry.Name = "clientEntry";
			this.table1.Add(this.clientEntry);
			global::Gtk.Table.TableChild w1 = ((global::Gtk.Table.TableChild)(this.table1[this.clientEntry]));
			w1.LeftAttach = ((uint)(1));
			w1.RightAttach = ((uint)(3));
			w1.XOptions = ((global::Gtk.AttachOptions)(4));
			w1.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.entityVMEntryDeliveryPoint = new global::QS.Widgets.GtkUI.EntityViewModelEntry();
			this.entityVMEntryDeliveryPoint.Events = ((global::Gdk.EventMask)(256));
			this.entityVMEntryDeliveryPoint.Name = "entityVMEntryDeliveryPoint";
			this.entityVMEntryDeliveryPoint.CanEditReference = true;
			this.entityVMEntryDeliveryPoint.CanOpenWithoutTabParent = false;
			this.table1.Add(this.entityVMEntryDeliveryPoint);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.table1[this.entityVMEntryDeliveryPoint]));
			w2.TopAttach = ((uint)(1));
			w2.BottomAttach = ((uint)(2));
			w2.LeftAttach = ((uint)(1));
			w2.RightAttach = ((uint)(3));
			w2.XOptions = ((global::Gtk.AttachOptions)(4));
			w2.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.hbox3 = new global::Gtk.HBox();
			this.hbox3.Name = "hbox3";
			this.hbox3.Spacing = 6;
			// Container child hbox3.Gtk.Box+BoxChild
			this.labelOnlineOrder = new global::Gtk.Label();
			this.labelOnlineOrder.Name = "labelOnlineOrder";
			this.labelOnlineOrder.Xalign = 1F;
			this.labelOnlineOrder.LabelProp = global::Mono.Unix.Catalog.GetString("Онлайн заказ:");
			this.hbox3.Add(this.labelOnlineOrder);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.hbox3[this.labelOnlineOrder]));
			w3.Position = 0;
			w3.Expand = false;
			w3.Fill = false;
			// Container child hbox3.Gtk.Box+BoxChild
			this.entryOnlineOrder = new global::Gamma.Widgets.yValidatedEntry();
			this.entryOnlineOrder.CanFocus = true;
			this.entryOnlineOrder.Name = "entryOnlineOrder";
			this.entryOnlineOrder.IsEditable = true;
			this.entryOnlineOrder.MaxLength = 9;
			this.entryOnlineOrder.InvisibleChar = '●';
			this.hbox3.Add(this.entryOnlineOrder);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hbox3[this.entryOnlineOrder]));
			w4.Position = 1;
			this.table1.Add(this.hbox3);
			global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.table1[this.hbox3]));
			w5.TopAttach = ((uint)(2));
			w5.BottomAttach = ((uint)(3));
			w5.LeftAttach = ((uint)(2));
			w5.RightAttach = ((uint)(3));
			w5.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.hbox4 = new global::Gtk.HBox();
			this.hbox4.Name = "hbox4";
			this.hbox4.Spacing = 6;
			// Container child hbox4.Gtk.Box+BoxChild
			this.entryTotal = new global::Gtk.Entry();
			this.entryTotal.CanFocus = true;
			this.entryTotal.Name = "entryTotal";
			this.entryTotal.IsEditable = false;
			this.entryTotal.InvisibleChar = '•';
			this.hbox4.Add(this.entryTotal);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.hbox4[this.entryTotal]));
			w6.Position = 0;
			w6.Expand = false;
			// Container child hbox4.Gtk.Box+BoxChild
			this.yenumcomboOrderPayment = new global::Gamma.Widgets.yEnumComboBox();
			this.yenumcomboOrderPayment.Name = "yenumcomboOrderPayment";
			this.yenumcomboOrderPayment.ShowSpecialStateAll = false;
			this.yenumcomboOrderPayment.ShowSpecialStateNot = false;
			this.yenumcomboOrderPayment.UseShortTitle = false;
			this.yenumcomboOrderPayment.DefaultFirst = false;
			this.hbox4.Add(this.yenumcomboOrderPayment);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.hbox4[this.yenumcomboOrderPayment]));
			w7.Position = 1;
			// Container child hbox4.Gtk.Box+BoxChild
			this.ySpecPaymentFrom = new global::Gamma.Widgets.ySpecComboBox();
			this.ySpecPaymentFrom.Name = "ySpecPaymentFrom";
			this.ySpecPaymentFrom.AddIfNotExist = false;
			this.ySpecPaymentFrom.DefaultFirst = false;
			this.ySpecPaymentFrom.ShowSpecialStateAll = false;
			this.ySpecPaymentFrom.ShowSpecialStateNot = false;
			this.hbox4.Add(this.ySpecPaymentFrom);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.hbox4[this.ySpecPaymentFrom]));
			w8.Position = 2;
			w8.Expand = false;
			w8.Fill = false;
			// Container child hbox4.Gtk.Box+BoxChild
			this.yenumcomboboxTerminalSubtype = new global::Gamma.Widgets.yEnumComboBox();
			this.yenumcomboboxTerminalSubtype.Name = "yenumcomboboxTerminalSubtype";
			this.yenumcomboboxTerminalSubtype.ShowSpecialStateAll = false;
			this.yenumcomboboxTerminalSubtype.ShowSpecialStateNot = false;
			this.yenumcomboboxTerminalSubtype.UseShortTitle = false;
			this.yenumcomboboxTerminalSubtype.DefaultFirst = false;
			this.hbox4.Add(this.yenumcomboboxTerminalSubtype);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.hbox4[this.yenumcomboboxTerminalSubtype]));
			w9.PackType = ((global::Gtk.PackType)(1));
			w9.Position = 3;
			w9.Expand = false;
			w9.Fill = false;
			this.table1.Add(this.hbox4);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.table1[this.hbox4]));
			w10.TopAttach = ((uint)(2));
			w10.BottomAttach = ((uint)(3));
			w10.LeftAttach = ((uint)(1));
			w10.RightAttach = ((uint)(2));
			w10.XOptions = ((global::Gtk.AttachOptions)(4));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.hbox5 = new global::Gtk.HBox();
			this.hbox5.Name = "hbox5";
			this.hbox5.Spacing = 6;
			// Container child hbox5.Gtk.Box+BoxChild
			this.buttonDelivered = new global::Gamma.GtkWidgets.yButton();
			this.buttonDelivered.CanFocus = true;
			this.buttonDelivered.Name = "buttonDelivered";
			this.buttonDelivered.UseUnderline = true;
			this.buttonDelivered.Label = global::Mono.Unix.Catalog.GetString("Доставлен");
			this.hbox5.Add(this.buttonDelivered);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.hbox5[this.buttonDelivered]));
			w11.Position = 0;
			w11.Expand = false;
			w11.Fill = false;
			// Container child hbox5.Gtk.Box+BoxChild
			this.buttonNotDelivered = new global::Gamma.GtkWidgets.yButton();
			this.buttonNotDelivered.CanFocus = true;
			this.buttonNotDelivered.Name = "buttonNotDelivered";
			this.buttonNotDelivered.UseUnderline = true;
			this.buttonNotDelivered.Label = global::Mono.Unix.Catalog.GetString("Полностью недовезен");
			this.hbox5.Add(this.buttonNotDelivered);
			global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.hbox5[this.buttonNotDelivered]));
			w12.Position = 1;
			w12.Expand = false;
			w12.Fill = false;
			// Container child hbox5.Gtk.Box+BoxChild
			this.buttonDeliveryCanceled = new global::Gamma.GtkWidgets.yButton();
			this.buttonDeliveryCanceled.CanFocus = true;
			this.buttonDeliveryCanceled.Name = "buttonDeliveryCanceled";
			this.buttonDeliveryCanceled.UseUnderline = true;
			this.buttonDeliveryCanceled.Label = global::Mono.Unix.Catalog.GetString("Доставка отменена");
			this.hbox5.Add(this.buttonDeliveryCanceled);
			global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.hbox5[this.buttonDeliveryCanceled]));
			w13.Position = 2;
			w13.Expand = false;
			w13.Fill = false;
			this.table1.Add(this.hbox5);
			global::Gtk.Table.TableChild w14 = ((global::Gtk.Table.TableChild)(this.table1[this.hbox5]));
			w14.TopAttach = ((uint)(3));
			w14.BottomAttach = ((uint)(4));
			w14.LeftAttach = ((uint)(1));
			w14.RightAttach = ((uint)(3));
			w14.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label1 = new global::Gtk.Label();
			this.label1.Name = "label1";
			this.label1.Xalign = 1F;
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString("Клиент:");
			this.table1.Add(this.label1);
			global::Gtk.Table.TableChild w15 = ((global::Gtk.Table.TableChild)(this.table1[this.label1]));
			w15.XOptions = ((global::Gtk.AttachOptions)(4));
			w15.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label2 = new global::Gtk.Label();
			this.label2.Name = "label2";
			this.label2.LabelProp = global::Mono.Unix.Catalog.GetString("Точка доставки:");
			this.table1.Add(this.label2);
			global::Gtk.Table.TableChild w16 = ((global::Gtk.Table.TableChild)(this.table1[this.label2]));
			w16.TopAttach = ((uint)(1));
			w16.BottomAttach = ((uint)(2));
			w16.XOptions = ((global::Gtk.AttachOptions)(4));
			w16.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label3 = new global::Gtk.Label();
			this.label3.Name = "label3";
			this.label3.LabelProp = global::Mono.Unix.Catalog.GetString("Сумма заказа:");
			this.table1.Add(this.label3);
			global::Gtk.Table.TableChild w17 = ((global::Gtk.Table.TableChild)(this.table1[this.label3]));
			w17.TopAttach = ((uint)(2));
			w17.BottomAttach = ((uint)(3));
			w17.XOptions = ((global::Gtk.AttachOptions)(4));
			w17.YOptions = ((global::Gtk.AttachOptions)(4));
			this.vbox1.Add(this.table1);
			global::Gtk.Box.BoxChild w18 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.table1]));
			w18.Position = 0;
			w18.Expand = false;
			w18.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.yhboxBottles = new global::Gamma.GtkWidgets.yHBox();
			this.yhboxBottles.Name = "yhboxBottles";
			this.yhboxBottles.Spacing = 6;
			// Container child yhboxBottles.Gtk.Box+BoxChild
			this.hboxBottlesByStock = new global::Gtk.HBox();
			this.hboxBottlesByStock.Name = "hboxBottlesByStock";
			this.hboxBottlesByStock.Spacing = 6;
			// Container child hboxBottlesByStock.Gtk.Box+BoxChild
			this.labelBottlesTyStockCount = new global::Gtk.Label();
			this.labelBottlesTyStockCount.Name = "labelBottlesTyStockCount";
			this.labelBottlesTyStockCount.LabelProp = global::Mono.Unix.Catalog.GetString("Бутылей по акции план:");
			this.hboxBottlesByStock.Add(this.labelBottlesTyStockCount);
			global::Gtk.Box.BoxChild w19 = ((global::Gtk.Box.BoxChild)(this.hboxBottlesByStock[this.labelBottlesTyStockCount]));
			w19.Position = 0;
			w19.Expand = false;
			w19.Fill = false;
			// Container child hboxBottlesByStock.Gtk.Box+BoxChild
			this.yspinbuttonBottlesByStockCount = new global::Gamma.GtkWidgets.ySpinButton(0D, 10000000D, 1D);
			this.yspinbuttonBottlesByStockCount.Sensitive = false;
			this.yspinbuttonBottlesByStockCount.CanFocus = true;
			this.yspinbuttonBottlesByStockCount.Name = "yspinbuttonBottlesByStockCount";
			this.yspinbuttonBottlesByStockCount.Adjustment.PageIncrement = 10D;
			this.yspinbuttonBottlesByStockCount.ClimbRate = 1D;
			this.yspinbuttonBottlesByStockCount.Numeric = true;
			this.yspinbuttonBottlesByStockCount.ValueAsDecimal = 0m;
			this.yspinbuttonBottlesByStockCount.ValueAsInt = 0;
			this.hboxBottlesByStock.Add(this.yspinbuttonBottlesByStockCount);
			global::Gtk.Box.BoxChild w20 = ((global::Gtk.Box.BoxChild)(this.hboxBottlesByStock[this.yspinbuttonBottlesByStockCount]));
			w20.Position = 1;
			w20.Expand = false;
			w20.Fill = false;
			// Container child hboxBottlesByStock.Gtk.Box+BoxChild
			this.labelBottlesTyStockActualCount = new global::Gtk.Label();
			this.labelBottlesTyStockActualCount.Name = "labelBottlesTyStockActualCount";
			this.labelBottlesTyStockActualCount.LabelProp = global::Mono.Unix.Catalog.GetString("факт:");
			this.hboxBottlesByStock.Add(this.labelBottlesTyStockActualCount);
			global::Gtk.Box.BoxChild w21 = ((global::Gtk.Box.BoxChild)(this.hboxBottlesByStock[this.labelBottlesTyStockActualCount]));
			w21.Position = 2;
			w21.Expand = false;
			w21.Fill = false;
			// Container child hboxBottlesByStock.Gtk.Box+BoxChild
			this.yspinbuttonBottlesByStockActualCount = new global::Gamma.GtkWidgets.ySpinButton(0D, 10000000D, 1D);
			this.yspinbuttonBottlesByStockActualCount.CanFocus = true;
			this.yspinbuttonBottlesByStockActualCount.Name = "yspinbuttonBottlesByStockActualCount";
			this.yspinbuttonBottlesByStockActualCount.Adjustment.PageIncrement = 10D;
			this.yspinbuttonBottlesByStockActualCount.ClimbRate = 1D;
			this.yspinbuttonBottlesByStockActualCount.Numeric = true;
			this.yspinbuttonBottlesByStockActualCount.ValueAsDecimal = 0m;
			this.yspinbuttonBottlesByStockActualCount.ValueAsInt = 0;
			this.hboxBottlesByStock.Add(this.yspinbuttonBottlesByStockActualCount);
			global::Gtk.Box.BoxChild w22 = ((global::Gtk.Box.BoxChild)(this.hboxBottlesByStock[this.yspinbuttonBottlesByStockActualCount]));
			w22.Position = 3;
			w22.Expand = false;
			w22.Fill = false;
			this.yhboxBottles.Add(this.hboxBottlesByStock);
			global::Gtk.Box.BoxChild w23 = ((global::Gtk.Box.BoxChild)(this.yhboxBottles[this.hboxBottlesByStock]));
			w23.Position = 0;
			w23.Expand = false;
			w23.Fill = false;
			this.vbox1.Add(this.yhboxBottles);
			global::Gtk.Box.BoxChild w24 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.yhboxBottles]));
			w24.Position = 1;
			w24.Expand = false;
			w24.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.frame1 = new global::Gtk.Frame();
			this.frame1.Name = "frame1";
			this.frame1.ShadowType = ((global::Gtk.ShadowType)(0));
			// Container child frame1.Gtk.Container+ContainerChild
			this.GtkAlignment6 = new global::Gtk.Alignment(0F, 0F, 1F, 1F);
			this.GtkAlignment6.Name = "GtkAlignment6";
			this.GtkAlignment6.LeftPadding = ((uint)(12));
			// Container child GtkAlignment6.Gtk.Container+ContainerChild
			this.vbox5 = new global::Gtk.VBox();
			this.vbox5.Name = "vbox5";
			this.vbox5.Spacing = 6;
			// Container child vbox5.Gtk.Box+BoxChild
			this.GtkScrolledWindow1 = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow1.Name = "GtkScrolledWindow1";
			this.GtkScrolledWindow1.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow1.Gtk.Container+ContainerChild
			this.ytreeToClient = new global::Gamma.GtkWidgets.yTreeView();
			this.ytreeToClient.CanFocus = true;
			this.ytreeToClient.Name = "ytreeToClient";
			this.GtkScrolledWindow1.Add(this.ytreeToClient);
			this.vbox5.Add(this.GtkScrolledWindow1);
			global::Gtk.Box.BoxChild w26 = ((global::Gtk.Box.BoxChild)(this.vbox5[this.GtkScrolledWindow1]));
			w26.Position = 0;
			// Container child vbox5.Gtk.Box+BoxChild
			this.hbox6 = new global::Gtk.HBox();
			this.hbox6.Name = "hbox6";
			this.hbox6.Spacing = 6;
			// Container child hbox6.Gtk.Box+BoxChild
			this.buttonAddOrderItem = new global::Gamma.GtkWidgets.yButton();
			this.buttonAddOrderItem.CanFocus = true;
			this.buttonAddOrderItem.Name = "buttonAddOrderItem";
			this.buttonAddOrderItem.UseUnderline = true;
			this.buttonAddOrderItem.Label = global::Mono.Unix.Catalog.GetString("Добавить товар");
			global::Gtk.Image w27 = new global::Gtk.Image();
			w27.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-add", global::Gtk.IconSize.Menu);
			this.buttonAddOrderItem.Image = w27;
			this.hbox6.Add(this.buttonAddOrderItem);
			global::Gtk.Box.BoxChild w28 = ((global::Gtk.Box.BoxChild)(this.hbox6[this.buttonAddOrderItem]));
			w28.Position = 0;
			w28.Expand = false;
			w28.Fill = false;
			// Container child hbox6.Gtk.Box+BoxChild
			this.buttonDeleteOrderItem = new global::Gamma.GtkWidgets.yButton();
			this.buttonDeleteOrderItem.CanFocus = true;
			this.buttonDeleteOrderItem.Name = "buttonDeleteOrderItem";
			this.buttonDeleteOrderItem.UseUnderline = true;
			this.buttonDeleteOrderItem.Label = global::Mono.Unix.Catalog.GetString("Удалить товар");
			global::Gtk.Image w29 = new global::Gtk.Image();
			w29.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-delete", global::Gtk.IconSize.Menu);
			this.buttonDeleteOrderItem.Image = w29;
			this.hbox6.Add(this.buttonDeleteOrderItem);
			global::Gtk.Box.BoxChild w30 = ((global::Gtk.Box.BoxChild)(this.hbox6[this.buttonDeleteOrderItem]));
			w30.Position = 1;
			w30.Expand = false;
			w30.Fill = false;
			this.vbox5.Add(this.hbox6);
			global::Gtk.Box.BoxChild w31 = ((global::Gtk.Box.BoxChild)(this.vbox5[this.hbox6]));
			w31.Position = 1;
			w31.Expand = false;
			w31.Fill = false;
			this.GtkAlignment6.Add(this.vbox5);
			this.frame1.Add(this.GtkAlignment6);
			this.GtkLabel11 = new global::Gtk.Label();
			this.GtkLabel11.Name = "GtkLabel11";
			this.GtkLabel11.LabelProp = global::Mono.Unix.Catalog.GetString("<b>Товары к клиенту</b>");
			this.GtkLabel11.UseMarkup = true;
			this.frame1.LabelWidget = this.GtkLabel11;
			this.vbox1.Add(this.frame1);
			global::Gtk.Box.BoxChild w34 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.frame1]));
			w34.Position = 2;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hbox7 = new global::Gtk.HBox();
			this.hbox7.Name = "hbox7";
			this.hbox7.Spacing = 6;
			// Container child hbox7.Gtk.Box+BoxChild
			this.frame2 = new global::Gtk.Frame();
			this.frame2.Name = "frame2";
			this.frame2.ShadowType = ((global::Gtk.ShadowType)(0));
			// Container child frame2.Gtk.Container+ContainerChild
			this.GtkAlignment9 = new global::Gtk.Alignment(0F, 0F, 1F, 1F);
			this.GtkAlignment9.Name = "GtkAlignment9";
			this.GtkAlignment9.LeftPadding = ((uint)(12));
			// Container child GtkAlignment9.Gtk.Container+ContainerChild
			this.orderEquipmentItemsView = new global::Vodovoz.ViewWidgets.OrderEquipmentItemsView();
			this.orderEquipmentItemsView.Sensitive = false;
			this.orderEquipmentItemsView.Events = ((global::Gdk.EventMask)(256));
			this.orderEquipmentItemsView.Name = "orderEquipmentItemsView";
			this.orderEquipmentItemsView.Sensitive = false;
			this.GtkAlignment9.Add(this.orderEquipmentItemsView);
			this.frame2.Add(this.GtkAlignment9);
			this.GtkLabel15 = new global::Gtk.Label();
			this.GtkLabel15.Name = "GtkLabel15";
			this.GtkLabel15.LabelProp = global::Mono.Unix.Catalog.GetString("<b>Оборудование</b>");
			this.GtkLabel15.UseMarkup = true;
			this.frame2.LabelWidget = this.GtkLabel15;
			this.hbox7.Add(this.frame2);
			global::Gtk.Box.BoxChild w37 = ((global::Gtk.Box.BoxChild)(this.hbox7[this.frame2]));
			w37.Position = 0;
			this.vbox1.Add(this.hbox7);
			global::Gtk.Box.BoxChild w38 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hbox7]));
			w38.Position = 3;
			// Container child vbox1.Gtk.Box+BoxChild
			this.frame3 = new global::Gtk.Frame();
			this.frame3.Name = "frame3";
			this.frame3.ShadowType = ((global::Gtk.ShadowType)(0));
			// Container child frame3.Gtk.Container+ContainerChild
			this.GtkAlignment13 = new global::Gtk.Alignment(0F, 0F, 1F, 1F);
			this.GtkAlignment13.Name = "GtkAlignment13";
			this.GtkAlignment13.LeftPadding = ((uint)(12));
			// Container child GtkAlignment13.Gtk.Container+ContainerChild
			this.vbox3 = new global::Gtk.VBox();
			this.vbox3.Name = "vbox3";
			this.vbox3.Spacing = 6;
			// Container child vbox3.Gtk.Box+BoxChild
			this.depositrefunditemsview1 = new global::Vodovoz.ViewWidgets.DepositRefundItemsView();
			this.depositrefunditemsview1.Sensitive = false;
			this.depositrefunditemsview1.Events = ((global::Gdk.EventMask)(256));
			this.depositrefunditemsview1.Name = "depositrefunditemsview1";
			this.depositrefunditemsview1.Sensitive = false;
			this.vbox3.Add(this.depositrefunditemsview1);
			global::Gtk.Box.BoxChild w39 = ((global::Gtk.Box.BoxChild)(this.vbox3[this.depositrefunditemsview1]));
			w39.Position = 0;
			this.GtkAlignment13.Add(this.vbox3);
			this.frame3.Add(this.GtkAlignment13);
			this.GtkLabel19 = new global::Gtk.Label();
			this.GtkLabel19.Name = "GtkLabel19";
			this.GtkLabel19.LabelProp = global::Mono.Unix.Catalog.GetString("<b>Возврат залогов</b>");
			this.GtkLabel19.UseMarkup = true;
			this.frame3.LabelWidget = this.GtkLabel19;
			this.vbox1.Add(this.frame3);
			global::Gtk.Box.BoxChild w42 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.frame3]));
			w42.Position = 4;
			this.Add(this.vbox1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
			this.buttonDelivered.Clicked += new global::System.EventHandler(this.OnButtonDeliveredClicked);
			this.buttonNotDelivered.Clicked += new global::System.EventHandler(this.OnButtonNotDeliveredClicked);
			this.buttonDeliveryCanceled.Clicked += new global::System.EventHandler(this.OnButtonDeliveryCanceledClicked);
			this.yenumcomboOrderPayment.ChangedByUser += new global::System.EventHandler(this.OnYenumcomboOrderPaymentChangedByUser);
			this.yenumcomboOrderPayment.Changed += new global::System.EventHandler(this.OnYenumcomboOrderPaymentChanged);
			this.entityVMEntryDeliveryPoint.ChangedByUser += new global::System.EventHandler(this.OnEntityVMEntryDeliveryPointChangedByUser);
			this.buttonAddOrderItem.Clicked += new global::System.EventHandler(this.OnButtonAddOrderItemClicked);
			this.buttonDeleteOrderItem.Clicked += new global::System.EventHandler(this.OnButtonDeleteOrderItemClicked);
		}
	}
}

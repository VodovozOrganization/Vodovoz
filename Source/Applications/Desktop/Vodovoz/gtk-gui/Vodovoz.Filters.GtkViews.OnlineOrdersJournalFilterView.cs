
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.Filters.GtkViews
{
	public partial class OnlineOrdersJournalFilterView
	{
		private global::Gtk.ScrolledWindow scrolledwindow1;

		private global::Gtk.Table table1;

		private global::QS.Widgets.GtkUI.DateRangePicker dateperiodOrders;

		private global::QS.Views.Control.EntityEntry entryCounterparty;

		private global::QS.Views.Control.EntityEntry entryDeliveryPoint;

		private global::QS.Views.Control.EntityEntry entryEmployeeWorkWith;

		private global::Gamma.Widgets.yEnumComboBox enumCmbEntityType;

		private global::Gamma.Widgets.yEnumComboBox enumCmbSource;

		private global::Gamma.Widgets.yEnumComboBox enumcomboPaymentType;

		private global::Gamma.Widgets.yEnumComboBox enumcomboStatus;

		private global::Gtk.HBox hboxCheckBoxFilters;

		private global::Gamma.GtkWidgets.yLabel lblSelfDelivery;

		private global::QS.Widgets.NullableCheckButton selfDeliveryBtn;

		private global::Gamma.GtkWidgets.yLabel lblNeedConfirmationByCall;

		private global::QS.Widgets.NullableCheckButton needConfirmationByCallBtn;

		private global::Gamma.GtkWidgets.yLabel lblFastDelivery;

		private global::QS.Widgets.NullableCheckButton fastDeliveryBtn;

		private global::Gtk.HBox hboxLastLineOfFilters;

		private global::Gamma.GtkWidgets.yLabel ylabelOrderId;

		private global::QS.Widgets.ValidatedEntry entryOrderId;

		private global::Gamma.GtkWidgets.yLabel ylblOnlineOrderId;

		private global::QS.Widgets.ValidatedEntry eOnlineOrderId;

		private global::Gamma.GtkWidgets.yLabel ylabelCounterpartyPhone;

		private global::QS.Widgets.ValidatedEntry entryCounterpartyPhone;

		private global::Gtk.Label label1;

		private global::Gtk.Label label2;

		private global::Gtk.Label label3;

		private global::Gtk.Label label5;

		private global::Gtk.Label label7;

		private global::Gtk.Label lblEmployeeWorkWith;

		private global::Gamma.GtkWidgets.yLabel lblEntityType;

		private global::Gtk.Label lblSource;

		private global::QS.Widgets.GtkUI.SpecialListComboBox speciallistCmbPaymentsFrom;

		private global::Gamma.Widgets.yEnumComboBox yenumcomboboxDateType;

		private global::Gamma.Widgets.yEnumComboBox yenumСmbboxOrderPaymentStatus;

		private global::Gamma.GtkWidgets.yLabel ylabel2;

		private global::Gamma.GtkWidgets.yLabel ylblPaymentFrom;

		private global::Gamma.Widgets.ySpecComboBox ySpecCmbGeographicGroup;

		private global::Gamma.GtkWidgets.yVBox yvboxAdditionalFilters;

		private global::Gtk.HBox hboxAdditionalFilters;

		private global::Gtk.Label lblentryCounteragentNameLike;

		private global::QS.Widgets.ValidatedEntry entryCounteragentNameLike;

		private global::Gtk.Label lblInn;

		private global::QS.Widgets.ValidatedEntry entryInn;

		private global::Gamma.GtkWidgets.yHBox yhboxSearchByAddressMain;

		private global::Gtk.Label lblDeliveryPointAddress;

		private global::Gamma.GtkWidgets.yHBox yhboxSearchByAddress;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.Filters.GtkViews.OnlineOrdersJournalFilterView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.Filters.GtkViews.OnlineOrdersJournalFilterView";
			// Container child Vodovoz.Filters.GtkViews.OnlineOrdersJournalFilterView.Gtk.Container+ContainerChild
			this.scrolledwindow1 = new global::Gtk.ScrolledWindow();
			this.scrolledwindow1.CanFocus = true;
			this.scrolledwindow1.Name = "scrolledwindow1";
			this.scrolledwindow1.VscrollbarPolicy = ((global::Gtk.PolicyType)(2));
			this.scrolledwindow1.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child scrolledwindow1.Gtk.Container+ContainerChild
			global::Gtk.Viewport w1 = new global::Gtk.Viewport();
			w1.ShadowType = ((global::Gtk.ShadowType)(0));
			// Container child GtkViewport.Gtk.Container+ContainerChild
			this.table1 = new global::Gtk.Table(((uint)(9)), ((uint)(7)), false);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(6));
			// Container child table1.Gtk.Table+TableChild
			this.dateperiodOrders = new global::QS.Widgets.GtkUI.DateRangePicker();
			this.dateperiodOrders.Events = ((global::Gdk.EventMask)(256));
			this.dateperiodOrders.Name = "dateperiodOrders";
			this.dateperiodOrders.StartDate = new global::System.DateTime(0);
			this.dateperiodOrders.EndDate = new global::System.DateTime(0);
			this.table1.Add(this.dateperiodOrders);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.table1[this.dateperiodOrders]));
			w2.TopAttach = ((uint)(1));
			w2.BottomAttach = ((uint)(2));
			w2.LeftAttach = ((uint)(5));
			w2.RightAttach = ((uint)(6));
			w2.XOptions = ((global::Gtk.AttachOptions)(4));
			w2.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.entryCounterparty = new global::QS.Views.Control.EntityEntry();
			this.entryCounterparty.Events = ((global::Gdk.EventMask)(256));
			this.entryCounterparty.Name = "entryCounterparty";
			this.table1.Add(this.entryCounterparty);
			global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.table1[this.entryCounterparty]));
			w3.TopAttach = ((uint)(3));
			w3.BottomAttach = ((uint)(4));
			w3.LeftAttach = ((uint)(1));
			w3.RightAttach = ((uint)(2));
			w3.XOptions = ((global::Gtk.AttachOptions)(4));
			w3.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.entryDeliveryPoint = new global::QS.Views.Control.EntityEntry();
			this.entryDeliveryPoint.Events = ((global::Gdk.EventMask)(256));
			this.entryDeliveryPoint.Name = "entryDeliveryPoint";
			this.table1.Add(this.entryDeliveryPoint);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.table1[this.entryDeliveryPoint]));
			w4.TopAttach = ((uint)(4));
			w4.BottomAttach = ((uint)(5));
			w4.LeftAttach = ((uint)(3));
			w4.RightAttach = ((uint)(6));
			w4.XOptions = ((global::Gtk.AttachOptions)(4));
			w4.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.entryEmployeeWorkWith = new global::QS.Views.Control.EntityEntry();
			this.entryEmployeeWorkWith.Events = ((global::Gdk.EventMask)(256));
			this.entryEmployeeWorkWith.Name = "entryEmployeeWorkWith";
			this.table1.Add(this.entryEmployeeWorkWith);
			global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.table1[this.entryEmployeeWorkWith]));
			w5.TopAttach = ((uint)(4));
			w5.BottomAttach = ((uint)(5));
			w5.LeftAttach = ((uint)(1));
			w5.RightAttach = ((uint)(2));
			w5.XOptions = ((global::Gtk.AttachOptions)(4));
			w5.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.enumCmbEntityType = new global::Gamma.Widgets.yEnumComboBox();
			this.enumCmbEntityType.Name = "enumCmbEntityType";
			this.enumCmbEntityType.ShowSpecialStateAll = false;
			this.enumCmbEntityType.ShowSpecialStateNot = false;
			this.enumCmbEntityType.UseShortTitle = false;
			this.enumCmbEntityType.DefaultFirst = false;
			this.table1.Add(this.enumCmbEntityType);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.table1[this.enumCmbEntityType]));
			w6.LeftAttach = ((uint)(1));
			w6.RightAttach = ((uint)(2));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.enumCmbSource = new global::Gamma.Widgets.yEnumComboBox();
			this.enumCmbSource.Name = "enumCmbSource";
			this.enumCmbSource.ShowSpecialStateAll = false;
			this.enumCmbSource.ShowSpecialStateNot = false;
			this.enumCmbSource.UseShortTitle = false;
			this.enumCmbSource.DefaultFirst = false;
			this.table1.Add(this.enumCmbSource);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.table1[this.enumCmbSource]));
			w7.TopAttach = ((uint)(1));
			w7.BottomAttach = ((uint)(2));
			w7.LeftAttach = ((uint)(1));
			w7.RightAttach = ((uint)(2));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.enumcomboPaymentType = new global::Gamma.Widgets.yEnumComboBox();
			this.enumcomboPaymentType.Name = "enumcomboPaymentType";
			this.enumcomboPaymentType.ShowSpecialStateAll = true;
			this.enumcomboPaymentType.ShowSpecialStateNot = false;
			this.enumcomboPaymentType.UseShortTitle = false;
			this.enumcomboPaymentType.DefaultFirst = false;
			this.table1.Add(this.enumcomboPaymentType);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.table1[this.enumcomboPaymentType]));
			w8.TopAttach = ((uint)(2));
			w8.BottomAttach = ((uint)(3));
			w8.LeftAttach = ((uint)(3));
			w8.RightAttach = ((uint)(4));
			w8.XOptions = ((global::Gtk.AttachOptions)(4));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.enumcomboStatus = new global::Gamma.Widgets.yEnumComboBox();
			this.enumcomboStatus.Name = "enumcomboStatus";
			this.enumcomboStatus.ShowSpecialStateAll = true;
			this.enumcomboStatus.ShowSpecialStateNot = false;
			this.enumcomboStatus.UseShortTitle = false;
			this.enumcomboStatus.DefaultFirst = false;
			this.table1.Add(this.enumcomboStatus);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.table1[this.enumcomboStatus]));
			w9.TopAttach = ((uint)(2));
			w9.BottomAttach = ((uint)(3));
			w9.LeftAttach = ((uint)(1));
			w9.RightAttach = ((uint)(2));
			w9.XOptions = ((global::Gtk.AttachOptions)(4));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.hboxCheckBoxFilters = new global::Gtk.HBox();
			this.hboxCheckBoxFilters.Name = "hboxCheckBoxFilters";
			this.hboxCheckBoxFilters.Spacing = 6;
			// Container child hboxCheckBoxFilters.Gtk.Box+BoxChild
			this.lblSelfDelivery = new global::Gamma.GtkWidgets.yLabel();
			this.lblSelfDelivery.Name = "lblSelfDelivery";
			this.lblSelfDelivery.LabelProp = global::Mono.Unix.Catalog.GetString("Самовывоз:");
			this.hboxCheckBoxFilters.Add(this.lblSelfDelivery);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.hboxCheckBoxFilters[this.lblSelfDelivery]));
			w10.Position = 0;
			w10.Expand = false;
			w10.Fill = false;
			// Container child hboxCheckBoxFilters.Gtk.Box+BoxChild
			this.selfDeliveryBtn = new global::QS.Widgets.NullableCheckButton();
			this.selfDeliveryBtn.CanFocus = true;
			this.selfDeliveryBtn.Name = "selfDeliveryBtn";
			this.selfDeliveryBtn.UseUnderline = true;
			this.selfDeliveryBtn.Label = global::Mono.Unix.Catalog.GetString("selfDeliveryBtn");
			this.hboxCheckBoxFilters.Add(this.selfDeliveryBtn);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.hboxCheckBoxFilters[this.selfDeliveryBtn]));
			w11.Position = 1;
			w11.Expand = false;
			w11.Fill = false;
			// Container child hboxCheckBoxFilters.Gtk.Box+BoxChild
			this.lblNeedConfirmationByCall = new global::Gamma.GtkWidgets.yLabel();
			this.lblNeedConfirmationByCall.Name = "lblNeedConfirmationByCall";
			this.lblNeedConfirmationByCall.LabelProp = global::Mono.Unix.Catalog.GetString("Нужно подтверждение по телефону:");
			this.hboxCheckBoxFilters.Add(this.lblNeedConfirmationByCall);
			global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.hboxCheckBoxFilters[this.lblNeedConfirmationByCall]));
			w12.Position = 2;
			w12.Expand = false;
			w12.Fill = false;
			// Container child hboxCheckBoxFilters.Gtk.Box+BoxChild
			this.needConfirmationByCallBtn = new global::QS.Widgets.NullableCheckButton();
			this.needConfirmationByCallBtn.CanFocus = true;
			this.needConfirmationByCallBtn.Name = "needConfirmationByCallBtn";
			this.needConfirmationByCallBtn.UseUnderline = true;
			this.needConfirmationByCallBtn.Label = global::Mono.Unix.Catalog.GetString("needConfirmationByCallBtn");
			this.hboxCheckBoxFilters.Add(this.needConfirmationByCallBtn);
			global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.hboxCheckBoxFilters[this.needConfirmationByCallBtn]));
			w13.Position = 3;
			w13.Expand = false;
			w13.Fill = false;
			// Container child hboxCheckBoxFilters.Gtk.Box+BoxChild
			this.lblFastDelivery = new global::Gamma.GtkWidgets.yLabel();
			this.lblFastDelivery.Name = "lblFastDelivery";
			this.lblFastDelivery.LabelProp = global::Mono.Unix.Catalog.GetString("Доставка за час:");
			this.hboxCheckBoxFilters.Add(this.lblFastDelivery);
			global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.hboxCheckBoxFilters[this.lblFastDelivery]));
			w14.Position = 4;
			w14.Expand = false;
			w14.Fill = false;
			// Container child hboxCheckBoxFilters.Gtk.Box+BoxChild
			this.fastDeliveryBtn = new global::QS.Widgets.NullableCheckButton();
			this.fastDeliveryBtn.CanFocus = true;
			this.fastDeliveryBtn.Name = "fastDeliveryBtn";
			this.fastDeliveryBtn.UseUnderline = true;
			this.fastDeliveryBtn.Label = global::Mono.Unix.Catalog.GetString("fastDeliveryBtn");
			this.hboxCheckBoxFilters.Add(this.fastDeliveryBtn);
			global::Gtk.Box.BoxChild w15 = ((global::Gtk.Box.BoxChild)(this.hboxCheckBoxFilters[this.fastDeliveryBtn]));
			w15.Position = 5;
			w15.Expand = false;
			w15.Fill = false;
			this.table1.Add(this.hboxCheckBoxFilters);
			global::Gtk.Table.TableChild w16 = ((global::Gtk.Table.TableChild)(this.table1[this.hboxCheckBoxFilters]));
			w16.TopAttach = ((uint)(6));
			w16.BottomAttach = ((uint)(7));
			w16.RightAttach = ((uint)(7));
			w16.XOptions = ((global::Gtk.AttachOptions)(4));
			w16.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.hboxLastLineOfFilters = new global::Gtk.HBox();
			this.hboxLastLineOfFilters.Name = "hboxLastLineOfFilters";
			this.hboxLastLineOfFilters.Spacing = 6;
			// Container child hboxLastLineOfFilters.Gtk.Box+BoxChild
			this.ylabelOrderId = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelOrderId.Name = "ylabelOrderId";
			this.ylabelOrderId.Xalign = 1F;
			this.ylabelOrderId.LabelProp = global::Mono.Unix.Catalog.GetString("Номер заказа:");
			this.hboxLastLineOfFilters.Add(this.ylabelOrderId);
			global::Gtk.Box.BoxChild w17 = ((global::Gtk.Box.BoxChild)(this.hboxLastLineOfFilters[this.ylabelOrderId]));
			w17.Position = 0;
			w17.Expand = false;
			w17.Fill = false;
			// Container child hboxLastLineOfFilters.Gtk.Box+BoxChild
			this.entryOrderId = new global::QS.Widgets.ValidatedEntry();
			this.entryOrderId.CanFocus = true;
			this.entryOrderId.Name = "entryOrderId";
			this.entryOrderId.IsEditable = true;
			this.entryOrderId.InvisibleChar = '•';
			this.hboxLastLineOfFilters.Add(this.entryOrderId);
			global::Gtk.Box.BoxChild w18 = ((global::Gtk.Box.BoxChild)(this.hboxLastLineOfFilters[this.entryOrderId]));
			w18.Position = 1;
			w18.Expand = false;
			w18.Fill = false;
			// Container child hboxLastLineOfFilters.Gtk.Box+BoxChild
			this.ylblOnlineOrderId = new global::Gamma.GtkWidgets.yLabel();
			this.ylblOnlineOrderId.Name = "ylblOnlineOrderId";
			this.ylblOnlineOrderId.LabelProp = global::Mono.Unix.Catalog.GetString("Номер онлайн заказа:");
			this.hboxLastLineOfFilters.Add(this.ylblOnlineOrderId);
			global::Gtk.Box.BoxChild w19 = ((global::Gtk.Box.BoxChild)(this.hboxLastLineOfFilters[this.ylblOnlineOrderId]));
			w19.Position = 2;
			w19.Expand = false;
			w19.Fill = false;
			// Container child hboxLastLineOfFilters.Gtk.Box+BoxChild
			this.eOnlineOrderId = new global::QS.Widgets.ValidatedEntry();
			this.eOnlineOrderId.CanFocus = true;
			this.eOnlineOrderId.Name = "eOnlineOrderId";
			this.eOnlineOrderId.IsEditable = true;
			this.eOnlineOrderId.InvisibleChar = '•';
			this.hboxLastLineOfFilters.Add(this.eOnlineOrderId);
			global::Gtk.Box.BoxChild w20 = ((global::Gtk.Box.BoxChild)(this.hboxLastLineOfFilters[this.eOnlineOrderId]));
			w20.Position = 3;
			w20.Expand = false;
			w20.Fill = false;
			// Container child hboxLastLineOfFilters.Gtk.Box+BoxChild
			this.ylabelCounterpartyPhone = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelCounterpartyPhone.Name = "ylabelCounterpartyPhone";
			this.ylabelCounterpartyPhone.Xalign = 1F;
			this.ylabelCounterpartyPhone.LabelProp = global::Mono.Unix.Catalog.GetString("Телефон контрагента:");
			this.hboxLastLineOfFilters.Add(this.ylabelCounterpartyPhone);
			global::Gtk.Box.BoxChild w21 = ((global::Gtk.Box.BoxChild)(this.hboxLastLineOfFilters[this.ylabelCounterpartyPhone]));
			w21.Position = 4;
			w21.Expand = false;
			w21.Fill = false;
			// Container child hboxLastLineOfFilters.Gtk.Box+BoxChild
			this.entryCounterpartyPhone = new global::QS.Widgets.ValidatedEntry();
			this.entryCounterpartyPhone.TooltipMarkup = "Формат телефона: Без +7 и 8 в начале";
			this.entryCounterpartyPhone.CanFocus = true;
			this.entryCounterpartyPhone.Name = "entryCounterpartyPhone";
			this.entryCounterpartyPhone.IsEditable = true;
			this.entryCounterpartyPhone.InvisibleChar = '•';
			this.hboxLastLineOfFilters.Add(this.entryCounterpartyPhone);
			global::Gtk.Box.BoxChild w22 = ((global::Gtk.Box.BoxChild)(this.hboxLastLineOfFilters[this.entryCounterpartyPhone]));
			w22.Position = 5;
			w22.Expand = false;
			w22.Fill = false;
			this.table1.Add(this.hboxLastLineOfFilters);
			global::Gtk.Table.TableChild w23 = ((global::Gtk.Table.TableChild)(this.table1[this.hboxLastLineOfFilters]));
			w23.TopAttach = ((uint)(7));
			w23.BottomAttach = ((uint)(8));
			w23.RightAttach = ((uint)(7));
			w23.XOptions = ((global::Gtk.AttachOptions)(4));
			w23.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label1 = new global::Gtk.Label();
			this.label1.Name = "label1";
			this.label1.Xalign = 1F;
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString("Статус:");
			this.table1.Add(this.label1);
			global::Gtk.Table.TableChild w24 = ((global::Gtk.Table.TableChild)(this.table1[this.label1]));
			w24.TopAttach = ((uint)(2));
			w24.BottomAttach = ((uint)(3));
			w24.XOptions = ((global::Gtk.AttachOptions)(4));
			w24.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label2 = new global::Gtk.Label();
			this.label2.Name = "label2";
			this.label2.Xalign = 1F;
			this.label2.LabelProp = global::Mono.Unix.Catalog.GetString("Контрагент:");
			this.table1.Add(this.label2);
			global::Gtk.Table.TableChild w25 = ((global::Gtk.Table.TableChild)(this.table1[this.label2]));
			w25.TopAttach = ((uint)(3));
			w25.BottomAttach = ((uint)(4));
			w25.XOptions = ((global::Gtk.AttachOptions)(4));
			w25.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label3 = new global::Gtk.Label();
			this.label3.Name = "label3";
			this.label3.Xalign = 1F;
			this.label3.LabelProp = global::Mono.Unix.Catalog.GetString("Точка доставки:");
			this.table1.Add(this.label3);
			global::Gtk.Table.TableChild w26 = ((global::Gtk.Table.TableChild)(this.table1[this.label3]));
			w26.TopAttach = ((uint)(4));
			w26.BottomAttach = ((uint)(5));
			w26.LeftAttach = ((uint)(2));
			w26.RightAttach = ((uint)(3));
			w26.XOptions = ((global::Gtk.AttachOptions)(4));
			w26.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label5 = new global::Gtk.Label();
			this.label5.Name = "label5";
			this.label5.Xalign = 1F;
			this.label5.LabelProp = global::Mono.Unix.Catalog.GetString("Тип оплаты:");
			this.table1.Add(this.label5);
			global::Gtk.Table.TableChild w27 = ((global::Gtk.Table.TableChild)(this.table1[this.label5]));
			w27.TopAttach = ((uint)(2));
			w27.BottomAttach = ((uint)(3));
			w27.LeftAttach = ((uint)(2));
			w27.RightAttach = ((uint)(3));
			w27.XOptions = ((global::Gtk.AttachOptions)(4));
			w27.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label7 = new global::Gtk.Label();
			this.label7.Name = "label7";
			this.label7.Xalign = 1F;
			this.label7.LabelProp = global::Mono.Unix.Catalog.GetString("Район города:");
			this.table1.Add(this.label7);
			global::Gtk.Table.TableChild w28 = ((global::Gtk.Table.TableChild)(this.table1[this.label7]));
			w28.TopAttach = ((uint)(5));
			w28.BottomAttach = ((uint)(6));
			w28.LeftAttach = ((uint)(2));
			w28.RightAttach = ((uint)(3));
			w28.XOptions = ((global::Gtk.AttachOptions)(4));
			w28.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.lblEmployeeWorkWith = new global::Gtk.Label();
			this.lblEmployeeWorkWith.Name = "lblEmployeeWorkWith";
			this.lblEmployeeWorkWith.Xalign = 1F;
			this.lblEmployeeWorkWith.LabelProp = global::Mono.Unix.Catalog.GetString("В работе у:");
			this.table1.Add(this.lblEmployeeWorkWith);
			global::Gtk.Table.TableChild w29 = ((global::Gtk.Table.TableChild)(this.table1[this.lblEmployeeWorkWith]));
			w29.TopAttach = ((uint)(4));
			w29.BottomAttach = ((uint)(5));
			w29.XOptions = ((global::Gtk.AttachOptions)(4));
			w29.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.lblEntityType = new global::Gamma.GtkWidgets.yLabel();
			this.lblEntityType.Name = "lblEntityType";
			this.lblEntityType.Xalign = 1F;
			this.lblEntityType.LabelProp = global::Mono.Unix.Catalog.GetString("Отображать:");
			this.table1.Add(this.lblEntityType);
			global::Gtk.Table.TableChild w30 = ((global::Gtk.Table.TableChild)(this.table1[this.lblEntityType]));
			w30.XOptions = ((global::Gtk.AttachOptions)(4));
			w30.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.lblSource = new global::Gtk.Label();
			this.lblSource.Name = "lblSource";
			this.lblSource.Xalign = 1F;
			this.lblSource.LabelProp = global::Mono.Unix.Catalog.GetString("Откуда заказ:");
			this.table1.Add(this.lblSource);
			global::Gtk.Table.TableChild w31 = ((global::Gtk.Table.TableChild)(this.table1[this.lblSource]));
			w31.TopAttach = ((uint)(1));
			w31.BottomAttach = ((uint)(2));
			w31.XOptions = ((global::Gtk.AttachOptions)(4));
			w31.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.speciallistCmbPaymentsFrom = new global::QS.Widgets.GtkUI.SpecialListComboBox();
			this.speciallistCmbPaymentsFrom.Name = "speciallistCmbPaymentsFrom";
			this.speciallistCmbPaymentsFrom.AddIfNotExist = false;
			this.speciallistCmbPaymentsFrom.DefaultFirst = false;
			this.speciallistCmbPaymentsFrom.ShowSpecialStateAll = true;
			this.speciallistCmbPaymentsFrom.ShowSpecialStateNot = false;
			this.table1.Add(this.speciallistCmbPaymentsFrom);
			global::Gtk.Table.TableChild w32 = ((global::Gtk.Table.TableChild)(this.table1[this.speciallistCmbPaymentsFrom]));
			w32.TopAttach = ((uint)(3));
			w32.BottomAttach = ((uint)(4));
			w32.LeftAttach = ((uint)(3));
			w32.RightAttach = ((uint)(4));
			w32.XOptions = ((global::Gtk.AttachOptions)(4));
			w32.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.yenumcomboboxDateType = new global::Gamma.Widgets.yEnumComboBox();
			this.yenumcomboboxDateType.Name = "yenumcomboboxDateType";
			this.yenumcomboboxDateType.ShowSpecialStateAll = false;
			this.yenumcomboboxDateType.ShowSpecialStateNot = false;
			this.yenumcomboboxDateType.UseShortTitle = false;
			this.yenumcomboboxDateType.DefaultFirst = false;
			this.table1.Add(this.yenumcomboboxDateType);
			global::Gtk.Table.TableChild w33 = ((global::Gtk.Table.TableChild)(this.table1[this.yenumcomboboxDateType]));
			w33.TopAttach = ((uint)(1));
			w33.BottomAttach = ((uint)(2));
			w33.LeftAttach = ((uint)(4));
			w33.RightAttach = ((uint)(5));
			w33.XOptions = ((global::Gtk.AttachOptions)(4));
			w33.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.yenumСmbboxOrderPaymentStatus = new global::Gamma.Widgets.yEnumComboBox();
			this.yenumСmbboxOrderPaymentStatus.Name = "yenumСmbboxOrderPaymentStatus";
			this.yenumСmbboxOrderPaymentStatus.ShowSpecialStateAll = true;
			this.yenumСmbboxOrderPaymentStatus.ShowSpecialStateNot = false;
			this.yenumСmbboxOrderPaymentStatus.UseShortTitle = false;
			this.yenumСmbboxOrderPaymentStatus.DefaultFirst = false;
			this.table1.Add(this.yenumСmbboxOrderPaymentStatus);
			global::Gtk.Table.TableChild w34 = ((global::Gtk.Table.TableChild)(this.table1[this.yenumСmbboxOrderPaymentStatus]));
			w34.TopAttach = ((uint)(1));
			w34.BottomAttach = ((uint)(2));
			w34.LeftAttach = ((uint)(3));
			w34.RightAttach = ((uint)(4));
			w34.XOptions = ((global::Gtk.AttachOptions)(4));
			w34.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabel2 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel2.Name = "ylabel2";
			this.ylabel2.Xalign = 1F;
			this.ylabel2.LabelProp = global::Mono.Unix.Catalog.GetString("Статус оплаты: ");
			this.table1.Add(this.ylabel2);
			global::Gtk.Table.TableChild w35 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabel2]));
			w35.TopAttach = ((uint)(1));
			w35.BottomAttach = ((uint)(2));
			w35.LeftAttach = ((uint)(2));
			w35.RightAttach = ((uint)(3));
			w35.XOptions = ((global::Gtk.AttachOptions)(4));
			w35.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylblPaymentFrom = new global::Gamma.GtkWidgets.yLabel();
			this.ylblPaymentFrom.Name = "ylblPaymentFrom";
			this.ylblPaymentFrom.Xalign = 1F;
			this.ylblPaymentFrom.LabelProp = global::Mono.Unix.Catalog.GetString("Откуда оплата:");
			this.table1.Add(this.ylblPaymentFrom);
			global::Gtk.Table.TableChild w36 = ((global::Gtk.Table.TableChild)(this.table1[this.ylblPaymentFrom]));
			w36.TopAttach = ((uint)(3));
			w36.BottomAttach = ((uint)(4));
			w36.LeftAttach = ((uint)(2));
			w36.RightAttach = ((uint)(3));
			w36.XOptions = ((global::Gtk.AttachOptions)(4));
			w36.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ySpecCmbGeographicGroup = new global::Gamma.Widgets.ySpecComboBox();
			this.ySpecCmbGeographicGroup.Name = "ySpecCmbGeographicGroup";
			this.ySpecCmbGeographicGroup.AddIfNotExist = false;
			this.ySpecCmbGeographicGroup.DefaultFirst = false;
			this.ySpecCmbGeographicGroup.ShowSpecialStateAll = true;
			this.ySpecCmbGeographicGroup.ShowSpecialStateNot = false;
			this.table1.Add(this.ySpecCmbGeographicGroup);
			global::Gtk.Table.TableChild w37 = ((global::Gtk.Table.TableChild)(this.table1[this.ySpecCmbGeographicGroup]));
			w37.TopAttach = ((uint)(5));
			w37.BottomAttach = ((uint)(6));
			w37.LeftAttach = ((uint)(3));
			w37.RightAttach = ((uint)(4));
			w37.XOptions = ((global::Gtk.AttachOptions)(4));
			w37.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.yvboxAdditionalFilters = new global::Gamma.GtkWidgets.yVBox();
			this.yvboxAdditionalFilters.Name = "yvboxAdditionalFilters";
			this.yvboxAdditionalFilters.Spacing = 6;
			// Container child yvboxAdditionalFilters.Gtk.Box+BoxChild
			this.hboxAdditionalFilters = new global::Gtk.HBox();
			this.hboxAdditionalFilters.Name = "hboxAdditionalFilters";
			this.hboxAdditionalFilters.Spacing = 6;
			// Container child hboxAdditionalFilters.Gtk.Box+BoxChild
			this.lblentryCounteragentNameLike = new global::Gtk.Label();
			this.lblentryCounteragentNameLike.Name = "lblentryCounteragentNameLike";
			this.lblentryCounteragentNameLike.LabelProp = global::Mono.Unix.Catalog.GetString("Название контрагента:");
			this.hboxAdditionalFilters.Add(this.lblentryCounteragentNameLike);
			global::Gtk.Box.BoxChild w38 = ((global::Gtk.Box.BoxChild)(this.hboxAdditionalFilters[this.lblentryCounteragentNameLike]));
			w38.Position = 0;
			w38.Expand = false;
			w38.Fill = false;
			// Container child hboxAdditionalFilters.Gtk.Box+BoxChild
			this.entryCounteragentNameLike = new global::QS.Widgets.ValidatedEntry();
			this.entryCounteragentNameLike.CanFocus = true;
			this.entryCounteragentNameLike.Name = "entryCounteragentNameLike";
			this.entryCounteragentNameLike.IsEditable = true;
			this.entryCounteragentNameLike.InvisibleChar = '•';
			this.hboxAdditionalFilters.Add(this.entryCounteragentNameLike);
			global::Gtk.Box.BoxChild w39 = ((global::Gtk.Box.BoxChild)(this.hboxAdditionalFilters[this.entryCounteragentNameLike]));
			w39.Position = 1;
			// Container child hboxAdditionalFilters.Gtk.Box+BoxChild
			this.lblInn = new global::Gtk.Label();
			this.lblInn.Name = "lblInn";
			this.lblInn.LabelProp = global::Mono.Unix.Catalog.GetString("ИНН контрагента:");
			this.hboxAdditionalFilters.Add(this.lblInn);
			global::Gtk.Box.BoxChild w40 = ((global::Gtk.Box.BoxChild)(this.hboxAdditionalFilters[this.lblInn]));
			w40.Position = 2;
			w40.Expand = false;
			w40.Fill = false;
			// Container child hboxAdditionalFilters.Gtk.Box+BoxChild
			this.entryInn = new global::QS.Widgets.ValidatedEntry();
			this.entryInn.CanFocus = true;
			this.entryInn.Name = "entryInn";
			this.entryInn.IsEditable = true;
			this.entryInn.InvisibleChar = '•';
			this.hboxAdditionalFilters.Add(this.entryInn);
			global::Gtk.Box.BoxChild w41 = ((global::Gtk.Box.BoxChild)(this.hboxAdditionalFilters[this.entryInn]));
			w41.Position = 3;
			w41.Expand = false;
			this.yvboxAdditionalFilters.Add(this.hboxAdditionalFilters);
			global::Gtk.Box.BoxChild w42 = ((global::Gtk.Box.BoxChild)(this.yvboxAdditionalFilters[this.hboxAdditionalFilters]));
			w42.Position = 0;
			w42.Expand = false;
			w42.Fill = false;
			// Container child yvboxAdditionalFilters.Gtk.Box+BoxChild
			this.yhboxSearchByAddressMain = new global::Gamma.GtkWidgets.yHBox();
			this.yhboxSearchByAddressMain.Name = "yhboxSearchByAddressMain";
			this.yhboxSearchByAddressMain.Spacing = 6;
			// Container child yhboxSearchByAddressMain.Gtk.Box+BoxChild
			this.lblDeliveryPointAddress = new global::Gtk.Label();
			this.lblDeliveryPointAddress.Name = "lblDeliveryPointAddress";
			this.lblDeliveryPointAddress.LabelProp = global::Mono.Unix.Catalog.GetString("Адрес точки доставки:");
			this.yhboxSearchByAddressMain.Add(this.lblDeliveryPointAddress);
			global::Gtk.Box.BoxChild w43 = ((global::Gtk.Box.BoxChild)(this.yhboxSearchByAddressMain[this.lblDeliveryPointAddress]));
			w43.Position = 0;
			w43.Expand = false;
			w43.Fill = false;
			// Container child yhboxSearchByAddressMain.Gtk.Box+BoxChild
			this.yhboxSearchByAddress = new global::Gamma.GtkWidgets.yHBox();
			this.yhboxSearchByAddress.Name = "yhboxSearchByAddress";
			this.yhboxSearchByAddress.Spacing = 6;
			this.yhboxSearchByAddressMain.Add(this.yhboxSearchByAddress);
			global::Gtk.Box.BoxChild w44 = ((global::Gtk.Box.BoxChild)(this.yhboxSearchByAddressMain[this.yhboxSearchByAddress]));
			w44.Position = 1;
			this.yvboxAdditionalFilters.Add(this.yhboxSearchByAddressMain);
			global::Gtk.Box.BoxChild w45 = ((global::Gtk.Box.BoxChild)(this.yvboxAdditionalFilters[this.yhboxSearchByAddressMain]));
			w45.Position = 1;
			w45.Expand = false;
			w45.Fill = false;
			this.table1.Add(this.yvboxAdditionalFilters);
			global::Gtk.Table.TableChild w46 = ((global::Gtk.Table.TableChild)(this.table1[this.yvboxAdditionalFilters]));
			w46.TopAttach = ((uint)(8));
			w46.BottomAttach = ((uint)(9));
			w46.RightAttach = ((uint)(7));
			w46.XOptions = ((global::Gtk.AttachOptions)(4));
			w1.Add(this.table1);
			this.scrolledwindow1.Add(w1);
			this.Add(this.scrolledwindow1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}

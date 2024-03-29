
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.Views.Orders
{
	public partial class PromotionalSetView
	{
		private global::Gamma.GtkWidgets.yVBox vboxMain;

		private global::Gamma.GtkWidgets.yHBox hboxHandleBtns;

		private global::Gamma.GtkWidgets.yButton btnSave;

		private global::Gamma.GtkWidgets.yButton btnCancel;

		private global::Gtk.VSeparator vseparator6;

		private global::Gamma.GtkWidgets.yRadioButton radioBtnInformation;

		private global::Gamma.GtkWidgets.yRadioButton radioBtnSitesAndApps;

		private global::Gamma.GtkWidgets.yNotebook notebook;

		private global::Gtk.VBox vbox2;

		private global::Gtk.HBox hbox1;

		private global::Gtk.Label lblCreationDateTitle;

		private global::Gamma.GtkWidgets.yLabel ylblCreationDate;

		private global::Gtk.VSeparator vseparator2;

		private global::Gamma.GtkWidgets.yCheckButton yChkIsArchive;

		private global::Gtk.Table table2;

		private global::Gtk.Label labelPromoSetName;

		private global::Gtk.Label lblName;

		private global::Gtk.VSeparator vseparator3;

		private global::Gtk.VSeparator vseparator4;

		private global::Vodovoz.Core.WidgetContainerView widgetcontainerview;

		private global::Gamma.GtkWidgets.yEntry yentryDiscountReason;

		private global::Gamma.GtkWidgets.yEntry yentryPromotionalSetName;

		private global::QS.Widgets.EnumMenuButton yEnumButtonAddAction;

		private global::Gtk.HBox hbox5;

		private global::Gamma.GtkWidgets.yCheckButton ycheckbCanEditNomCount;

		private global::Gtk.VSeparator vseparator5;

		private global::Gamma.GtkWidgets.yCheckButton chkPromoSetForNewClients;

		private global::Gamma.GtkWidgets.yHBox hboxSpecialBottlesCountForDeliveryPrice;

		private global::Gamma.GtkWidgets.yCheckButton chkBtnShowSpecialBottlesCountForDeliveryPrice;

		private global::Gamma.GtkWidgets.yEntry entrySpecialBottlesCountForDeliveryPrice;

		private global::Gtk.HBox hbox3;

		private global::Gtk.Frame frame1;

		private global::Gtk.Alignment GtkAlignment3;

		private global::Gtk.VBox vbxNomenclatures;

		private global::Gtk.ScrolledWindow GtkScrolledWindow;

		private global::Gamma.GtkWidgets.yTreeView yTreePromoSetItems;

		private global::Gtk.HBox hbox2;

		private global::Gamma.GtkWidgets.yButton ybtnAddNomenclature;

		private global::Gamma.GtkWidgets.yButton ybtnRemoveNomenclature;

		private global::Gtk.Label lblNomTblTitle;

		private global::Gtk.Frame frame2;

		private global::Gtk.Alignment GtkAlignment4;

		private global::Gtk.VBox vbxNomenclatures1;

		private global::Gtk.ScrolledWindow GtkScrolledWindow1;

		private global::Gamma.GtkWidgets.yTreeView yTreeActionsItems;

		private global::Gtk.HBox hbox4;

		private global::Gamma.GtkWidgets.yButton ybtnRemoveAction;

		private global::Gtk.Label lblActionsTblTitle;

		private global::Gtk.Label lblInformation;

		private global::Gamma.GtkWidgets.yVBox vboxSitesAndApps;

		private global::Gamma.GtkWidgets.yTable tableOnlineParameters;

		private global::Gamma.GtkWidgets.yEntry entryOnlineName;

		private global::Gamma.Widgets.yEnumComboBox enumCmbOnlineAvailabilityKulerSaleWebSite;

		private global::Gamma.Widgets.yEnumComboBox enumCmbOnlineAvailabilityMobileApp;

		private global::Gamma.Widgets.yEnumComboBox enumCmbOnlineAvailabilityVodovozWebSite;

		private global::Gtk.HSeparator hseparator;

		private global::Gamma.GtkWidgets.yLabel lblAppTitle;

		private global::Gamma.GtkWidgets.yLabel lblErpId;

		private global::Gamma.GtkWidgets.yLabel lblErpIdTitle;

		private global::Gamma.GtkWidgets.yLabel lblKulerSaleWebSiteTitle;

		private global::Gamma.GtkWidgets.yLabel lblOnlineAvailability;

		private global::Gamma.GtkWidgets.yLabel lblOnlineNameTitle;

		private global::Gamma.GtkWidgets.yLabel lblVodovozWebSiteTitle;

		private global::Gtk.Label lblSitesAndApps;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.Views.Orders.PromotionalSetView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.Views.Orders.PromotionalSetView";
			// Container child Vodovoz.Views.Orders.PromotionalSetView.Gtk.Container+ContainerChild
			this.vboxMain = new global::Gamma.GtkWidgets.yVBox();
			this.vboxMain.Name = "vboxMain";
			this.vboxMain.Spacing = 6;
			// Container child vboxMain.Gtk.Box+BoxChild
			this.hboxHandleBtns = new global::Gamma.GtkWidgets.yHBox();
			this.hboxHandleBtns.Name = "hboxHandleBtns";
			this.hboxHandleBtns.Spacing = 6;
			// Container child hboxHandleBtns.Gtk.Box+BoxChild
			this.btnSave = new global::Gamma.GtkWidgets.yButton();
			this.btnSave.CanFocus = true;
			this.btnSave.Name = "btnSave";
			this.btnSave.UseUnderline = true;
			this.btnSave.Label = global::Mono.Unix.Catalog.GetString("Сохранить");
			this.hboxHandleBtns.Add(this.btnSave);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.hboxHandleBtns[this.btnSave]));
			w1.Position = 0;
			w1.Expand = false;
			w1.Fill = false;
			// Container child hboxHandleBtns.Gtk.Box+BoxChild
			this.btnCancel = new global::Gamma.GtkWidgets.yButton();
			this.btnCancel.CanFocus = true;
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.UseUnderline = true;
			this.btnCancel.Label = global::Mono.Unix.Catalog.GetString("Отменить");
			this.hboxHandleBtns.Add(this.btnCancel);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hboxHandleBtns[this.btnCancel]));
			w2.Position = 1;
			w2.Expand = false;
			w2.Fill = false;
			// Container child hboxHandleBtns.Gtk.Box+BoxChild
			this.vseparator6 = new global::Gtk.VSeparator();
			this.vseparator6.Name = "vseparator6";
			this.hboxHandleBtns.Add(this.vseparator6);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.hboxHandleBtns[this.vseparator6]));
			w3.Position = 2;
			w3.Expand = false;
			w3.Fill = false;
			// Container child hboxHandleBtns.Gtk.Box+BoxChild
			this.radioBtnInformation = new global::Gamma.GtkWidgets.yRadioButton();
			this.radioBtnInformation.CanFocus = true;
			this.radioBtnInformation.Name = "radioBtnInformation";
			this.radioBtnInformation.Label = global::Mono.Unix.Catalog.GetString("Информация");
			this.radioBtnInformation.DrawIndicator = false;
			this.radioBtnInformation.UseUnderline = true;
			this.radioBtnInformation.Group = new global::GLib.SList(global::System.IntPtr.Zero);
			this.hboxHandleBtns.Add(this.radioBtnInformation);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hboxHandleBtns[this.radioBtnInformation]));
			w4.Position = 3;
			w4.Expand = false;
			w4.Fill = false;
			// Container child hboxHandleBtns.Gtk.Box+BoxChild
			this.radioBtnSitesAndApps = new global::Gamma.GtkWidgets.yRadioButton();
			this.radioBtnSitesAndApps.CanFocus = true;
			this.radioBtnSitesAndApps.Name = "radioBtnSitesAndApps";
			this.radioBtnSitesAndApps.Label = global::Mono.Unix.Catalog.GetString("Сайты и приложения");
			this.radioBtnSitesAndApps.DrawIndicator = false;
			this.radioBtnSitesAndApps.UseUnderline = true;
			this.radioBtnSitesAndApps.Group = this.radioBtnInformation.Group;
			this.hboxHandleBtns.Add(this.radioBtnSitesAndApps);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.hboxHandleBtns[this.radioBtnSitesAndApps]));
			w5.Position = 4;
			w5.Expand = false;
			w5.Fill = false;
			this.vboxMain.Add(this.hboxHandleBtns);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.vboxMain[this.hboxHandleBtns]));
			w6.Position = 0;
			w6.Expand = false;
			w6.Fill = false;
			// Container child vboxMain.Gtk.Box+BoxChild
			this.notebook = new global::Gamma.GtkWidgets.yNotebook();
			this.notebook.CanFocus = true;
			this.notebook.Name = "notebook";
			this.notebook.CurrentPage = 0;
			// Container child notebook.Gtk.Notebook+NotebookChild
			this.vbox2 = new global::Gtk.VBox();
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hbox1 = new global::Gtk.HBox();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.lblCreationDateTitle = new global::Gtk.Label();
			this.lblCreationDateTitle.Name = "lblCreationDateTitle";
			this.lblCreationDateTitle.LabelProp = global::Mono.Unix.Catalog.GetString("Создан:");
			this.hbox1.Add(this.lblCreationDateTitle);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.lblCreationDateTitle]));
			w7.Position = 0;
			w7.Expand = false;
			w7.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.ylblCreationDate = new global::Gamma.GtkWidgets.yLabel();
			this.ylblCreationDate.Name = "ylblCreationDate";
			this.ylblCreationDate.LabelProp = global::Mono.Unix.Catalog.GetString("Дата создания");
			this.hbox1.Add(this.ylblCreationDate);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.ylblCreationDate]));
			w8.Position = 1;
			w8.Expand = false;
			w8.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.vseparator2 = new global::Gtk.VSeparator();
			this.vseparator2.Name = "vseparator2";
			this.hbox1.Add(this.vseparator2);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.vseparator2]));
			w9.Position = 2;
			w9.Expand = false;
			w9.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.yChkIsArchive = new global::Gamma.GtkWidgets.yCheckButton();
			this.yChkIsArchive.CanFocus = true;
			this.yChkIsArchive.Name = "yChkIsArchive";
			this.yChkIsArchive.Label = global::Mono.Unix.Catalog.GetString("Архивный");
			this.yChkIsArchive.DrawIndicator = true;
			this.yChkIsArchive.UseUnderline = true;
			this.hbox1.Add(this.yChkIsArchive);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.yChkIsArchive]));
			w10.Position = 3;
			w10.Expand = false;
			w10.Fill = false;
			this.vbox2.Add(this.hbox1);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.hbox1]));
			w11.Position = 0;
			w11.Expand = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.table2 = new global::Gtk.Table(((uint)(2)), ((uint)(4)), false);
			this.table2.Name = "table2";
			this.table2.RowSpacing = ((uint)(6));
			this.table2.ColumnSpacing = ((uint)(6));
			// Container child table2.Gtk.Table+TableChild
			this.labelPromoSetName = new global::Gtk.Label();
			this.labelPromoSetName.Name = "labelPromoSetName";
			this.labelPromoSetName.Xalign = 1F;
			this.labelPromoSetName.LabelProp = global::Mono.Unix.Catalog.GetString("Название набора:");
			this.table2.Add(this.labelPromoSetName);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.table2[this.labelPromoSetName]));
			w12.TopAttach = ((uint)(1));
			w12.BottomAttach = ((uint)(2));
			w12.XOptions = ((global::Gtk.AttachOptions)(4));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.lblName = new global::Gtk.Label();
			this.lblName.Name = "lblName";
			this.lblName.Xalign = 1F;
			this.lblName.LabelProp = global::Mono.Unix.Catalog.GetString("Согласованная акция:");
			this.table2.Add(this.lblName);
			global::Gtk.Table.TableChild w13 = ((global::Gtk.Table.TableChild)(this.table2[this.lblName]));
			w13.XOptions = ((global::Gtk.AttachOptions)(4));
			w13.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.vseparator3 = new global::Gtk.VSeparator();
			this.vseparator3.Name = "vseparator3";
			this.table2.Add(this.vseparator3);
			global::Gtk.Table.TableChild w14 = ((global::Gtk.Table.TableChild)(this.table2[this.vseparator3]));
			w14.LeftAttach = ((uint)(2));
			w14.RightAttach = ((uint)(3));
			w14.XOptions = ((global::Gtk.AttachOptions)(4));
			w14.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.vseparator4 = new global::Gtk.VSeparator();
			this.vseparator4.Name = "vseparator4";
			this.table2.Add(this.vseparator4);
			global::Gtk.Table.TableChild w15 = ((global::Gtk.Table.TableChild)(this.table2[this.vseparator4]));
			w15.TopAttach = ((uint)(1));
			w15.BottomAttach = ((uint)(2));
			w15.LeftAttach = ((uint)(2));
			w15.RightAttach = ((uint)(3));
			w15.XOptions = ((global::Gtk.AttachOptions)(4));
			w15.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.widgetcontainerview = new global::Vodovoz.Core.WidgetContainerView();
			this.widgetcontainerview.Events = ((global::Gdk.EventMask)(256));
			this.widgetcontainerview.Name = "widgetcontainerview";
			this.table2.Add(this.widgetcontainerview);
			global::Gtk.Table.TableChild w16 = ((global::Gtk.Table.TableChild)(this.table2[this.widgetcontainerview]));
			w16.TopAttach = ((uint)(1));
			w16.BottomAttach = ((uint)(2));
			w16.LeftAttach = ((uint)(3));
			w16.RightAttach = ((uint)(4));
			w16.XOptions = ((global::Gtk.AttachOptions)(4));
			w16.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.yentryDiscountReason = new global::Gamma.GtkWidgets.yEntry();
			this.yentryDiscountReason.CanFocus = true;
			this.yentryDiscountReason.Name = "yentryDiscountReason";
			this.yentryDiscountReason.IsEditable = true;
			this.yentryDiscountReason.InvisibleChar = '•';
			this.table2.Add(this.yentryDiscountReason);
			global::Gtk.Table.TableChild w17 = ((global::Gtk.Table.TableChild)(this.table2[this.yentryDiscountReason]));
			w17.LeftAttach = ((uint)(1));
			w17.RightAttach = ((uint)(2));
			w17.XOptions = ((global::Gtk.AttachOptions)(4));
			w17.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.yentryPromotionalSetName = new global::Gamma.GtkWidgets.yEntry();
			this.yentryPromotionalSetName.CanFocus = true;
			this.yentryPromotionalSetName.Name = "yentryPromotionalSetName";
			this.yentryPromotionalSetName.IsEditable = true;
			this.yentryPromotionalSetName.InvisibleChar = '•';
			this.table2.Add(this.yentryPromotionalSetName);
			global::Gtk.Table.TableChild w18 = ((global::Gtk.Table.TableChild)(this.table2[this.yentryPromotionalSetName]));
			w18.TopAttach = ((uint)(1));
			w18.BottomAttach = ((uint)(2));
			w18.LeftAttach = ((uint)(1));
			w18.RightAttach = ((uint)(2));
			w18.XOptions = ((global::Gtk.AttachOptions)(4));
			w18.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.yEnumButtonAddAction = new global::QS.Widgets.EnumMenuButton();
			this.yEnumButtonAddAction.CanFocus = true;
			this.yEnumButtonAddAction.Name = "yEnumButtonAddAction";
			this.yEnumButtonAddAction.UseUnderline = true;
			this.yEnumButtonAddAction.UseMarkup = false;
			this.yEnumButtonAddAction.LabelXAlign = 0F;
			this.yEnumButtonAddAction.Label = global::Mono.Unix.Catalog.GetString("Добавить действие");
			global::Gtk.Image w19 = new global::Gtk.Image();
			w19.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-add", global::Gtk.IconSize.Menu);
			this.yEnumButtonAddAction.Image = w19;
			this.table2.Add(this.yEnumButtonAddAction);
			global::Gtk.Table.TableChild w20 = ((global::Gtk.Table.TableChild)(this.table2[this.yEnumButtonAddAction]));
			w20.LeftAttach = ((uint)(3));
			w20.RightAttach = ((uint)(4));
			w20.XOptions = ((global::Gtk.AttachOptions)(4));
			w20.YOptions = ((global::Gtk.AttachOptions)(4));
			this.vbox2.Add(this.table2);
			global::Gtk.Box.BoxChild w21 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.table2]));
			w21.Position = 1;
			w21.Expand = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hbox5 = new global::Gtk.HBox();
			this.hbox5.Name = "hbox5";
			this.hbox5.Spacing = 6;
			// Container child hbox5.Gtk.Box+BoxChild
			this.ycheckbCanEditNomCount = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckbCanEditNomCount.CanFocus = true;
			this.ycheckbCanEditNomCount.Name = "ycheckbCanEditNomCount";
			this.ycheckbCanEditNomCount.Label = global::Mono.Unix.Catalog.GetString("Можно менять количество номенклатур в заказе");
			this.ycheckbCanEditNomCount.DrawIndicator = true;
			this.ycheckbCanEditNomCount.UseUnderline = true;
			this.hbox5.Add(this.ycheckbCanEditNomCount);
			global::Gtk.Box.BoxChild w22 = ((global::Gtk.Box.BoxChild)(this.hbox5[this.ycheckbCanEditNomCount]));
			w22.Position = 0;
			w22.Expand = false;
			w22.Fill = false;
			// Container child hbox5.Gtk.Box+BoxChild
			this.vseparator5 = new global::Gtk.VSeparator();
			this.vseparator5.Name = "vseparator5";
			this.hbox5.Add(this.vseparator5);
			global::Gtk.Box.BoxChild w23 = ((global::Gtk.Box.BoxChild)(this.hbox5[this.vseparator5]));
			w23.Position = 1;
			w23.Expand = false;
			w23.Fill = false;
			// Container child hbox5.Gtk.Box+BoxChild
			this.chkPromoSetForNewClients = new global::Gamma.GtkWidgets.yCheckButton();
			this.chkPromoSetForNewClients.CanFocus = true;
			this.chkPromoSetForNewClients.Name = "chkPromoSetForNewClients";
			this.chkPromoSetForNewClients.Label = global::Mono.Unix.Catalog.GetString("Промонабор для новых клиентов");
			this.chkPromoSetForNewClients.DrawIndicator = true;
			this.chkPromoSetForNewClients.UseUnderline = true;
			this.hbox5.Add(this.chkPromoSetForNewClients);
			global::Gtk.Box.BoxChild w24 = ((global::Gtk.Box.BoxChild)(this.hbox5[this.chkPromoSetForNewClients]));
			w24.Position = 2;
			w24.Expand = false;
			w24.Fill = false;
			this.vbox2.Add(this.hbox5);
			global::Gtk.Box.BoxChild w25 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.hbox5]));
			w25.Position = 2;
			w25.Expand = false;
			w25.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hboxSpecialBottlesCountForDeliveryPrice = new global::Gamma.GtkWidgets.yHBox();
			this.hboxSpecialBottlesCountForDeliveryPrice.Name = "hboxSpecialBottlesCountForDeliveryPrice";
			this.hboxSpecialBottlesCountForDeliveryPrice.Spacing = 6;
			// Container child hboxSpecialBottlesCountForDeliveryPrice.Gtk.Box+BoxChild
			this.chkBtnShowSpecialBottlesCountForDeliveryPrice = new global::Gamma.GtkWidgets.yCheckButton();
			this.chkBtnShowSpecialBottlesCountForDeliveryPrice.CanFocus = true;
			this.chkBtnShowSpecialBottlesCountForDeliveryPrice.Name = "chkBtnShowSpecialBottlesCountForDeliveryPrice";
			this.chkBtnShowSpecialBottlesCountForDeliveryPrice.Label = global::Mono.Unix.Catalog.GetString("Особое количество бутылей для расчета платной доставки");
			this.chkBtnShowSpecialBottlesCountForDeliveryPrice.DrawIndicator = true;
			this.chkBtnShowSpecialBottlesCountForDeliveryPrice.UseUnderline = true;
			this.hboxSpecialBottlesCountForDeliveryPrice.Add(this.chkBtnShowSpecialBottlesCountForDeliveryPrice);
			global::Gtk.Box.BoxChild w26 = ((global::Gtk.Box.BoxChild)(this.hboxSpecialBottlesCountForDeliveryPrice[this.chkBtnShowSpecialBottlesCountForDeliveryPrice]));
			w26.Position = 0;
			w26.Expand = false;
			w26.Fill = false;
			// Container child hboxSpecialBottlesCountForDeliveryPrice.Gtk.Box+BoxChild
			this.entrySpecialBottlesCountForDeliveryPrice = new global::Gamma.GtkWidgets.yEntry();
			this.entrySpecialBottlesCountForDeliveryPrice.CanFocus = true;
			this.entrySpecialBottlesCountForDeliveryPrice.Name = "entrySpecialBottlesCountForDeliveryPrice";
			this.entrySpecialBottlesCountForDeliveryPrice.IsEditable = true;
			this.entrySpecialBottlesCountForDeliveryPrice.InvisibleChar = '•';
			this.hboxSpecialBottlesCountForDeliveryPrice.Add(this.entrySpecialBottlesCountForDeliveryPrice);
			global::Gtk.Box.BoxChild w27 = ((global::Gtk.Box.BoxChild)(this.hboxSpecialBottlesCountForDeliveryPrice[this.entrySpecialBottlesCountForDeliveryPrice]));
			w27.Position = 1;
			w27.Expand = false;
			w27.Fill = false;
			this.vbox2.Add(this.hboxSpecialBottlesCountForDeliveryPrice);
			global::Gtk.Box.BoxChild w28 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.hboxSpecialBottlesCountForDeliveryPrice]));
			w28.Position = 3;
			w28.Expand = false;
			w28.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hbox3 = new global::Gtk.HBox();
			this.hbox3.Name = "hbox3";
			this.hbox3.Spacing = 6;
			// Container child hbox3.Gtk.Box+BoxChild
			this.frame1 = new global::Gtk.Frame();
			this.frame1.Name = "frame1";
			this.frame1.ShadowType = ((global::Gtk.ShadowType)(0));
			// Container child frame1.Gtk.Container+ContainerChild
			this.GtkAlignment3 = new global::Gtk.Alignment(0F, 0F, 1F, 1F);
			this.GtkAlignment3.Name = "GtkAlignment3";
			this.GtkAlignment3.LeftPadding = ((uint)(12));
			// Container child GtkAlignment3.Gtk.Container+ContainerChild
			this.vbxNomenclatures = new global::Gtk.VBox();
			this.vbxNomenclatures.Name = "vbxNomenclatures";
			this.vbxNomenclatures.Spacing = 6;
			// Container child vbxNomenclatures.Gtk.Box+BoxChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.yTreePromoSetItems = new global::Gamma.GtkWidgets.yTreeView();
			this.yTreePromoSetItems.CanFocus = true;
			this.yTreePromoSetItems.Name = "yTreePromoSetItems";
			this.GtkScrolledWindow.Add(this.yTreePromoSetItems);
			this.vbxNomenclatures.Add(this.GtkScrolledWindow);
			global::Gtk.Box.BoxChild w30 = ((global::Gtk.Box.BoxChild)(this.vbxNomenclatures[this.GtkScrolledWindow]));
			w30.Position = 0;
			// Container child vbxNomenclatures.Gtk.Box+BoxChild
			this.hbox2 = new global::Gtk.HBox();
			this.hbox2.Name = "hbox2";
			this.hbox2.Spacing = 6;
			// Container child hbox2.Gtk.Box+BoxChild
			this.ybtnAddNomenclature = new global::Gamma.GtkWidgets.yButton();
			this.ybtnAddNomenclature.CanFocus = true;
			this.ybtnAddNomenclature.Name = "ybtnAddNomenclature";
			this.ybtnAddNomenclature.UseUnderline = true;
			this.ybtnAddNomenclature.Label = global::Mono.Unix.Catalog.GetString("Добавить");
			this.hbox2.Add(this.ybtnAddNomenclature);
			global::Gtk.Box.BoxChild w31 = ((global::Gtk.Box.BoxChild)(this.hbox2[this.ybtnAddNomenclature]));
			w31.Position = 0;
			w31.Expand = false;
			w31.Fill = false;
			// Container child hbox2.Gtk.Box+BoxChild
			this.ybtnRemoveNomenclature = new global::Gamma.GtkWidgets.yButton();
			this.ybtnRemoveNomenclature.CanFocus = true;
			this.ybtnRemoveNomenclature.Name = "ybtnRemoveNomenclature";
			this.ybtnRemoveNomenclature.UseUnderline = true;
			this.ybtnRemoveNomenclature.Label = global::Mono.Unix.Catalog.GetString("  Удалить  ");
			this.hbox2.Add(this.ybtnRemoveNomenclature);
			global::Gtk.Box.BoxChild w32 = ((global::Gtk.Box.BoxChild)(this.hbox2[this.ybtnRemoveNomenclature]));
			w32.Position = 1;
			w32.Expand = false;
			w32.Fill = false;
			this.vbxNomenclatures.Add(this.hbox2);
			global::Gtk.Box.BoxChild w33 = ((global::Gtk.Box.BoxChild)(this.vbxNomenclatures[this.hbox2]));
			w33.Position = 1;
			w33.Expand = false;
			w33.Fill = false;
			this.GtkAlignment3.Add(this.vbxNomenclatures);
			this.frame1.Add(this.GtkAlignment3);
			this.lblNomTblTitle = new global::Gtk.Label();
			this.lblNomTblTitle.Name = "lblNomTblTitle";
			this.lblNomTblTitle.LabelProp = global::Mono.Unix.Catalog.GetString("<b>Номенклатуры со скидкой:</b>");
			this.lblNomTblTitle.UseMarkup = true;
			this.frame1.LabelWidget = this.lblNomTblTitle;
			this.hbox3.Add(this.frame1);
			global::Gtk.Box.BoxChild w36 = ((global::Gtk.Box.BoxChild)(this.hbox3[this.frame1]));
			w36.Position = 0;
			// Container child hbox3.Gtk.Box+BoxChild
			this.frame2 = new global::Gtk.Frame();
			this.frame2.Name = "frame2";
			this.frame2.ShadowType = ((global::Gtk.ShadowType)(0));
			// Container child frame2.Gtk.Container+ContainerChild
			this.GtkAlignment4 = new global::Gtk.Alignment(0F, 0F, 1F, 1F);
			this.GtkAlignment4.Name = "GtkAlignment4";
			this.GtkAlignment4.LeftPadding = ((uint)(12));
			// Container child GtkAlignment4.Gtk.Container+ContainerChild
			this.vbxNomenclatures1 = new global::Gtk.VBox();
			this.vbxNomenclatures1.Name = "vbxNomenclatures1";
			this.vbxNomenclatures1.Spacing = 6;
			// Container child vbxNomenclatures1.Gtk.Box+BoxChild
			this.GtkScrolledWindow1 = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow1.Name = "GtkScrolledWindow1";
			this.GtkScrolledWindow1.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow1.Gtk.Container+ContainerChild
			this.yTreeActionsItems = new global::Gamma.GtkWidgets.yTreeView();
			this.yTreeActionsItems.CanFocus = true;
			this.yTreeActionsItems.Name = "yTreeActionsItems";
			this.GtkScrolledWindow1.Add(this.yTreeActionsItems);
			this.vbxNomenclatures1.Add(this.GtkScrolledWindow1);
			global::Gtk.Box.BoxChild w38 = ((global::Gtk.Box.BoxChild)(this.vbxNomenclatures1[this.GtkScrolledWindow1]));
			w38.Position = 0;
			// Container child vbxNomenclatures1.Gtk.Box+BoxChild
			this.hbox4 = new global::Gtk.HBox();
			this.hbox4.Name = "hbox4";
			this.hbox4.Spacing = 6;
			// Container child hbox4.Gtk.Box+BoxChild
			this.ybtnRemoveAction = new global::Gamma.GtkWidgets.yButton();
			this.ybtnRemoveAction.CanFocus = true;
			this.ybtnRemoveAction.Name = "ybtnRemoveAction";
			this.ybtnRemoveAction.UseUnderline = true;
			this.ybtnRemoveAction.Label = global::Mono.Unix.Catalog.GetString("  Удалить  ");
			this.hbox4.Add(this.ybtnRemoveAction);
			global::Gtk.Box.BoxChild w39 = ((global::Gtk.Box.BoxChild)(this.hbox4[this.ybtnRemoveAction]));
			w39.Position = 0;
			w39.Expand = false;
			w39.Fill = false;
			this.vbxNomenclatures1.Add(this.hbox4);
			global::Gtk.Box.BoxChild w40 = ((global::Gtk.Box.BoxChild)(this.vbxNomenclatures1[this.hbox4]));
			w40.Position = 1;
			w40.Expand = false;
			w40.Fill = false;
			this.GtkAlignment4.Add(this.vbxNomenclatures1);
			this.frame2.Add(this.GtkAlignment4);
			this.lblActionsTblTitle = new global::Gtk.Label();
			this.lblActionsTblTitle.Name = "lblActionsTblTitle";
			this.lblActionsTblTitle.LabelProp = global::Mono.Unix.Catalog.GetString("<b>Спец. действия:</b>");
			this.lblActionsTblTitle.UseMarkup = true;
			this.frame2.LabelWidget = this.lblActionsTblTitle;
			this.hbox3.Add(this.frame2);
			global::Gtk.Box.BoxChild w43 = ((global::Gtk.Box.BoxChild)(this.hbox3[this.frame2]));
			w43.Position = 1;
			this.vbox2.Add(this.hbox3);
			global::Gtk.Box.BoxChild w44 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.hbox3]));
			w44.PackType = ((global::Gtk.PackType)(1));
			w44.Position = 4;
			this.notebook.Add(this.vbox2);
			// Notebook tab
			this.lblInformation = new global::Gtk.Label();
			this.lblInformation.Name = "lblInformation";
			this.lblInformation.LabelProp = global::Mono.Unix.Catalog.GetString("Информация");
			this.notebook.SetTabLabel(this.vbox2, this.lblInformation);
			this.lblInformation.ShowAll();
			// Container child notebook.Gtk.Notebook+NotebookChild
			this.vboxSitesAndApps = new global::Gamma.GtkWidgets.yVBox();
			this.vboxSitesAndApps.Name = "vboxSitesAndApps";
			this.vboxSitesAndApps.Spacing = 6;
			// Container child vboxSitesAndApps.Gtk.Box+BoxChild
			this.tableOnlineParameters = new global::Gamma.GtkWidgets.yTable();
			this.tableOnlineParameters.Name = "tableOnlineParameters";
			this.tableOnlineParameters.NRows = ((uint)(5));
			this.tableOnlineParameters.NColumns = ((uint)(4));
			this.tableOnlineParameters.RowSpacing = ((uint)(6));
			this.tableOnlineParameters.ColumnSpacing = ((uint)(6));
			// Container child tableOnlineParameters.Gtk.Table+TableChild
			this.entryOnlineName = new global::Gamma.GtkWidgets.yEntry();
			this.entryOnlineName.CanFocus = true;
			this.entryOnlineName.Name = "entryOnlineName";
			this.entryOnlineName.IsEditable = true;
			this.entryOnlineName.InvisibleChar = '•';
			this.tableOnlineParameters.Add(this.entryOnlineName);
			global::Gtk.Table.TableChild w46 = ((global::Gtk.Table.TableChild)(this.tableOnlineParameters[this.entryOnlineName]));
			w46.TopAttach = ((uint)(1));
			w46.BottomAttach = ((uint)(2));
			w46.LeftAttach = ((uint)(1));
			w46.RightAttach = ((uint)(3));
			w46.XOptions = ((global::Gtk.AttachOptions)(4));
			w46.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableOnlineParameters.Gtk.Table+TableChild
			this.enumCmbOnlineAvailabilityKulerSaleWebSite = new global::Gamma.Widgets.yEnumComboBox();
			this.enumCmbOnlineAvailabilityKulerSaleWebSite.Name = "enumCmbOnlineAvailabilityKulerSaleWebSite";
			this.enumCmbOnlineAvailabilityKulerSaleWebSite.ShowSpecialStateAll = false;
			this.enumCmbOnlineAvailabilityKulerSaleWebSite.ShowSpecialStateNot = false;
			this.enumCmbOnlineAvailabilityKulerSaleWebSite.UseShortTitle = false;
			this.enumCmbOnlineAvailabilityKulerSaleWebSite.DefaultFirst = false;
			this.tableOnlineParameters.Add(this.enumCmbOnlineAvailabilityKulerSaleWebSite);
			global::Gtk.Table.TableChild w47 = ((global::Gtk.Table.TableChild)(this.tableOnlineParameters[this.enumCmbOnlineAvailabilityKulerSaleWebSite]));
			w47.TopAttach = ((uint)(4));
			w47.BottomAttach = ((uint)(5));
			w47.LeftAttach = ((uint)(3));
			w47.RightAttach = ((uint)(4));
			w47.XOptions = ((global::Gtk.AttachOptions)(4));
			w47.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableOnlineParameters.Gtk.Table+TableChild
			this.enumCmbOnlineAvailabilityMobileApp = new global::Gamma.Widgets.yEnumComboBox();
			this.enumCmbOnlineAvailabilityMobileApp.Name = "enumCmbOnlineAvailabilityMobileApp";
			this.enumCmbOnlineAvailabilityMobileApp.ShowSpecialStateAll = false;
			this.enumCmbOnlineAvailabilityMobileApp.ShowSpecialStateNot = false;
			this.enumCmbOnlineAvailabilityMobileApp.UseShortTitle = false;
			this.enumCmbOnlineAvailabilityMobileApp.DefaultFirst = false;
			this.tableOnlineParameters.Add(this.enumCmbOnlineAvailabilityMobileApp);
			global::Gtk.Table.TableChild w48 = ((global::Gtk.Table.TableChild)(this.tableOnlineParameters[this.enumCmbOnlineAvailabilityMobileApp]));
			w48.TopAttach = ((uint)(4));
			w48.BottomAttach = ((uint)(5));
			w48.LeftAttach = ((uint)(1));
			w48.RightAttach = ((uint)(2));
			w48.XOptions = ((global::Gtk.AttachOptions)(4));
			w48.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableOnlineParameters.Gtk.Table+TableChild
			this.enumCmbOnlineAvailabilityVodovozWebSite = new global::Gamma.Widgets.yEnumComboBox();
			this.enumCmbOnlineAvailabilityVodovozWebSite.Name = "enumCmbOnlineAvailabilityVodovozWebSite";
			this.enumCmbOnlineAvailabilityVodovozWebSite.ShowSpecialStateAll = false;
			this.enumCmbOnlineAvailabilityVodovozWebSite.ShowSpecialStateNot = false;
			this.enumCmbOnlineAvailabilityVodovozWebSite.UseShortTitle = false;
			this.enumCmbOnlineAvailabilityVodovozWebSite.DefaultFirst = false;
			this.tableOnlineParameters.Add(this.enumCmbOnlineAvailabilityVodovozWebSite);
			global::Gtk.Table.TableChild w49 = ((global::Gtk.Table.TableChild)(this.tableOnlineParameters[this.enumCmbOnlineAvailabilityVodovozWebSite]));
			w49.TopAttach = ((uint)(4));
			w49.BottomAttach = ((uint)(5));
			w49.LeftAttach = ((uint)(2));
			w49.RightAttach = ((uint)(3));
			w49.XOptions = ((global::Gtk.AttachOptions)(4));
			w49.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableOnlineParameters.Gtk.Table+TableChild
			this.hseparator = new global::Gtk.HSeparator();
			this.hseparator.Name = "hseparator";
			this.tableOnlineParameters.Add(this.hseparator);
			global::Gtk.Table.TableChild w50 = ((global::Gtk.Table.TableChild)(this.tableOnlineParameters[this.hseparator]));
			w50.TopAttach = ((uint)(2));
			w50.BottomAttach = ((uint)(3));
			w50.RightAttach = ((uint)(4));
			w50.XOptions = ((global::Gtk.AttachOptions)(4));
			w50.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableOnlineParameters.Gtk.Table+TableChild
			this.lblAppTitle = new global::Gamma.GtkWidgets.yLabel();
			this.lblAppTitle.Name = "lblAppTitle";
			this.lblAppTitle.LabelProp = global::Mono.Unix.Catalog.GetString("Приложение");
			this.tableOnlineParameters.Add(this.lblAppTitle);
			global::Gtk.Table.TableChild w51 = ((global::Gtk.Table.TableChild)(this.tableOnlineParameters[this.lblAppTitle]));
			w51.TopAttach = ((uint)(3));
			w51.BottomAttach = ((uint)(4));
			w51.LeftAttach = ((uint)(1));
			w51.RightAttach = ((uint)(2));
			w51.XOptions = ((global::Gtk.AttachOptions)(4));
			w51.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableOnlineParameters.Gtk.Table+TableChild
			this.lblErpId = new global::Gamma.GtkWidgets.yLabel();
			this.lblErpId.Name = "lblErpId";
			this.lblErpId.Xalign = 0F;
			this.lblErpId.LabelProp = global::Mono.Unix.Catalog.GetString("Код промонабора");
			this.tableOnlineParameters.Add(this.lblErpId);
			global::Gtk.Table.TableChild w52 = ((global::Gtk.Table.TableChild)(this.tableOnlineParameters[this.lblErpId]));
			w52.LeftAttach = ((uint)(1));
			w52.RightAttach = ((uint)(2));
			w52.XOptions = ((global::Gtk.AttachOptions)(4));
			w52.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableOnlineParameters.Gtk.Table+TableChild
			this.lblErpIdTitle = new global::Gamma.GtkWidgets.yLabel();
			this.lblErpIdTitle.Name = "lblErpIdTitle";
			this.lblErpIdTitle.Xalign = 1F;
			this.lblErpIdTitle.LabelProp = global::Mono.Unix.Catalog.GetString("ERP ID:");
			this.tableOnlineParameters.Add(this.lblErpIdTitle);
			global::Gtk.Table.TableChild w53 = ((global::Gtk.Table.TableChild)(this.tableOnlineParameters[this.lblErpIdTitle]));
			w53.XOptions = ((global::Gtk.AttachOptions)(4));
			w53.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableOnlineParameters.Gtk.Table+TableChild
			this.lblKulerSaleWebSiteTitle = new global::Gamma.GtkWidgets.yLabel();
			this.lblKulerSaleWebSiteTitle.Name = "lblKulerSaleWebSiteTitle";
			this.lblKulerSaleWebSiteTitle.LabelProp = global::Mono.Unix.Catalog.GetString("Сайт Кулер-сейл");
			this.tableOnlineParameters.Add(this.lblKulerSaleWebSiteTitle);
			global::Gtk.Table.TableChild w54 = ((global::Gtk.Table.TableChild)(this.tableOnlineParameters[this.lblKulerSaleWebSiteTitle]));
			w54.TopAttach = ((uint)(3));
			w54.BottomAttach = ((uint)(4));
			w54.LeftAttach = ((uint)(3));
			w54.RightAttach = ((uint)(4));
			w54.XOptions = ((global::Gtk.AttachOptions)(4));
			w54.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableOnlineParameters.Gtk.Table+TableChild
			this.lblOnlineAvailability = new global::Gamma.GtkWidgets.yLabel();
			this.lblOnlineAvailability.Name = "lblOnlineAvailability";
			this.lblOnlineAvailability.Xalign = 1F;
			this.lblOnlineAvailability.LabelProp = global::Mono.Unix.Catalog.GetString("Доступность для продажи:");
			this.tableOnlineParameters.Add(this.lblOnlineAvailability);
			global::Gtk.Table.TableChild w55 = ((global::Gtk.Table.TableChild)(this.tableOnlineParameters[this.lblOnlineAvailability]));
			w55.TopAttach = ((uint)(4));
			w55.BottomAttach = ((uint)(5));
			w55.XOptions = ((global::Gtk.AttachOptions)(4));
			w55.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableOnlineParameters.Gtk.Table+TableChild
			this.lblOnlineNameTitle = new global::Gamma.GtkWidgets.yLabel();
			this.lblOnlineNameTitle.Name = "lblOnlineNameTitle";
			this.lblOnlineNameTitle.Xalign = 1F;
			this.lblOnlineNameTitle.LabelProp = global::Mono.Unix.Catalog.GetString("Название:");
			this.tableOnlineParameters.Add(this.lblOnlineNameTitle);
			global::Gtk.Table.TableChild w56 = ((global::Gtk.Table.TableChild)(this.tableOnlineParameters[this.lblOnlineNameTitle]));
			w56.TopAttach = ((uint)(1));
			w56.BottomAttach = ((uint)(2));
			w56.XOptions = ((global::Gtk.AttachOptions)(4));
			w56.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableOnlineParameters.Gtk.Table+TableChild
			this.lblVodovozWebSiteTitle = new global::Gamma.GtkWidgets.yLabel();
			this.lblVodovozWebSiteTitle.Name = "lblVodovozWebSiteTitle";
			this.lblVodovozWebSiteTitle.LabelProp = global::Mono.Unix.Catalog.GetString("Сайт ВВ");
			this.tableOnlineParameters.Add(this.lblVodovozWebSiteTitle);
			global::Gtk.Table.TableChild w57 = ((global::Gtk.Table.TableChild)(this.tableOnlineParameters[this.lblVodovozWebSiteTitle]));
			w57.TopAttach = ((uint)(3));
			w57.BottomAttach = ((uint)(4));
			w57.LeftAttach = ((uint)(2));
			w57.RightAttach = ((uint)(3));
			w57.XOptions = ((global::Gtk.AttachOptions)(4));
			w57.YOptions = ((global::Gtk.AttachOptions)(4));
			this.vboxSitesAndApps.Add(this.tableOnlineParameters);
			global::Gtk.Box.BoxChild w58 = ((global::Gtk.Box.BoxChild)(this.vboxSitesAndApps[this.tableOnlineParameters]));
			w58.Position = 0;
			w58.Expand = false;
			w58.Fill = false;
			this.notebook.Add(this.vboxSitesAndApps);
			global::Gtk.Notebook.NotebookChild w59 = ((global::Gtk.Notebook.NotebookChild)(this.notebook[this.vboxSitesAndApps]));
			w59.Position = 1;
			// Notebook tab
			this.lblSitesAndApps = new global::Gtk.Label();
			this.lblSitesAndApps.Name = "lblSitesAndApps";
			this.lblSitesAndApps.LabelProp = global::Mono.Unix.Catalog.GetString("Сайты и приложения");
			this.notebook.SetTabLabel(this.vboxSitesAndApps, this.lblSitesAndApps);
			this.lblSitesAndApps.ShowAll();
			this.vboxMain.Add(this.notebook);
			global::Gtk.Box.BoxChild w60 = ((global::Gtk.Box.BoxChild)(this.vboxMain[this.notebook]));
			w60.Position = 1;
			this.Add(this.vboxMain);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}

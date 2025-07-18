
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.Views.Client
{
	public partial class EdoAccountView
	{
		private global::Gamma.GtkWidgets.yVBox vboxMain;

		private global::Gamma.GtkWidgets.yHBox hboxMain;

		private global::Gamma.GtkWidgets.yRadioButton radioBtnIsDefault;

		private global::Gamma.GtkWidgets.yVBox vboxCopyParametersFromOther;

		private global::QS.Widgets.MenuButton menuBtnCopyParametersFrom;

		private global::Gamma.GtkWidgets.yTable tableAccount;

		private global::Gtk.HSeparator hseparator;

		private global::QS.Views.Control.EntityEntry operatorEdoEntry;

		private global::QS.Widgets.GtkUI.SpecialListComboBox specialListCmbAllOperators;

		private global::Gamma.GtkWidgets.yButton ybuttonCheckClientInTaxcom;

		private global::Gamma.GtkWidgets.yButton ybuttonCheckConsentForEdo;

		private global::Gamma.GtkWidgets.yButton ybuttonSendInviteByTaxcom;

		private global::Gamma.GtkWidgets.yButton ybuttonSendManualInvite;

		private global::Gamma.GtkWidgets.yEntry yentryPersonalAccountCodeInEdo;

		private global::Gamma.Widgets.yEnumComboBox yEnumCmbConsentForEdo;

		private global::Gamma.GtkWidgets.yLabel ylabelAllOperators;

		private global::Gamma.GtkWidgets.yLabel ylabelConsentForEdo;

		private global::Gamma.GtkWidgets.yLabel ylabelOperatorEdo;

		private global::Gamma.GtkWidgets.yLabel ylabelPersonalAccountCodeInEdo;

		private global::Gamma.GtkWidgets.yVBox vboxDeleteWdget;

		private global::Gamma.GtkWidgets.yButton btnRemoveEdoAccount;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.Views.Client.EdoAccountView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.Views.Client.EdoAccountView";
			// Container child Vodovoz.Views.Client.EdoAccountView.Gtk.Container+ContainerChild
			this.vboxMain = new global::Gamma.GtkWidgets.yVBox();
			this.vboxMain.Name = "vboxMain";
			this.vboxMain.Spacing = 6;
			// Container child vboxMain.Gtk.Box+BoxChild
			this.hboxMain = new global::Gamma.GtkWidgets.yHBox();
			this.hboxMain.Name = "hboxMain";
			this.hboxMain.Spacing = 6;
			// Container child hboxMain.Gtk.Box+BoxChild
			this.radioBtnIsDefault = new global::Gamma.GtkWidgets.yRadioButton();
			this.radioBtnIsDefault.CanFocus = true;
			this.radioBtnIsDefault.Name = "radioBtnIsDefault";
			this.radioBtnIsDefault.Label = global::Mono.Unix.Catalog.GetString("Основной");
			this.radioBtnIsDefault.DrawIndicator = true;
			this.radioBtnIsDefault.UseUnderline = true;
			this.radioBtnIsDefault.Group = new global::GLib.SList(global::System.IntPtr.Zero);
			this.hboxMain.Add(this.radioBtnIsDefault);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.hboxMain[this.radioBtnIsDefault]));
			w1.Position = 0;
			w1.Expand = false;
			w1.Fill = false;
			// Container child hboxMain.Gtk.Box+BoxChild
			this.vboxCopyParametersFromOther = new global::Gamma.GtkWidgets.yVBox();
			this.vboxCopyParametersFromOther.Name = "vboxCopyParametersFromOther";
			this.vboxCopyParametersFromOther.Spacing = 6;
			// Container child vboxCopyParametersFromOther.Gtk.Box+BoxChild
			this.menuBtnCopyParametersFrom = new global::QS.Widgets.MenuButton();
			this.menuBtnCopyParametersFrom.CanFocus = true;
			this.menuBtnCopyParametersFrom.Name = "menuBtnCopyParametersFrom";
			this.menuBtnCopyParametersFrom.UseUnderline = true;
			this.menuBtnCopyParametersFrom.UseMarkup = false;
			this.menuBtnCopyParametersFrom.LabelXAlign = 0F;
			this.menuBtnCopyParametersFrom.Label = global::Mono.Unix.Catalog.GetString("Вставить данные\nоператора и кабинета");
			this.vboxCopyParametersFromOther.Add(this.menuBtnCopyParametersFrom);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.vboxCopyParametersFromOther[this.menuBtnCopyParametersFrom]));
			w2.Position = 1;
			w2.Fill = false;
			this.hboxMain.Add(this.vboxCopyParametersFromOther);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.hboxMain[this.vboxCopyParametersFromOther]));
			w3.Position = 1;
			w3.Expand = false;
			w3.Fill = false;
			// Container child hboxMain.Gtk.Box+BoxChild
			this.tableAccount = new global::Gamma.GtkWidgets.yTable();
			this.tableAccount.Name = "tableAccount";
			this.tableAccount.NRows = ((uint)(5));
			this.tableAccount.NColumns = ((uint)(4));
			this.tableAccount.RowSpacing = ((uint)(6));
			this.tableAccount.ColumnSpacing = ((uint)(6));
			// Container child tableAccount.Gtk.Table+TableChild
			this.hseparator = new global::Gtk.HSeparator();
			this.hseparator.Name = "hseparator";
			this.tableAccount.Add(this.hseparator);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.tableAccount[this.hseparator]));
			w4.TopAttach = ((uint)(4));
			w4.BottomAttach = ((uint)(5));
			w4.RightAttach = ((uint)(4));
			w4.YPadding = ((uint)(12));
			w4.XOptions = ((global::Gtk.AttachOptions)(4));
			w4.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableAccount.Gtk.Table+TableChild
			this.operatorEdoEntry = new global::QS.Views.Control.EntityEntry();
			this.operatorEdoEntry.Events = ((global::Gdk.EventMask)(256));
			this.operatorEdoEntry.Name = "operatorEdoEntry";
			this.tableAccount.Add(this.operatorEdoEntry);
			global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.tableAccount[this.operatorEdoEntry]));
			w5.TopAttach = ((uint)(1));
			w5.BottomAttach = ((uint)(2));
			w5.LeftAttach = ((uint)(1));
			w5.RightAttach = ((uint)(2));
			w5.XOptions = ((global::Gtk.AttachOptions)(4));
			w5.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableAccount.Gtk.Table+TableChild
			this.specialListCmbAllOperators = new global::QS.Widgets.GtkUI.SpecialListComboBox();
			this.specialListCmbAllOperators.Name = "specialListCmbAllOperators";
			this.specialListCmbAllOperators.AddIfNotExist = false;
			this.specialListCmbAllOperators.DefaultFirst = false;
			this.specialListCmbAllOperators.ShowSpecialStateAll = false;
			this.specialListCmbAllOperators.ShowSpecialStateNot = true;
			this.tableAccount.Add(this.specialListCmbAllOperators);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.tableAccount[this.specialListCmbAllOperators]));
			w6.LeftAttach = ((uint)(3));
			w6.RightAttach = ((uint)(4));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableAccount.Gtk.Table+TableChild
			this.ybuttonCheckClientInTaxcom = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonCheckClientInTaxcom.CanFocus = true;
			this.ybuttonCheckClientInTaxcom.Name = "ybuttonCheckClientInTaxcom";
			this.ybuttonCheckClientInTaxcom.UseUnderline = true;
			this.ybuttonCheckClientInTaxcom.Label = global::Mono.Unix.Catalog.GetString("Проверить клиента в Такском");
			this.tableAccount.Add(this.ybuttonCheckClientInTaxcom);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.tableAccount[this.ybuttonCheckClientInTaxcom]));
			w7.TopAttach = ((uint)(1));
			w7.BottomAttach = ((uint)(2));
			w7.LeftAttach = ((uint)(2));
			w7.RightAttach = ((uint)(3));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableAccount.Gtk.Table+TableChild
			this.ybuttonCheckConsentForEdo = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonCheckConsentForEdo.CanFocus = true;
			this.ybuttonCheckConsentForEdo.Name = "ybuttonCheckConsentForEdo";
			this.ybuttonCheckConsentForEdo.UseUnderline = true;
			this.ybuttonCheckConsentForEdo.Label = global::Mono.Unix.Catalog.GetString("Проверить согласие клиента");
			this.tableAccount.Add(this.ybuttonCheckConsentForEdo);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.tableAccount[this.ybuttonCheckConsentForEdo]));
			w8.TopAttach = ((uint)(3));
			w8.BottomAttach = ((uint)(4));
			w8.LeftAttach = ((uint)(2));
			w8.RightAttach = ((uint)(3));
			w8.XOptions = ((global::Gtk.AttachOptions)(4));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableAccount.Gtk.Table+TableChild
			this.ybuttonSendInviteByTaxcom = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonSendInviteByTaxcom.CanFocus = true;
			this.ybuttonSendInviteByTaxcom.Name = "ybuttonSendInviteByTaxcom";
			this.ybuttonSendInviteByTaxcom.UseUnderline = true;
			this.ybuttonSendInviteByTaxcom.Label = global::Mono.Unix.Catalog.GetString("Отправить приглашение через Такском");
			this.tableAccount.Add(this.ybuttonSendInviteByTaxcom);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.tableAccount[this.ybuttonSendInviteByTaxcom]));
			w9.TopAttach = ((uint)(2));
			w9.BottomAttach = ((uint)(3));
			w9.LeftAttach = ((uint)(2));
			w9.RightAttach = ((uint)(3));
			w9.XOptions = ((global::Gtk.AttachOptions)(4));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableAccount.Gtk.Table+TableChild
			this.ybuttonSendManualInvite = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonSendManualInvite.TooltipMarkup = "Время обработки до 10 дней";
			this.ybuttonSendManualInvite.CanFocus = true;
			this.ybuttonSendManualInvite.Name = "ybuttonSendManualInvite";
			this.ybuttonSendManualInvite.UseUnderline = true;
			this.ybuttonSendManualInvite.Label = "Отправить приглашение без кода личного кабинета";
			this.tableAccount.Add(this.ybuttonSendManualInvite);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.tableAccount[this.ybuttonSendManualInvite]));
			w10.TopAttach = ((uint)(2));
			w10.BottomAttach = ((uint)(3));
			w10.LeftAttach = ((uint)(3));
			w10.RightAttach = ((uint)(4));
			w10.XOptions = ((global::Gtk.AttachOptions)(4));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableAccount.Gtk.Table+TableChild
			this.yentryPersonalAccountCodeInEdo = new global::Gamma.GtkWidgets.yEntry();
			this.yentryPersonalAccountCodeInEdo.CanFocus = true;
			this.yentryPersonalAccountCodeInEdo.Name = "yentryPersonalAccountCodeInEdo";
			this.yentryPersonalAccountCodeInEdo.IsEditable = true;
			this.yentryPersonalAccountCodeInEdo.InvisibleChar = '•';
			this.tableAccount.Add(this.yentryPersonalAccountCodeInEdo);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.tableAccount[this.yentryPersonalAccountCodeInEdo]));
			w11.TopAttach = ((uint)(2));
			w11.BottomAttach = ((uint)(3));
			w11.LeftAttach = ((uint)(1));
			w11.RightAttach = ((uint)(2));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableAccount.Gtk.Table+TableChild
			this.yEnumCmbConsentForEdo = new global::Gamma.Widgets.yEnumComboBox();
			this.yEnumCmbConsentForEdo.Name = "yEnumCmbConsentForEdo";
			this.yEnumCmbConsentForEdo.ShowSpecialStateAll = false;
			this.yEnumCmbConsentForEdo.ShowSpecialStateNot = false;
			this.yEnumCmbConsentForEdo.UseShortTitle = false;
			this.yEnumCmbConsentForEdo.DefaultFirst = false;
			this.tableAccount.Add(this.yEnumCmbConsentForEdo);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.tableAccount[this.yEnumCmbConsentForEdo]));
			w12.TopAttach = ((uint)(3));
			w12.BottomAttach = ((uint)(4));
			w12.LeftAttach = ((uint)(1));
			w12.RightAttach = ((uint)(2));
			w12.XOptions = ((global::Gtk.AttachOptions)(4));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableAccount.Gtk.Table+TableChild
			this.ylabelAllOperators = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelAllOperators.Name = "ylabelAllOperators";
			this.ylabelAllOperators.Xalign = 1F;
			this.ylabelAllOperators.LabelProp = global::Mono.Unix.Catalog.GetString("Выбрать из операторов контрагента:");
			this.tableAccount.Add(this.ylabelAllOperators);
			global::Gtk.Table.TableChild w13 = ((global::Gtk.Table.TableChild)(this.tableAccount[this.ylabelAllOperators]));
			w13.LeftAttach = ((uint)(2));
			w13.RightAttach = ((uint)(3));
			w13.XOptions = ((global::Gtk.AttachOptions)(4));
			w13.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableAccount.Gtk.Table+TableChild
			this.ylabelConsentForEdo = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelConsentForEdo.Name = "ylabelConsentForEdo";
			this.ylabelConsentForEdo.Xalign = 1F;
			this.ylabelConsentForEdo.LabelProp = global::Mono.Unix.Catalog.GetString("Согласие клиента на ЭДО:");
			this.tableAccount.Add(this.ylabelConsentForEdo);
			global::Gtk.Table.TableChild w14 = ((global::Gtk.Table.TableChild)(this.tableAccount[this.ylabelConsentForEdo]));
			w14.TopAttach = ((uint)(3));
			w14.BottomAttach = ((uint)(4));
			w14.XOptions = ((global::Gtk.AttachOptions)(4));
			w14.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableAccount.Gtk.Table+TableChild
			this.ylabelOperatorEdo = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelOperatorEdo.Name = "ylabelOperatorEdo";
			this.ylabelOperatorEdo.Xalign = 1F;
			this.ylabelOperatorEdo.LabelProp = global::Mono.Unix.Catalog.GetString("Укажите оператора ЭДО у клиента:");
			this.tableAccount.Add(this.ylabelOperatorEdo);
			global::Gtk.Table.TableChild w15 = ((global::Gtk.Table.TableChild)(this.tableAccount[this.ylabelOperatorEdo]));
			w15.TopAttach = ((uint)(1));
			w15.BottomAttach = ((uint)(2));
			w15.XOptions = ((global::Gtk.AttachOptions)(4));
			w15.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableAccount.Gtk.Table+TableChild
			this.ylabelPersonalAccountCodeInEdo = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelPersonalAccountCodeInEdo.Name = "ylabelPersonalAccountCodeInEdo";
			this.ylabelPersonalAccountCodeInEdo.Xalign = 1F;
			this.ylabelPersonalAccountCodeInEdo.LabelProp = global::Mono.Unix.Catalog.GetString("Код личного кабинета клиента в ЭДО:");
			this.tableAccount.Add(this.ylabelPersonalAccountCodeInEdo);
			global::Gtk.Table.TableChild w16 = ((global::Gtk.Table.TableChild)(this.tableAccount[this.ylabelPersonalAccountCodeInEdo]));
			w16.TopAttach = ((uint)(2));
			w16.BottomAttach = ((uint)(3));
			w16.XOptions = ((global::Gtk.AttachOptions)(4));
			w16.YOptions = ((global::Gtk.AttachOptions)(4));
			this.hboxMain.Add(this.tableAccount);
			global::Gtk.Box.BoxChild w17 = ((global::Gtk.Box.BoxChild)(this.hboxMain[this.tableAccount]));
			w17.Position = 2;
			w17.Expand = false;
			w17.Fill = false;
			// Container child hboxMain.Gtk.Box+BoxChild
			this.vboxDeleteWdget = new global::Gamma.GtkWidgets.yVBox();
			this.vboxDeleteWdget.Name = "vboxDeleteWdget";
			this.vboxDeleteWdget.Spacing = 6;
			// Container child vboxDeleteWdget.Gtk.Box+BoxChild
			this.btnRemoveEdoAccount = new global::Gamma.GtkWidgets.yButton();
			this.btnRemoveEdoAccount.CanFocus = true;
			this.btnRemoveEdoAccount.Name = "btnRemoveEdoAccount";
			this.btnRemoveEdoAccount.UseUnderline = true;
			this.btnRemoveEdoAccount.Label = global::Mono.Unix.Catalog.GetString("Удалить");
			this.vboxDeleteWdget.Add(this.btnRemoveEdoAccount);
			global::Gtk.Box.BoxChild w18 = ((global::Gtk.Box.BoxChild)(this.vboxDeleteWdget[this.btnRemoveEdoAccount]));
			w18.Position = 1;
			w18.Fill = false;
			this.hboxMain.Add(this.vboxDeleteWdget);
			global::Gtk.Box.BoxChild w19 = ((global::Gtk.Box.BoxChild)(this.hboxMain[this.vboxDeleteWdget]));
			w19.Position = 3;
			w19.Expand = false;
			w19.Fill = false;
			this.vboxMain.Add(this.hboxMain);
			global::Gtk.Box.BoxChild w20 = ((global::Gtk.Box.BoxChild)(this.vboxMain[this.hboxMain]));
			w20.Position = 0;
			w20.Expand = false;
			w20.Fill = false;
			this.Add(this.vboxMain);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}

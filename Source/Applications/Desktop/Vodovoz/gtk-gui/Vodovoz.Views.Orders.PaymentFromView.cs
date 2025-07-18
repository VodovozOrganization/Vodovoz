
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.Views.Orders
{
	public partial class PaymentFromView
	{
		private global::Gamma.GtkWidgets.yVBox vboxMain;

		private global::Gamma.GtkWidgets.yHBox hboxSaveAndClose;

		private global::Gamma.GtkWidgets.yButton btnSave;

		private global::Gamma.GtkWidgets.yButton btnCancel;

		private global::Gtk.HSeparator hseparator1;

		private global::Gamma.GtkWidgets.yTable tableMain;

		private global::Gamma.GtkWidgets.yEntry entryName;

		private global::Gtk.ScrolledWindow GtkScrolledWindow;

		private global::Gamma.GtkWidgets.yTextView txtViewOrganizationCriterion;

		private global::Gamma.GtkWidgets.yLabel lblName;

		private global::Gamma.GtkWidgets.yLabel lblOrganizationCriterion;

		private global::Gamma.GtkWidgets.yCheckButton yChkAvangardShopIdRequired;

		private global::Gamma.GtkWidgets.yCheckButton yChkCashBoxIdRequired;

		private global::Gamma.GtkWidgets.yCheckButton yChkIsArchive;

		private global::Gamma.GtkWidgets.yCheckButton yChkTaxcomEdoAccountIdRequired;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.Views.Orders.PaymentFromView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.Views.Orders.PaymentFromView";
			// Container child Vodovoz.Views.Orders.PaymentFromView.Gtk.Container+ContainerChild
			this.vboxMain = new global::Gamma.GtkWidgets.yVBox();
			this.vboxMain.Name = "vboxMain";
			this.vboxMain.Spacing = 6;
			// Container child vboxMain.Gtk.Box+BoxChild
			this.hboxSaveAndClose = new global::Gamma.GtkWidgets.yHBox();
			this.hboxSaveAndClose.Name = "hboxSaveAndClose";
			this.hboxSaveAndClose.Spacing = 6;
			// Container child hboxSaveAndClose.Gtk.Box+BoxChild
			this.btnSave = new global::Gamma.GtkWidgets.yButton();
			this.btnSave.CanFocus = true;
			this.btnSave.Name = "btnSave";
			this.btnSave.UseUnderline = true;
			this.btnSave.Label = global::Mono.Unix.Catalog.GetString("Сохранить");
			this.hboxSaveAndClose.Add(this.btnSave);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.hboxSaveAndClose[this.btnSave]));
			w1.Position = 0;
			w1.Expand = false;
			w1.Fill = false;
			// Container child hboxSaveAndClose.Gtk.Box+BoxChild
			this.btnCancel = new global::Gamma.GtkWidgets.yButton();
			this.btnCancel.CanFocus = true;
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.UseUnderline = true;
			this.btnCancel.Label = global::Mono.Unix.Catalog.GetString("Отмена");
			this.hboxSaveAndClose.Add(this.btnCancel);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hboxSaveAndClose[this.btnCancel]));
			w2.Position = 1;
			w2.Expand = false;
			w2.Fill = false;
			this.vboxMain.Add(this.hboxSaveAndClose);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.vboxMain[this.hboxSaveAndClose]));
			w3.Position = 0;
			w3.Expand = false;
			w3.Fill = false;
			// Container child vboxMain.Gtk.Box+BoxChild
			this.hseparator1 = new global::Gtk.HSeparator();
			this.hseparator1.Name = "hseparator1";
			this.vboxMain.Add(this.hseparator1);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.vboxMain[this.hseparator1]));
			w4.Position = 1;
			w4.Expand = false;
			w4.Fill = false;
			// Container child vboxMain.Gtk.Box+BoxChild
			this.tableMain = new global::Gamma.GtkWidgets.yTable();
			this.tableMain.Name = "tableMain";
			this.tableMain.NRows = ((uint)(7));
			this.tableMain.NColumns = ((uint)(3));
			this.tableMain.RowSpacing = ((uint)(6));
			this.tableMain.ColumnSpacing = ((uint)(6));
			// Container child tableMain.Gtk.Table+TableChild
			this.entryName = new global::Gamma.GtkWidgets.yEntry();
			this.entryName.CanFocus = true;
			this.entryName.Name = "entryName";
			this.entryName.IsEditable = true;
			this.entryName.InvisibleChar = '•';
			this.tableMain.Add(this.entryName);
			global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.tableMain[this.entryName]));
			w5.LeftAttach = ((uint)(1));
			w5.RightAttach = ((uint)(2));
			w5.XOptions = ((global::Gtk.AttachOptions)(4));
			w5.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableMain.Gtk.Table+TableChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.txtViewOrganizationCriterion = new global::Gamma.GtkWidgets.yTextView();
			this.txtViewOrganizationCriterion.CanFocus = true;
			this.txtViewOrganizationCriterion.Name = "txtViewOrganizationCriterion";
			this.GtkScrolledWindow.Add(this.txtViewOrganizationCriterion);
			this.tableMain.Add(this.GtkScrolledWindow);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.tableMain[this.GtkScrolledWindow]));
			w7.TopAttach = ((uint)(2));
			w7.BottomAttach = ((uint)(3));
			w7.LeftAttach = ((uint)(1));
			w7.RightAttach = ((uint)(3));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableMain.Gtk.Table+TableChild
			this.lblName = new global::Gamma.GtkWidgets.yLabel();
			this.lblName.Name = "lblName";
			this.lblName.Xalign = 1F;
			this.lblName.LabelProp = global::Mono.Unix.Catalog.GetString("Название:");
			this.tableMain.Add(this.lblName);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.tableMain[this.lblName]));
			w8.XOptions = ((global::Gtk.AttachOptions)(4));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableMain.Gtk.Table+TableChild
			this.lblOrganizationCriterion = new global::Gamma.GtkWidgets.yLabel();
			this.lblOrganizationCriterion.Name = "lblOrganizationCriterion";
			this.lblOrganizationCriterion.Xalign = 1F;
			this.lblOrganizationCriterion.LabelProp = global::Mono.Unix.Catalog.GetString("Условия для установки организации:");
			this.tableMain.Add(this.lblOrganizationCriterion);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.tableMain[this.lblOrganizationCriterion]));
			w9.TopAttach = ((uint)(2));
			w9.BottomAttach = ((uint)(3));
			w9.XOptions = ((global::Gtk.AttachOptions)(4));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableMain.Gtk.Table+TableChild
			this.yChkAvangardShopIdRequired = new global::Gamma.GtkWidgets.yCheckButton();
			this.yChkAvangardShopIdRequired.CanFocus = true;
			this.yChkAvangardShopIdRequired.Name = "yChkAvangardShopIdRequired";
			this.yChkAvangardShopIdRequired.Label = global::Mono.Unix.Catalog.GetString("Нужна регистрация в Авангарде");
			this.yChkAvangardShopIdRequired.DrawIndicator = true;
			this.yChkAvangardShopIdRequired.UseUnderline = true;
			this.tableMain.Add(this.yChkAvangardShopIdRequired);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.tableMain[this.yChkAvangardShopIdRequired]));
			w10.TopAttach = ((uint)(4));
			w10.BottomAttach = ((uint)(5));
			w10.LeftAttach = ((uint)(1));
			w10.RightAttach = ((uint)(2));
			w10.XOptions = ((global::Gtk.AttachOptions)(4));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableMain.Gtk.Table+TableChild
			this.yChkCashBoxIdRequired = new global::Gamma.GtkWidgets.yCheckButton();
			this.yChkCashBoxIdRequired.CanFocus = true;
			this.yChkCashBoxIdRequired.Name = "yChkCashBoxIdRequired";
			this.yChkCashBoxIdRequired.Label = global::Mono.Unix.Catalog.GetString("Нужна регистрация онлайн кассы");
			this.yChkCashBoxIdRequired.DrawIndicator = true;
			this.yChkCashBoxIdRequired.UseUnderline = true;
			this.tableMain.Add(this.yChkCashBoxIdRequired);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.tableMain[this.yChkCashBoxIdRequired]));
			w11.TopAttach = ((uint)(3));
			w11.BottomAttach = ((uint)(4));
			w11.LeftAttach = ((uint)(1));
			w11.RightAttach = ((uint)(2));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableMain.Gtk.Table+TableChild
			this.yChkIsArchive = new global::Gamma.GtkWidgets.yCheckButton();
			this.yChkIsArchive.CanFocus = true;
			this.yChkIsArchive.Name = "yChkIsArchive";
			this.yChkIsArchive.Label = global::Mono.Unix.Catalog.GetString("Архивный");
			this.yChkIsArchive.DrawIndicator = true;
			this.yChkIsArchive.UseUnderline = true;
			this.tableMain.Add(this.yChkIsArchive);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.tableMain[this.yChkIsArchive]));
			w12.TopAttach = ((uint)(6));
			w12.BottomAttach = ((uint)(7));
			w12.LeftAttach = ((uint)(1));
			w12.RightAttach = ((uint)(2));
			w12.XOptions = ((global::Gtk.AttachOptions)(4));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableMain.Gtk.Table+TableChild
			this.yChkTaxcomEdoAccountIdRequired = new global::Gamma.GtkWidgets.yCheckButton();
			this.yChkTaxcomEdoAccountIdRequired.CanFocus = true;
			this.yChkTaxcomEdoAccountIdRequired.Name = "yChkTaxcomEdoAccountIdRequired";
			this.yChkTaxcomEdoAccountIdRequired.Label = global::Mono.Unix.Catalog.GetString("Нужна регистрация в Такскоме");
			this.yChkTaxcomEdoAccountIdRequired.DrawIndicator = true;
			this.yChkTaxcomEdoAccountIdRequired.UseUnderline = true;
			this.tableMain.Add(this.yChkTaxcomEdoAccountIdRequired);
			global::Gtk.Table.TableChild w13 = ((global::Gtk.Table.TableChild)(this.tableMain[this.yChkTaxcomEdoAccountIdRequired]));
			w13.TopAttach = ((uint)(5));
			w13.BottomAttach = ((uint)(6));
			w13.LeftAttach = ((uint)(1));
			w13.RightAttach = ((uint)(2));
			w13.XOptions = ((global::Gtk.AttachOptions)(4));
			w13.YOptions = ((global::Gtk.AttachOptions)(4));
			this.vboxMain.Add(this.tableMain);
			global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.vboxMain[this.tableMain]));
			w14.Position = 2;
			this.Add(this.vboxMain);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}

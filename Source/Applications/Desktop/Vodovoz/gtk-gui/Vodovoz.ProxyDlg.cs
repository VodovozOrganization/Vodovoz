
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz
{
	public partial class ProxyDlg
	{
		private global::Gtk.VBox vbox1;

		private global::Gtk.HBox hbox1;

		private global::Gamma.GtkWidgets.yButton buttonSave;

		private global::Gamma.GtkWidgets.yButton buttonCancel;

		private global::Gtk.Table datatable1;

		private global::QS.Widgets.GtkUI.DatePicker datepickerIssue;

		private global::Gamma.GtkWidgets.yEntry entryNumber;

		private global::Gtk.ScrolledWindow GtkScrolledWindow;

		private global::Gamma.GtkWidgets.yTreeView ytreeDeliveryPoints;

		private global::Gtk.HBox hbox4;

		private global::QS.Widgets.GtkUI.DatePicker datepickerStart;

		private global::Gtk.Label label5;

		private global::QS.Widgets.GtkUI.DatePicker datepickerExpiration;

		private global::Gtk.HBox hbox7;

		private global::Gamma.GtkWidgets.yButton buttonAddDeliveryPoints;

		private global::Gamma.GtkWidgets.yButton buttonDeleteDeliveryPoint;

		private global::Gtk.Label label1;

		private global::Gtk.Label label2;

		private global::Gtk.Label label3;

		private global::Gtk.Label label4;

		private global::Gtk.Label label7;

		private global::Vodovoz.Views.Contacts.PersonsView personsView;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.ProxyDlg
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.ProxyDlg";
			// Container child Vodovoz.ProxyDlg.Gtk.Container+ContainerChild
			this.vbox1 = new global::Gtk.VBox();
			this.vbox1.Name = "vbox1";
			this.vbox1.Spacing = 6;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hbox1 = new global::Gtk.HBox();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.buttonSave = new global::Gamma.GtkWidgets.yButton();
			this.buttonSave.CanFocus = true;
			this.buttonSave.Name = "buttonSave";
			this.buttonSave.UseUnderline = true;
			this.buttonSave.Label = global::Mono.Unix.Catalog.GetString("Сохранить");
			global::Gtk.Image w1 = new global::Gtk.Image();
			w1.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-floppy", global::Gtk.IconSize.Menu);
			this.buttonSave.Image = w1;
			this.hbox1.Add(this.buttonSave);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.buttonSave]));
			w2.Position = 0;
			w2.Expand = false;
			w2.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.buttonCancel = new global::Gamma.GtkWidgets.yButton();
			this.buttonCancel.CanFocus = true;
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.UseUnderline = true;
			this.buttonCancel.Label = global::Mono.Unix.Catalog.GetString("Отмена");
			global::Gtk.Image w3 = new global::Gtk.Image();
			w3.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-revert-to-saved", global::Gtk.IconSize.Menu);
			this.buttonCancel.Image = w3;
			this.hbox1.Add(this.buttonCancel);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.buttonCancel]));
			w4.Position = 1;
			w4.Expand = false;
			w4.Fill = false;
			this.vbox1.Add(this.hbox1);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hbox1]));
			w5.Position = 0;
			w5.Expand = false;
			w5.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.datatable1 = new global::Gtk.Table(((uint)(5)), ((uint)(3)), false);
			this.datatable1.Name = "datatable1";
			this.datatable1.RowSpacing = ((uint)(6));
			this.datatable1.ColumnSpacing = ((uint)(6));
			// Container child datatable1.Gtk.Table+TableChild
			this.datepickerIssue = new global::QS.Widgets.GtkUI.DatePicker();
			this.datepickerIssue.Events = ((global::Gdk.EventMask)(256));
			this.datepickerIssue.Name = "datepickerIssue";
			this.datepickerIssue.WithTime = false;
			this.datepickerIssue.HideCalendarButton = false;
			this.datepickerIssue.Date = new global::System.DateTime(0);
			this.datepickerIssue.IsEditable = true;
			this.datepickerIssue.AutoSeparation = false;
			this.datatable1.Add(this.datepickerIssue);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.datatable1[this.datepickerIssue]));
			w6.TopAttach = ((uint)(1));
			w6.BottomAttach = ((uint)(2));
			w6.LeftAttach = ((uint)(1));
			w6.RightAttach = ((uint)(2));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child datatable1.Gtk.Table+TableChild
			this.entryNumber = new global::Gamma.GtkWidgets.yEntry();
			this.entryNumber.CanFocus = true;
			this.entryNumber.Name = "entryNumber";
			this.entryNumber.IsEditable = true;
			this.entryNumber.InvisibleChar = '●';
			this.datatable1.Add(this.entryNumber);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.datatable1[this.entryNumber]));
			w7.LeftAttach = ((uint)(1));
			w7.RightAttach = ((uint)(2));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child datatable1.Gtk.Table+TableChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.ytreeDeliveryPoints = new global::Gamma.GtkWidgets.yTreeView();
			this.ytreeDeliveryPoints.WidthRequest = 400;
			this.ytreeDeliveryPoints.CanFocus = true;
			this.ytreeDeliveryPoints.Name = "ytreeDeliveryPoints";
			this.GtkScrolledWindow.Add(this.ytreeDeliveryPoints);
			this.datatable1.Add(this.GtkScrolledWindow);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.datatable1[this.GtkScrolledWindow]));
			w9.TopAttach = ((uint)(1));
			w9.BottomAttach = ((uint)(4));
			w9.LeftAttach = ((uint)(2));
			w9.RightAttach = ((uint)(3));
			w9.XOptions = ((global::Gtk.AttachOptions)(4));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child datatable1.Gtk.Table+TableChild
			this.hbox4 = new global::Gtk.HBox();
			this.hbox4.Name = "hbox4";
			this.hbox4.Spacing = 6;
			// Container child hbox4.Gtk.Box+BoxChild
			this.datepickerStart = new global::QS.Widgets.GtkUI.DatePicker();
			this.datepickerStart.Events = ((global::Gdk.EventMask)(256));
			this.datepickerStart.Name = "datepickerStart";
			this.datepickerStart.WithTime = false;
			this.datepickerStart.HideCalendarButton = false;
			this.datepickerStart.Date = new global::System.DateTime(0);
			this.datepickerStart.IsEditable = true;
			this.datepickerStart.AutoSeparation = false;
			this.hbox4.Add(this.datepickerStart);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.hbox4[this.datepickerStart]));
			w10.Position = 0;
			// Container child hbox4.Gtk.Box+BoxChild
			this.label5 = new global::Gtk.Label();
			this.label5.Name = "label5";
			this.label5.Xalign = 1F;
			this.label5.LabelProp = global::Mono.Unix.Catalog.GetString(" - ");
			this.hbox4.Add(this.label5);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.hbox4[this.label5]));
			w11.Position = 1;
			w11.Expand = false;
			w11.Fill = false;
			// Container child hbox4.Gtk.Box+BoxChild
			this.datepickerExpiration = new global::QS.Widgets.GtkUI.DatePicker();
			this.datepickerExpiration.Events = ((global::Gdk.EventMask)(256));
			this.datepickerExpiration.Name = "datepickerExpiration";
			this.datepickerExpiration.WithTime = false;
			this.datepickerExpiration.HideCalendarButton = false;
			this.datepickerExpiration.Date = new global::System.DateTime(0);
			this.datepickerExpiration.IsEditable = true;
			this.datepickerExpiration.AutoSeparation = false;
			this.hbox4.Add(this.datepickerExpiration);
			global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.hbox4[this.datepickerExpiration]));
			w12.Position = 2;
			this.datatable1.Add(this.hbox4);
			global::Gtk.Table.TableChild w13 = ((global::Gtk.Table.TableChild)(this.datatable1[this.hbox4]));
			w13.TopAttach = ((uint)(2));
			w13.BottomAttach = ((uint)(3));
			w13.LeftAttach = ((uint)(1));
			w13.RightAttach = ((uint)(2));
			w13.XOptions = ((global::Gtk.AttachOptions)(4));
			w13.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child datatable1.Gtk.Table+TableChild
			this.hbox7 = new global::Gtk.HBox();
			this.hbox7.Name = "hbox7";
			this.hbox7.Spacing = 6;
			// Container child hbox7.Gtk.Box+BoxChild
			this.buttonAddDeliveryPoints = new global::Gamma.GtkWidgets.yButton();
			this.buttonAddDeliveryPoints.CanFocus = true;
			this.buttonAddDeliveryPoints.Name = "buttonAddDeliveryPoints";
			this.buttonAddDeliveryPoints.UseUnderline = true;
			this.buttonAddDeliveryPoints.Label = global::Mono.Unix.Catalog.GetString("Добавить");
			this.hbox7.Add(this.buttonAddDeliveryPoints);
			global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.hbox7[this.buttonAddDeliveryPoints]));
			w14.Position = 0;
			w14.Expand = false;
			w14.Fill = false;
			// Container child hbox7.Gtk.Box+BoxChild
			this.buttonDeleteDeliveryPoint = new global::Gamma.GtkWidgets.yButton();
			this.buttonDeleteDeliveryPoint.CanFocus = true;
			this.buttonDeleteDeliveryPoint.Name = "buttonDeleteDeliveryPoint";
			this.buttonDeleteDeliveryPoint.UseUnderline = true;
			this.buttonDeleteDeliveryPoint.Label = global::Mono.Unix.Catalog.GetString("Удалить");
			this.hbox7.Add(this.buttonDeleteDeliveryPoint);
			global::Gtk.Box.BoxChild w15 = ((global::Gtk.Box.BoxChild)(this.hbox7[this.buttonDeleteDeliveryPoint]));
			w15.Position = 1;
			w15.Expand = false;
			w15.Fill = false;
			this.datatable1.Add(this.hbox7);
			global::Gtk.Table.TableChild w16 = ((global::Gtk.Table.TableChild)(this.datatable1[this.hbox7]));
			w16.TopAttach = ((uint)(4));
			w16.BottomAttach = ((uint)(5));
			w16.LeftAttach = ((uint)(2));
			w16.RightAttach = ((uint)(3));
			w16.XOptions = ((global::Gtk.AttachOptions)(4));
			w16.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child datatable1.Gtk.Table+TableChild
			this.label1 = new global::Gtk.Label();
			this.label1.Name = "label1";
			this.label1.Xalign = 1F;
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString("Список сотрудников:");
			this.datatable1.Add(this.label1);
			global::Gtk.Table.TableChild w17 = ((global::Gtk.Table.TableChild)(this.datatable1[this.label1]));
			w17.TopAttach = ((uint)(3));
			w17.BottomAttach = ((uint)(4));
			w17.XOptions = ((global::Gtk.AttachOptions)(4));
			w17.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child datatable1.Gtk.Table+TableChild
			this.label2 = new global::Gtk.Label();
			this.label2.Name = "label2";
			this.label2.Xalign = 1F;
			this.label2.LabelProp = global::Mono.Unix.Catalog.GetString("Номер:");
			this.datatable1.Add(this.label2);
			global::Gtk.Table.TableChild w18 = ((global::Gtk.Table.TableChild)(this.datatable1[this.label2]));
			w18.XOptions = ((global::Gtk.AttachOptions)(4));
			w18.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child datatable1.Gtk.Table+TableChild
			this.label3 = new global::Gtk.Label();
			this.label3.Name = "label3";
			this.label3.Xalign = 1F;
			this.label3.LabelProp = global::Mono.Unix.Catalog.GetString("Дата выдачи:");
			this.datatable1.Add(this.label3);
			global::Gtk.Table.TableChild w19 = ((global::Gtk.Table.TableChild)(this.datatable1[this.label3]));
			w19.TopAttach = ((uint)(1));
			w19.BottomAttach = ((uint)(2));
			w19.XOptions = ((global::Gtk.AttachOptions)(4));
			w19.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child datatable1.Gtk.Table+TableChild
			this.label4 = new global::Gtk.Label();
			this.label4.Name = "label4";
			this.label4.Xalign = 1F;
			this.label4.LabelProp = global::Mono.Unix.Catalog.GetString("Сроки действия:");
			this.datatable1.Add(this.label4);
			global::Gtk.Table.TableChild w20 = ((global::Gtk.Table.TableChild)(this.datatable1[this.label4]));
			w20.TopAttach = ((uint)(2));
			w20.BottomAttach = ((uint)(3));
			w20.XOptions = ((global::Gtk.AttachOptions)(4));
			w20.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child datatable1.Gtk.Table+TableChild
			this.label7 = new global::Gtk.Label();
			this.label7.Name = "label7";
			this.label7.LabelProp = global::Mono.Unix.Catalog.GetString("Точки доставки");
			this.datatable1.Add(this.label7);
			global::Gtk.Table.TableChild w21 = ((global::Gtk.Table.TableChild)(this.datatable1[this.label7]));
			w21.LeftAttach = ((uint)(2));
			w21.RightAttach = ((uint)(3));
			w21.XOptions = ((global::Gtk.AttachOptions)(4));
			w21.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child datatable1.Gtk.Table+TableChild
			this.personsView = new global::Vodovoz.Views.Contacts.PersonsView();
			this.personsView.Events = ((global::Gdk.EventMask)(256));
			this.personsView.Name = "personsView";
			this.datatable1.Add(this.personsView);
			global::Gtk.Table.TableChild w22 = ((global::Gtk.Table.TableChild)(this.datatable1[this.personsView]));
			w22.TopAttach = ((uint)(3));
			w22.BottomAttach = ((uint)(4));
			w22.LeftAttach = ((uint)(1));
			w22.RightAttach = ((uint)(2));
			w22.XOptions = ((global::Gtk.AttachOptions)(4));
			w22.YOptions = ((global::Gtk.AttachOptions)(4));
			this.vbox1.Add(this.datatable1);
			global::Gtk.Box.BoxChild w23 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.datatable1]));
			w23.Position = 1;
			w23.Expand = false;
			w23.Fill = false;
			this.Add(this.vbox1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
			this.buttonSave.Clicked += new global::System.EventHandler(this.OnButtonSaveClicked);
			this.buttonCancel.Clicked += new global::System.EventHandler(this.OnButtonCancelClicked);
			this.buttonAddDeliveryPoints.Clicked += new global::System.EventHandler(this.OnButtonAddDeliveryPointsClicked);
			this.buttonDeleteDeliveryPoint.Clicked += new global::System.EventHandler(this.OnButtonDeleteDekiveryPointClicked);
		}
	}
}

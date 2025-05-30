
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.Views.ReportsParameters.Logistic
{
	public partial class DeliveriesLateReportView
	{
		private global::Gtk.VBox vbox1;

		private global::Gtk.Table table1;

		private global::QS.Widgets.GtkUI.DateRangePicker dateperiodpicker;

		private global::Gtk.Label label1;

		private global::Gtk.Label label3;

		private global::Gamma.Widgets.ySpecComboBox ySpecCmbGeographicGroup;

		private global::Gamma.GtkWidgets.yCheckButton ychkDriverSort;

		private global::Gtk.HSeparator hseparator1;

		private global::Gamma.GtkWidgets.yHBox yhboxRouteListOwnType;

		private global::Gtk.Frame frameOrders;

		private global::Gtk.Alignment GtkAlignmentOrders;

		private global::Gamma.GtkWidgets.yVBox yvboxOrders;

		private global::Gamma.GtkWidgets.yRadioButton ycheckAllSelect;

		private global::Gamma.GtkWidgets.yRadioButton ycheckOnlyFastSelect;

		private global::Gamma.GtkWidgets.yRadioButton ycheckWithoutFastSelect;

		private global::Gtk.Label labelOrders;

		private global::Gamma.GtkWidgets.yHBox yhboxInterval;

		private global::Gtk.Frame frameInterval;

		private global::Gtk.Alignment GtkAlignmentInterval;

		private global::Gamma.GtkWidgets.yVBox yvboxInterval;

		private global::Gamma.GtkWidgets.yRadioButton ycheckIntervalFromCreateTime;

		private global::Gamma.GtkWidgets.yRadioButton ycheckIntervalFromTransferTime;

		private global::Gamma.GtkWidgets.yRadioButton ycheckIntervalFromFirstAddress;

		private global::Gtk.Label labelInterval;

		private global::Gamma.GtkWidgets.yButton buttonCreateReport;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.Views.ReportsParameters.Logistic.DeliveriesLateReportView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.Views.ReportsParameters.Logistic.DeliveriesLateReportView";
			// Container child Vodovoz.Views.ReportsParameters.Logistic.DeliveriesLateReportView.Gtk.Container+ContainerChild
			this.vbox1 = new global::Gtk.VBox();
			this.vbox1.Name = "vbox1";
			this.vbox1.Spacing = 6;
			// Container child vbox1.Gtk.Box+BoxChild
			this.table1 = new global::Gtk.Table(((uint)(2)), ((uint)(2)), false);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(6));
			// Container child table1.Gtk.Table+TableChild
			this.dateperiodpicker = new global::QS.Widgets.GtkUI.DateRangePicker();
			this.dateperiodpicker.Events = ((global::Gdk.EventMask)(256));
			this.dateperiodpicker.Name = "dateperiodpicker";
			this.dateperiodpicker.StartDate = new global::System.DateTime(0);
			this.dateperiodpicker.EndDate = new global::System.DateTime(0);
			this.table1.Add(this.dateperiodpicker);
			global::Gtk.Table.TableChild w1 = ((global::Gtk.Table.TableChild)(this.table1[this.dateperiodpicker]));
			w1.LeftAttach = ((uint)(1));
			w1.RightAttach = ((uint)(2));
			w1.XOptions = ((global::Gtk.AttachOptions)(4));
			w1.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label1 = new global::Gtk.Label();
			this.label1.Name = "label1";
			this.label1.Xalign = 1F;
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString("Дата:");
			this.table1.Add(this.label1);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.table1[this.label1]));
			w2.XOptions = ((global::Gtk.AttachOptions)(4));
			w2.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label3 = new global::Gtk.Label();
			this.label3.Name = "label3";
			this.label3.Xalign = 1F;
			this.label3.LabelProp = global::Mono.Unix.Catalog.GetString("Район города:");
			this.table1.Add(this.label3);
			global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.table1[this.label3]));
			w3.TopAttach = ((uint)(1));
			w3.BottomAttach = ((uint)(2));
			w3.XOptions = ((global::Gtk.AttachOptions)(4));
			w3.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ySpecCmbGeographicGroup = new global::Gamma.Widgets.ySpecComboBox();
			this.ySpecCmbGeographicGroup.Name = "ySpecCmbGeographicGroup";
			this.ySpecCmbGeographicGroup.AddIfNotExist = false;
			this.ySpecCmbGeographicGroup.DefaultFirst = false;
			this.ySpecCmbGeographicGroup.ShowSpecialStateAll = true;
			this.ySpecCmbGeographicGroup.ShowSpecialStateNot = false;
			this.table1.Add(this.ySpecCmbGeographicGroup);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.table1[this.ySpecCmbGeographicGroup]));
			w4.TopAttach = ((uint)(1));
			w4.BottomAttach = ((uint)(2));
			w4.LeftAttach = ((uint)(1));
			w4.RightAttach = ((uint)(2));
			w4.XOptions = ((global::Gtk.AttachOptions)(4));
			w4.YOptions = ((global::Gtk.AttachOptions)(4));
			this.vbox1.Add(this.table1);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.table1]));
			w5.Position = 0;
			w5.Expand = false;
			w5.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.ychkDriverSort = new global::Gamma.GtkWidgets.yCheckButton();
			this.ychkDriverSort.CanFocus = true;
			this.ychkDriverSort.Name = "ychkDriverSort";
			this.ychkDriverSort.Label = global::Mono.Unix.Catalog.GetString("Сортировать по водителям");
			this.ychkDriverSort.DrawIndicator = true;
			this.ychkDriverSort.UseUnderline = true;
			this.vbox1.Add(this.ychkDriverSort);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.ychkDriverSort]));
			w6.Position = 1;
			w6.Expand = false;
			w6.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hseparator1 = new global::Gtk.HSeparator();
			this.hseparator1.Name = "hseparator1";
			this.vbox1.Add(this.hseparator1);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hseparator1]));
			w7.Position = 2;
			w7.Expand = false;
			w7.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.yhboxRouteListOwnType = new global::Gamma.GtkWidgets.yHBox();
			this.yhboxRouteListOwnType.HeightRequest = 200;
			this.yhboxRouteListOwnType.Name = "yhboxRouteListOwnType";
			this.yhboxRouteListOwnType.Spacing = 6;
			this.vbox1.Add(this.yhboxRouteListOwnType);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.yhboxRouteListOwnType]));
			w8.Position = 3;
			w8.Expand = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.frameOrders = new global::Gtk.Frame();
			this.frameOrders.Name = "frameOrders";
			this.frameOrders.ShadowType = ((global::Gtk.ShadowType)(0));
			// Container child frameOrders.Gtk.Container+ContainerChild
			this.GtkAlignmentOrders = new global::Gtk.Alignment(0F, 0F, 1F, 1F);
			this.GtkAlignmentOrders.Name = "GtkAlignmentOrders";
			this.GtkAlignmentOrders.LeftPadding = ((uint)(12));
			// Container child GtkAlignmentOrders.Gtk.Container+ContainerChild
			this.yvboxOrders = new global::Gamma.GtkWidgets.yVBox();
			this.yvboxOrders.Name = "yvboxOrders";
			this.yvboxOrders.Spacing = 6;
			// Container child yvboxOrders.Gtk.Box+BoxChild
			this.ycheckAllSelect = new global::Gamma.GtkWidgets.yRadioButton();
			this.ycheckAllSelect.CanFocus = true;
			this.ycheckAllSelect.Name = "ycheckAllSelect";
			this.ycheckAllSelect.Label = global::Mono.Unix.Catalog.GetString("Все");
			this.ycheckAllSelect.DrawIndicator = true;
			this.ycheckAllSelect.UseUnderline = true;
			this.ycheckAllSelect.Group = new global::GLib.SList(global::System.IntPtr.Zero);
			this.yvboxOrders.Add(this.ycheckAllSelect);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.yvboxOrders[this.ycheckAllSelect]));
			w9.Position = 0;
			w9.Expand = false;
			w9.Fill = false;
			// Container child yvboxOrders.Gtk.Box+BoxChild
			this.ycheckOnlyFastSelect = new global::Gamma.GtkWidgets.yRadioButton();
			this.ycheckOnlyFastSelect.CanFocus = true;
			this.ycheckOnlyFastSelect.Name = "ycheckOnlyFastSelect";
			this.ycheckOnlyFastSelect.Label = global::Mono.Unix.Catalog.GetString("С доставкой за час");
			this.ycheckOnlyFastSelect.DrawIndicator = true;
			this.ycheckOnlyFastSelect.UseUnderline = true;
			this.ycheckOnlyFastSelect.Group = this.ycheckAllSelect.Group;
			this.yvboxOrders.Add(this.ycheckOnlyFastSelect);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.yvboxOrders[this.ycheckOnlyFastSelect]));
			w10.Position = 1;
			w10.Expand = false;
			w10.Fill = false;
			// Container child yvboxOrders.Gtk.Box+BoxChild
			this.ycheckWithoutFastSelect = new global::Gamma.GtkWidgets.yRadioButton();
			this.ycheckWithoutFastSelect.CanFocus = true;
			this.ycheckWithoutFastSelect.Name = "ycheckWithoutFastSelect";
			this.ycheckWithoutFastSelect.Label = global::Mono.Unix.Catalog.GetString("Без доставки за час");
			this.ycheckWithoutFastSelect.DrawIndicator = true;
			this.ycheckWithoutFastSelect.UseUnderline = true;
			this.ycheckWithoutFastSelect.Group = this.ycheckAllSelect.Group;
			this.yvboxOrders.Add(this.ycheckWithoutFastSelect);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.yvboxOrders[this.ycheckWithoutFastSelect]));
			w11.Position = 2;
			w11.Expand = false;
			w11.Fill = false;
			this.GtkAlignmentOrders.Add(this.yvboxOrders);
			this.frameOrders.Add(this.GtkAlignmentOrders);
			this.labelOrders = new global::Gtk.Label();
			this.labelOrders.Name = "labelOrders";
			this.labelOrders.LabelProp = global::Mono.Unix.Catalog.GetString("<b>Отображать заказы:</b>");
			this.labelOrders.UseMarkup = true;
			this.frameOrders.LabelWidget = this.labelOrders;
			this.vbox1.Add(this.frameOrders);
			global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.frameOrders]));
			w14.Position = 4;
			w14.Expand = false;
			w14.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.yhboxInterval = new global::Gamma.GtkWidgets.yHBox();
			this.yhboxInterval.Name = "yhboxInterval";
			this.yhboxInterval.Spacing = 6;
			// Container child yhboxInterval.Gtk.Box+BoxChild
			this.frameInterval = new global::Gtk.Frame();
			this.frameInterval.Name = "frameInterval";
			this.frameInterval.ShadowType = ((global::Gtk.ShadowType)(0));
			// Container child frameInterval.Gtk.Container+ContainerChild
			this.GtkAlignmentInterval = new global::Gtk.Alignment(0F, 0F, 1F, 1F);
			this.GtkAlignmentInterval.Name = "GtkAlignmentInterval";
			this.GtkAlignmentInterval.LeftPadding = ((uint)(12));
			// Container child GtkAlignmentInterval.Gtk.Container+ContainerChild
			this.yvboxInterval = new global::Gamma.GtkWidgets.yVBox();
			this.yvboxInterval.Name = "yvboxInterval";
			this.yvboxInterval.Spacing = 6;
			// Container child yvboxInterval.Gtk.Box+BoxChild
			this.ycheckIntervalFromCreateTime = new global::Gamma.GtkWidgets.yRadioButton();
			this.ycheckIntervalFromCreateTime.CanFocus = true;
			this.ycheckIntervalFromCreateTime.Name = "ycheckIntervalFromCreateTime";
			this.ycheckIntervalFromCreateTime.Label = global::Mono.Unix.Catalog.GetString("Создания заказа");
			this.ycheckIntervalFromCreateTime.DrawIndicator = true;
			this.ycheckIntervalFromCreateTime.UseUnderline = true;
			this.ycheckIntervalFromCreateTime.Group = new global::GLib.SList(global::System.IntPtr.Zero);
			this.yvboxInterval.Add(this.ycheckIntervalFromCreateTime);
			global::Gtk.Box.BoxChild w15 = ((global::Gtk.Box.BoxChild)(this.yvboxInterval[this.ycheckIntervalFromCreateTime]));
			w15.Position = 0;
			w15.Expand = false;
			w15.Fill = false;
			// Container child yvboxInterval.Gtk.Box+BoxChild
			this.ycheckIntervalFromTransferTime = new global::Gamma.GtkWidgets.yRadioButton();
			this.ycheckIntervalFromTransferTime.CanFocus = true;
			this.ycheckIntervalFromTransferTime.Name = "ycheckIntervalFromTransferTime";
			this.ycheckIntervalFromTransferTime.Label = global::Mono.Unix.Catalog.GetString("Переноса заказа в МЛ водителя");
			this.ycheckIntervalFromTransferTime.DrawIndicator = true;
			this.ycheckIntervalFromTransferTime.UseUnderline = true;
			this.ycheckIntervalFromTransferTime.Group = this.ycheckIntervalFromCreateTime.Group;
			this.yvboxInterval.Add(this.ycheckIntervalFromTransferTime);
			global::Gtk.Box.BoxChild w16 = ((global::Gtk.Box.BoxChild)(this.yvboxInterval[this.ycheckIntervalFromTransferTime]));
			w16.Position = 1;
			w16.Expand = false;
			w16.Fill = false;
			// Container child yvboxInterval.Gtk.Box+BoxChild
			this.ycheckIntervalFromFirstAddress = new global::Gamma.GtkWidgets.yRadioButton();
			this.ycheckIntervalFromFirstAddress.CanFocus = true;
			this.ycheckIntervalFromFirstAddress.Name = "ycheckIntervalFromFirstAddress";
			this.ycheckIntervalFromFirstAddress.Label = global::Mono.Unix.Catalog.GetString("Попадания в первый МЛ");
			this.ycheckIntervalFromFirstAddress.DrawIndicator = true;
			this.ycheckIntervalFromFirstAddress.UseUnderline = true;
			this.ycheckIntervalFromFirstAddress.Group = this.ycheckIntervalFromCreateTime.Group;
			this.yvboxInterval.Add(this.ycheckIntervalFromFirstAddress);
			global::Gtk.Box.BoxChild w17 = ((global::Gtk.Box.BoxChild)(this.yvboxInterval[this.ycheckIntervalFromFirstAddress]));
			w17.Position = 2;
			w17.Expand = false;
			w17.Fill = false;
			this.GtkAlignmentInterval.Add(this.yvboxInterval);
			this.frameInterval.Add(this.GtkAlignmentInterval);
			this.labelInterval = new global::Gtk.Label();
			this.labelInterval.Name = "labelInterval";
			this.labelInterval.LabelProp = global::Mono.Unix.Catalog.GetString("<b>Для экспресс-доставок считать интервал от времени:</b>");
			this.labelInterval.UseMarkup = true;
			this.frameInterval.LabelWidget = this.labelInterval;
			this.yhboxInterval.Add(this.frameInterval);
			global::Gtk.Box.BoxChild w20 = ((global::Gtk.Box.BoxChild)(this.yhboxInterval[this.frameInterval]));
			w20.Position = 0;
			w20.Expand = false;
			w20.Fill = false;
			this.vbox1.Add(this.yhboxInterval);
			global::Gtk.Box.BoxChild w21 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.yhboxInterval]));
			w21.Position = 5;
			w21.Expand = false;
			w21.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.buttonCreateReport = new global::Gamma.GtkWidgets.yButton();
			this.buttonCreateReport.CanFocus = true;
			this.buttonCreateReport.Name = "buttonCreateReport";
			this.buttonCreateReport.UseUnderline = true;
			this.buttonCreateReport.Label = global::Mono.Unix.Catalog.GetString("Сформировать отчет");
			this.vbox1.Add(this.buttonCreateReport);
			global::Gtk.Box.BoxChild w22 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.buttonCreateReport]));
			w22.Position = 6;
			w22.Expand = false;
			w22.Fill = false;
			this.Add(this.vbox1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}

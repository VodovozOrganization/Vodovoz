﻿
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.Views.Reports
{
	public partial class EdoUpdReportView
	{
		private global::Gtk.HBox hbox1;

		private global::Gtk.HBox hboxMain;

		private global::Gtk.VBox parametersContainer;

		private global::Gtk.Table table1;

		private global::QS.Widgets.GtkUI.DateRangePicker rangeDate;

		private global::QS.Widgets.GtkUI.SpecialListComboBox speccomboOrganization;

		private global::Gamma.Widgets.yEnumComboBox yenumcomboboxReportType;

		private global::Gamma.GtkWidgets.yLabel ylabel3;

		private global::Gamma.GtkWidgets.yLabel ylabel4;

		private global::Gamma.GtkWidgets.yLabel ylabel5;

		private global::Gamma.GtkWidgets.yButton ybuttonSave;

		private global::Gamma.GtkWidgets.yButton ybuttonCreateReport;

		private global::Gtk.EventBox eventboxArrow;

		private global::Gtk.VBox vbox4;

		private global::Gtk.VSeparator vseparator1;

		private global::Gtk.Arrow arrowSlider;

		private global::Gtk.Label labelTitle;

		private global::Gtk.VSeparator vseparator2;

		private global::Gtk.ScrolledWindow GtkScrolledWindow;

		private global::Gamma.GtkWidgets.yTreeView ytreeviewReport;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.Views.Reports.EdoUpdReportView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.Views.Reports.EdoUpdReportView";
			// Container child Vodovoz.Views.Reports.EdoUpdReportView.Gtk.Container+ContainerChild
			this.hbox1 = new global::Gtk.HBox();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.hboxMain = new global::Gtk.HBox();
			this.hboxMain.Name = "hboxMain";
			this.hboxMain.Spacing = 6;
			// Container child hboxMain.Gtk.Box+BoxChild
			this.parametersContainer = new global::Gtk.VBox();
			this.parametersContainer.Name = "parametersContainer";
			this.parametersContainer.Spacing = 6;
			// Container child parametersContainer.Gtk.Box+BoxChild
			this.table1 = new global::Gtk.Table(((uint)(3)), ((uint)(2)), false);
			this.table1.Name = "table1";
			this.table1.ColumnSpacing = ((uint)(6));
			// Container child table1.Gtk.Table+TableChild
			this.rangeDate = new global::QS.Widgets.GtkUI.DateRangePicker();
			this.rangeDate.Events = ((global::Gdk.EventMask)(256));
			this.rangeDate.Name = "rangeDate";
			this.rangeDate.StartDate = new global::System.DateTime(0);
			this.rangeDate.EndDate = new global::System.DateTime(0);
			this.table1.Add(this.rangeDate);
			global::Gtk.Table.TableChild w1 = ((global::Gtk.Table.TableChild)(this.table1[this.rangeDate]));
			w1.LeftAttach = ((uint)(1));
			w1.RightAttach = ((uint)(2));
			w1.XOptions = ((global::Gtk.AttachOptions)(4));
			w1.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.speccomboOrganization = new global::QS.Widgets.GtkUI.SpecialListComboBox();
			this.speccomboOrganization.Name = "speccomboOrganization";
			this.speccomboOrganization.AddIfNotExist = false;
			this.speccomboOrganization.DefaultFirst = false;
			this.speccomboOrganization.ShowSpecialStateAll = false;
			this.speccomboOrganization.ShowSpecialStateNot = false;
			this.table1.Add(this.speccomboOrganization);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.table1[this.speccomboOrganization]));
			w2.TopAttach = ((uint)(1));
			w2.BottomAttach = ((uint)(2));
			w2.LeftAttach = ((uint)(1));
			w2.RightAttach = ((uint)(2));
			w2.XOptions = ((global::Gtk.AttachOptions)(4));
			w2.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.yenumcomboboxReportType = new global::Gamma.Widgets.yEnumComboBox();
			this.yenumcomboboxReportType.Name = "yenumcomboboxReportType";
			this.yenumcomboboxReportType.ShowSpecialStateAll = false;
			this.yenumcomboboxReportType.ShowSpecialStateNot = false;
			this.yenumcomboboxReportType.UseShortTitle = false;
			this.yenumcomboboxReportType.DefaultFirst = false;
			this.table1.Add(this.yenumcomboboxReportType);
			global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.table1[this.yenumcomboboxReportType]));
			w3.TopAttach = ((uint)(2));
			w3.BottomAttach = ((uint)(3));
			w3.LeftAttach = ((uint)(1));
			w3.RightAttach = ((uint)(2));
			w3.XOptions = ((global::Gtk.AttachOptions)(4));
			w3.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabel3 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel3.Name = "ylabel3";
			this.ylabel3.Xalign = 1F;
			this.ylabel3.LabelProp = global::Mono.Unix.Catalog.GetString("Период:");
			this.table1.Add(this.ylabel3);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabel3]));
			w4.XOptions = ((global::Gtk.AttachOptions)(4));
			w4.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabel4 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel4.Name = "ylabel4";
			this.ylabel4.Xalign = 1F;
			this.ylabel4.LabelProp = global::Mono.Unix.Catalog.GetString("Тип:");
			this.table1.Add(this.ylabel4);
			global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabel4]));
			w5.TopAttach = ((uint)(2));
			w5.BottomAttach = ((uint)(3));
			w5.XOptions = ((global::Gtk.AttachOptions)(4));
			w5.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabel5 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel5.Name = "ylabel5";
			this.ylabel5.Xalign = 1F;
			this.ylabel5.LabelProp = global::Mono.Unix.Catalog.GetString("Организация:");
			this.table1.Add(this.ylabel5);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabel5]));
			w6.TopAttach = ((uint)(1));
			w6.BottomAttach = ((uint)(2));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			this.parametersContainer.Add(this.table1);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.parametersContainer[this.table1]));
			w7.Position = 0;
			w7.Expand = false;
			w7.Fill = false;
			// Container child parametersContainer.Gtk.Box+BoxChild
			this.ybuttonSave = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonSave.CanFocus = true;
			this.ybuttonSave.Name = "ybuttonSave";
			this.ybuttonSave.UseUnderline = true;
			this.ybuttonSave.Label = global::Mono.Unix.Catalog.GetString("Сохранить");
			this.parametersContainer.Add(this.ybuttonSave);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.parametersContainer[this.ybuttonSave]));
			w8.PackType = ((global::Gtk.PackType)(1));
			w8.Position = 1;
			w8.Expand = false;
			w8.Fill = false;
			// Container child parametersContainer.Gtk.Box+BoxChild
			this.ybuttonCreateReport = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonCreateReport.CanFocus = true;
			this.ybuttonCreateReport.Name = "ybuttonCreateReport";
			this.ybuttonCreateReport.UseUnderline = true;
			this.ybuttonCreateReport.Label = global::Mono.Unix.Catalog.GetString("Сформировать отчет");
			this.parametersContainer.Add(this.ybuttonCreateReport);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.parametersContainer[this.ybuttonCreateReport]));
			w9.PackType = ((global::Gtk.PackType)(1));
			w9.Position = 2;
			w9.Expand = false;
			w9.Fill = false;
			this.hboxMain.Add(this.parametersContainer);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.hboxMain[this.parametersContainer]));
			w10.Position = 0;
			w10.Expand = false;
			w10.Fill = false;
			// Container child hboxMain.Gtk.Box+BoxChild
			this.eventboxArrow = new global::Gtk.EventBox();
			this.eventboxArrow.Name = "eventboxArrow";
			// Container child eventboxArrow.Gtk.Container+ContainerChild
			this.vbox4 = new global::Gtk.VBox();
			this.vbox4.Name = "vbox4";
			this.vbox4.Spacing = 6;
			// Container child vbox4.Gtk.Box+BoxChild
			this.vseparator1 = new global::Gtk.VSeparator();
			this.vseparator1.Name = "vseparator1";
			this.vbox4.Add(this.vseparator1);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.vbox4[this.vseparator1]));
			w11.Position = 0;
			// Container child vbox4.Gtk.Box+BoxChild
			this.arrowSlider = new global::Gtk.Arrow(((global::Gtk.ArrowType)(2)), ((global::Gtk.ShadowType)(2)));
			this.arrowSlider.Name = "arrowSlider";
			this.vbox4.Add(this.arrowSlider);
			global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.vbox4[this.arrowSlider]));
			w12.Position = 1;
			w12.Expand = false;
			w12.Fill = false;
			// Container child vbox4.Gtk.Box+BoxChild
			this.labelTitle = new global::Gtk.Label();
			this.labelTitle.Name = "labelTitle";
			this.labelTitle.LabelProp = global::Mono.Unix.Catalog.GetString("Параметры");
			this.labelTitle.SingleLineMode = true;
			this.labelTitle.Angle = 90D;
			this.vbox4.Add(this.labelTitle);
			global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.vbox4[this.labelTitle]));
			w13.Position = 2;
			w13.Expand = false;
			w13.Fill = false;
			// Container child vbox4.Gtk.Box+BoxChild
			this.vseparator2 = new global::Gtk.VSeparator();
			this.vseparator2.Name = "vseparator2";
			this.vbox4.Add(this.vseparator2);
			global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.vbox4[this.vseparator2]));
			w14.Position = 3;
			this.eventboxArrow.Add(this.vbox4);
			this.hboxMain.Add(this.eventboxArrow);
			global::Gtk.Box.BoxChild w16 = ((global::Gtk.Box.BoxChild)(this.hboxMain[this.eventboxArrow]));
			w16.Position = 1;
			w16.Expand = false;
			w16.Fill = false;
			this.hbox1.Add(this.hboxMain);
			global::Gtk.Box.BoxChild w17 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.hboxMain]));
			w17.Position = 0;
			w17.Expand = false;
			w17.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.ytreeviewReport = new global::Gamma.GtkWidgets.yTreeView();
			this.ytreeviewReport.CanFocus = true;
			this.ytreeviewReport.Name = "ytreeviewReport";
			this.GtkScrolledWindow.Add(this.ytreeviewReport);
			this.hbox1.Add(this.GtkScrolledWindow);
			global::Gtk.Box.BoxChild w19 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.GtkScrolledWindow]));
			w19.Position = 1;
			this.Add(this.hbox1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
			this.eventboxArrow.ButtonPressEvent += new global::Gtk.ButtonPressEventHandler(this.OnEventboxArrowButtonPressEvent);
		}
	}
}

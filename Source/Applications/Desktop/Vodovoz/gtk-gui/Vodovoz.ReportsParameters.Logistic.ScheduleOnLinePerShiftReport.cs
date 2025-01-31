
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.ReportsParameters.Logistic
{
	public partial class ScheduleOnLinePerShiftReport
	{
		private global::Gtk.VBox vbox1;

		private global::Gtk.HBox hbox1;

		private global::Gtk.Label label1;

		private global::QS.Widgets.GtkUI.DateRangePicker dateperiodpicker;

		private global::Vodovoz.ViewWidgets.GeographicGroupsToStringWidget geographicGroup;

		private global::Gamma.GtkWidgets.yLabel ylabelTypeOfUse;

		private global::Gtk.ScrolledWindow GtkScrolledWindowTypeOfUse;

		private global::Gamma.Widgets.EnumCheckList enumcheckCarTypeOfUse;

		private global::Gamma.GtkWidgets.yLabel ylabelOwnType;

		private global::Gtk.ScrolledWindow GtkScrolledWindowOwnType;

		private global::Gamma.Widgets.EnumCheckList enumcheckCarOwnType;

		private global::Gamma.GtkWidgets.yButton buttonCreateReport;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.ReportsParameters.Logistic.ScheduleOnLinePerShiftReport
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.ReportsParameters.Logistic.ScheduleOnLinePerShiftReport";
			// Container child Vodovoz.ReportsParameters.Logistic.ScheduleOnLinePerShiftReport.Gtk.Container+ContainerChild
			this.vbox1 = new global::Gtk.VBox();
			this.vbox1.Name = "vbox1";
			this.vbox1.Spacing = 6;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hbox1 = new global::Gtk.HBox();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.label1 = new global::Gtk.Label();
			this.label1.Name = "label1";
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString("Дата:");
			this.hbox1.Add(this.label1);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.label1]));
			w1.Position = 0;
			w1.Expand = false;
			w1.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.dateperiodpicker = new global::QS.Widgets.GtkUI.DateRangePicker();
			this.dateperiodpicker.Events = ((global::Gdk.EventMask)(256));
			this.dateperiodpicker.Name = "dateperiodpicker";
			this.dateperiodpicker.StartDate = new global::System.DateTime(0);
			this.dateperiodpicker.EndDate = new global::System.DateTime(0);
			this.hbox1.Add(this.dateperiodpicker);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.dateperiodpicker]));
			w2.Position = 1;
			this.vbox1.Add(this.hbox1);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hbox1]));
			w3.Position = 0;
			w3.Expand = false;
			w3.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.geographicGroup = new global::Vodovoz.ViewWidgets.GeographicGroupsToStringWidget();
			this.geographicGroup.Events = ((global::Gdk.EventMask)(256));
			this.geographicGroup.Name = "geographicGroup";
			this.geographicGroup.Label = "";
			this.vbox1.Add(this.geographicGroup);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.geographicGroup]));
			w4.Position = 1;
			w4.Expand = false;
			w4.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.ylabelTypeOfUse = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelTypeOfUse.Name = "ylabelTypeOfUse";
			this.ylabelTypeOfUse.LabelProp = global::Mono.Unix.Catalog.GetString("Тип ТС:");
			this.vbox1.Add(this.ylabelTypeOfUse);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.ylabelTypeOfUse]));
			w5.Position = 2;
			w5.Expand = false;
			w5.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.GtkScrolledWindowTypeOfUse = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindowTypeOfUse.Name = "GtkScrolledWindowTypeOfUse";
			this.GtkScrolledWindowTypeOfUse.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindowTypeOfUse.Gtk.Container+ContainerChild
			global::Gtk.Viewport w6 = new global::Gtk.Viewport();
			w6.ShadowType = ((global::Gtk.ShadowType)(0));
			// Container child GtkViewport.Gtk.Container+ContainerChild
			this.enumcheckCarTypeOfUse = new global::Gamma.Widgets.EnumCheckList();
			this.enumcheckCarTypeOfUse.Name = "enumcheckCarTypeOfUse";
			w6.Add(this.enumcheckCarTypeOfUse);
			this.GtkScrolledWindowTypeOfUse.Add(w6);
			this.vbox1.Add(this.GtkScrolledWindowTypeOfUse);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.GtkScrolledWindowTypeOfUse]));
			w9.Position = 3;
			w9.Expand = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.ylabelOwnType = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelOwnType.Name = "ylabelOwnType";
			this.ylabelOwnType.LabelProp = global::Mono.Unix.Catalog.GetString("Принадлежность ТС:");
			this.vbox1.Add(this.ylabelOwnType);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.ylabelOwnType]));
			w10.Position = 4;
			w10.Expand = false;
			w10.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.GtkScrolledWindowOwnType = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindowOwnType.Name = "GtkScrolledWindowOwnType";
			this.GtkScrolledWindowOwnType.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindowOwnType.Gtk.Container+ContainerChild
			global::Gtk.Viewport w11 = new global::Gtk.Viewport();
			w11.ShadowType = ((global::Gtk.ShadowType)(0));
			// Container child GtkViewport1.Gtk.Container+ContainerChild
			this.enumcheckCarOwnType = new global::Gamma.Widgets.EnumCheckList();
			this.enumcheckCarOwnType.Name = "enumcheckCarOwnType";
			w11.Add(this.enumcheckCarOwnType);
			this.GtkScrolledWindowOwnType.Add(w11);
			this.vbox1.Add(this.GtkScrolledWindowOwnType);
			global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.GtkScrolledWindowOwnType]));
			w14.Position = 5;
			w14.Expand = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.buttonCreateReport = new global::Gamma.GtkWidgets.yButton();
			this.buttonCreateReport.CanFocus = true;
			this.buttonCreateReport.Name = "buttonCreateReport";
			this.buttonCreateReport.UseUnderline = true;
			this.buttonCreateReport.Label = global::Mono.Unix.Catalog.GetString("Сформировать отчет");
			this.vbox1.Add(this.buttonCreateReport);
			global::Gtk.Box.BoxChild w15 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.buttonCreateReport]));
			w15.PackType = ((global::Gtk.PackType)(1));
			w15.Position = 7;
			w15.Expand = false;
			w15.Fill = false;
			this.Add(this.vbox1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}

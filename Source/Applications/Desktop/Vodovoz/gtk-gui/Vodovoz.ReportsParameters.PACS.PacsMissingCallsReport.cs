﻿
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.ReportsParameters.PACS
{
	public partial class PacsMissingCallsReport
	{
		private global::Gtk.VBox vbox2;

		private global::Gtk.Table table2;

		private global::Gtk.Label label2;

		private global::QS.Widgets.GtkUI.DateRangePicker ydateperiodpicker;

		private global::Gamma.GtkWidgets.yButton buttonCreateRepot;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.ReportsParameters.PACS.PacsMissingCallsReport
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.ReportsParameters.PACS.PacsMissingCallsReport";
			// Container child Vodovoz.ReportsParameters.PACS.PacsMissingCallsReport.Gtk.Container+ContainerChild
			this.vbox2 = new global::Gtk.VBox();
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.table2 = new global::Gtk.Table(((uint)(3)), ((uint)(3)), false);
			this.table2.Name = "table2";
			this.table2.RowSpacing = ((uint)(6));
			this.table2.ColumnSpacing = ((uint)(6));
			// Container child table2.Gtk.Table+TableChild
			this.label2 = new global::Gtk.Label();
			this.label2.Name = "label2";
			this.label2.Xalign = 1F;
			this.label2.LabelProp = global::Mono.Unix.Catalog.GetString("Дата:");
			this.table2.Add(this.label2);
			global::Gtk.Table.TableChild w1 = ((global::Gtk.Table.TableChild)(this.table2[this.label2]));
			w1.XOptions = ((global::Gtk.AttachOptions)(4));
			w1.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.ydateperiodpicker = new global::QS.Widgets.GtkUI.DateRangePicker();
			this.ydateperiodpicker.Events = ((global::Gdk.EventMask)(256));
			this.ydateperiodpicker.Name = "ydateperiodpicker";
			this.ydateperiodpicker.StartDate = new global::System.DateTime(0);
			this.ydateperiodpicker.EndDate = new global::System.DateTime(0);
			this.table2.Add(this.ydateperiodpicker);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.table2[this.ydateperiodpicker]));
			w2.LeftAttach = ((uint)(1));
			w2.RightAttach = ((uint)(3));
			w2.YOptions = ((global::Gtk.AttachOptions)(4));
			this.vbox2.Add(this.table2);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.table2]));
			w3.Position = 0;
			// Container child vbox2.Gtk.Box+BoxChild
			this.buttonCreateRepot = new global::Gamma.GtkWidgets.yButton();
			this.buttonCreateRepot.CanFocus = true;
			this.buttonCreateRepot.Name = "buttonCreateRepot";
			this.buttonCreateRepot.UseUnderline = true;
			this.buttonCreateRepot.Label = global::Mono.Unix.Catalog.GetString("Сформировать отчет");
			this.vbox2.Add(this.buttonCreateRepot);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.buttonCreateRepot]));
			w4.PackType = ((global::Gtk.PackType)(1));
			w4.Position = 1;
			w4.Expand = false;
			w4.Fill = false;
			this.Add(this.vbox2);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}

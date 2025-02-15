﻿
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.ReportsParameters.Bottles
{
	public partial class ProfitabilityBottlesByStockReport
	{
		private global::Gtk.VBox vbox2;

		private global::Gtk.Table table1;

		private global::QS.Widgets.GtkUI.DateRangePicker dtrngPeriod;

		private global::Gamma.GtkWidgets.yLabel lblDiscountPct;

		private global::Gamma.GtkWidgets.yLabel lblPeriod;

		private global::Gamma.Widgets.ySpecComboBox specCmbDiscountPct;

		private global::Gamma.GtkWidgets.yButton buttonRun;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.ReportsParameters.Bottles.ProfitabilityBottlesByStockReport
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.ReportsParameters.Bottles.ProfitabilityBottlesByStockReport";
			// Container child Vodovoz.ReportsParameters.Bottles.ProfitabilityBottlesByStockReport.Gtk.Container+ContainerChild
			this.vbox2 = new global::Gtk.VBox();
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.table1 = new global::Gtk.Table(((uint)(2)), ((uint)(2)), false);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(6));
			this.table1.BorderWidth = ((uint)(6));
			// Container child table1.Gtk.Table+TableChild
			this.dtrngPeriod = new global::QS.Widgets.GtkUI.DateRangePicker();
			this.dtrngPeriod.Events = ((global::Gdk.EventMask)(256));
			this.dtrngPeriod.Name = "dtrngPeriod";
			this.dtrngPeriod.StartDate = new global::System.DateTime(0);
			this.dtrngPeriod.EndDate = new global::System.DateTime(0);
			this.table1.Add(this.dtrngPeriod);
			global::Gtk.Table.TableChild w1 = ((global::Gtk.Table.TableChild)(this.table1[this.dtrngPeriod]));
			w1.LeftAttach = ((uint)(1));
			w1.RightAttach = ((uint)(2));
			w1.XOptions = ((global::Gtk.AttachOptions)(4));
			w1.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.lblDiscountPct = new global::Gamma.GtkWidgets.yLabel();
			this.lblDiscountPct.Name = "lblDiscountPct";
			this.lblDiscountPct.Xalign = 1F;
			this.lblDiscountPct.LabelProp = global::Mono.Unix.Catalog.GetString("Скидка:");
			this.table1.Add(this.lblDiscountPct);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.table1[this.lblDiscountPct]));
			w2.TopAttach = ((uint)(1));
			w2.BottomAttach = ((uint)(2));
			w2.XOptions = ((global::Gtk.AttachOptions)(4));
			w2.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.lblPeriod = new global::Gamma.GtkWidgets.yLabel();
			this.lblPeriod.Name = "lblPeriod";
			this.lblPeriod.Xalign = 1F;
			this.lblPeriod.LabelProp = global::Mono.Unix.Catalog.GetString("Период:");
			this.table1.Add(this.lblPeriod);
			global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.table1[this.lblPeriod]));
			w3.XOptions = ((global::Gtk.AttachOptions)(4));
			w3.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.specCmbDiscountPct = new global::Gamma.Widgets.ySpecComboBox();
			this.specCmbDiscountPct.Name = "specCmbDiscountPct";
			this.specCmbDiscountPct.AddIfNotExist = false;
			this.specCmbDiscountPct.DefaultFirst = false;
			this.specCmbDiscountPct.ShowSpecialStateAll = true;
			this.specCmbDiscountPct.ShowSpecialStateNot = false;
			this.table1.Add(this.specCmbDiscountPct);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.table1[this.specCmbDiscountPct]));
			w4.TopAttach = ((uint)(1));
			w4.BottomAttach = ((uint)(2));
			w4.LeftAttach = ((uint)(1));
			w4.RightAttach = ((uint)(2));
			w4.XOptions = ((global::Gtk.AttachOptions)(4));
			w4.YOptions = ((global::Gtk.AttachOptions)(4));
			this.vbox2.Add(this.table1);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.table1]));
			w5.Position = 0;
			w5.Expand = false;
			w5.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.buttonRun = new global::Gamma.GtkWidgets.yButton();
			this.buttonRun.CanFocus = true;
			this.buttonRun.Name = "buttonRun";
			this.buttonRun.UseUnderline = true;
			this.buttonRun.Label = global::Mono.Unix.Catalog.GetString("Сформировать отчет");
			this.vbox2.Add(this.buttonRun);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.buttonRun]));
			w6.PackType = ((global::Gtk.PackType)(1));
			w6.Position = 2;
			w6.Expand = false;
			w6.Fill = false;
			this.Add(this.vbox2);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}

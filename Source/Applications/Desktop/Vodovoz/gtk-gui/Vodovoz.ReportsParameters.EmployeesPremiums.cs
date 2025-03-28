﻿
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.ReportsParameters
{
	public partial class EmployeesPremiums
	{
		private global::Gtk.Table table1;

		private global::Gamma.GtkWidgets.yButton buttonRun;

		private global::QS.Widgets.GtkUI.DateRangePicker dateperiodpicker1;

		private global::QS.Widgets.GtkUI.EntityViewModelEntry evmeDriver;

		private global::Gtk.Label label1;

		private global::Gtk.Label labelCategory;

		private global::Gamma.GtkWidgets.yRadioButton radioCatAll;

		private global::Gamma.GtkWidgets.yRadioButton radioCatDriver;

		private global::Gamma.GtkWidgets.yRadioButton radioCatForwarder;

		private global::Gamma.GtkWidgets.yRadioButton radioCatOffice;

		private global::Gamma.GtkWidgets.yLabel ylabel2;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.ReportsParameters.EmployeesPremiums
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.ReportsParameters.EmployeesPremiums";
			// Container child Vodovoz.ReportsParameters.EmployeesPremiums.Gtk.Container+ContainerChild
			this.table1 = new global::Gtk.Table(((uint)(7)), ((uint)(2)), false);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(6));
			this.table1.BorderWidth = ((uint)(6));
			// Container child table1.Gtk.Table+TableChild
			this.buttonRun = new global::Gamma.GtkWidgets.yButton();
			this.buttonRun.Sensitive = false;
			this.buttonRun.CanFocus = true;
			this.buttonRun.Name = "buttonRun";
			this.buttonRun.UseUnderline = true;
			this.buttonRun.Label = global::Mono.Unix.Catalog.GetString("Сформировать отчет");
			this.table1.Add(this.buttonRun);
			global::Gtk.Table.TableChild w1 = ((global::Gtk.Table.TableChild)(this.table1[this.buttonRun]));
			w1.TopAttach = ((uint)(6));
			w1.BottomAttach = ((uint)(7));
			w1.RightAttach = ((uint)(2));
			w1.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.dateperiodpicker1 = new global::QS.Widgets.GtkUI.DateRangePicker();
			this.dateperiodpicker1.Events = ((global::Gdk.EventMask)(256));
			this.dateperiodpicker1.Name = "dateperiodpicker1";
			this.dateperiodpicker1.StartDate = new global::System.DateTime(0);
			this.dateperiodpicker1.EndDate = new global::System.DateTime(0);
			this.table1.Add(this.dateperiodpicker1);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.table1[this.dateperiodpicker1]));
			w2.LeftAttach = ((uint)(1));
			w2.RightAttach = ((uint)(2));
			w2.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.evmeDriver = new global::QS.Widgets.GtkUI.EntityViewModelEntry();
			this.evmeDriver.Events = ((global::Gdk.EventMask)(256));
			this.evmeDriver.Name = "evmeDriver";
			this.evmeDriver.CanEditReference = true;
			this.table1.Add(this.evmeDriver);
			global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.table1[this.evmeDriver]));
			w3.TopAttach = ((uint)(5));
			w3.BottomAttach = ((uint)(6));
			w3.LeftAttach = ((uint)(1));
			w3.RightAttach = ((uint)(2));
			w3.XOptions = ((global::Gtk.AttachOptions)(4));
			w3.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label1 = new global::Gtk.Label();
			this.label1.Name = "label1";
			this.label1.Xalign = 1F;
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString("Период:");
			this.table1.Add(this.label1);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.table1[this.label1]));
			w4.XOptions = ((global::Gtk.AttachOptions)(4));
			w4.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.labelCategory = new global::Gtk.Label();
			this.labelCategory.Name = "labelCategory";
			this.labelCategory.Xalign = 1F;
			this.labelCategory.LabelProp = global::Mono.Unix.Catalog.GetString("Категория:");
			this.table1.Add(this.labelCategory);
			global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.table1[this.labelCategory]));
			w5.TopAttach = ((uint)(1));
			w5.BottomAttach = ((uint)(2));
			w5.XOptions = ((global::Gtk.AttachOptions)(4));
			w5.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.radioCatAll = new global::Gamma.GtkWidgets.yRadioButton(global::Mono.Unix.Catalog.GetString("Все"));
			this.radioCatAll.CanFocus = true;
			this.radioCatAll.Name = "radioCatAll";
			this.radioCatAll.DrawIndicator = true;
			this.radioCatAll.UseUnderline = true;
			this.radioCatAll.Group = new global::GLib.SList(global::System.IntPtr.Zero);
			this.table1.Add(this.radioCatAll);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.table1[this.radioCatAll]));
			w6.TopAttach = ((uint)(1));
			w6.BottomAttach = ((uint)(2));
			w6.LeftAttach = ((uint)(1));
			w6.RightAttach = ((uint)(2));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.radioCatDriver = new global::Gamma.GtkWidgets.yRadioButton(global::Mono.Unix.Catalog.GetString("Водители"));
			this.radioCatDriver.CanFocus = true;
			this.radioCatDriver.Name = "radioCatDriver";
			this.radioCatDriver.DrawIndicator = true;
			this.radioCatDriver.UseUnderline = true;
			this.radioCatDriver.Group = this.radioCatAll.Group;
			this.table1.Add(this.radioCatDriver);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.table1[this.radioCatDriver]));
			w7.TopAttach = ((uint)(2));
			w7.BottomAttach = ((uint)(3));
			w7.LeftAttach = ((uint)(1));
			w7.RightAttach = ((uint)(2));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.radioCatForwarder = new global::Gamma.GtkWidgets.yRadioButton(global::Mono.Unix.Catalog.GetString("Экспедиторы"));
			this.radioCatForwarder.CanFocus = true;
			this.radioCatForwarder.Name = "radioCatForwarder";
			this.radioCatForwarder.DrawIndicator = true;
			this.radioCatForwarder.UseUnderline = true;
			this.radioCatForwarder.Group = this.radioCatAll.Group;
			this.table1.Add(this.radioCatForwarder);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.table1[this.radioCatForwarder]));
			w8.TopAttach = ((uint)(3));
			w8.BottomAttach = ((uint)(4));
			w8.LeftAttach = ((uint)(1));
			w8.RightAttach = ((uint)(2));
			w8.XOptions = ((global::Gtk.AttachOptions)(4));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.radioCatOffice = new global::Gamma.GtkWidgets.yRadioButton(global::Mono.Unix.Catalog.GetString("Офисные работники"));
			this.radioCatOffice.CanFocus = true;
			this.radioCatOffice.Name = "radioCatOffice";
			this.radioCatOffice.DrawIndicator = true;
			this.radioCatOffice.UseUnderline = true;
			this.radioCatOffice.Group = this.radioCatAll.Group;
			this.table1.Add(this.radioCatOffice);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.table1[this.radioCatOffice]));
			w9.TopAttach = ((uint)(4));
			w9.BottomAttach = ((uint)(5));
			w9.LeftAttach = ((uint)(1));
			w9.RightAttach = ((uint)(2));
			w9.XOptions = ((global::Gtk.AttachOptions)(4));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabel2 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel2.Name = "ylabel2";
			this.ylabel2.Xalign = 1F;
			this.ylabel2.LabelProp = global::Mono.Unix.Catalog.GetString("Сотрудник:");
			this.table1.Add(this.ylabel2);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabel2]));
			w10.TopAttach = ((uint)(5));
			w10.BottomAttach = ((uint)(6));
			w10.XOptions = ((global::Gtk.AttachOptions)(4));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			this.Add(this.table1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}

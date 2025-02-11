
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.ReportsParameters
{
	public partial class ChainStoreDelayReport
	{
		private global::Gtk.Table table1;

		private global::Gamma.GtkWidgets.yButton buttonRun;

		private global::QS.Widgets.GtkUI.EntityViewModelEntry entityviewmodelentryCounterparty;

		private global::QS.Widgets.GtkUI.EntityViewModelEntry entityviewmodelentryOrderAuthor;

		private global::QS.Widgets.GtkUI.EntityViewModelEntry entityviewmodelentrySellManager;

		private global::Gtk.Label label1;

		private global::QS.Widgets.GtkUI.SpecialListComboBox speciallistcomboboxReportBy;

		private global::QS.Widgets.GtkUI.DatePicker ydatepicker;

		private global::Gamma.GtkWidgets.yLabel ylabel2;

		private global::Gamma.GtkWidgets.yLabel ylabel3;

		private global::Gamma.GtkWidgets.yLabel ylabel4;

		private global::Gamma.GtkWidgets.yLabel ylabelReportBy;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.ReportsParameters.ChainStoreDelayReport
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.ReportsParameters.ChainStoreDelayReport";
			// Container child Vodovoz.ReportsParameters.ChainStoreDelayReport.Gtk.Container+ContainerChild
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
			w1.XOptions = ((global::Gtk.AttachOptions)(0));
			w1.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.entityviewmodelentryCounterparty = new global::QS.Widgets.GtkUI.EntityViewModelEntry();
			this.entityviewmodelentryCounterparty.Events = ((global::Gdk.EventMask)(256));
			this.entityviewmodelentryCounterparty.Name = "entityviewmodelentryCounterparty";
			this.entityviewmodelentryCounterparty.CanEditReference = false;
			this.entityviewmodelentryCounterparty.CanOpenWithoutTabParent = false;
			this.table1.Add(this.entityviewmodelentryCounterparty);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.table1[this.entityviewmodelentryCounterparty]));
			w2.TopAttach = ((uint)(2));
			w2.BottomAttach = ((uint)(3));
			w2.LeftAttach = ((uint)(1));
			w2.RightAttach = ((uint)(2));
			w2.XOptions = ((global::Gtk.AttachOptions)(4));
			w2.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.entityviewmodelentryOrderAuthor = new global::QS.Widgets.GtkUI.EntityViewModelEntry();
			this.entityviewmodelentryOrderAuthor.Events = ((global::Gdk.EventMask)(256));
			this.entityviewmodelentryOrderAuthor.Name = "entityviewmodelentryOrderAuthor";
			this.entityviewmodelentryOrderAuthor.CanEditReference = false;
			this.entityviewmodelentryOrderAuthor.CanOpenWithoutTabParent = false;
			this.table1.Add(this.entityviewmodelentryOrderAuthor);
			global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.table1[this.entityviewmodelentryOrderAuthor]));
			w3.TopAttach = ((uint)(3));
			w3.BottomAttach = ((uint)(4));
			w3.LeftAttach = ((uint)(1));
			w3.RightAttach = ((uint)(2));
			w3.XOptions = ((global::Gtk.AttachOptions)(4));
			w3.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.entityviewmodelentrySellManager = new global::QS.Widgets.GtkUI.EntityViewModelEntry();
			this.entityviewmodelentrySellManager.Events = ((global::Gdk.EventMask)(256));
			this.entityviewmodelentrySellManager.Name = "entityviewmodelentrySellManager";
			this.entityviewmodelentrySellManager.CanEditReference = false;
			this.entityviewmodelentrySellManager.CanOpenWithoutTabParent = false;
			this.table1.Add(this.entityviewmodelentrySellManager);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.table1[this.entityviewmodelentrySellManager]));
			w4.TopAttach = ((uint)(4));
			w4.BottomAttach = ((uint)(5));
			w4.LeftAttach = ((uint)(1));
			w4.RightAttach = ((uint)(2));
			w4.XOptions = ((global::Gtk.AttachOptions)(4));
			w4.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label1 = new global::Gtk.Label();
			this.label1.Name = "label1";
			this.label1.Xalign = 1F;
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString("Дата");
			this.table1.Add(this.label1);
			global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.table1[this.label1]));
			w5.XOptions = ((global::Gtk.AttachOptions)(4));
			w5.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.speciallistcomboboxReportBy = new global::QS.Widgets.GtkUI.SpecialListComboBox();
			this.speciallistcomboboxReportBy.Name = "speciallistcomboboxReportBy";
			this.speciallistcomboboxReportBy.AddIfNotExist = false;
			this.speciallistcomboboxReportBy.DefaultFirst = false;
			this.speciallistcomboboxReportBy.ShowSpecialStateAll = false;
			this.speciallistcomboboxReportBy.ShowSpecialStateNot = false;
			this.table1.Add(this.speciallistcomboboxReportBy);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.table1[this.speciallistcomboboxReportBy]));
			w6.TopAttach = ((uint)(5));
			w6.BottomAttach = ((uint)(6));
			w6.LeftAttach = ((uint)(1));
			w6.RightAttach = ((uint)(2));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ydatepicker = new global::QS.Widgets.GtkUI.DatePicker();
			this.ydatepicker.Events = ((global::Gdk.EventMask)(256));
			this.ydatepicker.Name = "ydatepicker";
			this.ydatepicker.WithTime = false;
			this.ydatepicker.HideCalendarButton = false;
			this.ydatepicker.Date = new global::System.DateTime(0);
			this.ydatepicker.IsEditable = true;
			this.ydatepicker.AutoSeparation = false;
			this.ydatepicker.HideButtonClearDate = false;
			this.table1.Add(this.ydatepicker);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.table1[this.ydatepicker]));
			w7.LeftAttach = ((uint)(1));
			w7.RightAttach = ((uint)(2));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabel2 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel2.Name = "ylabel2";
			this.ylabel2.Xalign = 1F;
			this.ylabel2.LabelProp = global::Mono.Unix.Catalog.GetString("Автор заказа");
			this.table1.Add(this.ylabel2);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabel2]));
			w8.TopAttach = ((uint)(3));
			w8.BottomAttach = ((uint)(4));
			w8.XOptions = ((global::Gtk.AttachOptions)(4));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabel3 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel3.Name = "ylabel3";
			this.ylabel3.Xalign = 1F;
			this.ylabel3.LabelProp = global::Mono.Unix.Catalog.GetString("Контрагент");
			this.table1.Add(this.ylabel3);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabel3]));
			w9.TopAttach = ((uint)(2));
			w9.BottomAttach = ((uint)(3));
			w9.XOptions = ((global::Gtk.AttachOptions)(4));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabel4 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel4.Name = "ylabel4";
			this.ylabel4.Xalign = 1F;
			this.ylabel4.LabelProp = global::Mono.Unix.Catalog.GetString("Менеджер по продажам");
			this.table1.Add(this.ylabel4);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabel4]));
			w10.TopAttach = ((uint)(4));
			w10.BottomAttach = ((uint)(5));
			w10.XOptions = ((global::Gtk.AttachOptions)(4));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabelReportBy = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelReportBy.Name = "ylabelReportBy";
			this.ylabelReportBy.Xalign = 1F;
			this.ylabelReportBy.LabelProp = global::Mono.Unix.Catalog.GetString("Сформировать отчет по");
			this.table1.Add(this.ylabelReportBy);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabelReportBy]));
			w11.TopAttach = ((uint)(5));
			w11.BottomAttach = ((uint)(6));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			this.Add(this.table1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}

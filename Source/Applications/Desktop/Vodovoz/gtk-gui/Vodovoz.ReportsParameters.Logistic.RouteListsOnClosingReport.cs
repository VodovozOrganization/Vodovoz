﻿
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.ReportsParameters.Logistic
{
	public partial class RouteListsOnClosingReport
	{
		private global::Gtk.VBox vbox1;

		private global::Gtk.Table table1;

		private global::Gtk.HBox hbox3;

		private global::QS.Widgets.NullableCheckButton nullCheckVisitingMasters;

		private global::Gamma.GtkWidgets.yCheckButton ycheckTodayRouteLists;

		private global::Gamma.GtkWidgets.yLabel ylabelGeoGroup;

		private global::Gamma.GtkWidgets.yLabel ylabelTodayRouteLists;

		private global::Gamma.GtkWidgets.yLabel ylabelVisitingMasters;

		private global::Gamma.Widgets.ySpecComboBox ySpecCmbGeographicGroup;

		private global::Gtk.HSeparator hseparator2;

		private global::Gamma.GtkWidgets.yLabel ylabelTypeOfUse;

		private global::Gtk.ScrolledWindow GtkScrolledWindowTypeOfUse;

		private global::Gamma.Widgets.EnumCheckList enumcheckCarTypeOfUse;

		private global::Gtk.HSeparator hseparator3;

		private global::Gamma.GtkWidgets.yLabel ylabelOwnType;

		private global::Gtk.ScrolledWindow GtkScrolledWindowOwnType;

		private global::Gamma.Widgets.EnumCheckList enumcheckCarOwnType;

		private global::Gtk.HSeparator hseparator4;

		private global::Gamma.GtkWidgets.yLabel ylabelPeriodEndDate;

		private global::QS.Widgets.GtkUI.DatePicker dateEnd;

		private global::Gtk.HSeparator hseparator1;

		private global::Gamma.GtkWidgets.yButton buttonCreateReport;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.ReportsParameters.Logistic.RouteListsOnClosingReport
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.ReportsParameters.Logistic.RouteListsOnClosingReport";
			// Container child Vodovoz.ReportsParameters.Logistic.RouteListsOnClosingReport.Gtk.Container+ContainerChild
			this.vbox1 = new global::Gtk.VBox();
			this.vbox1.Name = "vbox1";
			this.vbox1.Spacing = 6;
			// Container child vbox1.Gtk.Box+BoxChild
			this.table1 = new global::Gtk.Table(((uint)(3)), ((uint)(2)), false);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(6));
			// Container child table1.Gtk.Table+TableChild
			this.hbox3 = new global::Gtk.HBox();
			this.hbox3.Name = "hbox3";
			this.hbox3.Spacing = 6;
			// Container child hbox3.Gtk.Box+BoxChild
			this.nullCheckVisitingMasters = new global::QS.Widgets.NullableCheckButton();
			this.nullCheckVisitingMasters.CanFocus = true;
			this.nullCheckVisitingMasters.Name = "nullCheckVisitingMasters";
			this.nullCheckVisitingMasters.UseUnderline = true;
			this.hbox3.Add(this.nullCheckVisitingMasters);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.hbox3[this.nullCheckVisitingMasters]));
			w1.Position = 0;
			w1.Expand = false;
			w1.Fill = false;
			this.table1.Add(this.hbox3);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.table1[this.hbox3]));
			w2.TopAttach = ((uint)(1));
			w2.BottomAttach = ((uint)(2));
			w2.LeftAttach = ((uint)(1));
			w2.RightAttach = ((uint)(2));
			w2.XOptions = ((global::Gtk.AttachOptions)(4));
			w2.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ycheckTodayRouteLists = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckTodayRouteLists.CanFocus = true;
			this.ycheckTodayRouteLists.Name = "ycheckTodayRouteLists";
			this.ycheckTodayRouteLists.Label = "";
			this.ycheckTodayRouteLists.DrawIndicator = true;
			this.ycheckTodayRouteLists.UseUnderline = true;
			this.ycheckTodayRouteLists.FocusOnClick = false;
			this.ycheckTodayRouteLists.Xalign = 0F;
			this.ycheckTodayRouteLists.Yalign = 0F;
			this.table1.Add(this.ycheckTodayRouteLists);
			global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.table1[this.ycheckTodayRouteLists]));
			w3.LeftAttach = ((uint)(1));
			w3.RightAttach = ((uint)(2));
			w3.XOptions = ((global::Gtk.AttachOptions)(4));
			w3.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabelGeoGroup = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelGeoGroup.Name = "ylabelGeoGroup";
			this.ylabelGeoGroup.Xalign = 1F;
			this.ylabelGeoGroup.LabelProp = global::Mono.Unix.Catalog.GetString("Район города:");
			this.table1.Add(this.ylabelGeoGroup);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabelGeoGroup]));
			w4.TopAttach = ((uint)(2));
			w4.BottomAttach = ((uint)(3));
			w4.XOptions = ((global::Gtk.AttachOptions)(4));
			w4.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabelTodayRouteLists = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelTodayRouteLists.Name = "ylabelTodayRouteLists";
			this.ylabelTodayRouteLists.Xalign = 1F;
			this.ylabelTodayRouteLists.LabelProp = global::Mono.Unix.Catalog.GetString("Включая сегодняшние МЛ:");
			this.table1.Add(this.ylabelTodayRouteLists);
			global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabelTodayRouteLists]));
			w5.XOptions = ((global::Gtk.AttachOptions)(4));
			w5.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabelVisitingMasters = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelVisitingMasters.Name = "ylabelVisitingMasters";
			this.ylabelVisitingMasters.Xalign = 1F;
			this.ylabelVisitingMasters.LabelProp = global::Mono.Unix.Catalog.GetString("Выездные мастера:");
			this.table1.Add(this.ylabelVisitingMasters);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabelVisitingMasters]));
			w6.TopAttach = ((uint)(1));
			w6.BottomAttach = ((uint)(2));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ySpecCmbGeographicGroup = new global::Gamma.Widgets.ySpecComboBox();
			this.ySpecCmbGeographicGroup.Name = "ySpecCmbGeographicGroup";
			this.ySpecCmbGeographicGroup.AddIfNotExist = false;
			this.ySpecCmbGeographicGroup.DefaultFirst = false;
			this.ySpecCmbGeographicGroup.ShowSpecialStateAll = true;
			this.ySpecCmbGeographicGroup.ShowSpecialStateNot = false;
			this.table1.Add(this.ySpecCmbGeographicGroup);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.table1[this.ySpecCmbGeographicGroup]));
			w7.TopAttach = ((uint)(2));
			w7.BottomAttach = ((uint)(3));
			w7.LeftAttach = ((uint)(1));
			w7.RightAttach = ((uint)(2));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			this.vbox1.Add(this.table1);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.table1]));
			w8.Position = 0;
			w8.Expand = false;
			w8.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hseparator2 = new global::Gtk.HSeparator();
			this.hseparator2.Name = "hseparator2";
			this.vbox1.Add(this.hseparator2);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hseparator2]));
			w9.Position = 1;
			w9.Expand = false;
			w9.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.ylabelTypeOfUse = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelTypeOfUse.Name = "ylabelTypeOfUse";
			this.ylabelTypeOfUse.Xalign = 0F;
			this.ylabelTypeOfUse.LabelProp = global::Mono.Unix.Catalog.GetString("Тип авто:");
			this.vbox1.Add(this.ylabelTypeOfUse);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.ylabelTypeOfUse]));
			w10.Position = 2;
			w10.Expand = false;
			w10.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.GtkScrolledWindowTypeOfUse = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindowTypeOfUse.Name = "GtkScrolledWindowTypeOfUse";
			this.GtkScrolledWindowTypeOfUse.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindowTypeOfUse.Gtk.Container+ContainerChild
			global::Gtk.Viewport w11 = new global::Gtk.Viewport();
			w11.ShadowType = ((global::Gtk.ShadowType)(0));
			// Container child GtkViewport1.Gtk.Container+ContainerChild
			this.enumcheckCarTypeOfUse = new global::Gamma.Widgets.EnumCheckList();
			this.enumcheckCarTypeOfUse.Name = "enumcheckCarTypeOfUse";
			w11.Add(this.enumcheckCarTypeOfUse);
			this.GtkScrolledWindowTypeOfUse.Add(w11);
			this.vbox1.Add(this.GtkScrolledWindowTypeOfUse);
			global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.GtkScrolledWindowTypeOfUse]));
			w14.Position = 3;
			w14.Expand = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hseparator3 = new global::Gtk.HSeparator();
			this.hseparator3.Name = "hseparator3";
			this.vbox1.Add(this.hseparator3);
			global::Gtk.Box.BoxChild w15 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hseparator3]));
			w15.Position = 4;
			w15.Expand = false;
			w15.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.ylabelOwnType = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelOwnType.Name = "ylabelOwnType";
			this.ylabelOwnType.Xalign = 0F;
			this.ylabelOwnType.LabelProp = global::Mono.Unix.Catalog.GetString("Принадлежность авто:");
			this.vbox1.Add(this.ylabelOwnType);
			global::Gtk.Box.BoxChild w16 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.ylabelOwnType]));
			w16.Position = 5;
			w16.Expand = false;
			w16.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.GtkScrolledWindowOwnType = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindowOwnType.Name = "GtkScrolledWindowOwnType";
			this.GtkScrolledWindowOwnType.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindowOwnType.Gtk.Container+ContainerChild
			global::Gtk.Viewport w17 = new global::Gtk.Viewport();
			w17.ShadowType = ((global::Gtk.ShadowType)(0));
			// Container child GtkViewport2.Gtk.Container+ContainerChild
			this.enumcheckCarOwnType = new global::Gamma.Widgets.EnumCheckList();
			this.enumcheckCarOwnType.Name = "enumcheckCarOwnType";
			w17.Add(this.enumcheckCarOwnType);
			this.GtkScrolledWindowOwnType.Add(w17);
			this.vbox1.Add(this.GtkScrolledWindowOwnType);
			global::Gtk.Box.BoxChild w20 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.GtkScrolledWindowOwnType]));
			w20.Position = 6;
			w20.Expand = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hseparator4 = new global::Gtk.HSeparator();
			this.hseparator4.Name = "hseparator4";
			this.vbox1.Add(this.hseparator4);
			global::Gtk.Box.BoxChild w21 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hseparator4]));
			w21.Position = 7;
			w21.Expand = false;
			w21.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.ylabelPeriodEndDate = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelPeriodEndDate.Name = "ylabelPeriodEndDate";
			this.ylabelPeriodEndDate.Xalign = 0F;
			this.ylabelPeriodEndDate.LabelProp = global::Mono.Unix.Catalog.GetString("За период до:");
			this.vbox1.Add(this.ylabelPeriodEndDate);
			global::Gtk.Box.BoxChild w22 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.ylabelPeriodEndDate]));
			w22.Position = 8;
			w22.Expand = false;
			w22.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.dateEnd = new global::QS.Widgets.GtkUI.DatePicker();
			this.dateEnd.Events = ((global::Gdk.EventMask)(256));
			this.dateEnd.Name = "dateEnd";
			this.dateEnd.WithTime = false;
			this.dateEnd.HideCalendarButton = false;
			this.dateEnd.Date = new global::System.DateTime(0);
			this.dateEnd.IsEditable = true;
			this.dateEnd.AutoSeparation = true;
			this.vbox1.Add(this.dateEnd);
			global::Gtk.Box.BoxChild w23 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.dateEnd]));
			w23.Position = 9;
			w23.Expand = false;
			w23.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hseparator1 = new global::Gtk.HSeparator();
			this.hseparator1.Name = "hseparator1";
			this.vbox1.Add(this.hseparator1);
			global::Gtk.Box.BoxChild w24 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hseparator1]));
			w24.PackType = ((global::Gtk.PackType)(1));
			w24.Position = 10;
			w24.Expand = false;
			w24.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.buttonCreateReport = new global::Gamma.GtkWidgets.yButton();
			this.buttonCreateReport.CanFocus = true;
			this.buttonCreateReport.Name = "buttonCreateReport";
			this.buttonCreateReport.UseUnderline = true;
			this.buttonCreateReport.Label = global::Mono.Unix.Catalog.GetString("Сформировать отчет");
			this.vbox1.Add(this.buttonCreateReport);
			global::Gtk.Box.BoxChild w25 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.buttonCreateReport]));
			w25.PackType = ((global::Gtk.PackType)(1));
			w25.Position = 11;
			w25.Expand = false;
			w25.Fill = false;
			this.Add(this.vbox1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}


// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.Views.Reports
{
	public partial class ChangingPaymentTypeByDriversReportView
	{
		private global::Gtk.HBox hboxMainContainer;

		private global::Gtk.HBox hboxFilters;

		private global::Gtk.VBox parametersContainer;

		private global::Gtk.Table table1;

		private global::Gamma.GtkWidgets.yButton buttonInfo;

		private global::QS.Widgets.GtkUI.SpecialListComboBox cmbGeoGroup;

		private global::QS.Widgets.GtkUI.DateRangePicker rangeDate;

		private global::Gamma.GtkWidgets.yCheckButton ycheckbuttonGroupByDriver;

		private global::Gamma.GtkWidgets.yLabel ylabel3;

		private global::Gamma.GtkWidgets.yLabel ylabelGeoGroup;

		private global::Gamma.GtkWidgets.yButton ybuttonSave;

		private global::Gamma.GtkWidgets.yButton ybuttonAbortCreateReport;

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
			// Widget Vodovoz.Views.Reports.ChangingPaymentTypeByDriversReportView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.Views.Reports.ChangingPaymentTypeByDriversReportView";
			// Container child Vodovoz.Views.Reports.ChangingPaymentTypeByDriversReportView.Gtk.Container+ContainerChild
			this.hboxMainContainer = new global::Gtk.HBox();
			this.hboxMainContainer.Name = "hboxMainContainer";
			this.hboxMainContainer.Spacing = 6;
			// Container child hboxMainContainer.Gtk.Box+BoxChild
			this.hboxFilters = new global::Gtk.HBox();
			this.hboxFilters.Name = "hboxFilters";
			this.hboxFilters.Spacing = 6;
			// Container child hboxFilters.Gtk.Box+BoxChild
			this.parametersContainer = new global::Gtk.VBox();
			this.parametersContainer.Name = "parametersContainer";
			this.parametersContainer.Spacing = 6;
			// Container child parametersContainer.Gtk.Box+BoxChild
			this.table1 = new global::Gtk.Table(((uint)(3)), ((uint)(3)), false);
			this.table1.Name = "table1";
			this.table1.ColumnSpacing = ((uint)(6));
			// Container child table1.Gtk.Table+TableChild
			this.buttonInfo = new global::Gamma.GtkWidgets.yButton();
			this.buttonInfo.TooltipMarkup = "Справка по работе с отчётом";
			this.buttonInfo.CanFocus = true;
			this.buttonInfo.Name = "buttonInfo";
			this.buttonInfo.UseUnderline = true;
			this.buttonInfo.Relief = ((global::Gtk.ReliefStyle)(1));
			global::Gtk.Image w1 = new global::Gtk.Image();
			w1.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-help", global::Gtk.IconSize.Menu);
			this.buttonInfo.Image = w1;
			this.table1.Add(this.buttonInfo);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.table1[this.buttonInfo]));
			w2.LeftAttach = ((uint)(2));
			w2.RightAttach = ((uint)(3));
			w2.XOptions = ((global::Gtk.AttachOptions)(4));
			w2.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.cmbGeoGroup = new global::QS.Widgets.GtkUI.SpecialListComboBox();
			this.cmbGeoGroup.Name = "cmbGeoGroup";
			this.cmbGeoGroup.AddIfNotExist = false;
			this.cmbGeoGroup.DefaultFirst = false;
			this.cmbGeoGroup.ShowSpecialStateAll = true;
			this.cmbGeoGroup.ShowSpecialStateNot = false;
			this.table1.Add(this.cmbGeoGroup);
			global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.table1[this.cmbGeoGroup]));
			w3.TopAttach = ((uint)(2));
			w3.BottomAttach = ((uint)(3));
			w3.LeftAttach = ((uint)(1));
			w3.RightAttach = ((uint)(2));
			w3.XOptions = ((global::Gtk.AttachOptions)(4));
			w3.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.rangeDate = new global::QS.Widgets.GtkUI.DateRangePicker();
			this.rangeDate.Events = ((global::Gdk.EventMask)(256));
			this.rangeDate.Name = "rangeDate";
			this.rangeDate.StartDate = new global::System.DateTime(0);
			this.rangeDate.EndDate = new global::System.DateTime(0);
			this.table1.Add(this.rangeDate);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.table1[this.rangeDate]));
			w4.LeftAttach = ((uint)(1));
			w4.RightAttach = ((uint)(2));
			w4.XOptions = ((global::Gtk.AttachOptions)(4));
			w4.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ycheckbuttonGroupByDriver = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckbuttonGroupByDriver.CanFocus = true;
			this.ycheckbuttonGroupByDriver.Name = "ycheckbuttonGroupByDriver";
			this.ycheckbuttonGroupByDriver.Label = global::Mono.Unix.Catalog.GetString("Группировать по водителям");
			this.ycheckbuttonGroupByDriver.DrawIndicator = true;
			this.ycheckbuttonGroupByDriver.UseUnderline = true;
			this.table1.Add(this.ycheckbuttonGroupByDriver);
			global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.table1[this.ycheckbuttonGroupByDriver]));
			w5.TopAttach = ((uint)(1));
			w5.BottomAttach = ((uint)(2));
			w5.RightAttach = ((uint)(2));
			w5.XOptions = ((global::Gtk.AttachOptions)(4));
			w5.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabel3 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel3.Name = "ylabel3";
			this.ylabel3.Xalign = 1F;
			this.ylabel3.LabelProp = global::Mono.Unix.Catalog.GetString("Период:");
			this.table1.Add(this.ylabel3);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabel3]));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabelGeoGroup = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelGeoGroup.Name = "ylabelGeoGroup";
			this.ylabelGeoGroup.Xalign = 1F;
			this.ylabelGeoGroup.LabelProp = global::Mono.Unix.Catalog.GetString("Часть города:");
			this.table1.Add(this.ylabelGeoGroup);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabelGeoGroup]));
			w7.TopAttach = ((uint)(2));
			w7.BottomAttach = ((uint)(3));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			this.parametersContainer.Add(this.table1);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.parametersContainer[this.table1]));
			w8.Position = 0;
			w8.Expand = false;
			w8.Fill = false;
			// Container child parametersContainer.Gtk.Box+BoxChild
			this.ybuttonSave = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonSave.CanFocus = true;
			this.ybuttonSave.Name = "ybuttonSave";
			this.ybuttonSave.UseUnderline = true;
			this.ybuttonSave.Label = global::Mono.Unix.Catalog.GetString("Сохранить");
			this.parametersContainer.Add(this.ybuttonSave);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.parametersContainer[this.ybuttonSave]));
			w9.PackType = ((global::Gtk.PackType)(1));
			w9.Position = 1;
			w9.Expand = false;
			w9.Fill = false;
			// Container child parametersContainer.Gtk.Box+BoxChild
			this.ybuttonAbortCreateReport = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonAbortCreateReport.CanFocus = true;
			this.ybuttonAbortCreateReport.Name = "ybuttonAbortCreateReport";
			this.ybuttonAbortCreateReport.UseUnderline = true;
			this.ybuttonAbortCreateReport.Label = global::Mono.Unix.Catalog.GetString("Отчет в процессе формирования... (Отменить)");
			this.parametersContainer.Add(this.ybuttonAbortCreateReport);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.parametersContainer[this.ybuttonAbortCreateReport]));
			w10.PackType = ((global::Gtk.PackType)(1));
			w10.Position = 2;
			w10.Expand = false;
			w10.Fill = false;
			// Container child parametersContainer.Gtk.Box+BoxChild
			this.ybuttonCreateReport = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonCreateReport.CanFocus = true;
			this.ybuttonCreateReport.Name = "ybuttonCreateReport";
			this.ybuttonCreateReport.UseUnderline = true;
			this.ybuttonCreateReport.Label = global::Mono.Unix.Catalog.GetString("Сформировать отчет");
			this.parametersContainer.Add(this.ybuttonCreateReport);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.parametersContainer[this.ybuttonCreateReport]));
			w11.PackType = ((global::Gtk.PackType)(1));
			w11.Position = 3;
			w11.Expand = false;
			w11.Fill = false;
			this.hboxFilters.Add(this.parametersContainer);
			global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.hboxFilters[this.parametersContainer]));
			w12.Position = 0;
			w12.Expand = false;
			w12.Fill = false;
			// Container child hboxFilters.Gtk.Box+BoxChild
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
			global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.vbox4[this.vseparator1]));
			w13.Position = 0;
			// Container child vbox4.Gtk.Box+BoxChild
			this.arrowSlider = new global::Gtk.Arrow(((global::Gtk.ArrowType)(2)), ((global::Gtk.ShadowType)(2)));
			this.arrowSlider.Name = "arrowSlider";
			this.vbox4.Add(this.arrowSlider);
			global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.vbox4[this.arrowSlider]));
			w14.Position = 1;
			w14.Expand = false;
			w14.Fill = false;
			// Container child vbox4.Gtk.Box+BoxChild
			this.labelTitle = new global::Gtk.Label();
			this.labelTitle.Name = "labelTitle";
			this.labelTitle.LabelProp = global::Mono.Unix.Catalog.GetString("Параметры");
			this.labelTitle.SingleLineMode = true;
			this.labelTitle.Angle = 90D;
			this.vbox4.Add(this.labelTitle);
			global::Gtk.Box.BoxChild w15 = ((global::Gtk.Box.BoxChild)(this.vbox4[this.labelTitle]));
			w15.Position = 2;
			w15.Expand = false;
			w15.Fill = false;
			// Container child vbox4.Gtk.Box+BoxChild
			this.vseparator2 = new global::Gtk.VSeparator();
			this.vseparator2.Name = "vseparator2";
			this.vbox4.Add(this.vseparator2);
			global::Gtk.Box.BoxChild w16 = ((global::Gtk.Box.BoxChild)(this.vbox4[this.vseparator2]));
			w16.Position = 3;
			this.eventboxArrow.Add(this.vbox4);
			this.hboxFilters.Add(this.eventboxArrow);
			global::Gtk.Box.BoxChild w18 = ((global::Gtk.Box.BoxChild)(this.hboxFilters[this.eventboxArrow]));
			w18.Position = 1;
			w18.Expand = false;
			w18.Fill = false;
			this.hboxMainContainer.Add(this.hboxFilters);
			global::Gtk.Box.BoxChild w19 = ((global::Gtk.Box.BoxChild)(this.hboxMainContainer[this.hboxFilters]));
			w19.Position = 0;
			w19.Expand = false;
			w19.Fill = false;
			// Container child hboxMainContainer.Gtk.Box+BoxChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.ytreeviewReport = new global::Gamma.GtkWidgets.yTreeView();
			this.ytreeviewReport.CanFocus = true;
			this.ytreeviewReport.Name = "ytreeviewReport";
			this.GtkScrolledWindow.Add(this.ytreeviewReport);
			this.hboxMainContainer.Add(this.GtkScrolledWindow);
			global::Gtk.Box.BoxChild w21 = ((global::Gtk.Box.BoxChild)(this.hboxMainContainer[this.GtkScrolledWindow]));
			w21.Position = 1;
			this.Add(this.hboxMainContainer);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}

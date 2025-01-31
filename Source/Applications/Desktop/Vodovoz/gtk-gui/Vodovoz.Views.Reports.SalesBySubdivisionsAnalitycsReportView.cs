
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.Views.Reports
{
	public partial class SalesBySubdivisionsAnalitycsReportView
	{
		private global::Gtk.HBox hbox1;

		private global::Gtk.HBox hboxMain;

		private global::Gtk.VBox vboxTurnoverWithDynamicsReportFilterContainer;

		private global::Gtk.Table tblRangesContainer;

		private global::QS.Widgets.GtkUI.DateRangePicker dateSecondPeriodPicker;

		private global::Gtk.HBox hboxPeriodInfoButtonContainer;

		private global::QS.Widgets.GtkUI.DateRangePicker dateFirstPeriodPicker;

		private global::Gamma.GtkWidgets.yButton btnReportInfo;

		private global::Gtk.Label lblFirstPeriod;

		private global::Gtk.Label lblSecondPeriod;

		private global::Gtk.VBox vbox1;

		private global::Gamma.GtkWidgets.yCheckButton ychkbtnSplitByNomenclatures;

		private global::Gamma.GtkWidgets.yCheckButton ychkbtnSplitBySubdivisions;

		private global::Gamma.GtkWidgets.yCheckButton ychkbtnSplitByWarhouses;

		private global::Gtk.VBox vboxParameters;

		private global::Gamma.GtkWidgets.yButton ybuttonCreateReport;

		private global::Gamma.GtkWidgets.yButton ybuttonAbortCreateReport;

		private global::Gamma.GtkWidgets.yButton ybuttonSave;

		private global::Gtk.EventBox eventboxArrow;

		private global::Gtk.VBox vbox4;

		private global::Gtk.VSeparator vseparator1;

		private global::Gtk.Arrow arrowSlider;

		private global::Gtk.Label labelTitle;

		private global::Gtk.VSeparator vseparator2;

		private global::Gtk.ScrolledWindow GtkScrolledWindow;

		private global::Gamma.GtkWidgets.yTreeView ytreeReportIndicatorsRows;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.Views.Reports.SalesBySubdivisionsAnalitycsReportView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.Views.Reports.SalesBySubdivisionsAnalitycsReportView";
			// Container child Vodovoz.Views.Reports.SalesBySubdivisionsAnalitycsReportView.Gtk.Container+ContainerChild
			this.hbox1 = new global::Gtk.HBox();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.hboxMain = new global::Gtk.HBox();
			this.hboxMain.Name = "hboxMain";
			this.hboxMain.Spacing = 6;
			// Container child hboxMain.Gtk.Box+BoxChild
			this.vboxTurnoverWithDynamicsReportFilterContainer = new global::Gtk.VBox();
			this.vboxTurnoverWithDynamicsReportFilterContainer.Name = "vboxTurnoverWithDynamicsReportFilterContainer";
			this.vboxTurnoverWithDynamicsReportFilterContainer.Spacing = 6;
			// Container child vboxTurnoverWithDynamicsReportFilterContainer.Gtk.Box+BoxChild
			this.tblRangesContainer = new global::Gtk.Table(((uint)(2)), ((uint)(2)), false);
			this.tblRangesContainer.Name = "tblRangesContainer";
			this.tblRangesContainer.RowSpacing = ((uint)(6));
			this.tblRangesContainer.ColumnSpacing = ((uint)(6));
			// Container child tblRangesContainer.Gtk.Table+TableChild
			this.dateSecondPeriodPicker = new global::QS.Widgets.GtkUI.DateRangePicker();
			this.dateSecondPeriodPicker.Events = ((global::Gdk.EventMask)(256));
			this.dateSecondPeriodPicker.Name = "dateSecondPeriodPicker";
			this.dateSecondPeriodPicker.StartDate = new global::System.DateTime(0);
			this.dateSecondPeriodPicker.EndDate = new global::System.DateTime(0);
			this.tblRangesContainer.Add(this.dateSecondPeriodPicker);
			global::Gtk.Table.TableChild w1 = ((global::Gtk.Table.TableChild)(this.tblRangesContainer[this.dateSecondPeriodPicker]));
			w1.TopAttach = ((uint)(1));
			w1.BottomAttach = ((uint)(2));
			w1.LeftAttach = ((uint)(1));
			w1.RightAttach = ((uint)(2));
			w1.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tblRangesContainer.Gtk.Table+TableChild
			this.hboxPeriodInfoButtonContainer = new global::Gtk.HBox();
			this.hboxPeriodInfoButtonContainer.Name = "hboxPeriodInfoButtonContainer";
			this.hboxPeriodInfoButtonContainer.Spacing = 6;
			// Container child hboxPeriodInfoButtonContainer.Gtk.Box+BoxChild
			this.dateFirstPeriodPicker = new global::QS.Widgets.GtkUI.DateRangePicker();
			this.dateFirstPeriodPicker.Events = ((global::Gdk.EventMask)(256));
			this.dateFirstPeriodPicker.Name = "dateFirstPeriodPicker";
			this.dateFirstPeriodPicker.StartDate = new global::System.DateTime(0);
			this.dateFirstPeriodPicker.EndDate = new global::System.DateTime(0);
			this.hboxPeriodInfoButtonContainer.Add(this.dateFirstPeriodPicker);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hboxPeriodInfoButtonContainer[this.dateFirstPeriodPicker]));
			w2.Position = 0;
			// Container child hboxPeriodInfoButtonContainer.Gtk.Box+BoxChild
			this.btnReportInfo = new global::Gamma.GtkWidgets.yButton();
			this.btnReportInfo.TooltipMarkup = "Справка по работе с отчётом";
			this.btnReportInfo.CanFocus = true;
			this.btnReportInfo.Name = "btnReportInfo";
			this.btnReportInfo.UseUnderline = true;
			this.btnReportInfo.Relief = ((global::Gtk.ReliefStyle)(1));
			global::Gtk.Image w3 = new global::Gtk.Image();
			w3.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-help", global::Gtk.IconSize.Menu);
			this.btnReportInfo.Image = w3;
			this.hboxPeriodInfoButtonContainer.Add(this.btnReportInfo);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hboxPeriodInfoButtonContainer[this.btnReportInfo]));
			w4.Position = 1;
			w4.Expand = false;
			w4.Fill = false;
			this.tblRangesContainer.Add(this.hboxPeriodInfoButtonContainer);
			global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.tblRangesContainer[this.hboxPeriodInfoButtonContainer]));
			w5.LeftAttach = ((uint)(1));
			w5.RightAttach = ((uint)(2));
			w5.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tblRangesContainer.Gtk.Table+TableChild
			this.lblFirstPeriod = new global::Gtk.Label();
			this.lblFirstPeriod.Name = "lblFirstPeriod";
			this.lblFirstPeriod.Xalign = 0F;
			this.lblFirstPeriod.LabelProp = global::Mono.Unix.Catalog.GetString("Первый период:");
			this.tblRangesContainer.Add(this.lblFirstPeriod);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.tblRangesContainer[this.lblFirstPeriod]));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tblRangesContainer.Gtk.Table+TableChild
			this.lblSecondPeriod = new global::Gtk.Label();
			this.lblSecondPeriod.Name = "lblSecondPeriod";
			this.lblSecondPeriod.Xalign = 0F;
			this.lblSecondPeriod.LabelProp = global::Mono.Unix.Catalog.GetString("Второй период:");
			this.tblRangesContainer.Add(this.lblSecondPeriod);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.tblRangesContainer[this.lblSecondPeriod]));
			w7.TopAttach = ((uint)(1));
			w7.BottomAttach = ((uint)(2));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			this.vboxTurnoverWithDynamicsReportFilterContainer.Add(this.tblRangesContainer);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.vboxTurnoverWithDynamicsReportFilterContainer[this.tblRangesContainer]));
			w8.Position = 0;
			w8.Expand = false;
			w8.Fill = false;
			// Container child vboxTurnoverWithDynamicsReportFilterContainer.Gtk.Box+BoxChild
			this.vbox1 = new global::Gtk.VBox();
			this.vbox1.Name = "vbox1";
			this.vbox1.Spacing = 6;
			// Container child vbox1.Gtk.Box+BoxChild
			this.ychkbtnSplitByNomenclatures = new global::Gamma.GtkWidgets.yCheckButton();
			this.ychkbtnSplitByNomenclatures.CanFocus = true;
			this.ychkbtnSplitByNomenclatures.Name = "ychkbtnSplitByNomenclatures";
			this.ychkbtnSplitByNomenclatures.Label = global::Mono.Unix.Catalog.GetString("Разбить на товары");
			this.ychkbtnSplitByNomenclatures.DrawIndicator = true;
			this.ychkbtnSplitByNomenclatures.UseUnderline = true;
			this.vbox1.Add(this.ychkbtnSplitByNomenclatures);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.ychkbtnSplitByNomenclatures]));
			w9.Position = 0;
			w9.Expand = false;
			w9.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.ychkbtnSplitBySubdivisions = new global::Gamma.GtkWidgets.yCheckButton();
			this.ychkbtnSplitBySubdivisions.CanFocus = true;
			this.ychkbtnSplitBySubdivisions.Name = "ychkbtnSplitBySubdivisions";
			this.ychkbtnSplitBySubdivisions.Label = global::Mono.Unix.Catalog.GetString("Разбить по отделам КБ");
			this.ychkbtnSplitBySubdivisions.DrawIndicator = true;
			this.ychkbtnSplitBySubdivisions.UseUnderline = true;
			this.vbox1.Add(this.ychkbtnSplitBySubdivisions);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.ychkbtnSplitBySubdivisions]));
			w10.Position = 1;
			w10.Expand = false;
			w10.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.ychkbtnSplitByWarhouses = new global::Gamma.GtkWidgets.yCheckButton();
			this.ychkbtnSplitByWarhouses.CanFocus = true;
			this.ychkbtnSplitByWarhouses.Name = "ychkbtnSplitByWarhouses";
			this.ychkbtnSplitByWarhouses.Label = global::Mono.Unix.Catalog.GetString("Разбить по складам");
			this.ychkbtnSplitByWarhouses.DrawIndicator = true;
			this.ychkbtnSplitByWarhouses.UseUnderline = true;
			this.vbox1.Add(this.ychkbtnSplitByWarhouses);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.ychkbtnSplitByWarhouses]));
			w11.Position = 2;
			w11.Expand = false;
			w11.Fill = false;
			this.vboxTurnoverWithDynamicsReportFilterContainer.Add(this.vbox1);
			global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.vboxTurnoverWithDynamicsReportFilterContainer[this.vbox1]));
			w12.Position = 1;
			w12.Expand = false;
			w12.Fill = false;
			// Container child vboxTurnoverWithDynamicsReportFilterContainer.Gtk.Box+BoxChild
			this.vboxParameters = new global::Gtk.VBox();
			this.vboxParameters.Name = "vboxParameters";
			this.vboxParameters.Spacing = 6;
			this.vboxTurnoverWithDynamicsReportFilterContainer.Add(this.vboxParameters);
			global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.vboxTurnoverWithDynamicsReportFilterContainer[this.vboxParameters]));
			w13.Position = 2;
			// Container child vboxTurnoverWithDynamicsReportFilterContainer.Gtk.Box+BoxChild
			this.ybuttonCreateReport = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonCreateReport.CanFocus = true;
			this.ybuttonCreateReport.Name = "ybuttonCreateReport";
			this.ybuttonCreateReport.UseUnderline = true;
			this.ybuttonCreateReport.Label = global::Mono.Unix.Catalog.GetString("Сформировать отчет");
			this.vboxTurnoverWithDynamicsReportFilterContainer.Add(this.ybuttonCreateReport);
			global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.vboxTurnoverWithDynamicsReportFilterContainer[this.ybuttonCreateReport]));
			w14.Position = 3;
			w14.Expand = false;
			w14.Fill = false;
			// Container child vboxTurnoverWithDynamicsReportFilterContainer.Gtk.Box+BoxChild
			this.ybuttonAbortCreateReport = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonAbortCreateReport.CanFocus = true;
			this.ybuttonAbortCreateReport.Name = "ybuttonAbortCreateReport";
			this.ybuttonAbortCreateReport.UseUnderline = true;
			this.ybuttonAbortCreateReport.Label = global::Mono.Unix.Catalog.GetString("Отчет в процессе формирования... (Отменить)");
			this.vboxTurnoverWithDynamicsReportFilterContainer.Add(this.ybuttonAbortCreateReport);
			global::Gtk.Box.BoxChild w15 = ((global::Gtk.Box.BoxChild)(this.vboxTurnoverWithDynamicsReportFilterContainer[this.ybuttonAbortCreateReport]));
			w15.Position = 4;
			w15.Expand = false;
			w15.Fill = false;
			// Container child vboxTurnoverWithDynamicsReportFilterContainer.Gtk.Box+BoxChild
			this.ybuttonSave = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonSave.CanFocus = true;
			this.ybuttonSave.Name = "ybuttonSave";
			this.ybuttonSave.UseUnderline = true;
			this.ybuttonSave.Label = global::Mono.Unix.Catalog.GetString("Сохранить");
			this.vboxTurnoverWithDynamicsReportFilterContainer.Add(this.ybuttonSave);
			global::Gtk.Box.BoxChild w16 = ((global::Gtk.Box.BoxChild)(this.vboxTurnoverWithDynamicsReportFilterContainer[this.ybuttonSave]));
			w16.Position = 5;
			w16.Expand = false;
			w16.Fill = false;
			this.hboxMain.Add(this.vboxTurnoverWithDynamicsReportFilterContainer);
			global::Gtk.Box.BoxChild w17 = ((global::Gtk.Box.BoxChild)(this.hboxMain[this.vboxTurnoverWithDynamicsReportFilterContainer]));
			w17.Position = 0;
			w17.Expand = false;
			w17.Fill = false;
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
			global::Gtk.Box.BoxChild w18 = ((global::Gtk.Box.BoxChild)(this.vbox4[this.vseparator1]));
			w18.Position = 0;
			// Container child vbox4.Gtk.Box+BoxChild
			this.arrowSlider = new global::Gtk.Arrow(((global::Gtk.ArrowType)(3)), ((global::Gtk.ShadowType)(2)));
			this.arrowSlider.Name = "arrowSlider";
			this.vbox4.Add(this.arrowSlider);
			global::Gtk.Box.BoxChild w19 = ((global::Gtk.Box.BoxChild)(this.vbox4[this.arrowSlider]));
			w19.Position = 1;
			w19.Expand = false;
			w19.Fill = false;
			// Container child vbox4.Gtk.Box+BoxChild
			this.labelTitle = new global::Gtk.Label();
			this.labelTitle.Name = "labelTitle";
			this.labelTitle.LabelProp = global::Mono.Unix.Catalog.GetString("Параметры");
			this.labelTitle.SingleLineMode = true;
			this.labelTitle.Angle = 90D;
			this.vbox4.Add(this.labelTitle);
			global::Gtk.Box.BoxChild w20 = ((global::Gtk.Box.BoxChild)(this.vbox4[this.labelTitle]));
			w20.Position = 2;
			w20.Expand = false;
			w20.Fill = false;
			// Container child vbox4.Gtk.Box+BoxChild
			this.vseparator2 = new global::Gtk.VSeparator();
			this.vseparator2.Name = "vseparator2";
			this.vbox4.Add(this.vseparator2);
			global::Gtk.Box.BoxChild w21 = ((global::Gtk.Box.BoxChild)(this.vbox4[this.vseparator2]));
			w21.Position = 3;
			this.eventboxArrow.Add(this.vbox4);
			this.hboxMain.Add(this.eventboxArrow);
			global::Gtk.Box.BoxChild w23 = ((global::Gtk.Box.BoxChild)(this.hboxMain[this.eventboxArrow]));
			w23.Position = 1;
			w23.Expand = false;
			w23.Fill = false;
			this.hbox1.Add(this.hboxMain);
			global::Gtk.Box.BoxChild w24 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.hboxMain]));
			w24.Position = 0;
			w24.Expand = false;
			w24.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.ytreeReportIndicatorsRows = new global::Gamma.GtkWidgets.yTreeView();
			this.ytreeReportIndicatorsRows.CanFocus = true;
			this.ytreeReportIndicatorsRows.Name = "ytreeReportIndicatorsRows";
			this.GtkScrolledWindow.Add(this.ytreeReportIndicatorsRows);
			this.hbox1.Add(this.GtkScrolledWindow);
			global::Gtk.Box.BoxChild w26 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.GtkScrolledWindow]));
			w26.Position = 1;
			this.Add(this.hbox1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}

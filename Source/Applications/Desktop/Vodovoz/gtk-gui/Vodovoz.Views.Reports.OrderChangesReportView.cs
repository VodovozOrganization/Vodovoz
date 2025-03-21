
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.Views.Reports
{
	public partial class OrderChangesReportView
	{
		private global::Gtk.HPaned hpanedMain;

		private global::Gtk.HBox hboxControls;

		private global::Gtk.ScrolledWindow scrolledwindowFilter;

		private global::Gamma.GtkWidgets.yVBox yvboxFilterContainer;

		private global::Gamma.GtkWidgets.yTable ytableFilter;

		private global::Gamma.GtkWidgets.yCheckButton ycheckbuttonArchive;

		private global::Gamma.GtkWidgets.yHBox yhboxChangeType;

		private global::Gamma.GtkWidgets.yLabel ylabelChangeType;

		private global::Gamma.GtkWidgets.yTreeView ytreeviewChangeTypes;

		private global::Gamma.GtkWidgets.yHBox yhboxDate;

		private global::Gamma.GtkWidgets.yLabel ylabelDate;

		private global::QS.Widgets.GtkUI.DateRangePicker datePeriodPicker;

		private global::Gamma.GtkWidgets.yHBox yhboxIssuesType;

		private global::Gamma.GtkWidgets.yLabel ylabelIssuesType;

		private global::Gamma.GtkWidgets.yTreeView ytreeviewIssueTypes;

		private global::Gamma.GtkWidgets.yHBox yhboxOrganization;

		private global::Gamma.GtkWidgets.yLabel ylabelOrganization;

		private global::QS.Widgets.GtkUI.SpecialListComboBox speciallistcomboboxOrganization;

		private global::Gamma.GtkWidgets.yLabel ylabelDateWarning;

		private global::Gamma.GtkWidgets.yTextView ytextview1;

		private global::Gamma.GtkWidgets.yButton ybuttonSave;

		private global::Gamma.GtkWidgets.yButton ybuttonAbortCreateReport;

		private global::Gamma.GtkWidgets.yButton ybuttonCreateReport;

		private global::Gtk.EventBox eventboxArrow;

		private global::Gtk.VBox vboxSlider;

		private global::Gtk.VSeparator vseparatorSliderTop;

		private global::Gtk.Arrow arrowSlider;

		private global::Gtk.Label labelParametersTitle;

		private global::Gtk.VSeparator vseparatorSliderBottom;

		private global::Gtk.ScrolledWindow GtkScrolledWindowData;

		private global::Gamma.GtkWidgets.yTreeView ytreeReportRows;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.Views.Reports.OrderChangesReportView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.Views.Reports.OrderChangesReportView";
			// Container child Vodovoz.Views.Reports.OrderChangesReportView.Gtk.Container+ContainerChild
			this.hpanedMain = new global::Gtk.HPaned();
			this.hpanedMain.CanFocus = true;
			this.hpanedMain.Name = "hpanedMain";
			this.hpanedMain.Position = 587;
			// Container child hpanedMain.Gtk.Paned+PanedChild
			this.hboxControls = new global::Gtk.HBox();
			this.hboxControls.Name = "hboxControls";
			this.hboxControls.Spacing = 6;
			// Container child hboxControls.Gtk.Box+BoxChild
			this.scrolledwindowFilter = new global::Gtk.ScrolledWindow();
			this.scrolledwindowFilter.CanFocus = true;
			this.scrolledwindowFilter.Name = "scrolledwindowFilter";
			this.scrolledwindowFilter.HscrollbarPolicy = ((global::Gtk.PolicyType)(2));
			this.scrolledwindowFilter.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child scrolledwindowFilter.Gtk.Container+ContainerChild
			global::Gtk.Viewport w1 = new global::Gtk.Viewport();
			w1.ShadowType = ((global::Gtk.ShadowType)(0));
			// Container child GtkViewport.Gtk.Container+ContainerChild
			this.yvboxFilterContainer = new global::Gamma.GtkWidgets.yVBox();
			this.yvboxFilterContainer.Name = "yvboxFilterContainer";
			this.yvboxFilterContainer.Spacing = 6;
			// Container child yvboxFilterContainer.Gtk.Box+BoxChild
			this.ytableFilter = new global::Gamma.GtkWidgets.yTable();
			this.ytableFilter.Name = "ytableFilter";
			this.ytableFilter.NRows = ((uint)(7));
			this.ytableFilter.NColumns = ((uint)(3));
			this.ytableFilter.RowSpacing = ((uint)(6));
			this.ytableFilter.ColumnSpacing = ((uint)(6));
			// Container child ytableFilter.Gtk.Table+TableChild
			this.ycheckbuttonArchive = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckbuttonArchive.CanFocus = true;
			this.ycheckbuttonArchive.Name = "ycheckbuttonArchive";
			this.ycheckbuttonArchive.Label = global::Mono.Unix.Catalog.GetString("Архив (медленно)");
			this.ycheckbuttonArchive.DrawIndicator = true;
			this.ycheckbuttonArchive.UseUnderline = true;
			this.ytableFilter.Add(this.ycheckbuttonArchive);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.ytableFilter[this.ycheckbuttonArchive]));
			w2.TopAttach = ((uint)(1));
			w2.BottomAttach = ((uint)(2));
			w2.RightAttach = ((uint)(3));
			w2.XOptions = ((global::Gtk.AttachOptions)(4));
			w2.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytableFilter.Gtk.Table+TableChild
			this.yhboxChangeType = new global::Gamma.GtkWidgets.yHBox();
			this.yhboxChangeType.Name = "yhboxChangeType";
			this.yhboxChangeType.Spacing = 6;
			// Container child yhboxChangeType.Gtk.Box+BoxChild
			this.ylabelChangeType = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelChangeType.Name = "ylabelChangeType";
			this.ylabelChangeType.Xalign = 1F;
			this.ylabelChangeType.Yalign = 0F;
			this.ylabelChangeType.LabelProp = global::Mono.Unix.Catalog.GetString("Типы изменений:");
			this.yhboxChangeType.Add(this.ylabelChangeType);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.yhboxChangeType[this.ylabelChangeType]));
			w3.Position = 0;
			w3.Expand = false;
			w3.Fill = false;
			// Container child yhboxChangeType.Gtk.Box+BoxChild
			this.ytreeviewChangeTypes = new global::Gamma.GtkWidgets.yTreeView();
			this.ytreeviewChangeTypes.CanFocus = true;
			this.ytreeviewChangeTypes.Name = "ytreeviewChangeTypes";
			this.yhboxChangeType.Add(this.ytreeviewChangeTypes);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.yhboxChangeType[this.ytreeviewChangeTypes]));
			w4.Position = 1;
			this.ytableFilter.Add(this.yhboxChangeType);
			global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.ytableFilter[this.yhboxChangeType]));
			w5.TopAttach = ((uint)(4));
			w5.BottomAttach = ((uint)(5));
			w5.RightAttach = ((uint)(3));
			w5.XOptions = ((global::Gtk.AttachOptions)(4));
			w5.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytableFilter.Gtk.Table+TableChild
			this.yhboxDate = new global::Gamma.GtkWidgets.yHBox();
			this.yhboxDate.Name = "yhboxDate";
			this.yhboxDate.Spacing = 6;
			// Container child yhboxDate.Gtk.Box+BoxChild
			this.ylabelDate = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelDate.Name = "ylabelDate";
			this.ylabelDate.Xalign = 1F;
			this.ylabelDate.LabelProp = global::Mono.Unix.Catalog.GetString("Дата:");
			this.yhboxDate.Add(this.ylabelDate);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.yhboxDate[this.ylabelDate]));
			w6.Position = 0;
			w6.Expand = false;
			w6.Fill = false;
			// Container child yhboxDate.Gtk.Box+BoxChild
			this.datePeriodPicker = new global::QS.Widgets.GtkUI.DateRangePicker();
			this.datePeriodPicker.Events = ((global::Gdk.EventMask)(256));
			this.datePeriodPicker.Name = "datePeriodPicker";
			this.datePeriodPicker.StartDate = new global::System.DateTime(0);
			this.datePeriodPicker.EndDate = new global::System.DateTime(0);
			this.yhboxDate.Add(this.datePeriodPicker);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.yhboxDate[this.datePeriodPicker]));
			w7.Position = 1;
			this.ytableFilter.Add(this.yhboxDate);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.ytableFilter[this.yhboxDate]));
			w8.RightAttach = ((uint)(3));
			w8.XOptions = ((global::Gtk.AttachOptions)(4));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytableFilter.Gtk.Table+TableChild
			this.yhboxIssuesType = new global::Gamma.GtkWidgets.yHBox();
			this.yhboxIssuesType.Name = "yhboxIssuesType";
			this.yhboxIssuesType.Spacing = 6;
			// Container child yhboxIssuesType.Gtk.Box+BoxChild
			this.ylabelIssuesType = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelIssuesType.Name = "ylabelIssuesType";
			this.ylabelIssuesType.Xalign = 1F;
			this.ylabelIssuesType.Yalign = 0F;
			this.ylabelIssuesType.LabelProp = global::Mono.Unix.Catalog.GetString("Типы проблем:");
			this.yhboxIssuesType.Add(this.ylabelIssuesType);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.yhboxIssuesType[this.ylabelIssuesType]));
			w9.Position = 0;
			w9.Expand = false;
			w9.Fill = false;
			// Container child yhboxIssuesType.Gtk.Box+BoxChild
			this.ytreeviewIssueTypes = new global::Gamma.GtkWidgets.yTreeView();
			this.ytreeviewIssueTypes.CanFocus = true;
			this.ytreeviewIssueTypes.Name = "ytreeviewIssueTypes";
			this.yhboxIssuesType.Add(this.ytreeviewIssueTypes);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.yhboxIssuesType[this.ytreeviewIssueTypes]));
			w10.Position = 1;
			this.ytableFilter.Add(this.yhboxIssuesType);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.ytableFilter[this.yhboxIssuesType]));
			w11.TopAttach = ((uint)(5));
			w11.BottomAttach = ((uint)(6));
			w11.RightAttach = ((uint)(3));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytableFilter.Gtk.Table+TableChild
			this.yhboxOrganization = new global::Gamma.GtkWidgets.yHBox();
			this.yhboxOrganization.Name = "yhboxOrganization";
			this.yhboxOrganization.Spacing = 6;
			// Container child yhboxOrganization.Gtk.Box+BoxChild
			this.ylabelOrganization = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelOrganization.Name = "ylabelOrganization";
			this.ylabelOrganization.Xalign = 1F;
			this.ylabelOrganization.LabelProp = global::Mono.Unix.Catalog.GetString("Организация:");
			this.yhboxOrganization.Add(this.ylabelOrganization);
			global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.yhboxOrganization[this.ylabelOrganization]));
			w12.Position = 0;
			w12.Expand = false;
			w12.Fill = false;
			// Container child yhboxOrganization.Gtk.Box+BoxChild
			this.speciallistcomboboxOrganization = new global::QS.Widgets.GtkUI.SpecialListComboBox();
			this.speciallistcomboboxOrganization.Name = "speciallistcomboboxOrganization";
			this.speciallistcomboboxOrganization.AddIfNotExist = false;
			this.speciallistcomboboxOrganization.DefaultFirst = false;
			this.speciallistcomboboxOrganization.ShowSpecialStateAll = false;
			this.speciallistcomboboxOrganization.ShowSpecialStateNot = false;
			this.yhboxOrganization.Add(this.speciallistcomboboxOrganization);
			global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.yhboxOrganization[this.speciallistcomboboxOrganization]));
			w13.Position = 1;
			w13.Expand = false;
			w13.Fill = false;
			this.ytableFilter.Add(this.yhboxOrganization);
			global::Gtk.Table.TableChild w14 = ((global::Gtk.Table.TableChild)(this.ytableFilter[this.yhboxOrganization]));
			w14.TopAttach = ((uint)(3));
			w14.BottomAttach = ((uint)(4));
			w14.RightAttach = ((uint)(3));
			w14.XOptions = ((global::Gtk.AttachOptions)(4));
			w14.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytableFilter.Gtk.Table+TableChild
			this.ylabelDateWarning = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelDateWarning.Name = "ylabelDateWarning";
			this.ylabelDateWarning.Xalign = 0F;
			this.ylabelDateWarning.LabelProp = global::Mono.Unix.Catalog.GetString("<span color=\"Red\">Выбранный период более 2-х недель,\nотчет может выполняться очен" +
					"ь долго!</span>");
			this.ylabelDateWarning.UseMarkup = true;
			this.ytableFilter.Add(this.ylabelDateWarning);
			global::Gtk.Table.TableChild w15 = ((global::Gtk.Table.TableChild)(this.ytableFilter[this.ylabelDateWarning]));
			w15.TopAttach = ((uint)(2));
			w15.BottomAttach = ((uint)(3));
			w15.RightAttach = ((uint)(3));
			w15.XOptions = ((global::Gtk.AttachOptions)(4));
			w15.YOptions = ((global::Gtk.AttachOptions)(0));
			// Container child ytableFilter.Gtk.Table+TableChild
			this.ytextview1 = new global::Gamma.GtkWidgets.yTextView();
			this.ytextview1.Buffer.Text = global::Mono.Unix.Catalog.GetString(@"Описание:
В отчет попадает информация об изменении полей заказа
и товаров заказа, после того как заказ был доставлен.
Поиск идет по заказам за выбранный интервал с выбранными
типами изменений и организацией .
Без выбора фильтра Архивный (медленно), можно смотреть записи
мониторинга за последние 60 дней, а при установке данного значения
к просмотру будут доступны записи, сделанные раньше 60дней назад");
			this.ytextview1.CanFocus = true;
			this.ytextview1.Name = "ytextview1";
			this.ytextview1.Editable = false;
			this.ytableFilter.Add(this.ytextview1);
			global::Gtk.Table.TableChild w16 = ((global::Gtk.Table.TableChild)(this.ytableFilter[this.ytextview1]));
			w16.TopAttach = ((uint)(6));
			w16.BottomAttach = ((uint)(7));
			w16.RightAttach = ((uint)(3));
			w16.XOptions = ((global::Gtk.AttachOptions)(4));
			w16.YOptions = ((global::Gtk.AttachOptions)(4));
			this.yvboxFilterContainer.Add(this.ytableFilter);
			global::Gtk.Box.BoxChild w17 = ((global::Gtk.Box.BoxChild)(this.yvboxFilterContainer[this.ytableFilter]));
			w17.Position = 0;
			w17.Expand = false;
			w17.Fill = false;
			// Container child yvboxFilterContainer.Gtk.Box+BoxChild
			this.ybuttonSave = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonSave.CanFocus = true;
			this.ybuttonSave.Name = "ybuttonSave";
			this.ybuttonSave.UseUnderline = true;
			this.ybuttonSave.Label = global::Mono.Unix.Catalog.GetString("Сохранить");
			this.yvboxFilterContainer.Add(this.ybuttonSave);
			global::Gtk.Box.BoxChild w18 = ((global::Gtk.Box.BoxChild)(this.yvboxFilterContainer[this.ybuttonSave]));
			w18.PackType = ((global::Gtk.PackType)(1));
			w18.Position = 2;
			w18.Expand = false;
			w18.Fill = false;
			// Container child yvboxFilterContainer.Gtk.Box+BoxChild
			this.ybuttonAbortCreateReport = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonAbortCreateReport.CanFocus = true;
			this.ybuttonAbortCreateReport.Name = "ybuttonAbortCreateReport";
			this.ybuttonAbortCreateReport.UseUnderline = true;
			this.ybuttonAbortCreateReport.Label = global::Mono.Unix.Catalog.GetString("Отчет в процессе формирования... (Отменить)");
			this.yvboxFilterContainer.Add(this.ybuttonAbortCreateReport);
			global::Gtk.Box.BoxChild w19 = ((global::Gtk.Box.BoxChild)(this.yvboxFilterContainer[this.ybuttonAbortCreateReport]));
			w19.PackType = ((global::Gtk.PackType)(1));
			w19.Position = 3;
			w19.Expand = false;
			w19.Fill = false;
			// Container child yvboxFilterContainer.Gtk.Box+BoxChild
			this.ybuttonCreateReport = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonCreateReport.CanFocus = true;
			this.ybuttonCreateReport.Name = "ybuttonCreateReport";
			this.ybuttonCreateReport.UseUnderline = true;
			this.ybuttonCreateReport.Label = global::Mono.Unix.Catalog.GetString("Сформировать отчет");
			this.yvboxFilterContainer.Add(this.ybuttonCreateReport);
			global::Gtk.Box.BoxChild w20 = ((global::Gtk.Box.BoxChild)(this.yvboxFilterContainer[this.ybuttonCreateReport]));
			w20.PackType = ((global::Gtk.PackType)(1));
			w20.Position = 4;
			w20.Expand = false;
			w20.Fill = false;
			w1.Add(this.yvboxFilterContainer);
			this.scrolledwindowFilter.Add(w1);
			this.hboxControls.Add(this.scrolledwindowFilter);
			global::Gtk.Box.BoxChild w23 = ((global::Gtk.Box.BoxChild)(this.hboxControls[this.scrolledwindowFilter]));
			w23.Position = 0;
			// Container child hboxControls.Gtk.Box+BoxChild
			this.eventboxArrow = new global::Gtk.EventBox();
			this.eventboxArrow.Name = "eventboxArrow";
			// Container child eventboxArrow.Gtk.Container+ContainerChild
			this.vboxSlider = new global::Gtk.VBox();
			this.vboxSlider.Name = "vboxSlider";
			this.vboxSlider.Spacing = 6;
			// Container child vboxSlider.Gtk.Box+BoxChild
			this.vseparatorSliderTop = new global::Gtk.VSeparator();
			this.vseparatorSliderTop.Name = "vseparatorSliderTop";
			this.vboxSlider.Add(this.vseparatorSliderTop);
			global::Gtk.Box.BoxChild w24 = ((global::Gtk.Box.BoxChild)(this.vboxSlider[this.vseparatorSliderTop]));
			w24.Position = 0;
			// Container child vboxSlider.Gtk.Box+BoxChild
			this.arrowSlider = new global::Gtk.Arrow(((global::Gtk.ArrowType)(3)), ((global::Gtk.ShadowType)(2)));
			this.arrowSlider.Name = "arrowSlider";
			this.vboxSlider.Add(this.arrowSlider);
			global::Gtk.Box.BoxChild w25 = ((global::Gtk.Box.BoxChild)(this.vboxSlider[this.arrowSlider]));
			w25.Position = 1;
			w25.Expand = false;
			w25.Fill = false;
			// Container child vboxSlider.Gtk.Box+BoxChild
			this.labelParametersTitle = new global::Gtk.Label();
			this.labelParametersTitle.Name = "labelParametersTitle";
			this.labelParametersTitle.LabelProp = global::Mono.Unix.Catalog.GetString("Параметры");
			this.labelParametersTitle.SingleLineMode = true;
			this.labelParametersTitle.Angle = 90D;
			this.vboxSlider.Add(this.labelParametersTitle);
			global::Gtk.Box.BoxChild w26 = ((global::Gtk.Box.BoxChild)(this.vboxSlider[this.labelParametersTitle]));
			w26.Position = 2;
			w26.Expand = false;
			w26.Fill = false;
			// Container child vboxSlider.Gtk.Box+BoxChild
			this.vseparatorSliderBottom = new global::Gtk.VSeparator();
			this.vseparatorSliderBottom.Name = "vseparatorSliderBottom";
			this.vboxSlider.Add(this.vseparatorSliderBottom);
			global::Gtk.Box.BoxChild w27 = ((global::Gtk.Box.BoxChild)(this.vboxSlider[this.vseparatorSliderBottom]));
			w27.Position = 3;
			this.eventboxArrow.Add(this.vboxSlider);
			this.hboxControls.Add(this.eventboxArrow);
			global::Gtk.Box.BoxChild w29 = ((global::Gtk.Box.BoxChild)(this.hboxControls[this.eventboxArrow]));
			w29.Position = 1;
			w29.Expand = false;
			w29.Fill = false;
			this.hpanedMain.Add(this.hboxControls);
			global::Gtk.Paned.PanedChild w30 = ((global::Gtk.Paned.PanedChild)(this.hpanedMain[this.hboxControls]));
			w30.Resize = false;
			// Container child hpanedMain.Gtk.Paned+PanedChild
			this.GtkScrolledWindowData = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindowData.Name = "GtkScrolledWindowData";
			this.GtkScrolledWindowData.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindowData.Gtk.Container+ContainerChild
			this.ytreeReportRows = new global::Gamma.GtkWidgets.yTreeView();
			this.ytreeReportRows.CanFocus = true;
			this.ytreeReportRows.Name = "ytreeReportRows";
			this.GtkScrolledWindowData.Add(this.ytreeReportRows);
			this.hpanedMain.Add(this.GtkScrolledWindowData);
			this.Add(this.hpanedMain);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.ylabelDateWarning.Hide();
			this.Hide();
		}
	}
}

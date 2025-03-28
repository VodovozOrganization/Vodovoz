
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.Reports
{
	public partial class FuelReport
	{
		private global::Gtk.VBox vbox1;

		private global::Gtk.HBox hbox1;

		private global::Gtk.Label label1;

		private global::QS.Widgets.GtkUI.DateRangePicker dateperiodpicker;

		private global::Gtk.HBox hbox5;

		private global::Gtk.VBox vbox2;

		private global::Gtk.RadioButton radioDriver;

		private global::Gtk.RadioButton radioCar;

		private global::Gtk.RadioButton radioSumm;

		private global::Gtk.VBox vbox3;

		private global::Gamma.GtkWidgets.yCheckButton yCheckButtonDatailedSummary;

		private global::Gtk.HBox hboxDriver;

		private global::Gtk.Label labelDriver;

		private global::QS.Widgets.GtkUI.EntityViewModelEntry evmeDriver;

		private global::Gtk.HBox hboxCar;

		private global::Gtk.Label label2;

		private global::QS.Views.Control.EntityEntry entityentryCar;

		private global::Gtk.HBox hboxAuthor;

		private global::Gtk.Label labelAuthor;

		private global::QS.Widgets.GtkUI.EntityViewModelEntry evmeAuthor;

		private global::Gamma.GtkWidgets.yVBox yvboxCarModel;

		private global::Gamma.GtkWidgets.yLabel ylabelCarModel;

		private global::Gamma.GtkWidgets.yHBox yhboxCarModelContainer;

		private global::Gamma.GtkWidgets.yButton buttonCreateReport;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.Reports.FuelReport
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.Reports.FuelReport";
			// Container child Vodovoz.Reports.FuelReport.Gtk.Container+ContainerChild
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
			this.label1.Xalign = 0F;
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString("Период:");
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
			this.hbox5 = new global::Gtk.HBox();
			this.hbox5.Name = "hbox5";
			this.hbox5.Spacing = 6;
			// Container child hbox5.Gtk.Box+BoxChild
			this.vbox2 = new global::Gtk.VBox();
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.radioDriver = new global::Gtk.RadioButton(global::Mono.Unix.Catalog.GetString("Баланс по водителям"));
			this.radioDriver.CanFocus = true;
			this.radioDriver.Name = "radioDriver";
			this.radioDriver.DrawIndicator = true;
			this.radioDriver.UseUnderline = true;
			this.radioDriver.Group = new global::GLib.SList(global::System.IntPtr.Zero);
			this.vbox2.Add(this.radioDriver);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.radioDriver]));
			w4.Position = 0;
			w4.Expand = false;
			w4.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.radioCar = new global::Gtk.RadioButton(global::Mono.Unix.Catalog.GetString("Баланс по автомобилям"));
			this.radioCar.CanFocus = true;
			this.radioCar.Name = "radioCar";
			this.radioCar.DrawIndicator = true;
			this.radioCar.UseUnderline = true;
			this.radioCar.Group = this.radioDriver.Group;
			this.vbox2.Add(this.radioCar);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.radioCar]));
			w5.Position = 1;
			w5.Expand = false;
			w5.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.radioSumm = new global::Gtk.RadioButton(global::Mono.Unix.Catalog.GetString("Суммарный отчет по топливу"));
			this.radioSumm.CanFocus = true;
			this.radioSumm.Name = "radioSumm";
			this.radioSumm.DrawIndicator = true;
			this.radioSumm.UseUnderline = true;
			this.radioSumm.Group = this.radioDriver.Group;
			this.vbox2.Add(this.radioSumm);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.radioSumm]));
			w6.Position = 2;
			w6.Expand = false;
			w6.Fill = false;
			this.hbox5.Add(this.vbox2);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.hbox5[this.vbox2]));
			w7.Position = 0;
			// Container child hbox5.Gtk.Box+BoxChild
			this.vbox3 = new global::Gtk.VBox();
			this.vbox3.Name = "vbox3";
			this.vbox3.Spacing = 6;
			// Container child vbox3.Gtk.Box+BoxChild
			this.yCheckButtonDatailedSummary = new global::Gamma.GtkWidgets.yCheckButton();
			this.yCheckButtonDatailedSummary.CanFocus = true;
			this.yCheckButtonDatailedSummary.Name = "yCheckButtonDatailedSummary";
			this.yCheckButtonDatailedSummary.Label = global::Mono.Unix.Catalog.GetString("Детализированный");
			this.yCheckButtonDatailedSummary.DrawIndicator = true;
			this.yCheckButtonDatailedSummary.UseUnderline = true;
			this.vbox3.Add(this.yCheckButtonDatailedSummary);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.vbox3[this.yCheckButtonDatailedSummary]));
			w8.PackType = ((global::Gtk.PackType)(1));
			w8.Position = 2;
			w8.Expand = false;
			w8.Fill = false;
			this.hbox5.Add(this.vbox3);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.hbox5[this.vbox3]));
			w9.Position = 1;
			w9.Expand = false;
			w9.Fill = false;
			this.vbox1.Add(this.hbox5);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hbox5]));
			w10.Position = 1;
			w10.Expand = false;
			w10.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hboxDriver = new global::Gtk.HBox();
			this.hboxDriver.Name = "hboxDriver";
			this.hboxDriver.Spacing = 6;
			// Container child hboxDriver.Gtk.Box+BoxChild
			this.labelDriver = new global::Gtk.Label();
			this.labelDriver.Name = "labelDriver";
			this.labelDriver.LabelProp = global::Mono.Unix.Catalog.GetString("Водитель:");
			this.hboxDriver.Add(this.labelDriver);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.hboxDriver[this.labelDriver]));
			w11.Position = 0;
			w11.Expand = false;
			w11.Fill = false;
			// Container child hboxDriver.Gtk.Box+BoxChild
			this.evmeDriver = new global::QS.Widgets.GtkUI.EntityViewModelEntry();
			this.evmeDriver.Events = ((global::Gdk.EventMask)(256));
			this.evmeDriver.Name = "evmeDriver";
			this.evmeDriver.CanEditReference = true;
			this.evmeDriver.CanDisposeEntitySelectorFactory = false;
			this.evmeDriver.CanOpenWithoutTabParent = false;
			this.hboxDriver.Add(this.evmeDriver);
			global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.hboxDriver[this.evmeDriver]));
			w12.Position = 1;
			this.vbox1.Add(this.hboxDriver);
			global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hboxDriver]));
			w13.Position = 2;
			w13.Expand = false;
			w13.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hboxCar = new global::Gtk.HBox();
			this.hboxCar.Name = "hboxCar";
			this.hboxCar.Spacing = 6;
			// Container child hboxCar.Gtk.Box+BoxChild
			this.label2 = new global::Gtk.Label();
			this.label2.Name = "label2";
			this.label2.LabelProp = global::Mono.Unix.Catalog.GetString("Автомобиль:");
			this.hboxCar.Add(this.label2);
			global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.hboxCar[this.label2]));
			w14.Position = 0;
			w14.Expand = false;
			w14.Fill = false;
			// Container child hboxCar.Gtk.Box+BoxChild
			this.entityentryCar = new global::QS.Views.Control.EntityEntry();
			this.entityentryCar.Events = ((global::Gdk.EventMask)(256));
			this.entityentryCar.Name = "entityentryCar";
			this.hboxCar.Add(this.entityentryCar);
			global::Gtk.Box.BoxChild w15 = ((global::Gtk.Box.BoxChild)(this.hboxCar[this.entityentryCar]));
			w15.Position = 1;
			this.vbox1.Add(this.hboxCar);
			global::Gtk.Box.BoxChild w16 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hboxCar]));
			w16.Position = 3;
			w16.Expand = false;
			w16.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hboxAuthor = new global::Gtk.HBox();
			this.hboxAuthor.Name = "hboxAuthor";
			this.hboxAuthor.Spacing = 6;
			// Container child hboxAuthor.Gtk.Box+BoxChild
			this.labelAuthor = new global::Gtk.Label();
			this.labelAuthor.Name = "labelAuthor";
			this.labelAuthor.LabelProp = global::Mono.Unix.Catalog.GetString("Автор:");
			this.hboxAuthor.Add(this.labelAuthor);
			global::Gtk.Box.BoxChild w17 = ((global::Gtk.Box.BoxChild)(this.hboxAuthor[this.labelAuthor]));
			w17.Position = 0;
			w17.Expand = false;
			w17.Fill = false;
			// Container child hboxAuthor.Gtk.Box+BoxChild
			this.evmeAuthor = new global::QS.Widgets.GtkUI.EntityViewModelEntry();
			this.evmeAuthor.Events = ((global::Gdk.EventMask)(256));
			this.evmeAuthor.Name = "evmeAuthor";
			this.evmeAuthor.CanEditReference = true;
			this.evmeAuthor.CanDisposeEntitySelectorFactory = false;
			this.evmeAuthor.CanOpenWithoutTabParent = false;
			this.hboxAuthor.Add(this.evmeAuthor);
			global::Gtk.Box.BoxChild w18 = ((global::Gtk.Box.BoxChild)(this.hboxAuthor[this.evmeAuthor]));
			w18.Position = 1;
			this.vbox1.Add(this.hboxAuthor);
			global::Gtk.Box.BoxChild w19 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hboxAuthor]));
			w19.Position = 4;
			w19.Expand = false;
			w19.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.yvboxCarModel = new global::Gamma.GtkWidgets.yVBox();
			this.yvboxCarModel.Name = "yvboxCarModel";
			this.yvboxCarModel.Spacing = 6;
			// Container child yvboxCarModel.Gtk.Box+BoxChild
			this.ylabelCarModel = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelCarModel.Name = "ylabelCarModel";
			this.ylabelCarModel.Xalign = 0F;
			this.ylabelCarModel.LabelProp = global::Mono.Unix.Catalog.GetString("Модель:");
			this.yvboxCarModel.Add(this.ylabelCarModel);
			global::Gtk.Box.BoxChild w20 = ((global::Gtk.Box.BoxChild)(this.yvboxCarModel[this.ylabelCarModel]));
			w20.Position = 0;
			w20.Expand = false;
			w20.Fill = false;
			// Container child yvboxCarModel.Gtk.Box+BoxChild
			this.yhboxCarModelContainer = new global::Gamma.GtkWidgets.yHBox();
			this.yhboxCarModelContainer.Name = "yhboxCarModelContainer";
			this.yhboxCarModelContainer.Spacing = 6;
			this.yvboxCarModel.Add(this.yhboxCarModelContainer);
			global::Gtk.Box.BoxChild w21 = ((global::Gtk.Box.BoxChild)(this.yvboxCarModel[this.yhboxCarModelContainer]));
			w21.Position = 1;
			this.vbox1.Add(this.yvboxCarModel);
			global::Gtk.Box.BoxChild w22 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.yvboxCarModel]));
			w22.Position = 5;
			// Container child vbox1.Gtk.Box+BoxChild
			this.buttonCreateReport = new global::Gamma.GtkWidgets.yButton();
			this.buttonCreateReport.CanFocus = true;
			this.buttonCreateReport.Name = "buttonCreateReport";
			this.buttonCreateReport.UseUnderline = true;
			this.buttonCreateReport.Label = global::Mono.Unix.Catalog.GetString("Сформировать отчет");
			this.vbox1.Add(this.buttonCreateReport);
			global::Gtk.Box.BoxChild w23 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.buttonCreateReport]));
			w23.Position = 6;
			w23.Expand = false;
			w23.Fill = false;
			this.Add(this.vbox1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.yCheckButtonDatailedSummary.Hide();
			this.hboxCar.Hide();
			this.hboxAuthor.Hide();
			this.Hide();
			this.radioDriver.Toggled += new global::System.EventHandler(this.OnRadioDriverToggled);
			this.radioCar.Toggled += new global::System.EventHandler(this.OnRadioCarToggled);
			this.radioSumm.Toggled += new global::System.EventHandler(this.OnRadioSummToggled);
		}
	}
}

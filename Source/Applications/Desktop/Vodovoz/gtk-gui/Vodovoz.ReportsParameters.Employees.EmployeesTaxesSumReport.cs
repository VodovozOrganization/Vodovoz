
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.ReportsParameters.Employees
{
	public partial class EmployeesTaxesSumReport
	{
		private global::Gtk.VBox vboxMain;

		private global::Gtk.VBox vbox4;

		private global::Gtk.HBox hbox1;

		private global::Gtk.Label lblDate;

		private global::QS.Widgets.GtkUI.DateRangePicker dateperiodpicker;

		private global::Gtk.VSeparator vseparator1;

		private global::Gtk.VBox vboxParameters;

		private global::Gamma.GtkWidgets.yHBox yhbox1;

		private global::Gamma.GtkWidgets.yVBox vboxRegistrationTypes;

		private global::Gamma.GtkWidgets.yLabel lblRegistrationTypes;

		private global::Gtk.ScrolledWindow GtkScrolledWindow;

		private global::Gamma.GtkWidgets.yTreeView treeViewRegistrationTypes;

		private global::Gamma.GtkWidgets.yVBox vboxPaymentForms;

		private global::Gamma.GtkWidgets.yLabel lblPaymentForm;

		private global::Gtk.ScrolledWindow GtkScrolledWindow1;

		private global::Gamma.GtkWidgets.yTreeView treeViewPaymentForms;

		private global::Gamma.GtkWidgets.yButton buttonCreateReport;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.ReportsParameters.Employees.EmployeesTaxesSumReport
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.ReportsParameters.Employees.EmployeesTaxesSumReport";
			// Container child Vodovoz.ReportsParameters.Employees.EmployeesTaxesSumReport.Gtk.Container+ContainerChild
			this.vboxMain = new global::Gtk.VBox();
			this.vboxMain.Name = "vboxMain";
			this.vboxMain.Spacing = 6;
			// Container child vboxMain.Gtk.Box+BoxChild
			this.vbox4 = new global::Gtk.VBox();
			this.vbox4.Name = "vbox4";
			this.vbox4.Spacing = 6;
			// Container child vbox4.Gtk.Box+BoxChild
			this.hbox1 = new global::Gtk.HBox();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.lblDate = new global::Gtk.Label();
			this.lblDate.Name = "lblDate";
			this.lblDate.Xalign = 0F;
			this.lblDate.LabelProp = global::Mono.Unix.Catalog.GetString("Дата:");
			this.hbox1.Add(this.lblDate);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.lblDate]));
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
			// Container child hbox1.Gtk.Box+BoxChild
			this.vseparator1 = new global::Gtk.VSeparator();
			this.vseparator1.Name = "vseparator1";
			this.hbox1.Add(this.vseparator1);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.vseparator1]));
			w3.Position = 2;
			w3.Expand = false;
			w3.Fill = false;
			this.vbox4.Add(this.hbox1);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.vbox4[this.hbox1]));
			w4.Position = 0;
			w4.Expand = false;
			w4.Fill = false;
			this.vboxMain.Add(this.vbox4);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.vboxMain[this.vbox4]));
			w5.Position = 0;
			w5.Expand = false;
			w5.Fill = false;
			// Container child vboxMain.Gtk.Box+BoxChild
			this.vboxParameters = new global::Gtk.VBox();
			this.vboxParameters.Name = "vboxParameters";
			this.vboxParameters.Spacing = 6;
			// Container child vboxParameters.Gtk.Box+BoxChild
			this.yhbox1 = new global::Gamma.GtkWidgets.yHBox();
			this.yhbox1.Name = "yhbox1";
			this.yhbox1.Spacing = 6;
			// Container child yhbox1.Gtk.Box+BoxChild
			this.vboxRegistrationTypes = new global::Gamma.GtkWidgets.yVBox();
			this.vboxRegistrationTypes.Name = "vboxRegistrationTypes";
			this.vboxRegistrationTypes.Spacing = 6;
			// Container child vboxRegistrationTypes.Gtk.Box+BoxChild
			this.lblRegistrationTypes = new global::Gamma.GtkWidgets.yLabel();
			this.lblRegistrationTypes.Name = "lblRegistrationTypes";
			this.lblRegistrationTypes.LabelProp = global::Mono.Unix.Catalog.GetString("<b>Оформление:</b>");
			this.lblRegistrationTypes.UseMarkup = true;
			this.vboxRegistrationTypes.Add(this.lblRegistrationTypes);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.vboxRegistrationTypes[this.lblRegistrationTypes]));
			w6.Position = 0;
			w6.Expand = false;
			w6.Fill = false;
			// Container child vboxRegistrationTypes.Gtk.Box+BoxChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.treeViewRegistrationTypes = new global::Gamma.GtkWidgets.yTreeView();
			this.treeViewRegistrationTypes.CanFocus = true;
			this.treeViewRegistrationTypes.Name = "treeViewRegistrationTypes";
			this.treeViewRegistrationTypes.EnableSearch = false;
			this.treeViewRegistrationTypes.HeadersVisible = false;
			this.GtkScrolledWindow.Add(this.treeViewRegistrationTypes);
			this.vboxRegistrationTypes.Add(this.GtkScrolledWindow);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.vboxRegistrationTypes[this.GtkScrolledWindow]));
			w8.Position = 1;
			w8.Expand = false;
			this.yhbox1.Add(this.vboxRegistrationTypes);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.yhbox1[this.vboxRegistrationTypes]));
			w9.Position = 0;
			w9.Expand = false;
			w9.Fill = false;
			// Container child yhbox1.Gtk.Box+BoxChild
			this.vboxPaymentForms = new global::Gamma.GtkWidgets.yVBox();
			this.vboxPaymentForms.Name = "vboxPaymentForms";
			this.vboxPaymentForms.Spacing = 6;
			// Container child vboxPaymentForms.Gtk.Box+BoxChild
			this.lblPaymentForm = new global::Gamma.GtkWidgets.yLabel();
			this.lblPaymentForm.Name = "lblPaymentForm";
			this.lblPaymentForm.LabelProp = global::Mono.Unix.Catalog.GetString("<b>Оплата:</b>");
			this.lblPaymentForm.UseMarkup = true;
			this.vboxPaymentForms.Add(this.lblPaymentForm);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.vboxPaymentForms[this.lblPaymentForm]));
			w10.Position = 0;
			w10.Expand = false;
			w10.Fill = false;
			// Container child vboxPaymentForms.Gtk.Box+BoxChild
			this.GtkScrolledWindow1 = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow1.Name = "GtkScrolledWindow1";
			this.GtkScrolledWindow1.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow1.Gtk.Container+ContainerChild
			this.treeViewPaymentForms = new global::Gamma.GtkWidgets.yTreeView();
			this.treeViewPaymentForms.CanFocus = true;
			this.treeViewPaymentForms.Name = "treeViewPaymentForms";
			this.treeViewPaymentForms.EnableSearch = false;
			this.treeViewPaymentForms.HeadersVisible = false;
			this.GtkScrolledWindow1.Add(this.treeViewPaymentForms);
			this.vboxPaymentForms.Add(this.GtkScrolledWindow1);
			global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.vboxPaymentForms[this.GtkScrolledWindow1]));
			w12.Position = 1;
			w12.Expand = false;
			this.yhbox1.Add(this.vboxPaymentForms);
			global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.yhbox1[this.vboxPaymentForms]));
			w13.Position = 1;
			w13.Expand = false;
			w13.Fill = false;
			this.vboxParameters.Add(this.yhbox1);
			global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.vboxParameters[this.yhbox1]));
			w14.Position = 0;
			w14.Expand = false;
			this.vboxMain.Add(this.vboxParameters);
			global::Gtk.Box.BoxChild w15 = ((global::Gtk.Box.BoxChild)(this.vboxMain[this.vboxParameters]));
			w15.Position = 1;
			// Container child vboxMain.Gtk.Box+BoxChild
			this.buttonCreateReport = new global::Gamma.GtkWidgets.yButton();
			this.buttonCreateReport.CanFocus = true;
			this.buttonCreateReport.Name = "buttonCreateReport";
			this.buttonCreateReport.UseUnderline = true;
			this.buttonCreateReport.Label = global::Mono.Unix.Catalog.GetString("Сформировать отчет");
			this.vboxMain.Add(this.buttonCreateReport);
			global::Gtk.Box.BoxChild w16 = ((global::Gtk.Box.BoxChild)(this.vboxMain[this.buttonCreateReport]));
			w16.Position = 2;
			w16.Expand = false;
			w16.Fill = false;
			this.Add(this.vboxMain);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
			this.buttonCreateReport.Clicked += new global::System.EventHandler(this.OnButtonCreateReportClicked);
		}
	}
}

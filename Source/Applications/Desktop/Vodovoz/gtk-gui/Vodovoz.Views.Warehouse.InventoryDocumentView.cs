
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.Views.Warehouse
{
	public partial class InventoryDocumentView
	{
		private global::Gamma.GtkWidgets.yVBox vboxMain;

		private global::Gamma.GtkWidgets.yHBox hboxHandleDlgBtns;

		private global::Gamma.GtkWidgets.yButton btnConfirm;

		private global::Gamma.GtkWidgets.yButton btnSave;

		private global::Gamma.GtkWidgets.yButton btnCancel;

		private global::Gamma.GtkWidgets.yButton btnPrint;

		private global::Gtk.VSeparator vseparator1;

		private global::Gamma.GtkWidgets.yRadioButton radioBtnBulkAccounting;

		private global::Gamma.GtkWidgets.yRadioButton radioBtnInstanceAccounting;

		private global::Gtk.HSeparator hseparator1;

		private global::Gtk.HBox hbox1;

		private global::Gtk.VBox vbox4;

		private global::Gtk.Table tableWriteoff;

		private global::Gamma.Widgets.yEnumComboBox enumCmbInventoryDocumentType;

		private global::Gtk.ScrolledWindow GtkScrolledWindow;

		private global::Gamma.GtkWidgets.yTextView ytextviewCommnet;

		private global::Gamma.GtkWidgets.yHBox hboxStorages;

		private global::Gamma.GtkWidgets.yHBox hboxWarehouseStorage;

		private global::Gtk.Label label7;

		private global::QS.Views.Control.EntityEntry warehouseStorageEntry;

		private global::Gamma.GtkWidgets.yHBox hboxEmployeeStorage;

		private global::Gamma.GtkWidgets.yLabel lblEmployee;

		private global::QS.Views.Control.EntityEntry employeeStorageEntry;

		private global::Gamma.GtkWidgets.yHBox hboxCarStorage;

		private global::Gamma.GtkWidgets.yLabel lblCarStorage;

		private global::QS.Views.Control.EntityEntry carStorageEntry;

		private global::Gtk.Label label1;

		private global::Gtk.Label label5;

		private global::Gamma.GtkWidgets.yLabel lblDocumentStatus;

		private global::Gtk.Label lblDocumentStatusTitle;

		private global::Gamma.GtkWidgets.yLabel lblInventoryDocumentType;

		private global::Gtk.VBox vboxParameters;

		private global::Gamma.GtkWidgets.yCheckButton ychkSortNomenclaturesByTitle;

		private global::QS.Widgets.GtkUI.DatePicker ydatepickerDocDate;

		private global::Gamma.GtkWidgets.yNotebook notebookItems;

		private global::Gamma.GtkWidgets.yVBox vboxNomenclatureItems;

		private global::Gtk.Label label3;

		private global::Gtk.ScrolledWindow GtkScrolledWindow1;

		private global::Gamma.GtkWidgets.yTreeView treeViewNomenclatureItems;

		private global::Gamma.GtkWidgets.yHBox hboxHandleNomenclatureItemsBtns;

		private global::Gamma.GtkWidgets.yButton btnFillNomenclatureItemsByStorage;

		private global::Gamma.GtkWidgets.yButton btnAddMissingNomenclatures;

		private global::Gamma.GtkWidgets.yButton btnAddFineToNomenclatureItem;

		private global::Gamma.GtkWidgets.yButton btnDeleteFineFromNomenclatureItem;

		private global::Gamma.GtkWidgets.yButton btnFillByAccounting;

		private global::Gtk.Label BulkAccountingPage;

		private global::Gamma.GtkWidgets.yVBox vboxNomenclatureInstanceItems;

		private global::Gtk.Expander expanderInstancesDiscrepancies;

		private global::Gtk.ScrolledWindow GtkScrolledWindow3;

		private global::Gamma.GtkWidgets.yTextView txtViewInstancesDiscrepancies;

		private global::Gtk.Label GtkLabelInstancesDiscrepancies;

		private global::Gtk.Label label4;

		private global::Gtk.ScrolledWindow GtkScrolledWindow2;

		private global::Gamma.GtkWidgets.yTreeView treeViewInstanceItems;

		private global::Gamma.GtkWidgets.yHBox hboxHandleInstanceItemsBtns;

		private global::Gamma.GtkWidgets.yButton btnFillNomenclatureInstanceItemsByStorage;

		private global::Gamma.GtkWidgets.yButton btnAddMissingNomenclatureInstances;

		private global::Gamma.GtkWidgets.yButton btnAddFineToNomenclatureInstanceItem;

		private global::Gamma.GtkWidgets.yButton btnDeleteFineFromNomenclatureInstanceItem;

		private global::Gtk.Label InstanceAccountingPage;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.Views.Warehouse.InventoryDocumentView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.Views.Warehouse.InventoryDocumentView";
			// Container child Vodovoz.Views.Warehouse.InventoryDocumentView.Gtk.Container+ContainerChild
			this.vboxMain = new global::Gamma.GtkWidgets.yVBox();
			this.vboxMain.Name = "vboxMain";
			this.vboxMain.Spacing = 6;
			// Container child vboxMain.Gtk.Box+BoxChild
			this.hboxHandleDlgBtns = new global::Gamma.GtkWidgets.yHBox();
			this.hboxHandleDlgBtns.Name = "hboxHandleDlgBtns";
			this.hboxHandleDlgBtns.Spacing = 6;
			// Container child hboxHandleDlgBtns.Gtk.Box+BoxChild
			this.btnConfirm = new global::Gamma.GtkWidgets.yButton();
			this.btnConfirm.CanFocus = true;
			this.btnConfirm.Name = "btnConfirm";
			this.btnConfirm.UseUnderline = true;
			this.btnConfirm.Label = global::Mono.Unix.Catalog.GetString("Подтвердить");
			this.hboxHandleDlgBtns.Add(this.btnConfirm);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.hboxHandleDlgBtns[this.btnConfirm]));
			w1.Position = 0;
			w1.Expand = false;
			w1.Fill = false;
			// Container child hboxHandleDlgBtns.Gtk.Box+BoxChild
			this.btnSave = new global::Gamma.GtkWidgets.yButton();
			this.btnSave.CanFocus = true;
			this.btnSave.Name = "btnSave";
			this.btnSave.UseUnderline = true;
			this.btnSave.Label = global::Mono.Unix.Catalog.GetString("Сохранить");
			this.hboxHandleDlgBtns.Add(this.btnSave);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hboxHandleDlgBtns[this.btnSave]));
			w2.Position = 1;
			w2.Expand = false;
			w2.Fill = false;
			// Container child hboxHandleDlgBtns.Gtk.Box+BoxChild
			this.btnCancel = new global::Gamma.GtkWidgets.yButton();
			this.btnCancel.CanFocus = true;
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.UseUnderline = true;
			this.btnCancel.Label = global::Mono.Unix.Catalog.GetString("Отменить");
			this.hboxHandleDlgBtns.Add(this.btnCancel);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.hboxHandleDlgBtns[this.btnCancel]));
			w3.Position = 2;
			w3.Expand = false;
			w3.Fill = false;
			// Container child hboxHandleDlgBtns.Gtk.Box+BoxChild
			this.btnPrint = new global::Gamma.GtkWidgets.yButton();
			this.btnPrint.CanFocus = true;
			this.btnPrint.Name = "btnPrint";
			this.btnPrint.UseUnderline = true;
			this.btnPrint.Label = global::Mono.Unix.Catalog.GetString("Печать");
			this.hboxHandleDlgBtns.Add(this.btnPrint);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hboxHandleDlgBtns[this.btnPrint]));
			w4.Position = 3;
			w4.Expand = false;
			w4.Fill = false;
			// Container child hboxHandleDlgBtns.Gtk.Box+BoxChild
			this.vseparator1 = new global::Gtk.VSeparator();
			this.vseparator1.Name = "vseparator1";
			this.hboxHandleDlgBtns.Add(this.vseparator1);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.hboxHandleDlgBtns[this.vseparator1]));
			w5.Position = 4;
			w5.Expand = false;
			w5.Fill = false;
			// Container child hboxHandleDlgBtns.Gtk.Box+BoxChild
			this.radioBtnBulkAccounting = new global::Gamma.GtkWidgets.yRadioButton();
			this.radioBtnBulkAccounting.CanFocus = true;
			this.radioBtnBulkAccounting.Name = "radioBtnBulkAccounting";
			this.radioBtnBulkAccounting.Label = global::Mono.Unix.Catalog.GetString("Объемный учет");
			this.radioBtnBulkAccounting.DrawIndicator = false;
			this.radioBtnBulkAccounting.Group = new global::GLib.SList(global::System.IntPtr.Zero);
			this.hboxHandleDlgBtns.Add(this.radioBtnBulkAccounting);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.hboxHandleDlgBtns[this.radioBtnBulkAccounting]));
			w6.Position = 5;
			w6.Expand = false;
			w6.Fill = false;
			// Container child hboxHandleDlgBtns.Gtk.Box+BoxChild
			this.radioBtnInstanceAccounting = new global::Gamma.GtkWidgets.yRadioButton();
			this.radioBtnInstanceAccounting.CanFocus = true;
			this.radioBtnInstanceAccounting.Name = "radioBtnInstanceAccounting";
			this.radioBtnInstanceAccounting.Label = global::Mono.Unix.Catalog.GetString("Экземплярный учет");
			this.radioBtnInstanceAccounting.DrawIndicator = false;
			this.radioBtnInstanceAccounting.Group = this.radioBtnBulkAccounting.Group;
			this.hboxHandleDlgBtns.Add(this.radioBtnInstanceAccounting);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.hboxHandleDlgBtns[this.radioBtnInstanceAccounting]));
			w7.Position = 6;
			w7.Expand = false;
			w7.Fill = false;
			this.vboxMain.Add(this.hboxHandleDlgBtns);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.vboxMain[this.hboxHandleDlgBtns]));
			w8.Position = 0;
			w8.Expand = false;
			w8.Fill = false;
			// Container child vboxMain.Gtk.Box+BoxChild
			this.hseparator1 = new global::Gtk.HSeparator();
			this.hseparator1.Name = "hseparator1";
			this.vboxMain.Add(this.hseparator1);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.vboxMain[this.hseparator1]));
			w9.Position = 1;
			w9.Expand = false;
			w9.Fill = false;
			// Container child vboxMain.Gtk.Box+BoxChild
			this.hbox1 = new global::Gtk.HBox();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.vbox4 = new global::Gtk.VBox();
			this.vbox4.Name = "vbox4";
			this.vbox4.Spacing = 6;
			this.vbox4.BorderWidth = ((uint)(6));
			// Container child vbox4.Gtk.Box+BoxChild
			this.tableWriteoff = new global::Gtk.Table(((uint)(7)), ((uint)(2)), false);
			this.tableWriteoff.Name = "tableWriteoff";
			this.tableWriteoff.RowSpacing = ((uint)(6));
			this.tableWriteoff.ColumnSpacing = ((uint)(6));
			// Container child tableWriteoff.Gtk.Table+TableChild
			this.enumCmbInventoryDocumentType = new global::Gamma.Widgets.yEnumComboBox();
			this.enumCmbInventoryDocumentType.Name = "enumCmbInventoryDocumentType";
			this.enumCmbInventoryDocumentType.ShowSpecialStateAll = false;
			this.enumCmbInventoryDocumentType.ShowSpecialStateNot = false;
			this.enumCmbInventoryDocumentType.UseShortTitle = false;
			this.enumCmbInventoryDocumentType.DefaultFirst = false;
			this.tableWriteoff.Add(this.enumCmbInventoryDocumentType);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.tableWriteoff[this.enumCmbInventoryDocumentType]));
			w10.TopAttach = ((uint)(2));
			w10.BottomAttach = ((uint)(3));
			w10.LeftAttach = ((uint)(1));
			w10.RightAttach = ((uint)(2));
			w10.XOptions = ((global::Gtk.AttachOptions)(4));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableWriteoff.Gtk.Table+TableChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.ytextviewCommnet = new global::Gamma.GtkWidgets.yTextView();
			this.ytextviewCommnet.CanFocus = true;
			this.ytextviewCommnet.Name = "ytextviewCommnet";
			this.GtkScrolledWindow.Add(this.ytextviewCommnet);
			this.tableWriteoff.Add(this.GtkScrolledWindow);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.tableWriteoff[this.GtkScrolledWindow]));
			w12.TopAttach = ((uint)(6));
			w12.BottomAttach = ((uint)(7));
			w12.LeftAttach = ((uint)(1));
			w12.RightAttach = ((uint)(2));
			w12.XOptions = ((global::Gtk.AttachOptions)(4));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableWriteoff.Gtk.Table+TableChild
			this.hboxStorages = new global::Gamma.GtkWidgets.yHBox();
			this.hboxStorages.Name = "hboxStorages";
			this.hboxStorages.Spacing = 6;
			// Container child hboxStorages.Gtk.Box+BoxChild
			this.hboxWarehouseStorage = new global::Gamma.GtkWidgets.yHBox();
			this.hboxWarehouseStorage.Name = "hboxWarehouseStorage";
			this.hboxWarehouseStorage.Spacing = 6;
			// Container child hboxWarehouseStorage.Gtk.Box+BoxChild
			this.label7 = new global::Gtk.Label();
			this.label7.Name = "label7";
			this.label7.Xalign = 1F;
			this.label7.LabelProp = global::Mono.Unix.Catalog.GetString("Склад:");
			this.hboxWarehouseStorage.Add(this.label7);
			global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.hboxWarehouseStorage[this.label7]));
			w13.Position = 0;
			// Container child hboxWarehouseStorage.Gtk.Box+BoxChild
			this.warehouseStorageEntry = new global::QS.Views.Control.EntityEntry();
			this.warehouseStorageEntry.Events = ((global::Gdk.EventMask)(256));
			this.warehouseStorageEntry.Name = "warehouseStorageEntry";
			this.hboxWarehouseStorage.Add(this.warehouseStorageEntry);
			global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.hboxWarehouseStorage[this.warehouseStorageEntry]));
			w14.Position = 1;
			this.hboxStorages.Add(this.hboxWarehouseStorage);
			global::Gtk.Box.BoxChild w15 = ((global::Gtk.Box.BoxChild)(this.hboxStorages[this.hboxWarehouseStorage]));
			w15.Position = 0;
			// Container child hboxStorages.Gtk.Box+BoxChild
			this.hboxEmployeeStorage = new global::Gamma.GtkWidgets.yHBox();
			this.hboxEmployeeStorage.Name = "hboxEmployeeStorage";
			this.hboxEmployeeStorage.Spacing = 6;
			// Container child hboxEmployeeStorage.Gtk.Box+BoxChild
			this.lblEmployee = new global::Gamma.GtkWidgets.yLabel();
			this.lblEmployee.Name = "lblEmployee";
			this.lblEmployee.Xalign = 1F;
			this.lblEmployee.LabelProp = global::Mono.Unix.Catalog.GetString("Сотрудник:");
			this.hboxEmployeeStorage.Add(this.lblEmployee);
			global::Gtk.Box.BoxChild w16 = ((global::Gtk.Box.BoxChild)(this.hboxEmployeeStorage[this.lblEmployee]));
			w16.Position = 0;
			// Container child hboxEmployeeStorage.Gtk.Box+BoxChild
			this.employeeStorageEntry = new global::QS.Views.Control.EntityEntry();
			this.employeeStorageEntry.Events = ((global::Gdk.EventMask)(256));
			this.employeeStorageEntry.Name = "employeeStorageEntry";
			this.hboxEmployeeStorage.Add(this.employeeStorageEntry);
			global::Gtk.Box.BoxChild w17 = ((global::Gtk.Box.BoxChild)(this.hboxEmployeeStorage[this.employeeStorageEntry]));
			w17.Position = 1;
			this.hboxStorages.Add(this.hboxEmployeeStorage);
			global::Gtk.Box.BoxChild w18 = ((global::Gtk.Box.BoxChild)(this.hboxStorages[this.hboxEmployeeStorage]));
			w18.Position = 1;
			// Container child hboxStorages.Gtk.Box+BoxChild
			this.hboxCarStorage = new global::Gamma.GtkWidgets.yHBox();
			this.hboxCarStorage.Name = "hboxCarStorage";
			this.hboxCarStorage.Spacing = 6;
			// Container child hboxCarStorage.Gtk.Box+BoxChild
			this.lblCarStorage = new global::Gamma.GtkWidgets.yLabel();
			this.lblCarStorage.Name = "lblCarStorage";
			this.lblCarStorage.Xalign = 1F;
			this.lblCarStorage.LabelProp = global::Mono.Unix.Catalog.GetString("Автомобиль:");
			this.hboxCarStorage.Add(this.lblCarStorage);
			global::Gtk.Box.BoxChild w19 = ((global::Gtk.Box.BoxChild)(this.hboxCarStorage[this.lblCarStorage]));
			w19.Position = 0;
			// Container child hboxCarStorage.Gtk.Box+BoxChild
			this.carStorageEntry = new global::QS.Views.Control.EntityEntry();
			this.carStorageEntry.Events = ((global::Gdk.EventMask)(256));
			this.carStorageEntry.Name = "carStorageEntry";
			this.hboxCarStorage.Add(this.carStorageEntry);
			global::Gtk.Box.BoxChild w20 = ((global::Gtk.Box.BoxChild)(this.hboxCarStorage[this.carStorageEntry]));
			w20.Position = 1;
			this.hboxStorages.Add(this.hboxCarStorage);
			global::Gtk.Box.BoxChild w21 = ((global::Gtk.Box.BoxChild)(this.hboxStorages[this.hboxCarStorage]));
			w21.Position = 2;
			this.tableWriteoff.Add(this.hboxStorages);
			global::Gtk.Table.TableChild w22 = ((global::Gtk.Table.TableChild)(this.tableWriteoff[this.hboxStorages]));
			w22.TopAttach = ((uint)(3));
			w22.BottomAttach = ((uint)(4));
			w22.RightAttach = ((uint)(2));
			w22.XOptions = ((global::Gtk.AttachOptions)(4));
			w22.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableWriteoff.Gtk.Table+TableChild
			this.label1 = new global::Gtk.Label();
			this.label1.Name = "label1";
			this.label1.Xalign = 1F;
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString("Дата:");
			this.tableWriteoff.Add(this.label1);
			global::Gtk.Table.TableChild w23 = ((global::Gtk.Table.TableChild)(this.tableWriteoff[this.label1]));
			w23.TopAttach = ((uint)(1));
			w23.BottomAttach = ((uint)(2));
			w23.XOptions = ((global::Gtk.AttachOptions)(4));
			w23.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableWriteoff.Gtk.Table+TableChild
			this.label5 = new global::Gtk.Label();
			this.label5.Name = "label5";
			this.label5.Xalign = 1F;
			this.label5.Yalign = 0F;
			this.label5.LabelProp = global::Mono.Unix.Catalog.GetString("Комментарий:");
			this.tableWriteoff.Add(this.label5);
			global::Gtk.Table.TableChild w24 = ((global::Gtk.Table.TableChild)(this.tableWriteoff[this.label5]));
			w24.TopAttach = ((uint)(6));
			w24.BottomAttach = ((uint)(7));
			w24.XOptions = ((global::Gtk.AttachOptions)(4));
			w24.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableWriteoff.Gtk.Table+TableChild
			this.lblDocumentStatus = new global::Gamma.GtkWidgets.yLabel();
			this.lblDocumentStatus.Name = "lblDocumentStatus";
			this.lblDocumentStatus.Xalign = 0F;
			this.lblDocumentStatus.LabelProp = global::Mono.Unix.Catalog.GetString("статус");
			this.tableWriteoff.Add(this.lblDocumentStatus);
			global::Gtk.Table.TableChild w25 = ((global::Gtk.Table.TableChild)(this.tableWriteoff[this.lblDocumentStatus]));
			w25.LeftAttach = ((uint)(1));
			w25.RightAttach = ((uint)(2));
			w25.XOptions = ((global::Gtk.AttachOptions)(4));
			w25.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableWriteoff.Gtk.Table+TableChild
			this.lblDocumentStatusTitle = new global::Gtk.Label();
			this.lblDocumentStatusTitle.Name = "lblDocumentStatusTitle";
			this.lblDocumentStatusTitle.Xalign = 1F;
			this.lblDocumentStatusTitle.LabelProp = global::Mono.Unix.Catalog.GetString("Статус:");
			this.tableWriteoff.Add(this.lblDocumentStatusTitle);
			global::Gtk.Table.TableChild w26 = ((global::Gtk.Table.TableChild)(this.tableWriteoff[this.lblDocumentStatusTitle]));
			w26.XOptions = ((global::Gtk.AttachOptions)(4));
			w26.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableWriteoff.Gtk.Table+TableChild
			this.lblInventoryDocumentType = new global::Gamma.GtkWidgets.yLabel();
			this.lblInventoryDocumentType.Name = "lblInventoryDocumentType";
			this.lblInventoryDocumentType.Xalign = 1F;
			this.lblInventoryDocumentType.LabelProp = global::Mono.Unix.Catalog.GetString("Тип документа:");
			this.tableWriteoff.Add(this.lblInventoryDocumentType);
			global::Gtk.Table.TableChild w27 = ((global::Gtk.Table.TableChild)(this.tableWriteoff[this.lblInventoryDocumentType]));
			w27.TopAttach = ((uint)(2));
			w27.BottomAttach = ((uint)(3));
			w27.XOptions = ((global::Gtk.AttachOptions)(4));
			w27.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableWriteoff.Gtk.Table+TableChild
			this.vboxParameters = new global::Gtk.VBox();
			this.vboxParameters.Name = "vboxParameters";
			this.vboxParameters.Spacing = 6;
			this.tableWriteoff.Add(this.vboxParameters);
			global::Gtk.Table.TableChild w28 = ((global::Gtk.Table.TableChild)(this.tableWriteoff[this.vboxParameters]));
			w28.TopAttach = ((uint)(5));
			w28.BottomAttach = ((uint)(6));
			w28.RightAttach = ((uint)(2));
			// Container child tableWriteoff.Gtk.Table+TableChild
			this.ychkSortNomenclaturesByTitle = new global::Gamma.GtkWidgets.yCheckButton();
			this.ychkSortNomenclaturesByTitle.CanFocus = true;
			this.ychkSortNomenclaturesByTitle.Name = "ychkSortNomenclaturesByTitle";
			this.ychkSortNomenclaturesByTitle.Label = global::Mono.Unix.Catalog.GetString("Сортировать номенклатуры по имени");
			this.ychkSortNomenclaturesByTitle.DrawIndicator = true;
			this.ychkSortNomenclaturesByTitle.UseUnderline = true;
			this.tableWriteoff.Add(this.ychkSortNomenclaturesByTitle);
			global::Gtk.Table.TableChild w29 = ((global::Gtk.Table.TableChild)(this.tableWriteoff[this.ychkSortNomenclaturesByTitle]));
			w29.TopAttach = ((uint)(4));
			w29.BottomAttach = ((uint)(5));
			w29.RightAttach = ((uint)(2));
			w29.XOptions = ((global::Gtk.AttachOptions)(0));
			w29.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableWriteoff.Gtk.Table+TableChild
			this.ydatepickerDocDate = new global::QS.Widgets.GtkUI.DatePicker();
			this.ydatepickerDocDate.Events = ((global::Gdk.EventMask)(256));
			this.ydatepickerDocDate.Name = "ydatepickerDocDate";
			this.ydatepickerDocDate.WithTime = true;
			this.ydatepickerDocDate.HideCalendarButton = false;
			this.ydatepickerDocDate.Date = new global::System.DateTime(0);
			this.ydatepickerDocDate.IsEditable = true;
			this.ydatepickerDocDate.AutoSeparation = true;
			this.tableWriteoff.Add(this.ydatepickerDocDate);
			global::Gtk.Table.TableChild w30 = ((global::Gtk.Table.TableChild)(this.tableWriteoff[this.ydatepickerDocDate]));
			w30.TopAttach = ((uint)(1));
			w30.BottomAttach = ((uint)(2));
			w30.LeftAttach = ((uint)(1));
			w30.RightAttach = ((uint)(2));
			w30.XOptions = ((global::Gtk.AttachOptions)(4));
			w30.YOptions = ((global::Gtk.AttachOptions)(4));
			this.vbox4.Add(this.tableWriteoff);
			global::Gtk.Box.BoxChild w31 = ((global::Gtk.Box.BoxChild)(this.vbox4[this.tableWriteoff]));
			w31.Position = 0;
			this.hbox1.Add(this.vbox4);
			global::Gtk.Box.BoxChild w32 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.vbox4]));
			w32.Position = 0;
			w32.Expand = false;
			w32.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.notebookItems = new global::Gamma.GtkWidgets.yNotebook();
			this.notebookItems.CanFocus = true;
			this.notebookItems.Name = "notebookItems";
			this.notebookItems.CurrentPage = 0;
			// Container child notebookItems.Gtk.Notebook+NotebookChild
			this.vboxNomenclatureItems = new global::Gamma.GtkWidgets.yVBox();
			this.vboxNomenclatureItems.Name = "vboxNomenclatureItems";
			this.vboxNomenclatureItems.Spacing = 6;
			// Container child vboxNomenclatureItems.Gtk.Box+BoxChild
			this.label3 = new global::Gtk.Label();
			this.label3.Name = "label3";
			this.label3.Xalign = 0F;
			this.label3.LabelProp = global::Mono.Unix.Catalog.GetString("Список номенклатур:");
			this.vboxNomenclatureItems.Add(this.label3);
			global::Gtk.Box.BoxChild w33 = ((global::Gtk.Box.BoxChild)(this.vboxNomenclatureItems[this.label3]));
			w33.Position = 0;
			w33.Expand = false;
			// Container child vboxNomenclatureItems.Gtk.Box+BoxChild
			this.GtkScrolledWindow1 = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow1.Name = "GtkScrolledWindow1";
			this.GtkScrolledWindow1.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow1.Gtk.Container+ContainerChild
			this.treeViewNomenclatureItems = new global::Gamma.GtkWidgets.yTreeView();
			this.treeViewNomenclatureItems.CanFocus = true;
			this.treeViewNomenclatureItems.Name = "treeViewNomenclatureItems";
			this.GtkScrolledWindow1.Add(this.treeViewNomenclatureItems);
			this.vboxNomenclatureItems.Add(this.GtkScrolledWindow1);
			global::Gtk.Box.BoxChild w35 = ((global::Gtk.Box.BoxChild)(this.vboxNomenclatureItems[this.GtkScrolledWindow1]));
			w35.Position = 1;
			// Container child vboxNomenclatureItems.Gtk.Box+BoxChild
			this.hboxHandleNomenclatureItemsBtns = new global::Gamma.GtkWidgets.yHBox();
			this.hboxHandleNomenclatureItemsBtns.Name = "hboxHandleNomenclatureItemsBtns";
			this.hboxHandleNomenclatureItemsBtns.Spacing = 6;
			// Container child hboxHandleNomenclatureItemsBtns.Gtk.Box+BoxChild
			this.btnFillNomenclatureItemsByStorage = new global::Gamma.GtkWidgets.yButton();
			this.btnFillNomenclatureItemsByStorage.CanFocus = true;
			this.btnFillNomenclatureItemsByStorage.Name = "btnFillNomenclatureItemsByStorage";
			this.btnFillNomenclatureItemsByStorage.UseUnderline = true;
			this.btnFillNomenclatureItemsByStorage.Label = global::Mono.Unix.Catalog.GetString("Заполнить по складу");
			this.hboxHandleNomenclatureItemsBtns.Add(this.btnFillNomenclatureItemsByStorage);
			global::Gtk.Box.BoxChild w36 = ((global::Gtk.Box.BoxChild)(this.hboxHandleNomenclatureItemsBtns[this.btnFillNomenclatureItemsByStorage]));
			w36.Position = 0;
			w36.Expand = false;
			w36.Fill = false;
			// Container child hboxHandleNomenclatureItemsBtns.Gtk.Box+BoxChild
			this.btnAddMissingNomenclatures = new global::Gamma.GtkWidgets.yButton();
			this.btnAddMissingNomenclatures.CanFocus = true;
			this.btnAddMissingNomenclatures.Name = "btnAddMissingNomenclatures";
			this.btnAddMissingNomenclatures.UseUnderline = true;
			this.btnAddMissingNomenclatures.Label = global::Mono.Unix.Catalog.GetString("Добавить отсутствующие");
			this.hboxHandleNomenclatureItemsBtns.Add(this.btnAddMissingNomenclatures);
			global::Gtk.Box.BoxChild w37 = ((global::Gtk.Box.BoxChild)(this.hboxHandleNomenclatureItemsBtns[this.btnAddMissingNomenclatures]));
			w37.Position = 1;
			w37.Expand = false;
			w37.Fill = false;
			// Container child hboxHandleNomenclatureItemsBtns.Gtk.Box+BoxChild
			this.btnAddFineToNomenclatureItem = new global::Gamma.GtkWidgets.yButton();
			this.btnAddFineToNomenclatureItem.CanFocus = true;
			this.btnAddFineToNomenclatureItem.Name = "btnAddFineToNomenclatureItem";
			this.btnAddFineToNomenclatureItem.UseUnderline = true;
			this.btnAddFineToNomenclatureItem.Label = global::Mono.Unix.Catalog.GetString("Добавить штраф");
			this.hboxHandleNomenclatureItemsBtns.Add(this.btnAddFineToNomenclatureItem);
			global::Gtk.Box.BoxChild w38 = ((global::Gtk.Box.BoxChild)(this.hboxHandleNomenclatureItemsBtns[this.btnAddFineToNomenclatureItem]));
			w38.Position = 2;
			w38.Expand = false;
			w38.Fill = false;
			// Container child hboxHandleNomenclatureItemsBtns.Gtk.Box+BoxChild
			this.btnDeleteFineFromNomenclatureItem = new global::Gamma.GtkWidgets.yButton();
			this.btnDeleteFineFromNomenclatureItem.CanFocus = true;
			this.btnDeleteFineFromNomenclatureItem.Name = "btnDeleteFineFromNomenclatureItem";
			this.btnDeleteFineFromNomenclatureItem.UseUnderline = true;
			this.btnDeleteFineFromNomenclatureItem.Label = global::Mono.Unix.Catalog.GetString("Удалить штраф");
			this.hboxHandleNomenclatureItemsBtns.Add(this.btnDeleteFineFromNomenclatureItem);
			global::Gtk.Box.BoxChild w39 = ((global::Gtk.Box.BoxChild)(this.hboxHandleNomenclatureItemsBtns[this.btnDeleteFineFromNomenclatureItem]));
			w39.Position = 3;
			w39.Expand = false;
			w39.Fill = false;
			// Container child hboxHandleNomenclatureItemsBtns.Gtk.Box+BoxChild
			this.btnFillByAccounting = new global::Gamma.GtkWidgets.yButton();
			this.btnFillByAccounting.CanFocus = true;
			this.btnFillByAccounting.Name = "btnFillByAccounting";
			this.btnFillByAccounting.UseUnderline = true;
			this.btnFillByAccounting.Label = global::Mono.Unix.Catalog.GetString("Заполнить по учету");
			this.hboxHandleNomenclatureItemsBtns.Add(this.btnFillByAccounting);
			global::Gtk.Box.BoxChild w40 = ((global::Gtk.Box.BoxChild)(this.hboxHandleNomenclatureItemsBtns[this.btnFillByAccounting]));
			w40.Position = 4;
			w40.Expand = false;
			w40.Fill = false;
			this.vboxNomenclatureItems.Add(this.hboxHandleNomenclatureItemsBtns);
			global::Gtk.Box.BoxChild w41 = ((global::Gtk.Box.BoxChild)(this.vboxNomenclatureItems[this.hboxHandleNomenclatureItemsBtns]));
			w41.Position = 2;
			w41.Expand = false;
			w41.Fill = false;
			this.notebookItems.Add(this.vboxNomenclatureItems);
			// Notebook tab
			this.BulkAccountingPage = new global::Gtk.Label();
			this.BulkAccountingPage.Name = "BulkAccountingPage";
			this.BulkAccountingPage.LabelProp = global::Mono.Unix.Catalog.GetString("Объемный учет");
			this.notebookItems.SetTabLabel(this.vboxNomenclatureItems, this.BulkAccountingPage);
			this.BulkAccountingPage.ShowAll();
			// Container child notebookItems.Gtk.Notebook+NotebookChild
			this.vboxNomenclatureInstanceItems = new global::Gamma.GtkWidgets.yVBox();
			this.vboxNomenclatureInstanceItems.Name = "vboxNomenclatureInstanceItems";
			this.vboxNomenclatureInstanceItems.Spacing = 6;
			// Container child vboxNomenclatureInstanceItems.Gtk.Box+BoxChild
			this.expanderInstancesDiscrepancies = new global::Gtk.Expander(null);
			this.expanderInstancesDiscrepancies.CanFocus = true;
			this.expanderInstancesDiscrepancies.Name = "expanderInstancesDiscrepancies";
			// Container child expanderInstancesDiscrepancies.Gtk.Container+ContainerChild
			this.GtkScrolledWindow3 = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow3.Name = "GtkScrolledWindow3";
			this.GtkScrolledWindow3.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow3.Gtk.Container+ContainerChild
			this.txtViewInstancesDiscrepancies = new global::Gamma.GtkWidgets.yTextView();
			this.txtViewInstancesDiscrepancies.CanFocus = true;
			this.txtViewInstancesDiscrepancies.Name = "txtViewInstancesDiscrepancies";
			this.GtkScrolledWindow3.Add(this.txtViewInstancesDiscrepancies);
			this.expanderInstancesDiscrepancies.Add(this.GtkScrolledWindow3);
			this.GtkLabelInstancesDiscrepancies = new global::Gtk.Label();
			this.GtkLabelInstancesDiscrepancies.Name = "GtkLabelInstancesDiscrepancies";
			this.GtkLabelInstancesDiscrepancies.LabelProp = global::Mono.Unix.Catalog.GetString("Расхождения экземплярного учета");
			this.GtkLabelInstancesDiscrepancies.UseUnderline = true;
			this.expanderInstancesDiscrepancies.LabelWidget = this.GtkLabelInstancesDiscrepancies;
			this.vboxNomenclatureInstanceItems.Add(this.expanderInstancesDiscrepancies);
			global::Gtk.Box.BoxChild w45 = ((global::Gtk.Box.BoxChild)(this.vboxNomenclatureInstanceItems[this.expanderInstancesDiscrepancies]));
			w45.Position = 0;
			w45.Expand = false;
			w45.Fill = false;
			// Container child vboxNomenclatureInstanceItems.Gtk.Box+BoxChild
			this.label4 = new global::Gtk.Label();
			this.label4.Name = "label4";
			this.label4.Xalign = 0F;
			this.label4.LabelProp = global::Mono.Unix.Catalog.GetString("Список экземпляров:");
			this.vboxNomenclatureInstanceItems.Add(this.label4);
			global::Gtk.Box.BoxChild w46 = ((global::Gtk.Box.BoxChild)(this.vboxNomenclatureInstanceItems[this.label4]));
			w46.Position = 1;
			w46.Expand = false;
			// Container child vboxNomenclatureInstanceItems.Gtk.Box+BoxChild
			this.GtkScrolledWindow2 = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow2.Name = "GtkScrolledWindow2";
			this.GtkScrolledWindow2.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow2.Gtk.Container+ContainerChild
			this.treeViewInstanceItems = new global::Gamma.GtkWidgets.yTreeView();
			this.treeViewInstanceItems.CanFocus = true;
			this.treeViewInstanceItems.Name = "treeViewInstanceItems";
			this.GtkScrolledWindow2.Add(this.treeViewInstanceItems);
			this.vboxNomenclatureInstanceItems.Add(this.GtkScrolledWindow2);
			global::Gtk.Box.BoxChild w48 = ((global::Gtk.Box.BoxChild)(this.vboxNomenclatureInstanceItems[this.GtkScrolledWindow2]));
			w48.Position = 2;
			// Container child vboxNomenclatureInstanceItems.Gtk.Box+BoxChild
			this.hboxHandleInstanceItemsBtns = new global::Gamma.GtkWidgets.yHBox();
			this.hboxHandleInstanceItemsBtns.Name = "hboxHandleInstanceItemsBtns";
			this.hboxHandleInstanceItemsBtns.Spacing = 6;
			// Container child hboxHandleInstanceItemsBtns.Gtk.Box+BoxChild
			this.btnFillNomenclatureInstanceItemsByStorage = new global::Gamma.GtkWidgets.yButton();
			this.btnFillNomenclatureInstanceItemsByStorage.CanFocus = true;
			this.btnFillNomenclatureInstanceItemsByStorage.Name = "btnFillNomenclatureInstanceItemsByStorage";
			this.btnFillNomenclatureInstanceItemsByStorage.UseUnderline = true;
			this.btnFillNomenclatureInstanceItemsByStorage.Label = global::Mono.Unix.Catalog.GetString("Заполнить по складу");
			this.hboxHandleInstanceItemsBtns.Add(this.btnFillNomenclatureInstanceItemsByStorage);
			global::Gtk.Box.BoxChild w49 = ((global::Gtk.Box.BoxChild)(this.hboxHandleInstanceItemsBtns[this.btnFillNomenclatureInstanceItemsByStorage]));
			w49.Position = 0;
			w49.Expand = false;
			w49.Fill = false;
			// Container child hboxHandleInstanceItemsBtns.Gtk.Box+BoxChild
			this.btnAddMissingNomenclatureInstances = new global::Gamma.GtkWidgets.yButton();
			this.btnAddMissingNomenclatureInstances.CanFocus = true;
			this.btnAddMissingNomenclatureInstances.Name = "btnAddMissingNomenclatureInstances";
			this.btnAddMissingNomenclatureInstances.UseUnderline = true;
			this.btnAddMissingNomenclatureInstances.Label = global::Mono.Unix.Catalog.GetString("Добавить отсутствующие");
			this.hboxHandleInstanceItemsBtns.Add(this.btnAddMissingNomenclatureInstances);
			global::Gtk.Box.BoxChild w50 = ((global::Gtk.Box.BoxChild)(this.hboxHandleInstanceItemsBtns[this.btnAddMissingNomenclatureInstances]));
			w50.Position = 1;
			w50.Expand = false;
			w50.Fill = false;
			// Container child hboxHandleInstanceItemsBtns.Gtk.Box+BoxChild
			this.btnAddFineToNomenclatureInstanceItem = new global::Gamma.GtkWidgets.yButton();
			this.btnAddFineToNomenclatureInstanceItem.CanFocus = true;
			this.btnAddFineToNomenclatureInstanceItem.Name = "btnAddFineToNomenclatureInstanceItem";
			this.btnAddFineToNomenclatureInstanceItem.UseUnderline = true;
			this.btnAddFineToNomenclatureInstanceItem.Label = global::Mono.Unix.Catalog.GetString("Добавить штраф");
			this.hboxHandleInstanceItemsBtns.Add(this.btnAddFineToNomenclatureInstanceItem);
			global::Gtk.Box.BoxChild w51 = ((global::Gtk.Box.BoxChild)(this.hboxHandleInstanceItemsBtns[this.btnAddFineToNomenclatureInstanceItem]));
			w51.Position = 2;
			w51.Expand = false;
			w51.Fill = false;
			// Container child hboxHandleInstanceItemsBtns.Gtk.Box+BoxChild
			this.btnDeleteFineFromNomenclatureInstanceItem = new global::Gamma.GtkWidgets.yButton();
			this.btnDeleteFineFromNomenclatureInstanceItem.CanFocus = true;
			this.btnDeleteFineFromNomenclatureInstanceItem.Name = "btnDeleteFineFromNomenclatureInstanceItem";
			this.btnDeleteFineFromNomenclatureInstanceItem.UseUnderline = true;
			this.btnDeleteFineFromNomenclatureInstanceItem.Label = global::Mono.Unix.Catalog.GetString("Удалить штраф");
			this.hboxHandleInstanceItemsBtns.Add(this.btnDeleteFineFromNomenclatureInstanceItem);
			global::Gtk.Box.BoxChild w52 = ((global::Gtk.Box.BoxChild)(this.hboxHandleInstanceItemsBtns[this.btnDeleteFineFromNomenclatureInstanceItem]));
			w52.Position = 3;
			w52.Expand = false;
			w52.Fill = false;
			this.vboxNomenclatureInstanceItems.Add(this.hboxHandleInstanceItemsBtns);
			global::Gtk.Box.BoxChild w53 = ((global::Gtk.Box.BoxChild)(this.vboxNomenclatureInstanceItems[this.hboxHandleInstanceItemsBtns]));
			w53.Position = 3;
			w53.Expand = false;
			w53.Fill = false;
			this.notebookItems.Add(this.vboxNomenclatureInstanceItems);
			global::Gtk.Notebook.NotebookChild w54 = ((global::Gtk.Notebook.NotebookChild)(this.notebookItems[this.vboxNomenclatureInstanceItems]));
			w54.Position = 1;
			// Notebook tab
			this.InstanceAccountingPage = new global::Gtk.Label();
			this.InstanceAccountingPage.Name = "InstanceAccountingPage";
			this.InstanceAccountingPage.LabelProp = global::Mono.Unix.Catalog.GetString("Экземплярный учет");
			this.notebookItems.SetTabLabel(this.vboxNomenclatureInstanceItems, this.InstanceAccountingPage);
			this.InstanceAccountingPage.ShowAll();
			this.hbox1.Add(this.notebookItems);
			global::Gtk.Box.BoxChild w55 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.notebookItems]));
			w55.Position = 1;
			this.vboxMain.Add(this.hbox1);
			global::Gtk.Box.BoxChild w56 = ((global::Gtk.Box.BoxChild)(this.vboxMain[this.hbox1]));
			w56.Position = 2;
			this.Add(this.vboxMain);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}

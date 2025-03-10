
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.Filters.GtkViews
{
	public partial class WarehouseDocumentsJournalFilterView
	{
		private global::Gamma.GtkWidgets.yHBox yhboxMainContainer;

		private global::Gamma.GtkWidgets.yTable ytableMain;

		private global::QS.Widgets.GtkUI.DateRangePicker daterangepickerPeriod;

		private global::QS.Views.Control.EntityEntry entityentryCar;

		private global::QS.Views.Control.EntityEntry entityentryDriver;

		private global::QS.Views.Control.EntityEntry entityentryEmployee;

		private global::QS.Views.Control.EntityEntry entityentryWarehouse;

		private global::Gamma.GtkWidgets.yCheckButton ychkbtnQRScanRequired;

		private global::Gamma.Widgets.yEnumComboBox yenumcomboboxDocumentType;

		private global::Gamma.Widgets.yEnumComboBox yenumcomboboxMovementStatus;

		private global::Gamma.GtkWidgets.yLabel ylabelCar;

		private global::Gamma.GtkWidgets.yLabel ylabelDocumentType;

		private global::Gamma.GtkWidgets.yLabel ylabelDriver;

		private global::Gamma.GtkWidgets.yLabel ylabelEmployee;

		private global::Gamma.GtkWidgets.yLabel ylabelMovementStatus;

		private global::Gamma.GtkWidgets.yLabel ylabelPeriod;

		private global::Gamma.GtkWidgets.yLabel ylabelWarehouse;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.Filters.GtkViews.WarehouseDocumentsJournalFilterView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.Filters.GtkViews.WarehouseDocumentsJournalFilterView";
			// Container child Vodovoz.Filters.GtkViews.WarehouseDocumentsJournalFilterView.Gtk.Container+ContainerChild
			this.yhboxMainContainer = new global::Gamma.GtkWidgets.yHBox();
			this.yhboxMainContainer.Name = "yhboxMainContainer";
			this.yhboxMainContainer.Spacing = 6;
			// Container child yhboxMainContainer.Gtk.Box+BoxChild
			this.ytableMain = new global::Gamma.GtkWidgets.yTable();
			this.ytableMain.Name = "ytableMain";
			this.ytableMain.NRows = ((uint)(3));
			this.ytableMain.NColumns = ((uint)(7));
			this.ytableMain.RowSpacing = ((uint)(6));
			this.ytableMain.ColumnSpacing = ((uint)(6));
			// Container child ytableMain.Gtk.Table+TableChild
			this.daterangepickerPeriod = new global::QS.Widgets.GtkUI.DateRangePicker();
			this.daterangepickerPeriod.Events = ((global::Gdk.EventMask)(256));
			this.daterangepickerPeriod.Name = "daterangepickerPeriod";
			this.daterangepickerPeriod.StartDate = new global::System.DateTime(0);
			this.daterangepickerPeriod.EndDate = new global::System.DateTime(0);
			this.ytableMain.Add(this.daterangepickerPeriod);
			global::Gtk.Table.TableChild w1 = ((global::Gtk.Table.TableChild)(this.ytableMain[this.daterangepickerPeriod]));
			w1.LeftAttach = ((uint)(4));
			w1.RightAttach = ((uint)(5));
			w1.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytableMain.Gtk.Table+TableChild
			this.entityentryCar = new global::QS.Views.Control.EntityEntry();
			this.entityentryCar.Events = ((global::Gdk.EventMask)(256));
			this.entityentryCar.Name = "entityentryCar";
			this.ytableMain.Add(this.entityentryCar);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.ytableMain[this.entityentryCar]));
			w2.TopAttach = ((uint)(1));
			w2.BottomAttach = ((uint)(2));
			w2.LeftAttach = ((uint)(6));
			w2.RightAttach = ((uint)(7));
			w2.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytableMain.Gtk.Table+TableChild
			this.entityentryDriver = new global::QS.Views.Control.EntityEntry();
			this.entityentryDriver.Events = ((global::Gdk.EventMask)(256));
			this.entityentryDriver.Name = "entityentryDriver";
			this.ytableMain.Add(this.entityentryDriver);
			global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.ytableMain[this.entityentryDriver]));
			w3.LeftAttach = ((uint)(6));
			w3.RightAttach = ((uint)(7));
			w3.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytableMain.Gtk.Table+TableChild
			this.entityentryEmployee = new global::QS.Views.Control.EntityEntry();
			this.entityentryEmployee.Events = ((global::Gdk.EventMask)(256));
			this.entityentryEmployee.Name = "entityentryEmployee";
			this.ytableMain.Add(this.entityentryEmployee);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.ytableMain[this.entityentryEmployee]));
			w4.TopAttach = ((uint)(1));
			w4.BottomAttach = ((uint)(2));
			w4.LeftAttach = ((uint)(4));
			w4.RightAttach = ((uint)(5));
			w4.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytableMain.Gtk.Table+TableChild
			this.entityentryWarehouse = new global::QS.Views.Control.EntityEntry();
			this.entityentryWarehouse.Events = ((global::Gdk.EventMask)(256));
			this.entityentryWarehouse.Name = "entityentryWarehouse";
			this.ytableMain.Add(this.entityentryWarehouse);
			global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.ytableMain[this.entityentryWarehouse]));
			w5.TopAttach = ((uint)(1));
			w5.BottomAttach = ((uint)(2));
			w5.LeftAttach = ((uint)(1));
			w5.RightAttach = ((uint)(2));
			w5.XOptions = ((global::Gtk.AttachOptions)(4));
			w5.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytableMain.Gtk.Table+TableChild
			this.ychkbtnQRScanRequired = new global::Gamma.GtkWidgets.yCheckButton();
			this.ychkbtnQRScanRequired.CanFocus = true;
			this.ychkbtnQRScanRequired.Name = "ychkbtnQRScanRequired";
			this.ychkbtnQRScanRequired.Label = global::Mono.Unix.Catalog.GetString("Требуют\nсканирование\nна складе(сети)");
			this.ychkbtnQRScanRequired.DrawIndicator = true;
			this.ychkbtnQRScanRequired.UseUnderline = true;
			this.ytableMain.Add(this.ychkbtnQRScanRequired);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.ytableMain[this.ychkbtnQRScanRequired]));
			w6.BottomAttach = ((uint)(3));
			w6.LeftAttach = ((uint)(2));
			w6.RightAttach = ((uint)(3));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytableMain.Gtk.Table+TableChild
			this.yenumcomboboxDocumentType = new global::Gamma.Widgets.yEnumComboBox();
			this.yenumcomboboxDocumentType.Name = "yenumcomboboxDocumentType";
			this.yenumcomboboxDocumentType.ShowSpecialStateAll = true;
			this.yenumcomboboxDocumentType.ShowSpecialStateNot = false;
			this.yenumcomboboxDocumentType.UseShortTitle = false;
			this.yenumcomboboxDocumentType.DefaultFirst = false;
			this.ytableMain.Add(this.yenumcomboboxDocumentType);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.ytableMain[this.yenumcomboboxDocumentType]));
			w7.LeftAttach = ((uint)(1));
			w7.RightAttach = ((uint)(2));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytableMain.Gtk.Table+TableChild
			this.yenumcomboboxMovementStatus = new global::Gamma.Widgets.yEnumComboBox();
			this.yenumcomboboxMovementStatus.Name = "yenumcomboboxMovementStatus";
			this.yenumcomboboxMovementStatus.ShowSpecialStateAll = true;
			this.yenumcomboboxMovementStatus.ShowSpecialStateNot = false;
			this.yenumcomboboxMovementStatus.UseShortTitle = false;
			this.yenumcomboboxMovementStatus.DefaultFirst = false;
			this.ytableMain.Add(this.yenumcomboboxMovementStatus);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.ytableMain[this.yenumcomboboxMovementStatus]));
			w8.TopAttach = ((uint)(2));
			w8.BottomAttach = ((uint)(3));
			w8.LeftAttach = ((uint)(1));
			w8.RightAttach = ((uint)(2));
			w8.XOptions = ((global::Gtk.AttachOptions)(4));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytableMain.Gtk.Table+TableChild
			this.ylabelCar = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelCar.Name = "ylabelCar";
			this.ylabelCar.Xalign = 1F;
			this.ylabelCar.LabelProp = global::Mono.Unix.Catalog.GetString("Автомобиль:");
			this.ytableMain.Add(this.ylabelCar);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.ytableMain[this.ylabelCar]));
			w9.TopAttach = ((uint)(1));
			w9.BottomAttach = ((uint)(2));
			w9.LeftAttach = ((uint)(5));
			w9.RightAttach = ((uint)(6));
			w9.XOptions = ((global::Gtk.AttachOptions)(4));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytableMain.Gtk.Table+TableChild
			this.ylabelDocumentType = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelDocumentType.Name = "ylabelDocumentType";
			this.ylabelDocumentType.Xalign = 1F;
			this.ylabelDocumentType.LabelProp = global::Mono.Unix.Catalog.GetString("Вид документа:");
			this.ytableMain.Add(this.ylabelDocumentType);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.ytableMain[this.ylabelDocumentType]));
			w10.XOptions = ((global::Gtk.AttachOptions)(4));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytableMain.Gtk.Table+TableChild
			this.ylabelDriver = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelDriver.Name = "ylabelDriver";
			this.ylabelDriver.Xalign = 1F;
			this.ylabelDriver.LabelProp = global::Mono.Unix.Catalog.GetString("Водитель в МЛ:");
			this.ytableMain.Add(this.ylabelDriver);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.ytableMain[this.ylabelDriver]));
			w11.LeftAttach = ((uint)(5));
			w11.RightAttach = ((uint)(6));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytableMain.Gtk.Table+TableChild
			this.ylabelEmployee = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelEmployee.Name = "ylabelEmployee";
			this.ylabelEmployee.Xalign = 1F;
			this.ylabelEmployee.LabelProp = global::Mono.Unix.Catalog.GetString("Сотрудник:");
			this.ytableMain.Add(this.ylabelEmployee);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.ytableMain[this.ylabelEmployee]));
			w12.TopAttach = ((uint)(1));
			w12.BottomAttach = ((uint)(2));
			w12.LeftAttach = ((uint)(3));
			w12.RightAttach = ((uint)(4));
			w12.XOptions = ((global::Gtk.AttachOptions)(4));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytableMain.Gtk.Table+TableChild
			this.ylabelMovementStatus = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelMovementStatus.Name = "ylabelMovementStatus";
			this.ylabelMovementStatus.Xalign = 1F;
			this.ylabelMovementStatus.LabelProp = global::Mono.Unix.Catalog.GetString("Статус перемещения:");
			this.ytableMain.Add(this.ylabelMovementStatus);
			global::Gtk.Table.TableChild w13 = ((global::Gtk.Table.TableChild)(this.ytableMain[this.ylabelMovementStatus]));
			w13.TopAttach = ((uint)(2));
			w13.BottomAttach = ((uint)(3));
			w13.XOptions = ((global::Gtk.AttachOptions)(4));
			w13.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytableMain.Gtk.Table+TableChild
			this.ylabelPeriod = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelPeriod.Name = "ylabelPeriod";
			this.ylabelPeriod.Xalign = 1F;
			this.ylabelPeriod.LabelProp = global::Mono.Unix.Catalog.GetString("За период:");
			this.ytableMain.Add(this.ylabelPeriod);
			global::Gtk.Table.TableChild w14 = ((global::Gtk.Table.TableChild)(this.ytableMain[this.ylabelPeriod]));
			w14.LeftAttach = ((uint)(3));
			w14.RightAttach = ((uint)(4));
			w14.XOptions = ((global::Gtk.AttachOptions)(4));
			w14.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytableMain.Gtk.Table+TableChild
			this.ylabelWarehouse = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelWarehouse.Name = "ylabelWarehouse";
			this.ylabelWarehouse.Xalign = 1F;
			this.ylabelWarehouse.LabelProp = global::Mono.Unix.Catalog.GetString(" Склад:");
			this.ytableMain.Add(this.ylabelWarehouse);
			global::Gtk.Table.TableChild w15 = ((global::Gtk.Table.TableChild)(this.ytableMain[this.ylabelWarehouse]));
			w15.TopAttach = ((uint)(1));
			w15.BottomAttach = ((uint)(2));
			w15.XOptions = ((global::Gtk.AttachOptions)(4));
			w15.YOptions = ((global::Gtk.AttachOptions)(4));
			this.yhboxMainContainer.Add(this.ytableMain);
			global::Gtk.Box.BoxChild w16 = ((global::Gtk.Box.BoxChild)(this.yhboxMainContainer[this.ytableMain]));
			w16.Position = 0;
			w16.Expand = false;
			this.Add(this.yhboxMainContainer);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}

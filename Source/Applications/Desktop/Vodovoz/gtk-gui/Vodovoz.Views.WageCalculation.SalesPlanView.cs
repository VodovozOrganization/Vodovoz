﻿
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.Views.WageCalculation
{
	public partial class SalesPlanView
	{
		private global::Gtk.ScrolledWindow scrolledwindow1;

		private global::Gtk.VBox vbox2;

		private global::Gtk.HBox hboxDialogButtons;

		private global::Gamma.GtkWidgets.yButton btnSave;

		private global::Gamma.GtkWidgets.yButton btnCancel;

		private global::Gtk.Table tableWidget;

		private global::Gamma.GtkWidgets.yCheckButton chkIsArchive;

		private global::Gamma.GtkWidgets.ySpinButton entryEmptyBottlesToTake;

		private global::Gamma.GtkWidgets.ySpinButton entryFullBottlesToSell;

		private global::Gamma.GtkWidgets.ySpinButton entryProceedsDay;

		private global::Gamma.GtkWidgets.ySpinButton entryProceedsMonth;

		private global::Gtk.ScrolledWindow GtkScrolledWindow;

		private global::Gamma.GtkWidgets.yTreeView ytreeviewEquipmentKindSalesPlan;

		private global::Gtk.ScrolledWindow GtkScrolledWindow5;

		private global::Gamma.GtkWidgets.yTreeView ytreeviewNomenclatureSalesPlan;

		private global::Gtk.ScrolledWindow GtkScrolledWindow7;

		private global::Gamma.GtkWidgets.yTreeView ytreeviewEquipmentTypeSalesPlan;

		private global::Gtk.HSeparator hseparator1;

		private global::Gtk.HSeparator hseparator2;

		private global::Gtk.HSeparator hseparator3;

		private global::Gtk.Label labelName;

		private global::Gamma.GtkWidgets.yButton ybuttonAddEquipmentKind;

		private global::Gamma.GtkWidgets.yButton ybuttonAddEquipmentType;

		private global::Gamma.GtkWidgets.yButton ybuttonAddNomenclature;

		private global::Gamma.GtkWidgets.yButton ybuttonCancelEquipmentType;

		private global::Gamma.GtkWidgets.yButton ybuttonDeleteEquipmentKind;

		private global::Gamma.GtkWidgets.yButton ybuttonDeleteEquipmentType;

		private global::Gamma.GtkWidgets.yButton ybuttonDeleteNomenclature;

		private global::Gamma.GtkWidgets.yButton ybuttonSaveEquipmentType;

		private global::Gamma.GtkWidgets.yEntry yentryName;

		private global::Gamma.GtkWidgets.yLabel ylabelEquipmentKindSalesPlan;

		private global::Gamma.GtkWidgets.yLabel ylabelEquipmentKindSalesPlan1;

		private global::Gamma.GtkWidgets.yLabel ylabelFullBottlesToSell;

		private global::Gamma.GtkWidgets.yLabel ylabelFullBottlesToTake;

		private global::Gamma.GtkWidgets.yLabel ylabelNomenclatureSalesPlan;

		private global::Gamma.GtkWidgets.yLabel ylabelProceedsDay;

		private global::Gamma.GtkWidgets.yLabel ylabelProceedsMonth;

		private global::Gamma.Widgets.ySpecComboBox yspeccomboboxEquipmentType;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.Views.WageCalculation.SalesPlanView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.Views.WageCalculation.SalesPlanView";
			// Container child Vodovoz.Views.WageCalculation.SalesPlanView.Gtk.Container+ContainerChild
			this.scrolledwindow1 = new global::Gtk.ScrolledWindow();
			this.scrolledwindow1.CanFocus = true;
			this.scrolledwindow1.Name = "scrolledwindow1";
			this.scrolledwindow1.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child scrolledwindow1.Gtk.Container+ContainerChild
			global::Gtk.Viewport w1 = new global::Gtk.Viewport();
			w1.ShadowType = ((global::Gtk.ShadowType)(0));
			// Container child GtkViewport.Gtk.Container+ContainerChild
			this.vbox2 = new global::Gtk.VBox();
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hboxDialogButtons = new global::Gtk.HBox();
			this.hboxDialogButtons.Name = "hboxDialogButtons";
			this.hboxDialogButtons.Spacing = 6;
			// Container child hboxDialogButtons.Gtk.Box+BoxChild
			this.btnSave = new global::Gamma.GtkWidgets.yButton();
			this.btnSave.CanFocus = true;
			this.btnSave.Name = "btnSave";
			this.btnSave.UseUnderline = true;
			this.btnSave.Label = global::Mono.Unix.Catalog.GetString("Сохранить");
			global::Gtk.Image w2 = new global::Gtk.Image();
			w2.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-save", global::Gtk.IconSize.Menu);
			this.btnSave.Image = w2;
			this.hboxDialogButtons.Add(this.btnSave);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.hboxDialogButtons[this.btnSave]));
			w3.Position = 0;
			w3.Expand = false;
			// Container child hboxDialogButtons.Gtk.Box+BoxChild
			this.btnCancel = new global::Gamma.GtkWidgets.yButton();
			this.btnCancel.CanFocus = true;
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.UseUnderline = true;
			this.btnCancel.Label = global::Mono.Unix.Catalog.GetString("Отменить");
			global::Gtk.Image w4 = new global::Gtk.Image();
			w4.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-revert-to-saved", global::Gtk.IconSize.Menu);
			this.btnCancel.Image = w4;
			this.hboxDialogButtons.Add(this.btnCancel);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.hboxDialogButtons[this.btnCancel]));
			w5.Position = 1;
			w5.Expand = false;
			this.vbox2.Add(this.hboxDialogButtons);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.hboxDialogButtons]));
			w6.Position = 0;
			w6.Expand = false;
			w6.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.tableWidget = new global::Gtk.Table(((uint)(16)), ((uint)(5)), false);
			this.tableWidget.Name = "tableWidget";
			this.tableWidget.RowSpacing = ((uint)(6));
			this.tableWidget.ColumnSpacing = ((uint)(6));
			// Container child tableWidget.Gtk.Table+TableChild
			this.chkIsArchive = new global::Gamma.GtkWidgets.yCheckButton();
			this.chkIsArchive.CanFocus = true;
			this.chkIsArchive.Name = "chkIsArchive";
			this.chkIsArchive.Label = global::Mono.Unix.Catalog.GetString("Архивный");
			this.chkIsArchive.DrawIndicator = true;
			this.chkIsArchive.UseUnderline = true;
			this.tableWidget.Add(this.chkIsArchive);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.tableWidget[this.chkIsArchive]));
			w7.TopAttach = ((uint)(5));
			w7.BottomAttach = ((uint)(6));
			w7.LeftAttach = ((uint)(1));
			w7.RightAttach = ((uint)(4));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableWidget.Gtk.Table+TableChild
			this.entryEmptyBottlesToTake = new global::Gamma.GtkWidgets.ySpinButton(0D, 99999999D, 1D);
			this.entryEmptyBottlesToTake.CanFocus = true;
			this.entryEmptyBottlesToTake.Name = "entryEmptyBottlesToTake";
			this.entryEmptyBottlesToTake.Adjustment.PageIncrement = 10D;
			this.entryEmptyBottlesToTake.ClimbRate = 1D;
			this.entryEmptyBottlesToTake.Numeric = true;
			this.entryEmptyBottlesToTake.ValueAsDecimal = 0m;
			this.entryEmptyBottlesToTake.ValueAsInt = 0;
			this.tableWidget.Add(this.entryEmptyBottlesToTake);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.tableWidget[this.entryEmptyBottlesToTake]));
			w8.TopAttach = ((uint)(2));
			w8.BottomAttach = ((uint)(3));
			w8.LeftAttach = ((uint)(1));
			w8.RightAttach = ((uint)(3));
			w8.XOptions = ((global::Gtk.AttachOptions)(4));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableWidget.Gtk.Table+TableChild
			this.entryFullBottlesToSell = new global::Gamma.GtkWidgets.ySpinButton(0D, 99999999D, 1D);
			this.entryFullBottlesToSell.CanFocus = true;
			this.entryFullBottlesToSell.Name = "entryFullBottlesToSell";
			this.entryFullBottlesToSell.Adjustment.PageIncrement = 10D;
			this.entryFullBottlesToSell.ClimbRate = 1D;
			this.entryFullBottlesToSell.Numeric = true;
			this.entryFullBottlesToSell.ValueAsDecimal = 0m;
			this.entryFullBottlesToSell.ValueAsInt = 0;
			this.tableWidget.Add(this.entryFullBottlesToSell);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.tableWidget[this.entryFullBottlesToSell]));
			w9.TopAttach = ((uint)(1));
			w9.BottomAttach = ((uint)(2));
			w9.LeftAttach = ((uint)(1));
			w9.RightAttach = ((uint)(3));
			w9.XOptions = ((global::Gtk.AttachOptions)(4));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableWidget.Gtk.Table+TableChild
			this.entryProceedsDay = new global::Gamma.GtkWidgets.ySpinButton(0D, 99999999D, 1D);
			this.entryProceedsDay.CanFocus = true;
			this.entryProceedsDay.Name = "entryProceedsDay";
			this.entryProceedsDay.Adjustment.PageIncrement = 10D;
			this.entryProceedsDay.ClimbRate = 1D;
			this.entryProceedsDay.Digits = ((uint)(2));
			this.entryProceedsDay.Numeric = true;
			this.entryProceedsDay.ValueAsDecimal = 0m;
			this.entryProceedsDay.ValueAsInt = 0;
			this.tableWidget.Add(this.entryProceedsDay);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.tableWidget[this.entryProceedsDay]));
			w10.TopAttach = ((uint)(3));
			w10.BottomAttach = ((uint)(4));
			w10.LeftAttach = ((uint)(1));
			w10.RightAttach = ((uint)(3));
			w10.XOptions = ((global::Gtk.AttachOptions)(4));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableWidget.Gtk.Table+TableChild
			this.entryProceedsMonth = new global::Gamma.GtkWidgets.ySpinButton(0D, 99999999D, 1D);
			this.entryProceedsMonth.CanFocus = true;
			this.entryProceedsMonth.Name = "entryProceedsMonth";
			this.entryProceedsMonth.Adjustment.PageIncrement = 10D;
			this.entryProceedsMonth.ClimbRate = 1D;
			this.entryProceedsMonth.Digits = ((uint)(2));
			this.entryProceedsMonth.Numeric = true;
			this.entryProceedsMonth.ValueAsDecimal = 0m;
			this.entryProceedsMonth.ValueAsInt = 0;
			this.tableWidget.Add(this.entryProceedsMonth);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.tableWidget[this.entryProceedsMonth]));
			w11.TopAttach = ((uint)(4));
			w11.BottomAttach = ((uint)(5));
			w11.LeftAttach = ((uint)(1));
			w11.RightAttach = ((uint)(3));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableWidget.Gtk.Table+TableChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.ytreeviewEquipmentKindSalesPlan = new global::Gamma.GtkWidgets.yTreeView();
			this.ytreeviewEquipmentKindSalesPlan.CanFocus = true;
			this.ytreeviewEquipmentKindSalesPlan.Name = "ytreeviewEquipmentKindSalesPlan";
			this.ytreeviewEquipmentKindSalesPlan.EnableSearch = false;
			this.ytreeviewEquipmentKindSalesPlan.SearchColumn = 0;
			this.GtkScrolledWindow.Add(this.ytreeviewEquipmentKindSalesPlan);
			this.tableWidget.Add(this.GtkScrolledWindow);
			global::Gtk.Table.TableChild w13 = ((global::Gtk.Table.TableChild)(this.tableWidget[this.GtkScrolledWindow]));
			w13.TopAttach = ((uint)(10));
			w13.BottomAttach = ((uint)(11));
			w13.LeftAttach = ((uint)(1));
			w13.RightAttach = ((uint)(3));
			// Container child tableWidget.Gtk.Table+TableChild
			this.GtkScrolledWindow5 = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow5.Name = "GtkScrolledWindow5";
			this.GtkScrolledWindow5.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow5.Gtk.Container+ContainerChild
			this.ytreeviewNomenclatureSalesPlan = new global::Gamma.GtkWidgets.yTreeView();
			this.ytreeviewNomenclatureSalesPlan.CanFocus = true;
			this.ytreeviewNomenclatureSalesPlan.Name = "ytreeviewNomenclatureSalesPlan";
			this.GtkScrolledWindow5.Add(this.ytreeviewNomenclatureSalesPlan);
			this.tableWidget.Add(this.GtkScrolledWindow5);
			global::Gtk.Table.TableChild w15 = ((global::Gtk.Table.TableChild)(this.tableWidget[this.GtkScrolledWindow5]));
			w15.TopAttach = ((uint)(7));
			w15.BottomAttach = ((uint)(8));
			w15.LeftAttach = ((uint)(1));
			w15.RightAttach = ((uint)(3));
			// Container child tableWidget.Gtk.Table+TableChild
			this.GtkScrolledWindow7 = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow7.Name = "GtkScrolledWindow7";
			this.GtkScrolledWindow7.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow7.Gtk.Container+ContainerChild
			this.ytreeviewEquipmentTypeSalesPlan = new global::Gamma.GtkWidgets.yTreeView();
			this.ytreeviewEquipmentTypeSalesPlan.CanFocus = true;
			this.ytreeviewEquipmentTypeSalesPlan.Name = "ytreeviewEquipmentTypeSalesPlan";
			this.GtkScrolledWindow7.Add(this.ytreeviewEquipmentTypeSalesPlan);
			this.tableWidget.Add(this.GtkScrolledWindow7);
			global::Gtk.Table.TableChild w17 = ((global::Gtk.Table.TableChild)(this.tableWidget[this.GtkScrolledWindow7]));
			w17.TopAttach = ((uint)(13));
			w17.BottomAttach = ((uint)(14));
			w17.LeftAttach = ((uint)(1));
			w17.RightAttach = ((uint)(3));
			// Container child tableWidget.Gtk.Table+TableChild
			this.hseparator1 = new global::Gtk.HSeparator();
			this.hseparator1.Name = "hseparator1";
			this.tableWidget.Add(this.hseparator1);
			global::Gtk.Table.TableChild w18 = ((global::Gtk.Table.TableChild)(this.tableWidget[this.hseparator1]));
			w18.TopAttach = ((uint)(6));
			w18.BottomAttach = ((uint)(7));
			w18.RightAttach = ((uint)(5));
			w18.XOptions = ((global::Gtk.AttachOptions)(4));
			w18.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableWidget.Gtk.Table+TableChild
			this.hseparator2 = new global::Gtk.HSeparator();
			this.hseparator2.Name = "hseparator2";
			this.tableWidget.Add(this.hseparator2);
			global::Gtk.Table.TableChild w19 = ((global::Gtk.Table.TableChild)(this.tableWidget[this.hseparator2]));
			w19.TopAttach = ((uint)(9));
			w19.BottomAttach = ((uint)(10));
			w19.RightAttach = ((uint)(5));
			w19.XOptions = ((global::Gtk.AttachOptions)(4));
			w19.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableWidget.Gtk.Table+TableChild
			this.hseparator3 = new global::Gtk.HSeparator();
			this.hseparator3.Name = "hseparator3";
			this.tableWidget.Add(this.hseparator3);
			global::Gtk.Table.TableChild w20 = ((global::Gtk.Table.TableChild)(this.tableWidget[this.hseparator3]));
			w20.TopAttach = ((uint)(12));
			w20.BottomAttach = ((uint)(13));
			w20.RightAttach = ((uint)(5));
			w20.XOptions = ((global::Gtk.AttachOptions)(4));
			w20.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableWidget.Gtk.Table+TableChild
			this.labelName = new global::Gtk.Label();
			this.labelName.Name = "labelName";
			this.labelName.Xalign = 1F;
			this.labelName.LabelProp = global::Mono.Unix.Catalog.GetString("Название:");
			this.tableWidget.Add(this.labelName);
			global::Gtk.Table.TableChild w21 = ((global::Gtk.Table.TableChild)(this.tableWidget[this.labelName]));
			w21.XOptions = ((global::Gtk.AttachOptions)(4));
			w21.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableWidget.Gtk.Table+TableChild
			this.ybuttonAddEquipmentKind = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonAddEquipmentKind.CanFocus = true;
			this.ybuttonAddEquipmentKind.Name = "ybuttonAddEquipmentKind";
			this.ybuttonAddEquipmentKind.UseUnderline = true;
			this.ybuttonAddEquipmentKind.Label = global::Mono.Unix.Catalog.GetString("Добавить виды оборудования");
			this.tableWidget.Add(this.ybuttonAddEquipmentKind);
			global::Gtk.Table.TableChild w22 = ((global::Gtk.Table.TableChild)(this.tableWidget[this.ybuttonAddEquipmentKind]));
			w22.TopAttach = ((uint)(11));
			w22.BottomAttach = ((uint)(12));
			w22.LeftAttach = ((uint)(1));
			w22.RightAttach = ((uint)(2));
			w22.XOptions = ((global::Gtk.AttachOptions)(4));
			w22.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableWidget.Gtk.Table+TableChild
			this.ybuttonAddEquipmentType = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonAddEquipmentType.CanFocus = true;
			this.ybuttonAddEquipmentType.Name = "ybuttonAddEquipmentType";
			this.ybuttonAddEquipmentType.UseUnderline = true;
			this.ybuttonAddEquipmentType.Label = global::Mono.Unix.Catalog.GetString("Добавить типы оборудования");
			this.tableWidget.Add(this.ybuttonAddEquipmentType);
			global::Gtk.Table.TableChild w23 = ((global::Gtk.Table.TableChild)(this.tableWidget[this.ybuttonAddEquipmentType]));
			w23.TopAttach = ((uint)(14));
			w23.BottomAttach = ((uint)(15));
			w23.LeftAttach = ((uint)(1));
			w23.RightAttach = ((uint)(2));
			w23.XOptions = ((global::Gtk.AttachOptions)(4));
			w23.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableWidget.Gtk.Table+TableChild
			this.ybuttonAddNomenclature = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonAddNomenclature.CanFocus = true;
			this.ybuttonAddNomenclature.Name = "ybuttonAddNomenclature";
			this.ybuttonAddNomenclature.UseUnderline = true;
			this.ybuttonAddNomenclature.Label = global::Mono.Unix.Catalog.GetString("Добавить номенклатуры");
			this.tableWidget.Add(this.ybuttonAddNomenclature);
			global::Gtk.Table.TableChild w24 = ((global::Gtk.Table.TableChild)(this.tableWidget[this.ybuttonAddNomenclature]));
			w24.TopAttach = ((uint)(8));
			w24.BottomAttach = ((uint)(9));
			w24.LeftAttach = ((uint)(1));
			w24.RightAttach = ((uint)(2));
			w24.XOptions = ((global::Gtk.AttachOptions)(4));
			w24.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableWidget.Gtk.Table+TableChild
			this.ybuttonCancelEquipmentType = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonCancelEquipmentType.CanFocus = true;
			this.ybuttonCancelEquipmentType.Name = "ybuttonCancelEquipmentType";
			this.ybuttonCancelEquipmentType.UseUnderline = true;
			this.ybuttonCancelEquipmentType.Label = global::Mono.Unix.Catalog.GetString("Отмена");
			this.tableWidget.Add(this.ybuttonCancelEquipmentType);
			global::Gtk.Table.TableChild w25 = ((global::Gtk.Table.TableChild)(this.tableWidget[this.ybuttonCancelEquipmentType]));
			w25.TopAttach = ((uint)(14));
			w25.BottomAttach = ((uint)(15));
			w25.LeftAttach = ((uint)(4));
			w25.RightAttach = ((uint)(5));
			w25.XOptions = ((global::Gtk.AttachOptions)(4));
			w25.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableWidget.Gtk.Table+TableChild
			this.ybuttonDeleteEquipmentKind = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonDeleteEquipmentKind.CanFocus = true;
			this.ybuttonDeleteEquipmentKind.Name = "ybuttonDeleteEquipmentKind";
			this.ybuttonDeleteEquipmentKind.UseUnderline = true;
			this.ybuttonDeleteEquipmentKind.Label = global::Mono.Unix.Catalog.GetString("Удалить");
			this.tableWidget.Add(this.ybuttonDeleteEquipmentKind);
			global::Gtk.Table.TableChild w26 = ((global::Gtk.Table.TableChild)(this.tableWidget[this.ybuttonDeleteEquipmentKind]));
			w26.TopAttach = ((uint)(11));
			w26.BottomAttach = ((uint)(12));
			w26.LeftAttach = ((uint)(2));
			w26.RightAttach = ((uint)(3));
			w26.XOptions = ((global::Gtk.AttachOptions)(4));
			w26.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableWidget.Gtk.Table+TableChild
			this.ybuttonDeleteEquipmentType = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonDeleteEquipmentType.CanFocus = true;
			this.ybuttonDeleteEquipmentType.Name = "ybuttonDeleteEquipmentType";
			this.ybuttonDeleteEquipmentType.UseUnderline = true;
			this.ybuttonDeleteEquipmentType.Label = global::Mono.Unix.Catalog.GetString("Удалить");
			this.tableWidget.Add(this.ybuttonDeleteEquipmentType);
			global::Gtk.Table.TableChild w27 = ((global::Gtk.Table.TableChild)(this.tableWidget[this.ybuttonDeleteEquipmentType]));
			w27.TopAttach = ((uint)(14));
			w27.BottomAttach = ((uint)(15));
			w27.LeftAttach = ((uint)(2));
			w27.RightAttach = ((uint)(3));
			w27.XOptions = ((global::Gtk.AttachOptions)(4));
			w27.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableWidget.Gtk.Table+TableChild
			this.ybuttonDeleteNomenclature = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonDeleteNomenclature.CanFocus = true;
			this.ybuttonDeleteNomenclature.Name = "ybuttonDeleteNomenclature";
			this.ybuttonDeleteNomenclature.UseUnderline = true;
			this.ybuttonDeleteNomenclature.Label = global::Mono.Unix.Catalog.GetString("Удалить");
			this.tableWidget.Add(this.ybuttonDeleteNomenclature);
			global::Gtk.Table.TableChild w28 = ((global::Gtk.Table.TableChild)(this.tableWidget[this.ybuttonDeleteNomenclature]));
			w28.TopAttach = ((uint)(8));
			w28.BottomAttach = ((uint)(9));
			w28.LeftAttach = ((uint)(2));
			w28.RightAttach = ((uint)(3));
			w28.XOptions = ((global::Gtk.AttachOptions)(4));
			w28.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableWidget.Gtk.Table+TableChild
			this.ybuttonSaveEquipmentType = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonSaveEquipmentType.CanFocus = true;
			this.ybuttonSaveEquipmentType.Name = "ybuttonSaveEquipmentType";
			this.ybuttonSaveEquipmentType.UseUnderline = true;
			this.ybuttonSaveEquipmentType.Label = global::Mono.Unix.Catalog.GetString("Сохранить");
			this.tableWidget.Add(this.ybuttonSaveEquipmentType);
			global::Gtk.Table.TableChild w29 = ((global::Gtk.Table.TableChild)(this.tableWidget[this.ybuttonSaveEquipmentType]));
			w29.TopAttach = ((uint)(14));
			w29.BottomAttach = ((uint)(15));
			w29.LeftAttach = ((uint)(3));
			w29.RightAttach = ((uint)(4));
			w29.XOptions = ((global::Gtk.AttachOptions)(4));
			w29.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableWidget.Gtk.Table+TableChild
			this.yentryName = new global::Gamma.GtkWidgets.yEntry();
			this.yentryName.CanFocus = true;
			this.yentryName.Name = "yentryName";
			this.yentryName.IsEditable = true;
			this.yentryName.InvisibleChar = '•';
			this.tableWidget.Add(this.yentryName);
			global::Gtk.Table.TableChild w30 = ((global::Gtk.Table.TableChild)(this.tableWidget[this.yentryName]));
			w30.LeftAttach = ((uint)(1));
			w30.RightAttach = ((uint)(3));
			w30.XOptions = ((global::Gtk.AttachOptions)(4));
			w30.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableWidget.Gtk.Table+TableChild
			this.ylabelEquipmentKindSalesPlan = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelEquipmentKindSalesPlan.Name = "ylabelEquipmentKindSalesPlan";
			this.ylabelEquipmentKindSalesPlan.Xalign = 1F;
			this.ylabelEquipmentKindSalesPlan.LabelProp = global::Mono.Unix.Catalog.GetString("Типы оборудования:");
			this.ylabelEquipmentKindSalesPlan.Justify = ((global::Gtk.Justification)(1));
			this.ylabelEquipmentKindSalesPlan.Ellipsize = ((global::Pango.EllipsizeMode)(3));
			this.tableWidget.Add(this.ylabelEquipmentKindSalesPlan);
			global::Gtk.Table.TableChild w31 = ((global::Gtk.Table.TableChild)(this.tableWidget[this.ylabelEquipmentKindSalesPlan]));
			w31.TopAttach = ((uint)(13));
			w31.BottomAttach = ((uint)(14));
			w31.XOptions = ((global::Gtk.AttachOptions)(4));
			w31.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableWidget.Gtk.Table+TableChild
			this.ylabelEquipmentKindSalesPlan1 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelEquipmentKindSalesPlan1.Name = "ylabelEquipmentKindSalesPlan1";
			this.ylabelEquipmentKindSalesPlan1.Xalign = 1F;
			this.ylabelEquipmentKindSalesPlan1.LabelProp = global::Mono.Unix.Catalog.GetString("Виды оборудования:");
			this.ylabelEquipmentKindSalesPlan1.Justify = ((global::Gtk.Justification)(1));
			this.ylabelEquipmentKindSalesPlan1.Ellipsize = ((global::Pango.EllipsizeMode)(3));
			this.tableWidget.Add(this.ylabelEquipmentKindSalesPlan1);
			global::Gtk.Table.TableChild w32 = ((global::Gtk.Table.TableChild)(this.tableWidget[this.ylabelEquipmentKindSalesPlan1]));
			w32.TopAttach = ((uint)(10));
			w32.BottomAttach = ((uint)(11));
			w32.XOptions = ((global::Gtk.AttachOptions)(4));
			w32.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableWidget.Gtk.Table+TableChild
			this.ylabelFullBottlesToSell = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelFullBottlesToSell.Name = "ylabelFullBottlesToSell";
			this.ylabelFullBottlesToSell.Xalign = 1F;
			this.ylabelFullBottlesToSell.LabelProp = global::Mono.Unix.Catalog.GetString("Бутылей на продажу:");
			this.tableWidget.Add(this.ylabelFullBottlesToSell);
			global::Gtk.Table.TableChild w33 = ((global::Gtk.Table.TableChild)(this.tableWidget[this.ylabelFullBottlesToSell]));
			w33.TopAttach = ((uint)(1));
			w33.BottomAttach = ((uint)(2));
			w33.XOptions = ((global::Gtk.AttachOptions)(4));
			w33.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableWidget.Gtk.Table+TableChild
			this.ylabelFullBottlesToTake = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelFullBottlesToTake.Name = "ylabelFullBottlesToTake";
			this.ylabelFullBottlesToTake.Xalign = 1F;
			this.ylabelFullBottlesToTake.LabelProp = global::Mono.Unix.Catalog.GetString("Бутылей на забор:");
			this.tableWidget.Add(this.ylabelFullBottlesToTake);
			global::Gtk.Table.TableChild w34 = ((global::Gtk.Table.TableChild)(this.tableWidget[this.ylabelFullBottlesToTake]));
			w34.TopAttach = ((uint)(2));
			w34.BottomAttach = ((uint)(3));
			w34.XOptions = ((global::Gtk.AttachOptions)(4));
			w34.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableWidget.Gtk.Table+TableChild
			this.ylabelNomenclatureSalesPlan = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelNomenclatureSalesPlan.Name = "ylabelNomenclatureSalesPlan";
			this.ylabelNomenclatureSalesPlan.Xalign = 1F;
			this.ylabelNomenclatureSalesPlan.LabelProp = global::Mono.Unix.Catalog.GetString("Номенклатуры:");
			this.ylabelNomenclatureSalesPlan.Justify = ((global::Gtk.Justification)(1));
			this.ylabelNomenclatureSalesPlan.Ellipsize = ((global::Pango.EllipsizeMode)(3));
			this.tableWidget.Add(this.ylabelNomenclatureSalesPlan);
			global::Gtk.Table.TableChild w35 = ((global::Gtk.Table.TableChild)(this.tableWidget[this.ylabelNomenclatureSalesPlan]));
			w35.TopAttach = ((uint)(7));
			w35.BottomAttach = ((uint)(8));
			w35.XOptions = ((global::Gtk.AttachOptions)(4));
			w35.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableWidget.Gtk.Table+TableChild
			this.ylabelProceedsDay = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelProceedsDay.Name = "ylabelProceedsDay";
			this.ylabelProceedsDay.Xalign = 1F;
			this.ylabelProceedsDay.LabelProp = global::Mono.Unix.Catalog.GetString("Выручка за день:");
			this.tableWidget.Add(this.ylabelProceedsDay);
			global::Gtk.Table.TableChild w36 = ((global::Gtk.Table.TableChild)(this.tableWidget[this.ylabelProceedsDay]));
			w36.TopAttach = ((uint)(3));
			w36.BottomAttach = ((uint)(4));
			w36.XOptions = ((global::Gtk.AttachOptions)(4));
			w36.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableWidget.Gtk.Table+TableChild
			this.ylabelProceedsMonth = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelProceedsMonth.Name = "ylabelProceedsMonth";
			this.ylabelProceedsMonth.Xalign = 1F;
			this.ylabelProceedsMonth.LabelProp = global::Mono.Unix.Catalog.GetString("Выручка за месяц:");
			this.tableWidget.Add(this.ylabelProceedsMonth);
			global::Gtk.Table.TableChild w37 = ((global::Gtk.Table.TableChild)(this.tableWidget[this.ylabelProceedsMonth]));
			w37.TopAttach = ((uint)(4));
			w37.BottomAttach = ((uint)(5));
			w37.XOptions = ((global::Gtk.AttachOptions)(4));
			w37.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableWidget.Gtk.Table+TableChild
			this.yspeccomboboxEquipmentType = new global::Gamma.Widgets.ySpecComboBox();
			this.yspeccomboboxEquipmentType.Name = "yspeccomboboxEquipmentType";
			this.yspeccomboboxEquipmentType.AddIfNotExist = false;
			this.yspeccomboboxEquipmentType.DefaultFirst = false;
			this.yspeccomboboxEquipmentType.ShowSpecialStateAll = false;
			this.yspeccomboboxEquipmentType.ShowSpecialStateNot = false;
			this.tableWidget.Add(this.yspeccomboboxEquipmentType);
			global::Gtk.Table.TableChild w38 = ((global::Gtk.Table.TableChild)(this.tableWidget[this.yspeccomboboxEquipmentType]));
			w38.TopAttach = ((uint)(13));
			w38.BottomAttach = ((uint)(14));
			w38.LeftAttach = ((uint)(3));
			w38.RightAttach = ((uint)(5));
			w38.XOptions = ((global::Gtk.AttachOptions)(4));
			w38.YOptions = ((global::Gtk.AttachOptions)(0));
			this.vbox2.Add(this.tableWidget);
			global::Gtk.Box.BoxChild w39 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.tableWidget]));
			w39.Position = 1;
			w1.Add(this.vbox2);
			this.scrolledwindow1.Add(w1);
			this.Add(this.scrolledwindow1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.ybuttonCancelEquipmentType.Hide();
			this.ybuttonSaveEquipmentType.Hide();
			this.yspeccomboboxEquipmentType.Hide();
			this.Hide();
		}
	}
}


namespace Vodovoz.Dialogs.Cash
{
	public partial class ExpenseCategoryView
	{
		private Gtk.VBox vboxDialog;

		private Gtk.HBox hboxDialogButtons;

		private Gamma.GtkWidgets.yButton buttonSave;

		private Gamma.GtkWidgets.yButton buttonCancel;

		private Gtk.Table table1;

		private Gamma.Widgets.yEnumComboBox yenumTypeDocument;

		private Gamma.Widgets.yEntryReference yentryParent;


		private global::Gamma.GtkWidgets.yButton buttonCancel;

		private global::Gtk.Table table1;

		private global::Gtk.Label label1;

		private global::Gtk.Label labelName;

		private global::Gtk.Label labelNumbering;

		private global::Gtk.Label labelSubdivision;

		private global::Gtk.Label labelType;

		private global::Gtk.Label labelType1;

		private global::QS.Widgets.GtkUI.EntityViewModelEntry ParentEntityviewmodelentry;

		private global::QS.Widgets.GtkUI.EntityViewModelEntry SubdivisionEntityviewmodelentry;

		private global::Gamma.GtkWidgets.yCheckButton ycheckArchived;

		private global::Gamma.GtkWidgets.yEntry yentryName;


		private Gtk.Label label1;

		private void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.Dialogs.Cash.ExpenseCategoryView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.Dialogs.Cash.ExpenseCategoryView";
			// Container child Vodovoz.Dialogs.Cash.ExpenseCategoryView.Gtk.Container+ContainerChild
			this.vboxDialog = new global::Gtk.VBox();
			this.vboxDialog.Name = "vboxDialog";
			this.vboxDialog.Spacing = 6;
			// Container child vboxDialog.Gtk.Box+BoxChild
			this.hboxDialogButtons = new global::Gtk.HBox();
			this.hboxDialogButtons.Name = "hboxDialogButtons";
			this.hboxDialogButtons.Spacing = 6;
			// Container child hboxDialogButtons.Gtk.Box+BoxChild
			this.buttonSave = new global::Gamma.GtkWidgets.yButton();
			this.buttonSave.CanFocus = true;
			this.buttonSave.Name = "buttonSave";
			this.buttonSave.UseUnderline = true;
			this.buttonSave.Label = global::Mono.Unix.Catalog.GetString("Сохранить");
			global::Gtk.Image w1 = new global::Gtk.Image();
			w1.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-save", global::Gtk.IconSize.Menu);
			this.buttonSave.Image = w1;
			this.hboxDialogButtons.Add(this.buttonSave);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hboxDialogButtons[this.buttonSave]));
			w2.Position = 0;
			w2.Expand = false;
			w2.Fill = false;
			// Container child hboxDialogButtons.Gtk.Box+BoxChild
			this.buttonCancel = new global::Gamma.GtkWidgets.yButton();
			this.buttonCancel.CanFocus = true;
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.UseUnderline = true;
			this.buttonCancel.Label = global::Mono.Unix.Catalog.GetString("Отменить");
			global::Gtk.Image w3 = new global::Gtk.Image();
			w3.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-revert-to-saved", global::Gtk.IconSize.Menu);
			this.buttonCancel.Image = w3;
			this.hboxDialogButtons.Add(this.buttonCancel);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hboxDialogButtons[this.buttonCancel]));
			w4.Position = 1;
			w4.Expand = false;
			w4.Fill = false;
			this.vboxDialog.Add(this.hboxDialogButtons);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.vboxDialog[this.hboxDialogButtons]));
			w5.Position = 0;
			w5.Expand = false;
			w5.Fill = false;
			// Container child vboxDialog.Gtk.Box+BoxChild
			this.table1 = new global::Gtk.Table(((uint)(6)), ((uint)(2)), false);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(6));
			// Container child table1.Gtk.Table+TableChild
			this.label1 = new global::Gtk.Label();
			this.label1.Name = "label1";
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString("Родитель:");
			this.table1.Add(this.label1);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.table1[this.label1]));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.labelName = new global::Gtk.Label();
			this.labelName.Name = "labelName";
			this.labelName.Xalign = 1F;
			this.labelName.LabelProp = global::Mono.Unix.Catalog.GetString("Название:");
			this.table1.Add(this.labelName);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.table1[this.labelName]));
			w7.TopAttach = ((uint)(1));
			w7.BottomAttach = ((uint)(2));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.labelNumbering = new global::Gtk.Label();
			this.labelNumbering.Name = "labelNumbering";
			this.labelNumbering.LabelProp = global::Mono.Unix.Catalog.GetString("Нумерация:");
			this.table1.Add(this.labelNumbering);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.table1[this.labelNumbering]));
			w8.TopAttach = ((uint)(5));
			w8.BottomAttach = ((uint)(6));
			w8.XOptions = ((global::Gtk.AttachOptions)(4));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.labelSubdivision = new global::Gtk.Label();
			this.labelSubdivision.Name = "labelSubdivision";
			this.labelSubdivision.Xalign = 0F;
			this.labelSubdivision.LabelProp = global::Mono.Unix.Catalog.GetString("Подразделение:");
			this.table1.Add(this.labelSubdivision);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.table1[this.labelSubdivision]));
			w9.TopAttach = ((uint)(4));
			w9.BottomAttach = ((uint)(5));
			w9.XOptions = ((global::Gtk.AttachOptions)(4));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.labelType = new global::Gtk.Label();
			this.labelType.Name = "labelType";
			this.labelType.Xalign = 0F;
			this.labelType.LabelProp = global::Mono.Unix.Catalog.GetString("Тип документа:");
			this.table1.Add(this.labelType);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.table1[this.labelType]));
			w10.TopAttach = ((uint)(2));
			w10.BottomAttach = ((uint)(3));
			w10.XOptions = ((global::Gtk.AttachOptions)(4));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.labelType1 = new global::Gtk.Label();
			this.labelType1.Name = "labelType1";
			this.labelType1.Xalign = 0F;
			this.labelType1.LabelProp = global::Mono.Unix.Catalog.GetString("Архивировать:");
			this.table1.Add(this.labelType1);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.table1[this.labelType1]));
			w11.TopAttach = ((uint)(3));
			w11.BottomAttach = ((uint)(4));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ParentEntityviewmodelentry = new global::QS.Widgets.GtkUI.EntityViewModelEntry();
			this.ParentEntityviewmodelentry.Events = ((global::Gdk.EventMask)(256));
			this.ParentEntityviewmodelentry.Name = "ParentEntityviewmodelentry";
			this.ParentEntityviewmodelentry.CanEditReference = false;
			this.table1.Add(this.ParentEntityviewmodelentry);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.table1[this.ParentEntityviewmodelentry]));
			w12.LeftAttach = ((uint)(1));
			w12.RightAttach = ((uint)(2));
			w12.XOptions = ((global::Gtk.AttachOptions)(4));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.SubdivisionEntityviewmodelentry = new global::QS.Widgets.GtkUI.EntityViewModelEntry();
			this.SubdivisionEntityviewmodelentry.Events = ((global::Gdk.EventMask)(256));
			this.SubdivisionEntityviewmodelentry.Name = "SubdivisionEntityviewmodelentry";
			this.SubdivisionEntityviewmodelentry.CanEditReference = false;
			this.table1.Add(this.SubdivisionEntityviewmodelentry);
			global::Gtk.Table.TableChild w13 = ((global::Gtk.Table.TableChild)(this.table1[this.SubdivisionEntityviewmodelentry]));
			w13.TopAttach = ((uint)(4));
			w13.BottomAttach = ((uint)(5));
			w13.LeftAttach = ((uint)(1));
			w13.RightAttach = ((uint)(2));
			w13.XOptions = ((global::Gtk.AttachOptions)(4));
			w13.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ycheckArchived = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckArchived.CanFocus = true;
			this.ycheckArchived.Name = "ycheckArchived";
			this.ycheckArchived.Label = "";
			this.ycheckArchived.DrawIndicator = true;
			this.ycheckArchived.UseUnderline = true;
			this.table1.Add(this.ycheckArchived);
			global::Gtk.Table.TableChild w14 = ((global::Gtk.Table.TableChild)(this.table1[this.ycheckArchived]));
			w14.TopAttach = ((uint)(3));
			w14.BottomAttach = ((uint)(4));
			w14.LeftAttach = ((uint)(1));
			w14.RightAttach = ((uint)(2));
			w14.XOptions = ((global::Gtk.AttachOptions)(4));
			w14.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.yentryName = new global::Gamma.GtkWidgets.yEntry();
			this.yentryName.CanFocus = true;
			this.yentryName.Name = "yentryName";
			this.yentryName.IsEditable = true;
			this.yentryName.InvisibleChar = '•';
			this.table1.Add(this.yentryName);
			global::Gtk.Table.TableChild w15 = ((global::Gtk.Table.TableChild)(this.table1[this.yentryName]));
			w15.TopAttach = ((uint)(1));
			w15.BottomAttach = ((uint)(2));
			w15.LeftAttach = ((uint)(1));
			w15.RightAttach = ((uint)(2));
			w15.XOptions = ((global::Gtk.AttachOptions)(4));
			w15.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.yentryNumbering = new global::Gamma.GtkWidgets.yEntry();
			this.yentryNumbering.CanFocus = true;
			this.yentryNumbering.Name = "yentryNumbering";
			this.yentryNumbering.IsEditable = true;
			this.yentryNumbering.InvisibleChar = '•';
			this.table1.Add(this.yentryNumbering);
			global::Gtk.Table.TableChild w16 = ((global::Gtk.Table.TableChild)(this.table1[this.yentryNumbering]));
			w16.TopAttach = ((uint)(5));
			w16.BottomAttach = ((uint)(6));
			w16.LeftAttach = ((uint)(1));
			w16.RightAttach = ((uint)(2));
			w16.XOptions = ((global::Gtk.AttachOptions)(4));
			w16.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.yenumTypeDocument = new global::Gamma.Widgets.yEnumComboBox();
			this.yenumTypeDocument.Name = "yenumTypeDocument";
			this.yenumTypeDocument.ShowSpecialStateAll = false;
			this.yenumTypeDocument.ShowSpecialStateNot = false;
			this.yenumTypeDocument.UseShortTitle = false;
			this.yenumTypeDocument.DefaultFirst = false;
			this.table1.Add(this.yenumTypeDocument);
			global::Gtk.Table.TableChild w17 = ((global::Gtk.Table.TableChild)(this.table1[this.yenumTypeDocument]));
			w17.TopAttach = ((uint)(2));
			w17.BottomAttach = ((uint)(3));
			w17.LeftAttach = ((uint)(1));
			w17.RightAttach = ((uint)(2));
			w17.XOptions = ((global::Gtk.AttachOptions)(4));
			w17.YOptions = ((global::Gtk.AttachOptions)(4));
			this.vboxDialog.Add(this.table1);
			global::Gtk.Box.BoxChild w18 = ((global::Gtk.Box.BoxChild)(this.vboxDialog[this.table1]));
			w18.Position = 1;
			w18.Expand = false;
			w18.Fill = false;
			this.Add(this.vboxDialog);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}


// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.Views.Complaints
{
	public partial class CreateInnerComplaintView
	{
		private global::Gtk.VBox vboxDialog;

		private global::Gtk.HBox hboxDialogButtons;

		private global::Gamma.GtkWidgets.yButton buttonSave;

		private global::Gamma.GtkWidgets.yButton buttonCancel;

		private global::Gtk.Table tableFields;

		private global::Gtk.ScrolledWindow GtkScrolledWindow1;

		private global::Gamma.GtkWidgets.yTextView ytextviewComplaintText;

		private global::Vodovoz.Views.Complaints.GuiltyItemsView guiltyitemsview;

		private global::Gtk.HBox hboxObjectAndKind;

		private global::Gamma.Widgets.ySpecComboBox yspeccomboboxComplaintObject;

		private global::Gtk.Label labelComplaintKind;

		private global::QS.Widgets.GtkUI.SpecialListComboBox spLstComplaintKind;

		private global::Gtk.Label labelComplaintText;

		private global::Gtk.Label labelGuilty;

		private global::Gtk.Label lblComplaintObject;

		private global::Vodovoz.Presentation.Views.SmallFileInformationsView smallfileinformationsview;

		private global::Gtk.HSeparator hseparator1;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.Views.Complaints.CreateInnerComplaintView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.Views.Complaints.CreateInnerComplaintView";
			// Container child Vodovoz.Views.Complaints.CreateInnerComplaintView.Gtk.Container+ContainerChild
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
			this.tableFields = new global::Gtk.Table(((uint)(5)), ((uint)(2)), false);
			this.tableFields.Name = "tableFields";
			this.tableFields.RowSpacing = ((uint)(6));
			this.tableFields.ColumnSpacing = ((uint)(6));
			// Container child tableFields.Gtk.Table+TableChild
			this.GtkScrolledWindow1 = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow1.Name = "GtkScrolledWindow1";
			this.GtkScrolledWindow1.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow1.Gtk.Container+ContainerChild
			this.ytextviewComplaintText = new global::Gamma.GtkWidgets.yTextView();
			this.ytextviewComplaintText.CanFocus = true;
			this.ytextviewComplaintText.Name = "ytextviewComplaintText";
			this.GtkScrolledWindow1.Add(this.ytextviewComplaintText);
			this.tableFields.Add(this.GtkScrolledWindow1);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.tableFields[this.GtkScrolledWindow1]));
			w7.TopAttach = ((uint)(3));
			w7.BottomAttach = ((uint)(4));
			w7.LeftAttach = ((uint)(1));
			w7.RightAttach = ((uint)(2));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableFields.Gtk.Table+TableChild
			this.guiltyitemsview = new global::Vodovoz.Views.Complaints.GuiltyItemsView();
			this.guiltyitemsview.Events = ((global::Gdk.EventMask)(256));
			this.guiltyitemsview.Name = "guiltyitemsview";
			this.tableFields.Add(this.guiltyitemsview);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.tableFields[this.guiltyitemsview]));
			w8.LeftAttach = ((uint)(1));
			w8.RightAttach = ((uint)(2));
			w8.XOptions = ((global::Gtk.AttachOptions)(4));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableFields.Gtk.Table+TableChild
			this.hboxObjectAndKind = new global::Gtk.HBox();
			this.hboxObjectAndKind.Name = "hboxObjectAndKind";
			this.hboxObjectAndKind.Spacing = 6;
			// Container child hboxObjectAndKind.Gtk.Box+BoxChild
			this.yspeccomboboxComplaintObject = new global::Gamma.Widgets.ySpecComboBox();
			this.yspeccomboboxComplaintObject.Name = "yspeccomboboxComplaintObject";
			this.yspeccomboboxComplaintObject.AddIfNotExist = false;
			this.yspeccomboboxComplaintObject.DefaultFirst = false;
			this.yspeccomboboxComplaintObject.ShowSpecialStateAll = false;
			this.yspeccomboboxComplaintObject.ShowSpecialStateNot = false;
			this.hboxObjectAndKind.Add(this.yspeccomboboxComplaintObject);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.hboxObjectAndKind[this.yspeccomboboxComplaintObject]));
			w9.Position = 0;
			w9.Expand = false;
			w9.Fill = false;
			// Container child hboxObjectAndKind.Gtk.Box+BoxChild
			this.labelComplaintKind = new global::Gtk.Label();
			this.labelComplaintKind.Name = "labelComplaintKind";
			this.labelComplaintKind.Xalign = 1F;
			this.labelComplaintKind.LabelProp = global::Mono.Unix.Catalog.GetString("Вид рекламации:");
			this.hboxObjectAndKind.Add(this.labelComplaintKind);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.hboxObjectAndKind[this.labelComplaintKind]));
			w10.Position = 1;
			w10.Expand = false;
			w10.Fill = false;
			// Container child hboxObjectAndKind.Gtk.Box+BoxChild
			this.spLstComplaintKind = new global::QS.Widgets.GtkUI.SpecialListComboBox();
			this.spLstComplaintKind.Name = "spLstComplaintKind";
			this.spLstComplaintKind.AddIfNotExist = false;
			this.spLstComplaintKind.DefaultFirst = false;
			this.spLstComplaintKind.ShowSpecialStateAll = false;
			this.spLstComplaintKind.ShowSpecialStateNot = false;
			this.hboxObjectAndKind.Add(this.spLstComplaintKind);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.hboxObjectAndKind[this.spLstComplaintKind]));
			w11.Position = 2;
			w11.Expand = false;
			w11.Fill = false;
			this.tableFields.Add(this.hboxObjectAndKind);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.tableFields[this.hboxObjectAndKind]));
			w12.TopAttach = ((uint)(2));
			w12.BottomAttach = ((uint)(3));
			w12.LeftAttach = ((uint)(1));
			w12.RightAttach = ((uint)(2));
			w12.XOptions = ((global::Gtk.AttachOptions)(4));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableFields.Gtk.Table+TableChild
			this.labelComplaintText = new global::Gtk.Label();
			this.labelComplaintText.Name = "labelComplaintText";
			this.labelComplaintText.Xalign = 1F;
			this.labelComplaintText.Yalign = 0F;
			this.labelComplaintText.LabelProp = global::Mono.Unix.Catalog.GetString("Проблема:");
			this.tableFields.Add(this.labelComplaintText);
			global::Gtk.Table.TableChild w13 = ((global::Gtk.Table.TableChild)(this.tableFields[this.labelComplaintText]));
			w13.TopAttach = ((uint)(3));
			w13.BottomAttach = ((uint)(4));
			w13.XOptions = ((global::Gtk.AttachOptions)(4));
			w13.YOptions = ((global::Gtk.AttachOptions)(0));
			// Container child tableFields.Gtk.Table+TableChild
			this.labelGuilty = new global::Gtk.Label();
			this.labelGuilty.Name = "labelGuilty";
			this.labelGuilty.Xalign = 1F;
			this.labelGuilty.Yalign = 0F;
			this.labelGuilty.LabelProp = global::Mono.Unix.Catalog.GetString("Ответственные:");
			this.tableFields.Add(this.labelGuilty);
			global::Gtk.Table.TableChild w14 = ((global::Gtk.Table.TableChild)(this.tableFields[this.labelGuilty]));
			w14.XOptions = ((global::Gtk.AttachOptions)(4));
			w14.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableFields.Gtk.Table+TableChild
			this.lblComplaintObject = new global::Gtk.Label();
			this.lblComplaintObject.Name = "lblComplaintObject";
			this.lblComplaintObject.Xalign = 1F;
			this.lblComplaintObject.LabelProp = global::Mono.Unix.Catalog.GetString("Объект рекламаций:");
			this.tableFields.Add(this.lblComplaintObject);
			global::Gtk.Table.TableChild w15 = ((global::Gtk.Table.TableChild)(this.tableFields[this.lblComplaintObject]));
			w15.TopAttach = ((uint)(2));
			w15.BottomAttach = ((uint)(3));
			w15.XOptions = ((global::Gtk.AttachOptions)(4));
			w15.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableFields.Gtk.Table+TableChild
			this.smallfileinformationsview = new global::Vodovoz.Presentation.Views.SmallFileInformationsView();
			this.smallfileinformationsview.Events = ((global::Gdk.EventMask)(256));
			this.smallfileinformationsview.Name = "smallfileinformationsview";
			this.tableFields.Add(this.smallfileinformationsview);
			global::Gtk.Table.TableChild w16 = ((global::Gtk.Table.TableChild)(this.tableFields[this.smallfileinformationsview]));
			w16.TopAttach = ((uint)(4));
			w16.BottomAttach = ((uint)(5));
			w16.LeftAttach = ((uint)(1));
			w16.RightAttach = ((uint)(2));
			w16.XOptions = ((global::Gtk.AttachOptions)(4));
			w16.YOptions = ((global::Gtk.AttachOptions)(4));
			this.vboxDialog.Add(this.tableFields);
			global::Gtk.Box.BoxChild w17 = ((global::Gtk.Box.BoxChild)(this.vboxDialog[this.tableFields]));
			w17.Position = 1;
			// Container child vboxDialog.Gtk.Box+BoxChild
			this.hseparator1 = new global::Gtk.HSeparator();
			this.hseparator1.Name = "hseparator1";
			this.vboxDialog.Add(this.hseparator1);
			global::Gtk.Box.BoxChild w18 = ((global::Gtk.Box.BoxChild)(this.vboxDialog[this.hseparator1]));
			w18.Position = 2;
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

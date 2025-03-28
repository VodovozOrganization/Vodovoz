
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.Views.Logistic
{
	public partial class CarEventTypeView
	{
		private global::Gtk.VBox vbox1;

		private global::Gtk.HBox hbox1;

		private global::Gamma.GtkWidgets.yButton buttonSave;

		private global::Gamma.GtkWidgets.yButton buttonCancel;

		private global::Gtk.Table table1;

		private global::Gtk.Label labelName;

		private global::Gtk.Label labelShortName;

		private global::Gamma.GtkWidgets.yCheckButton ycheckbuttonDoNotShowInOperation;

		private global::Gamma.GtkWidgets.yCheckButton ycheckbuttonIsArchive;

		private global::Gamma.GtkWidgets.yCheckButton ycheckbuttonIsAttachWriteOffDocument;

		private global::Gamma.GtkWidgets.yCheckButton ycheckbuttonNeedComment;

		private global::Gamma.GtkWidgets.yEntry yentryName;

		private global::Gamma.GtkWidgets.yEntry yentryShortName;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.Views.Logistic.CarEventTypeView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.Views.Logistic.CarEventTypeView";
			// Container child Vodovoz.Views.Logistic.CarEventTypeView.Gtk.Container+ContainerChild
			this.vbox1 = new global::Gtk.VBox();
			this.vbox1.Name = "vbox1";
			this.vbox1.Spacing = 6;
			this.vbox1.BorderWidth = ((uint)(6));
			// Container child vbox1.Gtk.Box+BoxChild
			this.hbox1 = new global::Gtk.HBox();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.buttonSave = new global::Gamma.GtkWidgets.yButton();
			this.buttonSave.CanFocus = true;
			this.buttonSave.Name = "buttonSave";
			this.buttonSave.UseUnderline = true;
			this.buttonSave.Label = global::Mono.Unix.Catalog.GetString("Сохранить");
			global::Gtk.Image w1 = new global::Gtk.Image();
			w1.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-floppy", global::Gtk.IconSize.Menu);
			this.buttonSave.Image = w1;
			this.hbox1.Add(this.buttonSave);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.buttonSave]));
			w2.Position = 0;
			w2.Expand = false;
			w2.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.buttonCancel = new global::Gamma.GtkWidgets.yButton();
			this.buttonCancel.CanFocus = true;
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.UseUnderline = true;
			this.buttonCancel.Label = global::Mono.Unix.Catalog.GetString("Отменить");
			global::Gtk.Image w3 = new global::Gtk.Image();
			w3.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-close", global::Gtk.IconSize.Menu);
			this.buttonCancel.Image = w3;
			this.hbox1.Add(this.buttonCancel);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.buttonCancel]));
			w4.Position = 1;
			w4.Expand = false;
			w4.Fill = false;
			this.vbox1.Add(this.hbox1);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hbox1]));
			w5.Position = 0;
			w5.Expand = false;
			w5.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.table1 = new global::Gtk.Table(((uint)(6)), ((uint)(2)), false);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(6));
			// Container child table1.Gtk.Table+TableChild
			this.labelName = new global::Gtk.Label();
			this.labelName.Name = "labelName";
			this.labelName.Xalign = 1F;
			this.labelName.LabelProp = global::Mono.Unix.Catalog.GetString("Название:");
			this.table1.Add(this.labelName);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.table1[this.labelName]));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.labelShortName = new global::Gtk.Label();
			this.labelShortName.Name = "labelShortName";
			this.labelShortName.Xalign = 1F;
			this.labelShortName.LabelProp = global::Mono.Unix.Catalog.GetString("Сокращённое название:");
			this.table1.Add(this.labelShortName);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.table1[this.labelShortName]));
			w7.TopAttach = ((uint)(1));
			w7.BottomAttach = ((uint)(2));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ycheckbuttonDoNotShowInOperation = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckbuttonDoNotShowInOperation.CanFocus = true;
			this.ycheckbuttonDoNotShowInOperation.Name = "ycheckbuttonDoNotShowInOperation";
			this.ycheckbuttonDoNotShowInOperation.Label = global::Mono.Unix.Catalog.GetString("Не отображать в эксплуатации ТС события данного вида");
			this.ycheckbuttonDoNotShowInOperation.DrawIndicator = true;
			this.ycheckbuttonDoNotShowInOperation.UseUnderline = true;
			this.table1.Add(this.ycheckbuttonDoNotShowInOperation);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.table1[this.ycheckbuttonDoNotShowInOperation]));
			w8.TopAttach = ((uint)(4));
			w8.BottomAttach = ((uint)(5));
			w8.LeftAttach = ((uint)(1));
			w8.RightAttach = ((uint)(2));
			w8.XOptions = ((global::Gtk.AttachOptions)(4));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ycheckbuttonIsArchive = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckbuttonIsArchive.CanFocus = true;
			this.ycheckbuttonIsArchive.Name = "ycheckbuttonIsArchive";
			this.ycheckbuttonIsArchive.Label = global::Mono.Unix.Catalog.GetString("Архив");
			this.ycheckbuttonIsArchive.DrawIndicator = true;
			this.ycheckbuttonIsArchive.UseUnderline = true;
			this.table1.Add(this.ycheckbuttonIsArchive);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.table1[this.ycheckbuttonIsArchive]));
			w9.TopAttach = ((uint)(3));
			w9.BottomAttach = ((uint)(4));
			w9.LeftAttach = ((uint)(1));
			w9.RightAttach = ((uint)(2));
			w9.XOptions = ((global::Gtk.AttachOptions)(4));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ycheckbuttonIsAttachWriteOffDocument = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckbuttonIsAttachWriteOffDocument.CanFocus = true;
			this.ycheckbuttonIsAttachWriteOffDocument.Name = "ycheckbuttonIsAttachWriteOffDocument";
			this.ycheckbuttonIsAttachWriteOffDocument.Label = global::Mono.Unix.Catalog.GetString("Прикреплять акт списания");
			this.ycheckbuttonIsAttachWriteOffDocument.DrawIndicator = true;
			this.ycheckbuttonIsAttachWriteOffDocument.UseUnderline = true;
			this.table1.Add(this.ycheckbuttonIsAttachWriteOffDocument);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.table1[this.ycheckbuttonIsAttachWriteOffDocument]));
			w10.TopAttach = ((uint)(5));
			w10.BottomAttach = ((uint)(6));
			w10.LeftAttach = ((uint)(1));
			w10.RightAttach = ((uint)(2));
			w10.XOptions = ((global::Gtk.AttachOptions)(4));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ycheckbuttonNeedComment = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckbuttonNeedComment.CanFocus = true;
			this.ycheckbuttonNeedComment.Name = "ycheckbuttonNeedComment";
			this.ycheckbuttonNeedComment.Label = global::Mono.Unix.Catalog.GetString("Комментарий обязателен");
			this.ycheckbuttonNeedComment.DrawIndicator = true;
			this.ycheckbuttonNeedComment.UseUnderline = true;
			this.table1.Add(this.ycheckbuttonNeedComment);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.table1[this.ycheckbuttonNeedComment]));
			w11.TopAttach = ((uint)(2));
			w11.BottomAttach = ((uint)(3));
			w11.LeftAttach = ((uint)(1));
			w11.RightAttach = ((uint)(2));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.yentryName = new global::Gamma.GtkWidgets.yEntry();
			this.yentryName.CanFocus = true;
			this.yentryName.Name = "yentryName";
			this.yentryName.IsEditable = true;
			this.yentryName.InvisibleChar = '●';
			this.table1.Add(this.yentryName);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.table1[this.yentryName]));
			w12.LeftAttach = ((uint)(1));
			w12.RightAttach = ((uint)(2));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.yentryShortName = new global::Gamma.GtkWidgets.yEntry();
			this.yentryShortName.CanFocus = true;
			this.yentryShortName.Name = "yentryShortName";
			this.yentryShortName.IsEditable = true;
			this.yentryShortName.InvisibleChar = '•';
			this.table1.Add(this.yentryShortName);
			global::Gtk.Table.TableChild w13 = ((global::Gtk.Table.TableChild)(this.table1[this.yentryShortName]));
			w13.TopAttach = ((uint)(1));
			w13.BottomAttach = ((uint)(2));
			w13.LeftAttach = ((uint)(1));
			w13.RightAttach = ((uint)(2));
			w13.YOptions = ((global::Gtk.AttachOptions)(4));
			this.vbox1.Add(this.table1);
			global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.table1]));
			w14.Position = 1;
			w14.Expand = false;
			w14.Fill = false;
			this.Add(this.vbox1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}

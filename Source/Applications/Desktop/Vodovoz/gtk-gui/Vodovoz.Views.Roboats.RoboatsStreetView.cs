
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.Views.Roboats
{
	public partial class RoboatsStreetView
	{
		private global::Gtk.VBox vbox1;

		private global::Gtk.HBox hbox1;

		private global::Gamma.GtkWidgets.yButton buttonSave;

		private global::Gamma.GtkWidgets.yButton buttonCancel;

		private global::Gtk.Table table1;

		private global::Gtk.HBox boxRoboatsHolder;

		private global::Gtk.Label labelId;

		private global::Gamma.GtkWidgets.yLabel labelIdValue;

		private global::Gtk.Label labelStreet;

		private global::Gtk.Label labelStreetType;

		private global::Gamma.GtkWidgets.yEntry yentryStreet;

		private global::Gamma.GtkWidgets.yEntry yentryStreetType;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.Views.Roboats.RoboatsStreetView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.Views.Roboats.RoboatsStreetView";
			// Container child Vodovoz.Views.Roboats.RoboatsStreetView.Gtk.Container+ContainerChild
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
			this.table1 = new global::Gtk.Table(((uint)(4)), ((uint)(2)), false);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(6));
			// Container child table1.Gtk.Table+TableChild
			this.boxRoboatsHolder = new global::Gtk.HBox();
			this.boxRoboatsHolder.Name = "boxRoboatsHolder";
			this.boxRoboatsHolder.Spacing = 6;
			this.table1.Add(this.boxRoboatsHolder);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.table1[this.boxRoboatsHolder]));
			w6.TopAttach = ((uint)(3));
			w6.BottomAttach = ((uint)(4));
			w6.RightAttach = ((uint)(2));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.labelId = new global::Gtk.Label();
			this.labelId.Name = "labelId";
			this.labelId.Xalign = 1F;
			this.labelId.LabelProp = global::Mono.Unix.Catalog.GetString("Id:");
			this.table1.Add(this.labelId);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.table1[this.labelId]));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.labelIdValue = new global::Gamma.GtkWidgets.yLabel();
			this.labelIdValue.Name = "labelIdValue";
			this.labelIdValue.Xalign = 0F;
			this.labelIdValue.LabelProp = global::Mono.Unix.Catalog.GetString("###");
			this.table1.Add(this.labelIdValue);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.table1[this.labelIdValue]));
			w8.LeftAttach = ((uint)(1));
			w8.RightAttach = ((uint)(2));
			w8.XOptions = ((global::Gtk.AttachOptions)(4));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.labelStreet = new global::Gtk.Label();
			this.labelStreet.Name = "labelStreet";
			this.labelStreet.Xalign = 1F;
			this.labelStreet.LabelProp = global::Mono.Unix.Catalog.GetString("Название:");
			this.table1.Add(this.labelStreet);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.table1[this.labelStreet]));
			w9.TopAttach = ((uint)(2));
			w9.BottomAttach = ((uint)(3));
			w9.XOptions = ((global::Gtk.AttachOptions)(4));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.labelStreetType = new global::Gtk.Label();
			this.labelStreetType.Name = "labelStreetType";
			this.labelStreetType.Xalign = 1F;
			this.labelStreetType.LabelProp = global::Mono.Unix.Catalog.GetString("Тип:");
			this.table1.Add(this.labelStreetType);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.table1[this.labelStreetType]));
			w10.TopAttach = ((uint)(1));
			w10.BottomAttach = ((uint)(2));
			w10.XOptions = ((global::Gtk.AttachOptions)(4));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.yentryStreet = new global::Gamma.GtkWidgets.yEntry();
			this.yentryStreet.CanFocus = true;
			this.yentryStreet.Name = "yentryStreet";
			this.yentryStreet.IsEditable = true;
			this.yentryStreet.InvisibleChar = '•';
			this.table1.Add(this.yentryStreet);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.table1[this.yentryStreet]));
			w11.TopAttach = ((uint)(2));
			w11.BottomAttach = ((uint)(3));
			w11.LeftAttach = ((uint)(1));
			w11.RightAttach = ((uint)(2));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.yentryStreetType = new global::Gamma.GtkWidgets.yEntry();
			this.yentryStreetType.CanFocus = true;
			this.yentryStreetType.Name = "yentryStreetType";
			this.yentryStreetType.IsEditable = true;
			this.yentryStreetType.InvisibleChar = '●';
			this.table1.Add(this.yentryStreetType);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.table1[this.yentryStreetType]));
			w12.TopAttach = ((uint)(1));
			w12.BottomAttach = ((uint)(2));
			w12.LeftAttach = ((uint)(1));
			w12.RightAttach = ((uint)(2));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			this.vbox1.Add(this.table1);
			global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.table1]));
			w13.Position = 1;
			this.Add(this.vbox1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}

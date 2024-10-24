
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.Views.Fuel
{
	public partial class FuelCardView
	{
		private global::Gamma.GtkWidgets.yVBox yvboxMain;

		private global::Gamma.GtkWidgets.yHBox yhboxButtons;

		private global::Gamma.GtkWidgets.yButton ybuttonSave;

		private global::Gamma.GtkWidgets.yButton ybuttonCancel;

		private global::Gamma.GtkWidgets.yTable ytableData;

		private global::Gamma.GtkWidgets.yButton ybuttonGetCardId;

		private global::Gamma.GtkWidgets.yCheckButton ycheckbuttonIsArchived;

		private global::Gamma.GtkWidgets.yEntry yentryCardId;

		private global::Gamma.GtkWidgets.yEntry yentryCardNumber;

		private global::Gamma.GtkWidgets.yLabel ylabelCardId;

		private global::Gamma.GtkWidgets.yLabel ylabelCardNumber;

		private global::Gamma.GtkWidgets.yLabel ylabelIsArchived;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.Views.Fuel.FuelCardView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.Views.Fuel.FuelCardView";
			// Container child Vodovoz.Views.Fuel.FuelCardView.Gtk.Container+ContainerChild
			this.yvboxMain = new global::Gamma.GtkWidgets.yVBox();
			this.yvboxMain.Name = "yvboxMain";
			this.yvboxMain.Spacing = 6;
			// Container child yvboxMain.Gtk.Box+BoxChild
			this.yhboxButtons = new global::Gamma.GtkWidgets.yHBox();
			this.yhboxButtons.Name = "yhboxButtons";
			this.yhboxButtons.Spacing = 6;
			// Container child yhboxButtons.Gtk.Box+BoxChild
			this.ybuttonSave = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonSave.CanFocus = true;
			this.ybuttonSave.Name = "ybuttonSave";
			this.ybuttonSave.UseUnderline = true;
			this.ybuttonSave.Label = global::Mono.Unix.Catalog.GetString("Сохранить");
			global::Gtk.Image w1 = new global::Gtk.Image();
			w1.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-floppy", global::Gtk.IconSize.Menu);
			this.ybuttonSave.Image = w1;
			this.yhboxButtons.Add(this.ybuttonSave);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.yhboxButtons[this.ybuttonSave]));
			w2.Position = 0;
			w2.Expand = false;
			w2.Fill = false;
			// Container child yhboxButtons.Gtk.Box+BoxChild
			this.ybuttonCancel = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonCancel.CanFocus = true;
			this.ybuttonCancel.Name = "ybuttonCancel";
			this.ybuttonCancel.UseUnderline = true;
			this.ybuttonCancel.Label = global::Mono.Unix.Catalog.GetString("Отмена");
			global::Gtk.Image w3 = new global::Gtk.Image();
			w3.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-revert-to-saved", global::Gtk.IconSize.Menu);
			this.ybuttonCancel.Image = w3;
			this.yhboxButtons.Add(this.ybuttonCancel);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.yhboxButtons[this.ybuttonCancel]));
			w4.Position = 1;
			w4.Expand = false;
			w4.Fill = false;
			this.yvboxMain.Add(this.yhboxButtons);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.yvboxMain[this.yhboxButtons]));
			w5.Position = 0;
			w5.Expand = false;
			w5.Fill = false;
			// Container child yvboxMain.Gtk.Box+BoxChild
			this.ytableData = new global::Gamma.GtkWidgets.yTable();
			this.ytableData.Name = "ytableData";
			this.ytableData.NRows = ((uint)(4));
			this.ytableData.NColumns = ((uint)(3));
			this.ytableData.RowSpacing = ((uint)(6));
			this.ytableData.ColumnSpacing = ((uint)(6));
			// Container child ytableData.Gtk.Table+TableChild
			this.ybuttonGetCardId = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonGetCardId.CanFocus = true;
			this.ybuttonGetCardId.Name = "ybuttonGetCardId";
			this.ybuttonGetCardId.UseUnderline = true;
			this.ybuttonGetCardId.Label = global::Mono.Unix.Catalog.GetString("Получить Id");
			this.ytableData.Add(this.ybuttonGetCardId);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.ytableData[this.ybuttonGetCardId]));
			w6.TopAttach = ((uint)(2));
			w6.BottomAttach = ((uint)(3));
			w6.LeftAttach = ((uint)(2));
			w6.RightAttach = ((uint)(3));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytableData.Gtk.Table+TableChild
			this.ycheckbuttonIsArchived = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckbuttonIsArchived.CanFocus = true;
			this.ycheckbuttonIsArchived.Name = "ycheckbuttonIsArchived";
			this.ycheckbuttonIsArchived.Label = "";
			this.ycheckbuttonIsArchived.DrawIndicator = true;
			this.ycheckbuttonIsArchived.UseUnderline = true;
			this.ytableData.Add(this.ycheckbuttonIsArchived);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.ytableData[this.ycheckbuttonIsArchived]));
			w7.LeftAttach = ((uint)(1));
			w7.RightAttach = ((uint)(2));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytableData.Gtk.Table+TableChild
			this.yentryCardId = new global::Gamma.GtkWidgets.yEntry();
			this.yentryCardId.Sensitive = false;
			this.yentryCardId.CanFocus = true;
			this.yentryCardId.Name = "yentryCardId";
			this.yentryCardId.IsEditable = true;
			this.yentryCardId.InvisibleChar = '•';
			this.ytableData.Add(this.yentryCardId);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.ytableData[this.yentryCardId]));
			w8.TopAttach = ((uint)(2));
			w8.BottomAttach = ((uint)(3));
			w8.LeftAttach = ((uint)(1));
			w8.RightAttach = ((uint)(2));
			w8.XOptions = ((global::Gtk.AttachOptions)(4));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytableData.Gtk.Table+TableChild
			this.yentryCardNumber = new global::Gamma.GtkWidgets.yEntry();
			this.yentryCardNumber.WidthRequest = 250;
			this.yentryCardNumber.CanFocus = true;
			this.yentryCardNumber.Name = "yentryCardNumber";
			this.yentryCardNumber.IsEditable = true;
			this.yentryCardNumber.InvisibleChar = '•';
			this.ytableData.Add(this.yentryCardNumber);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.ytableData[this.yentryCardNumber]));
			w9.TopAttach = ((uint)(1));
			w9.BottomAttach = ((uint)(2));
			w9.LeftAttach = ((uint)(1));
			w9.RightAttach = ((uint)(2));
			w9.XOptions = ((global::Gtk.AttachOptions)(4));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytableData.Gtk.Table+TableChild
			this.ylabelCardId = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelCardId.Name = "ylabelCardId";
			this.ylabelCardId.Xalign = 1F;
			this.ylabelCardId.LabelProp = global::Mono.Unix.Catalog.GetString("Id карты:");
			this.ytableData.Add(this.ylabelCardId);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.ytableData[this.ylabelCardId]));
			w10.TopAttach = ((uint)(2));
			w10.BottomAttach = ((uint)(3));
			w10.XOptions = ((global::Gtk.AttachOptions)(4));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytableData.Gtk.Table+TableChild
			this.ylabelCardNumber = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelCardNumber.Name = "ylabelCardNumber";
			this.ylabelCardNumber.Xalign = 1F;
			this.ylabelCardNumber.LabelProp = global::Mono.Unix.Catalog.GetString("Номер карты:");
			this.ytableData.Add(this.ylabelCardNumber);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.ytableData[this.ylabelCardNumber]));
			w11.TopAttach = ((uint)(1));
			w11.BottomAttach = ((uint)(2));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child ytableData.Gtk.Table+TableChild
			this.ylabelIsArchived = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelIsArchived.Name = "ylabelIsArchived";
			this.ylabelIsArchived.Xalign = 1F;
			this.ylabelIsArchived.LabelProp = global::Mono.Unix.Catalog.GetString("В архиве:");
			this.ytableData.Add(this.ylabelIsArchived);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.ytableData[this.ylabelIsArchived]));
			w12.XOptions = ((global::Gtk.AttachOptions)(4));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			this.yvboxMain.Add(this.ytableData);
			global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.yvboxMain[this.ytableData]));
			w13.Position = 1;
			this.Add(this.yvboxMain);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}

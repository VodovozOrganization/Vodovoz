
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.Views.Mango.Talks
{
	public partial class UnknowTalkView
	{
		private global::Gtk.VBox MainVbox;

		private global::Gtk.HBox hbox1;

		private global::Gtk.Label Label1_Conversation;

		private global::Gamma.GtkWidgets.yLabel CallNumberLabel;

		private global::Gtk.HBox hbox3;

		private global::Gamma.GtkWidgets.yButton NewClientButton;

		private global::Gamma.GtkWidgets.yButton ExistingClientButton;

		private global::Gtk.VBox vbox6;

		private global::Gtk.Table table5;

		private global::Gamma.GtkWidgets.yButton ComplaintButton;

		private global::Gamma.GtkWidgets.yButton CostAndDeliveryIntervalButton;

		private global::Gamma.GtkWidgets.yButton StockBalnce;

		private global::Gtk.Table table4;

		private global::Gamma.GtkWidgets.yButton FinishButton;

		private global::Gamma.GtkWidgets.yButton ForwardingButton;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.Views.Mango.Talks.UnknowTalkView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.Views.Mango.Talks.UnknowTalkView";
			// Container child Vodovoz.Views.Mango.Talks.UnknowTalkView.Gtk.Container+ContainerChild
			this.MainVbox = new global::Gtk.VBox();
			this.MainVbox.Name = "MainVbox";
			this.MainVbox.Spacing = 6;
			this.MainVbox.BorderWidth = ((uint)(9));
			// Container child MainVbox.Gtk.Box+BoxChild
			this.hbox1 = new global::Gtk.HBox();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.Label1_Conversation = new global::Gtk.Label();
			this.Label1_Conversation.Name = "Label1_Conversation";
			this.Label1_Conversation.Xalign = 1F;
			this.Label1_Conversation.LabelProp = global::Mono.Unix.Catalog.GetString("Разговор:");
			this.hbox1.Add(this.Label1_Conversation);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.Label1_Conversation]));
			w1.Position = 0;
			w1.Expand = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.CallNumberLabel = new global::Gamma.GtkWidgets.yLabel();
			this.CallNumberLabel.Name = "CallNumberLabel";
			this.CallNumberLabel.Xalign = 0F;
			this.CallNumberLabel.Yalign = 0F;
			this.CallNumberLabel.LabelProp = global::Mono.Unix.Catalog.GetString("Телефон");
			this.CallNumberLabel.Justify = ((global::Gtk.Justification)(2));
			this.CallNumberLabel.Selectable = true;
			this.CallNumberLabel.WidthChars = 0;
			this.CallNumberLabel.MaxWidthChars = 0;
			this.hbox1.Add(this.CallNumberLabel);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.CallNumberLabel]));
			w2.Position = 1;
			w2.Padding = ((uint)(72));
			this.MainVbox.Add(this.hbox1);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.MainVbox[this.hbox1]));
			w3.Position = 0;
			w3.Expand = false;
			w3.Fill = false;
			// Container child MainVbox.Gtk.Box+BoxChild
			this.hbox3 = new global::Gtk.HBox();
			this.hbox3.Name = "hbox3";
			this.hbox3.Spacing = 6;
			// Container child hbox3.Gtk.Box+BoxChild
			this.NewClientButton = new global::Gamma.GtkWidgets.yButton();
			this.NewClientButton.CanFocus = true;
			this.NewClientButton.Name = "NewClientButton";
			this.NewClientButton.UseUnderline = true;
			this.NewClientButton.Label = global::Mono.Unix.Catalog.GetString("Новый клиент");
			this.hbox3.Add(this.NewClientButton);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hbox3[this.NewClientButton]));
			w4.Position = 0;
			// Container child hbox3.Gtk.Box+BoxChild
			this.ExistingClientButton = new global::Gamma.GtkWidgets.yButton();
			this.ExistingClientButton.CanFocus = true;
			this.ExistingClientButton.Name = "ExistingClientButton";
			this.ExistingClientButton.UseUnderline = true;
			this.ExistingClientButton.Label = global::Mono.Unix.Catalog.GetString("Существующий клиент");
			this.hbox3.Add(this.ExistingClientButton);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.hbox3[this.ExistingClientButton]));
			w5.Position = 1;
			this.MainVbox.Add(this.hbox3);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.MainVbox[this.hbox3]));
			w6.Position = 1;
			w6.Expand = false;
			w6.Fill = false;
			// Container child MainVbox.Gtk.Box+BoxChild
			this.vbox6 = new global::Gtk.VBox();
			this.vbox6.Name = "vbox6";
			this.vbox6.Spacing = 6;
			// Container child vbox6.Gtk.Box+BoxChild
			this.table5 = new global::Gtk.Table(((uint)(1)), ((uint)(5)), false);
			this.table5.Name = "table5";
			this.table5.RowSpacing = ((uint)(6));
			this.table5.ColumnSpacing = ((uint)(6));
			// Container child table5.Gtk.Table+TableChild
			this.ComplaintButton = new global::Gamma.GtkWidgets.yButton();
			this.ComplaintButton.CanFocus = true;
			this.ComplaintButton.Name = "ComplaintButton";
			this.ComplaintButton.UseUnderline = true;
			this.ComplaintButton.Label = global::Mono.Unix.Catalog.GetString("Обращение");
			this.table5.Add(this.ComplaintButton);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.table5[this.ComplaintButton]));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table5.Gtk.Table+TableChild
			this.CostAndDeliveryIntervalButton = new global::Gamma.GtkWidgets.yButton();
			this.CostAndDeliveryIntervalButton.CanFocus = true;
			this.CostAndDeliveryIntervalButton.Name = "CostAndDeliveryIntervalButton";
			this.CostAndDeliveryIntervalButton.UseUnderline = true;
			this.CostAndDeliveryIntervalButton.Label = global::Mono.Unix.Catalog.GetString("Стоимость и \n  интервалы\n   доставки");
			this.table5.Add(this.CostAndDeliveryIntervalButton);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.table5[this.CostAndDeliveryIntervalButton]));
			w8.LeftAttach = ((uint)(4));
			w8.RightAttach = ((uint)(5));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table5.Gtk.Table+TableChild
			this.StockBalnce = new global::Gamma.GtkWidgets.yButton();
			this.StockBalnce.CanFocus = true;
			this.StockBalnce.Name = "StockBalnce";
			this.StockBalnce.UseUnderline = true;
			this.StockBalnce.Label = global::Mono.Unix.Catalog.GetString("Складские остатки");
			this.table5.Add(this.StockBalnce);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.table5[this.StockBalnce]));
			w9.LeftAttach = ((uint)(2));
			w9.RightAttach = ((uint)(3));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			this.vbox6.Add(this.table5);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.vbox6[this.table5]));
			w10.Position = 0;
			w10.Expand = false;
			w10.Fill = false;
			// Container child vbox6.Gtk.Box+BoxChild
			this.table4 = new global::Gtk.Table(((uint)(1)), ((uint)(6)), false);
			this.table4.Name = "table4";
			this.table4.RowSpacing = ((uint)(6));
			this.table4.ColumnSpacing = ((uint)(6));
			// Container child table4.Gtk.Table+TableChild
			this.FinishButton = new global::Gamma.GtkWidgets.yButton();
			this.FinishButton.CanFocus = true;
			this.FinishButton.Name = "FinishButton";
			this.FinishButton.UseUnderline = true;
			this.FinishButton.Label = global::Mono.Unix.Catalog.GetString("Завершить");
			this.table4.Add(this.FinishButton);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.table4[this.FinishButton]));
			w11.LeftAttach = ((uint)(5));
			w11.RightAttach = ((uint)(6));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table4.Gtk.Table+TableChild
			this.ForwardingButton = new global::Gamma.GtkWidgets.yButton();
			this.ForwardingButton.CanFocus = true;
			this.ForwardingButton.Name = "ForwardingButton";
			this.ForwardingButton.UseUnderline = true;
			this.ForwardingButton.Label = global::Mono.Unix.Catalog.GetString("|| Переадресация");
			this.table4.Add(this.ForwardingButton);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.table4[this.ForwardingButton]));
			w12.RightAttach = ((uint)(3));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			this.vbox6.Add(this.table4);
			global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.vbox6[this.table4]));
			w13.Position = 1;
			w13.Expand = false;
			w13.Fill = false;
			this.MainVbox.Add(this.vbox6);
			global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.MainVbox[this.vbox6]));
			w14.Position = 2;
			w14.Expand = false;
			w14.Fill = false;
			this.Add(this.MainVbox);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
			this.NewClientButton.Clicked += new global::System.EventHandler(this.Clicked_NewClientButton);
			this.ExistingClientButton.Clicked += new global::System.EventHandler(this.Clicked_ExistingClientButton);
			this.StockBalnce.Clicked += new global::System.EventHandler(this.Clicked_StockBalnce);
			this.CostAndDeliveryIntervalButton.Clicked += new global::System.EventHandler(this.Clicked_CostAndDeliveryIntervalButton);
			this.ComplaintButton.Clicked += new global::System.EventHandler(this.Clicked_ComplaintButton);
			this.ForwardingButton.Clicked += new global::System.EventHandler(this.Clicked_ForwardingButton);
			this.FinishButton.Clicked += new global::System.EventHandler(this.Clicked_FinishButton);
		}
	}
}

﻿
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.ViewWidgets
{
	public partial class GuiltyInUndeliveryView
	{
		private global::Gtk.Table table1;

		private global::Gamma.GtkWidgets.yButton btnRemove;

		private global::QS.Widgets.EnumMenuButton enumBtnGuiltySide;

		private global::Gtk.ScrolledWindow GtkScrolledWindow;

		private global::Gamma.GtkWidgets.yTreeView treeViewGuilty;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.ViewWidgets.GuiltyInUndeliveryView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.ViewWidgets.GuiltyInUndeliveryView";
			// Container child Vodovoz.ViewWidgets.GuiltyInUndeliveryView.Gtk.Container+ContainerChild
			this.table1 = new global::Gtk.Table(((uint)(2)), ((uint)(2)), false);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(6));
			// Container child table1.Gtk.Table+TableChild
			this.btnRemove = new global::Gamma.GtkWidgets.yButton();
			this.btnRemove.CanFocus = true;
			this.btnRemove.Name = "btnRemove";
			this.btnRemove.UseUnderline = true;
			this.btnRemove.Label = global::Mono.Unix.Catalog.GetString("Удалить");
			this.table1.Add(this.btnRemove);
			global::Gtk.Table.TableChild w1 = ((global::Gtk.Table.TableChild)(this.table1[this.btnRemove]));
			w1.TopAttach = ((uint)(1));
			w1.BottomAttach = ((uint)(2));
			w1.LeftAttach = ((uint)(1));
			w1.RightAttach = ((uint)(2));
			w1.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.enumBtnGuiltySide = new global::QS.Widgets.EnumMenuButton();
			this.enumBtnGuiltySide.CanFocus = true;
			this.enumBtnGuiltySide.Name = "enumBtnGuiltySide";
			this.enumBtnGuiltySide.UseUnderline = true;
			this.enumBtnGuiltySide.UseMarkup = false;
			this.enumBtnGuiltySide.LabelXAlign = 0F;
			this.enumBtnGuiltySide.Label = global::Mono.Unix.Catalog.GetString("Добавить");
			this.table1.Add(this.enumBtnGuiltySide);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.table1[this.enumBtnGuiltySide]));
			w2.TopAttach = ((uint)(1));
			w2.BottomAttach = ((uint)(2));
			w2.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.VscrollbarPolicy = ((global::Gtk.PolicyType)(2));
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.treeViewGuilty = new global::Gamma.GtkWidgets.yTreeView();
			this.treeViewGuilty.CanFocus = true;
			this.treeViewGuilty.Name = "treeViewGuilty";
			this.GtkScrolledWindow.Add(this.treeViewGuilty);
			this.table1.Add(this.GtkScrolledWindow);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.table1[this.GtkScrolledWindow]));
			w4.RightAttach = ((uint)(2));
			this.Add(this.table1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
			this.btnRemove.Clicked += new global::System.EventHandler(this.OnBtnRemoveClicked);
		}
	}
}

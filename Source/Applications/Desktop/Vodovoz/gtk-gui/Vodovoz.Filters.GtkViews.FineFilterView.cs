
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.Filters.GtkViews
{
	public partial class FineFilterView
	{
		private global::Gtk.Table table1;

		private global::QS.Views.Control.EntityEntry entryAuthor;

		private global::QS.Views.Control.EntityEntry entrySubdivision;

		private global::Gtk.Label labelAuthor;

		private global::Gtk.Label labelSubdivision;

		private global::QS.Widgets.GtkUI.DateRangePicker ydateperiodpickerFineDate;

		private global::QS.Widgets.GtkUI.DateRangePicker ydateperiodpickerRouteList;

		private global::Gamma.GtkWidgets.yLabel ylabel1;

		private global::Gamma.GtkWidgets.yLabel ylabel2;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.Filters.GtkViews.FineFilterView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.Filters.GtkViews.FineFilterView";
			// Container child Vodovoz.Filters.GtkViews.FineFilterView.Gtk.Container+ContainerChild
			this.table1 = new global::Gtk.Table(((uint)(3)), ((uint)(4)), false);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(6));
			// Container child table1.Gtk.Table+TableChild
			this.entryAuthor = new global::QS.Views.Control.EntityEntry();
			this.entryAuthor.Events = ((global::Gdk.EventMask)(256));
			this.entryAuthor.Name = "entryAuthor";
			this.table1.Add(this.entryAuthor);
			global::Gtk.Table.TableChild w1 = ((global::Gtk.Table.TableChild)(this.table1[this.entryAuthor]));
			w1.LeftAttach = ((uint)(3));
			w1.RightAttach = ((uint)(4));
			w1.XOptions = ((global::Gtk.AttachOptions)(4));
			w1.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.entrySubdivision = new global::QS.Views.Control.EntityEntry();
			this.entrySubdivision.Events = ((global::Gdk.EventMask)(256));
			this.entrySubdivision.Name = "entrySubdivision";
			this.table1.Add(this.entrySubdivision);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.table1[this.entrySubdivision]));
			w2.LeftAttach = ((uint)(1));
			w2.RightAttach = ((uint)(2));
			w2.XOptions = ((global::Gtk.AttachOptions)(0));
			w2.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.labelAuthor = new global::Gtk.Label();
			this.labelAuthor.Name = "labelAuthor";
			this.labelAuthor.Xalign = 1F;
			this.labelAuthor.LabelProp = global::Mono.Unix.Catalog.GetString("Автор:");
			this.table1.Add(this.labelAuthor);
			global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.table1[this.labelAuthor]));
			w3.LeftAttach = ((uint)(2));
			w3.RightAttach = ((uint)(3));
			w3.XOptions = ((global::Gtk.AttachOptions)(4));
			w3.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.labelSubdivision = new global::Gtk.Label();
			this.labelSubdivision.Name = "labelSubdivision";
			this.labelSubdivision.Xalign = 1F;
			this.labelSubdivision.LabelProp = global::Mono.Unix.Catalog.GetString("Подраздление:");
			this.table1.Add(this.labelSubdivision);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.table1[this.labelSubdivision]));
			w4.XOptions = ((global::Gtk.AttachOptions)(4));
			w4.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ydateperiodpickerFineDate = new global::QS.Widgets.GtkUI.DateRangePicker();
			this.ydateperiodpickerFineDate.Events = ((global::Gdk.EventMask)(256));
			this.ydateperiodpickerFineDate.Name = "ydateperiodpickerFineDate";
			this.ydateperiodpickerFineDate.StartDate = new global::System.DateTime(0);
			this.ydateperiodpickerFineDate.EndDate = new global::System.DateTime(0);
			this.table1.Add(this.ydateperiodpickerFineDate);
			global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.table1[this.ydateperiodpickerFineDate]));
			w5.TopAttach = ((uint)(1));
			w5.BottomAttach = ((uint)(2));
			w5.LeftAttach = ((uint)(1));
			w5.RightAttach = ((uint)(2));
			w5.XOptions = ((global::Gtk.AttachOptions)(4));
			w5.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ydateperiodpickerRouteList = new global::QS.Widgets.GtkUI.DateRangePicker();
			this.ydateperiodpickerRouteList.Events = ((global::Gdk.EventMask)(256));
			this.ydateperiodpickerRouteList.Name = "ydateperiodpickerRouteList";
			this.ydateperiodpickerRouteList.StartDate = new global::System.DateTime(0);
			this.ydateperiodpickerRouteList.EndDate = new global::System.DateTime(0);
			this.table1.Add(this.ydateperiodpickerRouteList);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.table1[this.ydateperiodpickerRouteList]));
			w6.TopAttach = ((uint)(2));
			w6.BottomAttach = ((uint)(3));
			w6.LeftAttach = ((uint)(1));
			w6.RightAttach = ((uint)(2));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabel1 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel1.Name = "ylabel1";
			this.ylabel1.Xalign = 1F;
			this.ylabel1.LabelProp = global::Mono.Unix.Catalog.GetString("Дата создания штрафа:");
			this.table1.Add(this.ylabel1);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabel1]));
			w7.TopAttach = ((uint)(1));
			w7.BottomAttach = ((uint)(2));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabel2 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel2.Name = "ylabel2";
			this.ylabel2.Xalign = 1F;
			this.ylabel2.LabelProp = global::Mono.Unix.Catalog.GetString("Дата МЛ:");
			this.table1.Add(this.ylabel2);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabel2]));
			w8.TopAttach = ((uint)(2));
			w8.BottomAttach = ((uint)(3));
			w8.XOptions = ((global::Gtk.AttachOptions)(4));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			this.Add(this.table1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}

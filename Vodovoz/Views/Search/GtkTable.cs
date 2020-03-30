using System;
using GLib;
using Gtk;

namespace Vodovoz.Views.Search
{
	public class GtkTable : Gtk.Table
	{
		public GtkTable(IntPtr raw) : base(raw)
		{
		}

		public GtkTable(uint rows, uint columns, bool homogeneous) : base(rows, columns, homogeneous)
		{
		}

		protected GtkTable(GType gtype) : base(gtype)
		{
		}

		public Action<DirectionType> OnFocusMovedAction { get; set; }

		public virtual void OnFocusMovedBaseAction(DirectionType p0)
		{
			base.OnFocusMoved(p0);
		}

		protected override void OnFocusMoved(DirectionType p0)
		{
			if(OnFocusMovedAction != null) {
				OnFocusMovedAction(p0);
			} else {
				OnFocusMovedBaseAction(p0);
			}
		}

		public Func<DirectionType, bool> OnFocusedAction { get; set; }

		public virtual bool OnFocusedBaseAction(DirectionType direction)
		{
			return base.OnFocused(direction);
		}

		protected override bool OnFocused(DirectionType direction)
		{
			if(OnFocusedAction != null) {
				return OnFocusedAction(direction);
			} else {
				return OnFocusedBaseAction(direction);
			}
		}


		public Action<Widget> OnFocusChildSetAction { get; set; }

		public virtual void OnFocusChildSetBaseAction(Widget widget)
		{
			base.OnFocusChildSet(widget);
		}

		protected override void OnFocusChildSet(Widget widget)
		{
			/*
			if(OnFocusChildSetAction != null) {
				OnFocusChildSetAction(widget);
			} else {
				OnFocusChildSetBaseAction(widget);
			}*/

			return;
		}
	}
}

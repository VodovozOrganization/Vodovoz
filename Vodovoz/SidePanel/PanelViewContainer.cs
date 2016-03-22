using System;
using Gtk;

namespace Vodovoz.Panel
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PanelViewContainer : Gtk.Bin
	{
		Gtk.Image iconPinned;
		Gtk.Image iconUnpinned;

		public event EventHandler<EventArgs> Unpinned;

		IInfoProvider infoProvider;
		public IInfoProvider InfoProvider{ 
			get{
				return infoProvider;
			}
			set{
				infoProvider = value;
				var panelView = Widget as IPanelView;
				if(panelView!=null)
					panelView.InfoProvider=value;
			}
		}

		public Widget Widget
		{
			get{
				return GtkAlignment1.Child;
			}
			protected set{
				if(GtkAlignment1.Child!=null)
					GtkAlignment1.Remove(GtkAlignment1.Child);
				if(value!=null)
					GtkAlignment1.Add(value);
			}
		}

		public bool Pinned{
			get{				
				return buttonPin.Active;
			}
		}

		public bool IsOrphan()
		{
			return InfoProvider==null;
		}

		public bool VisibleOnPanel
		{
			get
			{
				return (this.Widget as IPanelView)?.VisibleOnPanel ?? true;
			}
		}
		protected override void OnShown()
		{
			base.OnShown();
			this.Widget?.Show();
		}			

		public PanelViewContainer()
		{
			this.Build();
			iconPinned = Gtk.Image.LoadFromResource("Vodovoz.icons.buttons.pin-up.png");
			iconUnpinned = Gtk.Image.LoadFromResource("Vodovoz.icons.buttons.pin-down.png");
			buttonPin.Toggled += OnButtonPinToggled;
		}

		public static PanelViewContainer Wrap(Widget widget)
		{
			var container = new PanelViewContainer();
			container.Widget = widget;
			return container;
		}			

		protected void OnButtonPinToggled (object sender, EventArgs e)
		{
			buttonPin.Image = buttonPin.Active ? iconUnpinned : iconPinned;
			if(!buttonPin.Active && Unpinned!=null)
				Unpinned(this, EventArgs.Empty);
		}
	}
}


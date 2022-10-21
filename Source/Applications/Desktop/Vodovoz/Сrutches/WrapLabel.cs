using System;
using Gtk;
using Gamma.GtkWidgets;

namespace Vodovoz
{
	/// <summary>
	/// Workaround для Gtk.Label с корректным переносом при неуказанном WidthRequest.
	/// Корректно работает только при Fill==true и нулевых Xalign, Yalign.
	/// </summary>
	[System.ComponentModel.ToolboxItem(true)]
	public class WrapLabel : yLabel
	{
		public WrapLabel()
		{
			Wrap = true;
			base.SetAlignment(0, 0);
		}

		/// <summary>
		/// Workaround корректно работает только при нулевых Xalign, Yalign, которые проставляются по умолчанию.
		/// Установка этих свойств игнорируется.
		/// </summary>
		public new void SetAlignment(float width, float height)
		{
		}

		/// <summary>
		/// Workaround корректно работает только при нулевых Xalign, Yalign, которые проставляются по умолчанию.
		/// Установка этих свойств игнорируется.
		/// </summary>
		[System.ComponentModel.Browsable(false)]
		public new float Xalign
		{
			get{
				return base.Xalign;
			}
			set{ }
		}

		/// <summary>
		/// Workaround корректно работает только при нулевых Xalign, Yalign, которые проставляются по умолчанию.
		/// Установка этих свойств игнорируется.
		/// </summary>
		[System.ComponentModel.Browsable(false)]
		public new float Yalign
		{
			get{
				return base.Yalign;
			}
			set{ }
		}

		bool WorkaroundNeeded{
			get{
				return WidthRequest == -1 && Wrap;
			}
		}

		int wrapWidth;
		int wrapHeight;

		[System.ComponentModel.Browsable(true)]
		public new string Text
		{
			get{
				return base.Text;
			}
			set{
				base.Text = value;
				if(WorkaroundNeeded)
					SetWrapWidth(wrapWidth);
			}
		}
			
		public new string Markup{
			set{
				base.Markup = value;
				if(WorkaroundNeeded)
					SetWrapWidth(wrapWidth);
			}
		}

		[System.ComponentModel.Browsable(true)]
		public new Pango.WrapMode LineWrapMode
		{
			get{
				return base.LineWrapMode;
			}
			set{
				base.LineWrapMode = value;
			}
		}			

		protected override void OnSizeAllocated (Gdk.Rectangle allocation)
		{
			base.OnSizeAllocated (allocation);
			if(WorkaroundNeeded)
				SetWrapWidth((int)(allocation.Width));
		}           

		protected override void OnSizeRequested (ref Gtk.Requisition requisition)
		{			
			if (WorkaroundNeeded)
			{
				requisition.Width = 0;
				requisition.Height = wrapHeight;
			}
			else
			{
				base.OnSizeRequested(ref requisition);
			}
		}

		private void SetWrapWidth(int width)
		{
			if (!WorkaroundNeeded)
			{
				return;
			}

			if (width == 0) {
				return;
			}

			Layout.Width = (int)width * (int)Pango.Scale.PangoScale;

			int unused;
			Layout.GetPixelSize(out unused, out wrapHeight);

			if (wrapWidth != width) {
				wrapWidth = width;
				QueueResize();
			}
		}
	}
}





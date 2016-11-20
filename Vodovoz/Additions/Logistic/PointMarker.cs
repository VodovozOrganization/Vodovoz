using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.Serialization;
using GMap.NET;
using GMap.NET.GtkSharp;

namespace Vodovoz.Additions.Logistic
{

	public enum PointMarkerType
	{
		none = 0,
		black_and_red,
		black,
		blue,
		blue_stripes,
		white,
		gray,
		green,
		orange,
		purple,
		red,
		color2,
		color3,
		color4,
		color5,
		color6,
		color7,
		color8,
		color9,
		color10,
		color11,
		color12,
		color13,
		color14,
		color15,
		color16,
		color17,
		color18,
		color20,
		color21,
		color22,
		color23,
		color24,
	}

	[Serializable]
	public class PointMarker : GMapMarker, ISerializable, IDeserializationCallback
	{
		Bitmap Bitmap;
		Bitmap BitmapShadow;

		private PointMarkerType type;

		public PointMarkerType Type
		{
			get
			{
				return type;
			}
			set
			{
				type = value;
				if (type != PointMarkerType.none)
				{
					LoadBitmap();
				}

				if (IsVisible)
				{
					if (Overlay != null && Overlay.Control != null)
					{
						if (!Overlay.Control.HoldInvalidation)
						{
							Overlay.Control.Invalidate();
						}
					}
				}
			}
		}

		public PointMarker(PointLatLng p, PointMarkerType type)
			: base(p)
		{
			this.Type = type;
		}

		void LoadBitmap()
		{
			Bitmap = GetIcon(type.ToString());
			Size = new System.Drawing.Size(Bitmap.Width, Bitmap.Height);

			Offset = new Point(-Size.Width / 2, -Size.Height + 1);

			BitmapShadow = GetIcon("marker_shadow");
		}

		static readonly Dictionary<string, Bitmap> iconCache = new Dictionary<string, Bitmap>();

		internal static Bitmap GetIcon(string name)
		{
			Bitmap ret;
			if (!iconCache.TryGetValue(name, out ret))
			{
				string resourceName = String.Format("Vodovoz.icons.map.points.{0}.png", name);
				ret = new Bitmap(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName));
				iconCache.Add(name, ret);
			}
			return ret;
		}

		public static Gdk.Pixbuf GetIconPixbuf(string name)
		{
			string resourceName = String.Format("Vodovoz.icons.map.points.{0}.png", name);
			return new Gdk.Pixbuf(System.Reflection.Assembly.GetExecutingAssembly(), resourceName);
		}

		public override void OnRender(Graphics g)
		{
			if(BitmapShadow != null)
			{
				g.DrawImage(BitmapShadow, LocalPosition.X, LocalPosition.Y, BitmapShadow.Width, BitmapShadow.Height);
			}                
			g.DrawImage(Bitmap, LocalPosition.X, LocalPosition.Y, Size.Width, Size.Height);
		}

		public override void Dispose()
		{
			if (Bitmap != null)
			{
				if (!iconCache.ContainsValue(Bitmap))
				{
					Bitmap.Dispose();
					Bitmap = null;
				}
			}

			base.Dispose();
		}

		#region ISerializable Members

		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("type", this.type);

			base.GetObjectData(info, context);
		}

		protected PointMarker(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			this.type = Extensions.GetStruct<PointMarkerType>(info, "type", PointMarkerType.none);
		}

		#endregion

		#region IDeserializationCallback Members

		public void OnDeserialization(object sender)
		{
			if (type != PointMarkerType.none)
			{
				LoadBitmap();
			}
		}

		#endregion
	}
}

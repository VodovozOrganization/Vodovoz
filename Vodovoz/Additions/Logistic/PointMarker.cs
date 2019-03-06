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
		red_stripes,
		yellow_stripes,
		green_stripes,
		grey_stripes,
		vodonos
	}

	public enum PointMarkerShape
	{
		none = 0, 
		// < 6 бутылей
		circle,
		// 6 - 10 бутылей
		triangle,
		// 10 - 20 бутылей
		square,
		// 20 - 40 бутылей
		cross,
		// > 40 бутылей
		star,
		//без формы
		custom
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
				if (type != PointMarkerType.none && shape != PointMarkerShape.none)
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

		private PointMarkerShape shape;
		public PointMarkerShape Shape {
			get => shape;
			set {
				shape = value;
				if(shape != PointMarkerShape.none && type != PointMarkerType.none)
					LoadBitmap();				

				if(IsVisible
				   && Overlay != null && Overlay.Control != null
				   && !Overlay.Control.HoldInvalidation)
					Overlay.Control.Invalidate();
			}
		}

		public PointMarker(PointLatLng p, PointMarkerType type)
			: base(p)
		{
			this.Type = type;
		}

		public PointMarker(PointLatLng p, PointMarkerType type, PointMarkerShape shape)
			: this(p, type)
		{
			this.Shape = shape;
		}

		void LoadBitmap()
		{
			string iconPath = string.Join(".", Shape.ToString(), Type.ToString());
			Bitmap = GetIcon(iconPath);
			Size = new Size(Bitmap.Width, Bitmap.Height);

			Offset = new Point(-Size.Width / 2, -Size.Height + 1);

			string shadowPath = string.Join(".", Shape.ToString(), "marker_shadow");
			BitmapShadow = GetIcon(shadowPath);
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

		public static Gdk.Pixbuf GetIconPixbuf(string name, PointMarkerShape shape = PointMarkerShape.circle)
		{
			string resourceName = String.Format("Vodovoz.icons.map.points.{0}.{1}.png", shape.ToString(), name);
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
			info.AddValue("shape", this.shape);

			base.GetObjectData(info, context);
		}

		protected PointMarker(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			this.type = Extensions.GetStruct<PointMarkerType>(info, "type", PointMarkerType.none);
			this.Shape = Extensions.GetStruct<PointMarkerShape>(info, "shape", PointMarkerShape.none);
		}

		#endregion

		#region IDeserializationCallback Members

		public void OnDeserialization(object sender)
		{
			if (type != PointMarkerType.none && Shape != PointMarkerShape.none)
			{
				LoadBitmap();
			}
		}

		#endregion
	}
}

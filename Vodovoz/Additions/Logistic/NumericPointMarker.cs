using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.Serialization;
using GMap.NET;
using GMap.NET.GtkSharp;

namespace Vodovoz.Additions.Logistic
{
	public enum NumericPointMarkerType
	{
		none = 0,
		green_large,
		white_large
	}

	[Serializable]
	public class NumericPointMarker : GMapMarker, ISerializable, IDeserializationCallback
	{
		Bitmap Bitmap;
		Bitmap BitmapShadow;

		private NumericPointMarkerType type;

		public NumericPointMarkerType Type
		{
			get
			{
				return type;
			}
			set
			{
				type = value;
				if (type != NumericPointMarkerType.none)
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

		public int Number { get; set;}

		public NumericPointMarker(PointLatLng p, NumericPointMarkerType type, int number)
			: base(p)
		{
			this.Type = type;
			this.Number = number;
		}

		void LoadBitmap()
		{
			Bitmap = GetIcon(type.ToString());
			Size = new System.Drawing.Size(Bitmap.Width, Bitmap.Height);

			Offset = new Point(-Size.Width / 2, -Size.Height + 1);

			BitmapShadow = GetIcon("large_marker_shadow");
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

			using (Font font1 = new Font("Arial", 9, FontStyle.Bold, GraphicsUnit.Point))
			{
				Rectangle rect1 = new Rectangle(LocalPosition.X, LocalPosition.Y, Size.Width - 2, Size.Height / 2);

				StringFormat stringFormat = new StringFormat();
				stringFormat.Alignment = StringAlignment.Center;
				stringFormat.LineAlignment = StringAlignment.Center;

				g.DrawString(Number.ToString(), font1, Brushes.Blue, rect1, stringFormat);
			}
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

		protected NumericPointMarker(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			this.type = Extensions.GetStruct<NumericPointMarkerType>(info, "type", NumericPointMarkerType.none);
		}

		#endregion

		#region IDeserializationCallback Members

		public void OnDeserialization(object sender)
		{
			if (type != NumericPointMarkerType.none)
			{
				LoadBitmap();
			}
		}

		#endregion
	}
}

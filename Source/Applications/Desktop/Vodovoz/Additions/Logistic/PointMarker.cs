using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using GMap.NET;
using GMap.NET.GtkSharp;
using NHibernate.Util;

namespace Vodovoz.Additions.Logistic
{
	[Serializable]
	public class PointMarker : GMapMarker, ISerializable, IDeserializationCallback
	{
		Bitmap Bitmap;
		Bitmap BitmapShadow;
		Bitmap _bitmapLogisticsRequirements;

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

		private PointMarkerShape? _logisticsRequirementsMarkerShape;
		public PointMarkerShape? LogisticsRequirementsMarkerShape
		{
			get { return _logisticsRequirementsMarkerShape; }
			set { _logisticsRequirementsMarkerShape = value; }
		}

		private PointMarkerType? _logisticsRequirementsMarkerType;

		public PointMarkerType? LogisticsRequirementsMarkerType
		{
			get { return _logisticsRequirementsMarkerType; }
			set { _logisticsRequirementsMarkerType = value; }
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

		public PointMarker(PointLatLng p, PointMarkerType type, PointMarkerShape shape, PointMarkerType logisticsRequirementsType, PointMarkerShape logisticsRequirementsShape)
			: this(p, type)
		{
			LogisticsRequirementsMarkerShape = logisticsRequirementsShape;
			LogisticsRequirementsMarkerType = logisticsRequirementsType;
			Shape = shape;
		}

		void LoadBitmap()
		{
			string iconPath = string.Join(".", Shape.ToString(), Type.ToString());
			Bitmap = GetIcon(iconPath);
			Size = new Size(Bitmap.Width, Bitmap.Height);

			Offset = new Point(-Size.Width / 2, -Size.Height + 1);

			string shadowPath = string.Join(".", Shape.ToString(), "marker_shadow");
			BitmapShadow = GetIcon(shadowPath);

			if (LogisticsRequirementsMarkerShape != null 
				&& LogisticsRequirementsMarkerType != null 
				&& LogisticsRequirementsMarkerShape != PointMarkerShape.none 
				&& LogisticsRequirementsMarkerType != PointMarkerType.none)
			{
				string logisticsRequirementsIconPath = string.Join(".", LogisticsRequirementsMarkerShape.ToString(), LogisticsRequirementsMarkerType.ToString());
				_bitmapLogisticsRequirements = GetIcon(logisticsRequirementsIconPath);
			}
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
			if(_bitmapLogisticsRequirements != null)
			{
				g.DrawImage(
					_bitmapLogisticsRequirements, 
					LocalPosition.X + Size.Width, 
					LocalPosition.Y - Size.Height + 8, 
					_bitmapLogisticsRequirements.Width, 
					_bitmapLogisticsRequirements.Height
					);
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
			this.type = GMap.NET.Extensions.GetStruct<PointMarkerType>(info, "type", PointMarkerType.none);
			this.Shape = GMap.NET.Extensions.GetStruct<PointMarkerShape>(info, "shape", PointMarkerShape.none);
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

using GMap.NET;
using GMap.NET.GtkSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.Serialization;

namespace Vodovoz.Additions.Logistic
{
	[Serializable]
	public class PointMarker : GMapMarker, ISerializable, IDeserializationCallback
	{
		private static readonly Dictionary<string, Bitmap> _iconCache = new Dictionary<string, Bitmap>();

		private Bitmap _mainMarker;
		private Bitmap _shadowMarker;
		private Bitmap _logisticsRequirementsMarker;
		private Bitmap _orderInfoMarker;

		private PointMarkerType _mainMarkerType;
		private PointMarkerShape _mainMarkerShape;
		private PointMarkerType? _orderInfoMarkerType;
		private PointMarkerShape? _orderInfoMarkerShape;
		private PointMarkerType? _logisticsRequirementsMarkerType;
		private PointMarkerShape? _logisticsRequirementsMarkerShape;

		public PointMarker(PointLatLng p, PointMarkerType mainMarkerType)
			: base(p)
		{
			MainMarkerType = mainMarkerType;
		}

		public PointMarker(PointLatLng p, PointMarkerType mainMarkerType, PointMarkerShape mainMarkerShape)
			: this(p, mainMarkerType)
		{
			MainMarkerShape = mainMarkerShape;
		}

		public PointMarkerType MainMarkerType
		{
			get => _mainMarkerType;
			set
			{
				_mainMarkerType = value;

				if(_mainMarkerType != PointMarkerType.none && _mainMarkerShape != PointMarkerShape.none)
				{
					SetMainAndShadowMarkers();
				}

				if(IsVisible)
				{
					if(Overlay != null && Overlay.Control != null)
					{
						if(!Overlay.Control.HoldInvalidation)
						{
							Overlay.Control.Invalidate();
						}
					}
				}
			}
		}

		public PointMarkerShape MainMarkerShape
		{
			get => _mainMarkerShape;
			set
			{
				_mainMarkerShape = value;
				if(_mainMarkerShape != PointMarkerShape.none && _mainMarkerType != PointMarkerType.none)
				{
					SetMainAndShadowMarkers();
				}

				if(IsVisible
				   && Overlay != null && Overlay.Control != null
				   && !Overlay.Control.HoldInvalidation)
				{
					Overlay.Control.Invalidate();
				}
			}
		}

		public PointMarkerType? LogisticsRequirementsMarkerType
		{
			get => _logisticsRequirementsMarkerType;
			set
			{
				_logisticsRequirementsMarkerType = value;
				SetLogisticsRequirementsMarker();
			}
		}

		public PointMarkerShape? LogisticsRequirementsMarkerShape
		{
			get => _logisticsRequirementsMarkerShape;
			set
			{
				_logisticsRequirementsMarkerShape = value;
				SetLogisticsRequirementsMarker();
			}
		}

		public PointMarkerType? OrderInfoMarkerType
		{
			get => _orderInfoMarkerType;
			set
			{
				_orderInfoMarkerType = value;
				SetOrderInfoMarker();
			}
		}

		public PointMarkerShape? OrderInfoMarkerShape
		{
			get => _orderInfoMarkerShape;
			set
			{
				_orderInfoMarkerShape = value;
				SetOrderInfoMarker();
			}
		}

		private void SetMainAndShadowMarkers()
		{
			SetMainMarker();
			SetShadowMarker();
		}

		private void SetMainMarker()
		{
			var iconPath = string.Join(".", MainMarkerShape.ToString(), MainMarkerType.ToString());

			_mainMarker = GetIcon(iconPath);

			Size = new Size(_mainMarker.Width, _mainMarker.Height);
			Offset = new Point(-Size.Width / 2, -Size.Height + 1);
		}

		private void SetShadowMarker()
		{
			var shadowPath = string.Join(".", MainMarkerShape.ToString(), "marker_shadow");
			_shadowMarker = GetIcon(shadowPath);
		}

		private void SetLogisticsRequirementsMarker()
		{
			if(LogisticsRequirementsMarkerShape == null
				|| LogisticsRequirementsMarkerShape == PointMarkerShape.none
				|| LogisticsRequirementsMarkerType == null
				|| LogisticsRequirementsMarkerType == PointMarkerType.none)
			{
				_logisticsRequirementsMarker = null;

				return;
			}

			var logisticsRequirementsIconPath = string.Join(
				 ".",
				 LogisticsRequirementsMarkerShape.ToString(),
				 LogisticsRequirementsMarkerType.ToString());

			_logisticsRequirementsMarker = GetIcon(logisticsRequirementsIconPath);
		}

		private void SetOrderInfoMarker()
		{
			if(OrderInfoMarkerShape == null || OrderInfoMarkerShape == PointMarkerShape.none
				|| OrderInfoMarkerType == null || OrderInfoMarkerType == PointMarkerType.none)
			{
				_orderInfoMarker = null;

				return;
			}

			var orderInfoIconPath = string.Join(".", OrderInfoMarkerShape.ToString(), OrderInfoMarkerType.ToString());
			_orderInfoMarker = GetIcon(orderInfoIconPath);
		}

		internal static Bitmap GetIcon(string name)
		{
			if(!_iconCache.TryGetValue(name, out Bitmap ret))
			{
				string resourceName = string.Format("Vodovoz.icons.map.points.{0}.png", name);
				ret = new Bitmap(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName));
				_iconCache.Add(name, ret);
			}

			return ret;
		}

		public static Gdk.Pixbuf GetIconPixbuf(string name, PointMarkerShape shape = PointMarkerShape.circle)
		{
			var resourceName = string.Format("Vodovoz.icons.map.points.{0}.{1}.png", shape.ToString(), name);
			return new Gdk.Pixbuf(System.Reflection.Assembly.GetExecutingAssembly(), resourceName);
		}

		public override void OnRender(Graphics g)
		{
			if(_shadowMarker != null)
			{
				g.DrawImage(_shadowMarker, LocalPosition.X, LocalPosition.Y, _shadowMarker.Width, _shadowMarker.Height);
			}
			if(_logisticsRequirementsMarker != null)
			{
				g.DrawImage(
					_logisticsRequirementsMarker,
					LocalPosition.X + Size.Width + 1,
					LocalPosition.Y - Size.Height + 8,
					_logisticsRequirementsMarker.Width,
					_logisticsRequirementsMarker.Height
					);
			}
			if(_orderInfoMarker != null)
			{
				g.DrawImage(
					_orderInfoMarker,
					LocalPosition.X - _orderInfoMarker.Width - 1,
					LocalPosition.Y - Size.Height + 8,
					_orderInfoMarker.Width,
					_orderInfoMarker.Height
					);
			}
			g.DrawImage(_mainMarker, LocalPosition.X, LocalPosition.Y, Size.Width, Size.Height);
		}

		public override void Dispose()
		{
			if(_mainMarker != null)
			{
				if(!_iconCache.ContainsValue(_mainMarker))
				{
					_mainMarker.Dispose();
					_mainMarker = null;
				}
			}

			if(_shadowMarker != null)
			{
				if(!_iconCache.ContainsValue(_shadowMarker))
				{
					_shadowMarker.Dispose();
					_shadowMarker = null;
				}
			}

			if(_logisticsRequirementsMarker != null)
			{
				if(!_iconCache.ContainsValue(_logisticsRequirementsMarker))
				{
					_logisticsRequirementsMarker.Dispose();
					_logisticsRequirementsMarker = null;
				}
			}

			if(_orderInfoMarker != null)
			{
				if(!_iconCache.ContainsValue(_orderInfoMarker))
				{
					_orderInfoMarker.Dispose();
					_orderInfoMarker = null;
				}
			}

			base.Dispose();
		}

		#region ISerializable Members

		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("type", _mainMarkerType);
			info.AddValue("shape", _mainMarkerShape);

			base.GetObjectData(info, context);
		}

		protected PointMarker(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			MainMarkerType = GMap.NET.Extensions.GetStruct<PointMarkerType>(info, "type", PointMarkerType.none);
			MainMarkerShape = GMap.NET.Extensions.GetStruct<PointMarkerShape>(info, "shape", PointMarkerShape.none);
		}

		#endregion

		#region IDeserializationCallback Members

		public void OnDeserialization(object sender)
		{
			if(_mainMarkerType != PointMarkerType.none && MainMarkerShape != PointMarkerShape.none)
			{
				SetMainAndShadowMarkers();
			}
		}

		#endregion
	}
}

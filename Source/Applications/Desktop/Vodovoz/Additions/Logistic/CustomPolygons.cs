using GMap.NET;
using GMap.NET.GtkSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using Vodovoz.Core.Domain;
using Vodovoz.Tools.Logistic;

namespace Vodovoz.Additions.Logistic
{
	public class CustomPolygons
	{
		public const int _defaultCirclePointsCount = 36;
		public const int _defaultBorderWidth = 1;
		public const int _defaultFillAlpha = 30;

		/// <summary>
		/// Создает полигон, близкий к окружности
		/// </summary>
		/// <param name="center">координаты центра окружности</param>
		/// <param name="radiusInKilometers">Радиус окружности в километрах</param>
		/// <param name="segmentsPointsCount">Количество точек окружности (чем больше, тем плавнее линия)</param>
		/// <param name="borderWidth">Толщина линии контура</param>
		/// <param name="color">Цвет линии контура и заливки</param>
		/// <param name="fillAlpha">Прозрачноть заливки (от 0-прозрачный до 255-непрозрачный)</param>
		public static GMapPolygon CreateCirclePolygon(
			PointLatLng center,
			double radiusInKilometers,
			Color color,
			int segmentsPointsCount = _defaultCirclePointsCount,
			int borderWidth = _defaultBorderWidth,
			int fillAlpha = _defaultFillAlpha)
		{
			List<PointLatLng> pointList = new List<PointLatLng>();

			var twoPi = Math.PI * 2;

			for(double radian = 0; radian < twoPi; radian += twoPi / segmentsPointsCount)
			{
				pointList.Add(DistanceCalculator.FindPointByDistanceAndRadians(center, radian, radiusInKilometers));
			}

			return new GMapPolygon(pointList, "Circle")
			{
				Stroke = new Pen(color, borderWidth),
				Fill = new SolidBrush(Color.FromArgb(fillAlpha, color))
			};
		}
	}
}

using GMap.NET;
using GMap.NET.GtkSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using Vodovoz.Tools.Logistic;

namespace Vodovoz.Additions.Logistic
{
	public class CustomPolygons
	{
		/// <summary>
		/// Рисует окружность по расчитанным точкам
		/// </summary>
		/// <param name="overlay">Оверлей, на который выводится окружность</param>
		/// <param name="centerLat">Широта центра окружности</param>
		/// <param name="centerLon">Долгота центра окружности</param>
		/// <param name="radiusInKm">Радиус окружности в километрах</param>
		/// <param name="segmentsPointsCount">Количество точек окружности (чем больше, тем плавнее линия)</param>
		/// <param name="borderWidth">Толщина линии контура</param>
		/// <param name="color">Цвет линии контура и заливки</param>
		/// <param name="fillAlpha">Прозрачноть заливки (от 0-прозрачный до 255-непрозрачный)</param>
		public static void CreateRoundPolygon(GMapOverlay overlay, Double centerLat, Double centerLon, double radiusInKm, int segmentsPointsCount, int borderWidth, Color color, int fillAlpha)
		{
			PointLatLng centerPoint = new PointLatLng(centerLat, centerLon);
			List<PointLatLng> pointList = new List<PointLatLng>();

			var twoPi = 2 * Math.PI;

			for(double radian = 0; radian < twoPi; radian += twoPi / segmentsPointsCount)
			{
				pointList.Add(DistanceCalculator.FindPointByDistanceAndRadians(centerPoint, radian, radiusInKm));
			}

			GMapPolygon polygon = new GMapPolygon(pointList, "Circle")
			{
				Stroke = new Pen(color, borderWidth),
				Fill = new SolidBrush(Color.FromArgb(fillAlpha, color))
			};

			overlay.Polygons.Add(polygon);
		}
	}
}

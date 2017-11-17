using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GMap.NET;
using GMap.NET.GtkSharp;
using GMap.NET.GtkSharp.Markers;
using GMap.NET.MapProviders;
using NetTopologySuite.Geometries;
using QSOrmProject;
using QSOsm;
using QSOsm.DTO;
using QSOsm.Osrm;
using QSProjectsLib;
using Vodovoz.Domain.Logistic;
using Vodovoz.Repository.Logistics;

namespace Vodovoz.Dialogs.Sale
{
	public partial class DeliveryPriceDlg : QSTDI.TdiTabBase
	{
		private Gtk.Clipboard clipboard = Gtk.Clipboard.Get(Gdk.Atom.Intern("CLIPBOARD", false));

		readonly GMapOverlay addressOverlay = new GMapOverlay();
		GMapMarker addressMarker;
		//RouteGeometryCalculator routeCal = new Tools.Logistic.RouteGeometryCalculator(Tools.Logistic.DistanceProvider.Osrm);
		IList<LogisticsArea> districts;
		double fuelCost;

		decimal? Latitude;
		decimal? Longitude;
		double? distance;

		public double? Distance { 
			get => distance; 
			set{ 
				distance = value;
				ylabelDistance.LabelProp = distance.HasValue ? distance.Value.ToString("N1") + " км." : "Нет";
				CalculatePrice();
			}}

		public DeliveryPriceDlg()
		{
			this.Build();

			TabName = "Рассчет стоимости доставки";

			entryCity.CitySelected += (sender, e) => {
				entryBuilding.Text = string.Empty;
				entryStreet.CityId = entryCity.OsmId;
				entryStreet.Street = string.Empty;
				entryStreet.StreetDistrict = string.Empty;
			};

			entryStreet.StreetSelected += (sender, e) => {
				entryBuilding.Street = new OsmStreet(-1, entryStreet.CityId, entryStreet.Street, entryStreet.StreetDistrict);
			};

			entryBuilding.CompletionLoaded += EntryBuilding_Changed;
			entryBuilding.Changed += EntryBuilding_Changed;

			//Configure map
			MapWidget.MapProvider = GMapProviders.OpenStreetMap;
			MapWidget.Position = new PointLatLng(59.93900, 30.31646);
			MapWidget.MinZoom = 0;
			MapWidget.MaxZoom = 24;
			MapWidget.Zoom = 9;
			MapWidget.WidthRequest = 450;
			MapWidget.HasFrame = true;
			MapWidget.Overlays.Add(addressOverlay);

			//Получаем стоимость литра
			var uow = UnitOfWorkFactory.CreateWithoutRoot();
			var fuel = FuelTypeRepository.GetDefaultFuel(uow);
			if(fuel == null)
			{
				FailInitialize = true;
				MessageDialogWorks.RunErrorDialog("Топливо по умолчанию «АИ-92» не найдено в справочке. Работа с диалогом не может быть продолжена.");
				return;
			}
			fuelCost = (double)fuel.Cost;
			districts = LogisticAreaRepository.AreaWithGeometry(uow);
		}

		void EntryBuilding_Changed(object sender, EventArgs e)
		{
			if(entryBuilding.OsmCompletion.HasValue) {
				decimal? latitude, longitude;
				entryBuilding.GetCoordinates(out longitude, out latitude);
				SetCoordinates(latitude, longitude);
			}
		}

		protected void OnButtonInsertFromBufferClicked(object sender, EventArgs e)
		{
			bool error = true;

			string booferCoordinates = clipboard.WaitForText();

			string[] coordinates = booferCoordinates?.Split(',');
			if(coordinates?.Length == 2) {
				decimal latitude, longitude;
				bool goodLat = decimal.TryParse(coordinates[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out latitude);
				bool goodLon = decimal.TryParse(coordinates[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out longitude);

				if(goodLat && goodLon) {
					SetCoordinates(latitude, longitude);
					error = false;
				}
			}
			if(error)
				MessageDialogWorks.RunErrorDialog(
					"Буфер обмена не содержит координат или содержит неправильные координаты");
		}

		private void SetCoordinates(decimal? latitude, decimal? longitude)
		{
			Latitude = latitude;
			Longitude = longitude;

			if(addressMarker != null) {
				addressOverlay.Markers.Clear();
				addressMarker = null;
			}

			if(Latitude.HasValue && Longitude.HasValue) {
				addressMarker = new GMarkerGoogle(new PointLatLng((double)Latitude.Value, (double)Longitude.Value),
					GMarkerGoogleType.arrow);
				addressOverlay.Markers.Add(addressMarker);

				var position = new PointLatLng((double)Latitude.Value, (double)Longitude.Value);
				MapWidget.Position = position;
				MapWidget.Zoom = 15;

				ylabelFoundOnOsm.LabelProp = String.Format("(ш. {0:F5}, д. {1:F5})", Latitude, Longitude);
			}
			else
			{
				MapWidget.Position = new PointLatLng(59.93900, 30.31646);
				MapWidget.Zoom = 9;
				ylabelFoundOnOsm.LabelProp = "нет координат";
			}

			if(Latitude == null || Longitude == null) {
				Distance = null;
				return;
			}

			var route = new List<PointOnEarth>(2);
			route.Add(new PointOnEarth(Constants.CenterOfCityLatitude, Constants.CenterOfCityLongitude));
			route.Add(new PointOnEarth(Latitude.Value, Longitude.Value));

			var result = OsrmMain.GetRoute(route, false, GeometryOverview.False);
			if(result.Code != "Ok") {
				MessageDialogWorks.RunErrorDialog("Сервер расчета расстояний вернул следующее сообщение:\n" + result.StatusMessageRus);
				return;
			}

			Distance = result.Routes[0].TotalDistance / 1000d;
		}

		void CalculatePrice()
		{
			if(Distance == null) {
				labelPrice.LabelProp = "Не рассчитана";
				return;
			}

			var point = new Point((double)Latitude, (double)Longitude);
			if(districts.Where(x => x.IsCity).Any(x => x.Geometry.Contains(point))) {
				labelPrice.LabelProp = "Это город! Расчет по прайсу";
				return;
			}

			//((а * 2/100)*20*б)/в+110
			//а - расстояние от границы города минус
			//б - стоимость литра топлива(есть в справочниках)
			//в - кол-во бут
			var price = ((Distance.Value * 2 / 100) * 20 * fuelCost) / yspinBottles.Value + 120;
			labelPrice.LabelProp = price.ToString("N2");
		}

		protected void OnYspinBottlesValueChanged(object sender, EventArgs e)
		{
			CalculatePrice();
		}
	}
}

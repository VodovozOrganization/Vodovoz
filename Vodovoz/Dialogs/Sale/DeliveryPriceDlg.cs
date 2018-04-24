using System;
using System.Globalization;
using GMap.NET;
using GMap.NET.GtkSharp;
using GMap.NET.GtkSharp.Markers;
using GMap.NET.MapProviders;
using QSOsm.DTO;
using QSProjectsLib;
using Vodovoz.Tools.Logistic;

namespace Vodovoz.Dialogs.Sale
{
	public partial class DeliveryPriceDlg : QSTDI.TdiTabBase
	{
		private Gtk.Clipboard clipboard = Gtk.Clipboard.Get(Gdk.Atom.Intern("CLIPBOARD", false));

		readonly GMapOverlay addressOverlay = new GMapOverlay();
		GMapMarker addressMarker;
		decimal? latitude;
		decimal? longitude;

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

			deliverypriceview.OnError += (sender, e) => {
				MessageDialogWorks.RunErrorDialog(e);
			};
		}

		void EntryBuilding_Changed(object sender, EventArgs e)
		{
			if(entryBuilding.OsmCompletion.HasValue && entryBuilding.OsmCompletion.Value) {
				decimal? lat, lng;
				entryBuilding.GetCoordinates(out lng, out lat);
				SetCoordinates(lat, lng);
				deliverypriceview.DeliveryPrice = DeliveryPriceCalculator.Calculate(latitude, longitude, yspinBottles.ValueAsInt);
			}
		}

		protected void OnButtonInsertFromBufferClicked(object sender, EventArgs e)
		{
			bool error = true;

			string booferCoordinates = clipboard.WaitForText();

			string[] coordinates = booferCoordinates?.Split(',');
			if(coordinates?.Length == 2) {
				decimal lat, lng;
				bool goodLat = decimal.TryParse(coordinates[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out lat);
				bool goodLon = decimal.TryParse(coordinates[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out lng);
				SetCoordinates(lat, lng);

				if(goodLat && goodLon) {
					deliverypriceview.DeliveryPrice = DeliveryPriceCalculator.Calculate(latitude, longitude, yspinBottles.ValueAsInt);
					error = false;
				}
			}
			if(error)
				MessageDialogWorks.RunErrorDialog(
					"Буфер обмена не содержит координат или содержит неправильные координаты");
		}

		private void SetCoordinates(decimal? lat, decimal? lng)
		{
			latitude = lat;
			longitude = lng;

			if(addressMarker != null) {
				addressOverlay.Markers.Clear();
				addressMarker = null;
			}

			if(latitude.HasValue && longitude.HasValue) {
				addressMarker = new GMarkerGoogle(new PointLatLng((double)latitude.Value, (double)longitude.Value),
					GMarkerGoogleType.arrow);
				addressOverlay.Markers.Add(addressMarker);

				var position = new PointLatLng((double)latitude.Value, (double)longitude.Value);
				MapWidget.Position = position;
				MapWidget.Zoom = 15;

				ylabelFoundOnOsm.LabelProp = String.Format("(ш. {0:F5}, д. {1:F5})", latitude, longitude);
			}
			else
			{
				MapWidget.Position = new PointLatLng(59.93900, 30.31646);
				MapWidget.Zoom = 9;
				ylabelFoundOnOsm.LabelProp = "нет координат";
			}
		}

		protected void OnYspinBottlesValueChanged(object sender, EventArgs e)
		{
			deliverypriceview.DeliveryPrice = DeliveryPriceCalculator.Calculate(latitude, longitude, yspinBottles.ValueAsInt);
		}
	}
}

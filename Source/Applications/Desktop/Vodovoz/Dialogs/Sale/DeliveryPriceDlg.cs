using System;
using System.Globalization;
using GMap.NET;
using GMap.NET.GtkSharp;
using GMap.NET.GtkSharp.Markers;
using GMap.NET.MapProviders;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using Vodovoz.Additions.Logistic;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Sale;
using Vodovoz.Tools.Logistic;

namespace Vodovoz.Dialogs.Sale
{
	public partial class DeliveryPriceDlg : QS.Dialog.Gtk.TdiTabBase
	{
		private Gtk.Clipboard _clipboard = Gtk.Clipboard.Get(Gdk.Atom.Intern("CLIPBOARD", false));
		private readonly GMapOverlay _addressOverlay = new GMapOverlay();
		private readonly IDeliveryPriceCalculator _deliveryPriceCalculator;
		private GMapMarker _addressMarker;
		private decimal? _latitude;
		private decimal? _longitude;
		private IUnitOfWork _unitOfWork;

		public DeliveryPriceDlg(IUnitOfWorkFactory unitOfWorkFactory, IDeliveryPriceCalculator deliveryPriceCalculator)
		{
			_deliveryPriceCalculator = deliveryPriceCalculator ?? throw new ArgumentNullException(nameof(deliveryPriceCalculator));

			Build();

			TabName = "Расчет стоимости доставки";

			_unitOfWork = unitOfWorkFactory.CreateWithoutRoot(TabName);
			_unitOfWork.Session.DefaultReadOnly = true;

			entryCity.CitySelected += (sender, e) => {
				entryStreet.CityGuid = entryCity.FiasGuid;
				entryStreet.StreetTypeName = string.Empty;
				entryStreet.StreetTypeNameShort = string.Empty;
				entryStreet.StreetName = string.Empty;
				entryStreet.StreetDistrict = string.Empty;
				entryStreet.FireStreetChange();
				entryBuilding.StreetGuid = null;
				entryBuilding.CityGuid = entryCity.FiasGuid;
				entryBuilding.BuildingName = string.Empty;
			};

			entryStreet.StreetSelected += (sender, e) =>
			{
				entryBuilding.StreetGuid = entryStreet.FiasGuid;
			};

			entryBuilding.CompletionLoaded += EntryBuilding_Changed;
			entryBuilding.Changed += EntryBuilding_Changed;

			//Configure map
			MapWidget.MapProvider = GMapProviders.GoogleMap;
			MapWidget.Position = new PointLatLng(59.93900, 30.31646);
			MapWidget.MinZoom = 0;
			MapWidget.MaxZoom = 24;
			MapWidget.Zoom = 9;
			MapWidget.WidthRequest = 450;
			MapWidget.HasFrame = true;
			MapWidget.Overlays.Add(_addressOverlay);

			yenumcomboMapType.ItemsEnum = typeof(MapProviders);
			yenumcomboMapType.EnumItemSelected += (sender, args) =>
				MapWidget.MapProvider = MapProvidersHelper.GetPovider((MapProviders)args.SelectedItem);
		}

		public DeliveryPriceDlg(IUnitOfWorkFactory unitOfWorkFactory, IDeliveryPriceCalculator deliveryPriceCalculator, DeliveryPoint deliveryPoint) : this(unitOfWorkFactory, deliveryPriceCalculator)
		{
			SetCoordinates(deliveryPoint.Latitude, deliveryPoint.Longitude);
			deliverypriceview.DeliveryPrice = _deliveryPriceCalculator.Calculate(_latitude, _longitude, yspinBottles.ValueAsInt);
		}

		private void EntryBuilding_Changed(object sender, EventArgs e)
		{
			if(entryBuilding.FiasCompletion.HasValue && entryBuilding.FiasCompletion.Value) {
				entryBuilding.GetCoordinates(out decimal? lng, out decimal? lat);
				SetCoordinates(lat, lng);
				deliverypriceview.DeliveryPrice = _deliveryPriceCalculator.Calculate(_latitude, _longitude, yspinBottles.ValueAsInt);
			}
		}

		protected void OnButtonInsertFromBufferClicked(object sender, EventArgs e)
		{
			bool error = true;

			string booferCoordinates = _clipboard.WaitForText();

			string[] coordinates = booferCoordinates?.Split(',');
			if(coordinates?.Length == 2) {
				bool goodLat = decimal.TryParse(coordinates[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out decimal lat);
				bool goodLon = decimal.TryParse(coordinates[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out decimal lng);
				SetCoordinates(lat, lng);

				if(goodLat && goodLon) {
					var price = _deliveryPriceCalculator.Calculate(_latitude, _longitude, yspinBottles.ValueAsInt);
					deliverypriceview.District = _unitOfWork.GetById<District>(price.DistrictId);
					deliverypriceview.DeliveryPrice = price;
					error = false;
				}
			}
			if(error)
			{
				MessageDialogHelper.RunErrorDialog(
					"Буфер обмена не содержит координат или содержит неправильные координаты");
			}
		}

		private void SetCoordinates(decimal? lat, decimal? lng)
		{
			_latitude = lat;
			_longitude = lng;

			if(_addressMarker != null) {
				_addressOverlay.Markers.Clear();
				_addressMarker = null;
			}

			if(_latitude.HasValue && _longitude.HasValue) {
				_addressMarker = new GMarkerGoogle(new PointLatLng((double)_latitude.Value, (double)_longitude.Value),
					GMarkerGoogleType.arrow);
				_addressOverlay.Markers.Add(_addressMarker);

				var position = new PointLatLng((double)_latitude.Value, (double)_longitude.Value);
				MapWidget.Position = position;
				MapWidget.Zoom = 15;

				ylabelFoundOnOsm.LabelProp = $"(ш. {_latitude:F5}, д. {_longitude:F5})";
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
			deliverypriceview.DeliveryPrice = _deliveryPriceCalculator.Calculate(_latitude, _longitude, yspinBottles.ValueAsInt);
		}
	}
}

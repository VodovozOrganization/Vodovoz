using Fias.Client.Loaders;
using GeoCoderApi.Client;
using GMap.NET;
using GMap.NET.GtkSharp;
using GMap.NET.GtkSharp.Markers;
using GMap.NET.MapProviders;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using System;
using System.Globalization;
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
		private readonly IGeoCoderApiClient _geoCoderApiClient;
		private GMapMarker _addressMarker;
		private decimal? _latitude;
		private decimal? _longitude;
		private IUnitOfWork _unitOfWork;

		public DeliveryPriceDlg(
			IUnitOfWorkFactory unitOfWorkFactory,
			IDeliveryPriceCalculator deliveryPriceCalculator,
			ICitiesDataLoader citiesDataLoader,
			IStreetsDataLoader streetsDataLoader,
			IHousesDataLoader housesDataLoader,
			IGeoCoderApiClient geoCoderApiClient)
		{
			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			if(citiesDataLoader is null)
			{
				throw new ArgumentNullException(nameof(citiesDataLoader));
			}

			if(streetsDataLoader is null)
			{
				throw new ArgumentNullException(nameof(streetsDataLoader));
			}

			if(housesDataLoader is null)
			{
				throw new ArgumentNullException(nameof(housesDataLoader));
			}

			_deliveryPriceCalculator = deliveryPriceCalculator ?? throw new ArgumentNullException(nameof(deliveryPriceCalculator));
			_geoCoderApiClient = geoCoderApiClient ?? throw new ArgumentNullException(nameof(geoCoderApiClient));

			Build();

			TabName = "Расчет стоимости доставки";

			_unitOfWork = unitOfWorkFactory.CreateWithoutRoot(TabName);
			_unitOfWork.Session.DefaultReadOnly = true;

			entryCity.CitiesDataLoader = citiesDataLoader;
			entryStreet.StreetsDataLoader = streetsDataLoader;
			entryBuilding.HousesDataLoader = housesDataLoader;

			entryCity.CitySelected += EntryCityOnCitySelected;
			entryStreet.StreetSelected += EntryStreetOnStreetSelected;
			entryBuilding.FocusOutEvent += EntryBuildingOnFocusOutEvent;

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

		public DeliveryPriceDlg(
			IUnitOfWorkFactory unitOfWorkFactory,
			IDeliveryPriceCalculator deliveryPriceCalculator,
			DeliveryPoint deliveryPoint,
			ICitiesDataLoader citiesDataLoader,
			IStreetsDataLoader streetsDataLoader,
			IHousesDataLoader housesDataLoader,
			IGeoCoderApiClient geoCoderApiClient)
			: this(unitOfWorkFactory, deliveryPriceCalculator, citiesDataLoader, streetsDataLoader, housesDataLoader, geoCoderApiClient)
		{
			SetCoordinates(deliveryPoint.Latitude, deliveryPoint.Longitude);
			UpdatePriceData();
		}

		private void EntryStreetOnStreetSelected(object sender, EventArgs e)
		{
			entryBuilding.StreetGuid = entryStreet.FiasGuid;
			entryBuilding.BuildingName = string.Empty;
		}

		private void EntryCityOnCitySelected(object sender, EventArgs e)
		{
			ClearStreet();
			ClearBuilding();

			entryStreet.CityGuid = entryCity.FiasGuid;
			entryStreet.FireStreetChange();
			entryBuilding.CityGuid = entryCity.FiasGuid;
		}

		private void ClearCity()
		{
			entryCity.FiasGuid = null;
			entryCity.CityName = string.Empty;
			entryCity.Text = string.Empty;
		}

		private void ClearBuilding()
		{
			entryStreet.CityGuid = null;
			entryStreet.StreetTypeName = string.Empty;
			entryStreet.StreetTypeNameShort = string.Empty;
			entryStreet.StreetName = string.Empty;
			entryStreet.StreetDistrict = string.Empty;
			entryStreet.FireStreetChange();
		}

		private void ClearStreet()
		{
			entryBuilding.StreetGuid = null;
			entryBuilding.CityGuid = null;
			entryBuilding.BuildingName = string.Empty;
		}

		private void EntryBuildingOnFocusOutEvent(object sender, EventArgs e)
		{
			if(entryBuilding.FiasCompletion.HasValue && entryBuilding.FiasCompletion.Value)
			{
				entryBuilding.GetCoordinates(out decimal? lng, out decimal? lat);

				if(!string.IsNullOrWhiteSpace(entryBuilding.BuildingName)
					|| lng == null
					|| lat == null)
				{
					var (Latitude, Longitude) = UpdateCoordinatesFromGeoCoder();
					lat = Latitude;
					lng = Longitude;
				}

				SetCoordinates(lat, lng);
				UpdatePriceData();
			}
		}

		public (decimal? Latitude, decimal? Longitude) UpdateCoordinatesFromGeoCoder()
		{
			decimal? latitude = null;
			decimal? longitude = null;

			var address =
				$"{entryCity.CityName}, {entryStreet.StreetName} {entryStreet.StreetTypeNameShort}, {entryBuilding.BuildingName}";

			try
			{
				var findedByGeoCoder = _geoCoderApiClient.GetCoordinateAtAddressAsync(address).GetAwaiter().GetResult();

				if(findedByGeoCoder != null)
				{
					latitude = findedByGeoCoder.Latitude;
					longitude = findedByGeoCoder.Longitude;
				}
			}
			catch(Exception ex)
			{
				MessageDialogHelper.RunErrorDialog(
					"Произошла ошибка при запросе координат в геокодере");
			}

			return (latitude, longitude);
		}

		protected void OnButtonInsertFromBufferClicked(object sender, EventArgs e)
		{
			ClearCity();
			ClearStreet();
			ClearBuilding();

			bool error = true;

			string booferCoordinates = _clipboard.WaitForText();

			string[] coordinates = booferCoordinates?.Split(',');
			if(coordinates?.Length == 2)
			{
				bool goodLat = decimal.TryParse(coordinates[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out decimal lat);
				bool goodLon = decimal.TryParse(coordinates[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out decimal lng);
				SetCoordinates(lat, lng);

				if(goodLat && goodLon)
				{
					UpdatePriceData();
					error = false;
				}
			}
			if(error)
			{
				MessageDialogHelper.RunErrorDialog(
					"Буфер обмена не содержит координат или содержит неправильные координаты");
			}
		}

		private void UpdatePriceData()
		{
			var price = _deliveryPriceCalculator.Calculate(_latitude, _longitude, yspinBottles.ValueAsInt);
			deliverypriceview.District = _unitOfWork.GetById<District>(price.DistrictId);
			deliverypriceview.DeliveryPrice = price;
		}

		private void SetCoordinates(decimal? lat, decimal? lng)
		{
			_latitude = lat;
			_longitude = lng;

			if(_addressMarker != null)
			{
				_addressOverlay.Markers.Clear();
				_addressMarker = null;
			}

			if(_latitude.HasValue && _longitude.HasValue)
			{
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

using GMap.NET;
using GMap.NET.GtkSharp;
using GMap.NET.GtkSharp.Markers;
using GMap.NET.MapProviders;
using QS.Views.Dialog;
using System;
using System.Globalization;
using Vodovoz.Additions.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.Dialogs.Sale;

namespace Vodovoz.Views.Dialogs.Sale
{
	public partial class MasterVisitCostView : DialogViewBase<MasterVisitCostViewModel>
	{
		private readonly Gtk.Clipboard _clipboard = Gtk.Clipboard.Get(Gdk.Atom.Intern("CLIPBOARD", false));
		private readonly GMapOverlay _addressOverlay = new GMapOverlay();
		private GMapMarker _addressMarker;

		public MasterVisitCostView(MasterVisitCostViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			entryCity.CitiesDataLoader = ViewModel.CitiesDataLoader;
			entryStreet.StreetsDataLoader = ViewModel.StreetsDataLoader;
			entryBuilding.HousesDataLoader = ViewModel.HousesDataLoader;

			entryCity.CitySelected += EntryCityOnCitySelected;
			entryStreet.StreetSelected += EntryStreetOnStreetSelected;
			entryBuilding.FocusOutEvent += EntryBuildingOnFocusOutEvent;

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
					var address =
						$"{entryCity.CityName}, {entryStreet.StreetName} {entryStreet.StreetTypeNameShort}, {entryBuilding.BuildingName}";
					(lat, lng) = ViewModel.FindCoordinates(address);
				}

				SetCoordinates(lat, lng);
				ViewModel.UpdatePrice();
				ShowPrice();
			}
		}

		private void OnButtonInsertFromBufferClicked(object sender, EventArgs e)
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
					ViewModel.UpdatePrice();
					ShowPrice();
					error = false;
				}
			}
			if(error)
			{
				ViewModel.ShowError("Буфер обмена не содержит координат или содержит неправильные координаты");
			}
		}

		private void ShowPrice()
		{
			deliverypriceview.Visible = ViewModel.HasPrice;
			if(!ViewModel.HasPrice)
			{
				return;
			}

			deliverypriceview.ServiceDistrict = ViewModel.ServiceDistrict;
			deliverypriceview.TypeOfAddress = OrderAddressType.Service;
			deliverypriceview.DeliveryPrice = ViewModel.Price;
		}

		private void SetCoordinates(decimal? lat, decimal? lng)
		{
			ViewModel.SetCoordinates(lat, lng);

			if(_addressMarker != null)
			{
				_addressOverlay.Markers.Clear();
				_addressMarker = null;
			}

			if(ViewModel.Latitude.HasValue && ViewModel.Longitude.HasValue)
			{
				_addressMarker = new GMarkerGoogle(
					new PointLatLng((double)ViewModel.Latitude.Value, (double)ViewModel.Longitude.Value),
					GMarkerGoogleType.arrow);
				_addressOverlay.Markers.Add(_addressMarker);

				var position = new PointLatLng((double)ViewModel.Latitude.Value, (double)ViewModel.Longitude.Value);
				MapWidget.Position = position;
				MapWidget.Zoom = 15;

				ylabelFoundOnOsm.LabelProp = $"(ш. {ViewModel.Latitude:F5}, д. {ViewModel.Longitude:F5})";
			}
			else
			{
				MapWidget.Position = new PointLatLng(59.93900, 30.31646);
				MapWidget.Zoom = 9;
				ylabelFoundOnOsm.LabelProp = "нет координат";
			}
		}
	}
}

using GMap.NET;
using GMap.NET.GtkSharp;
using GMap.NET.GtkSharp.Markers;
using GMap.NET.MapProviders;
using QS.Views.Dialog;
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

			ViewModel.ReadClipboardText = () => _clipboard.WaitForText();
			buttonInsertFromBuffer.BindCommand(ViewModel.InsertFromBufferCommand);

			entryCity.CitySelected += (sender, args) => ViewModel.SelectCity(entryCity.FiasGuid);
			entryStreet.StreetSelected += (sender, args) => ViewModel.SelectStreet(entryStreet.FiasGuid);
			entryBuilding.FocusOutEvent += (sender, args) => CompleteBuilding();

			ViewModel.CitySelectionChanged += (sender, args) => ApplyCity();
			ViewModel.StreetSelectionChanged += (sender, args) => ApplyStreet();
			ViewModel.AddressCleared += (sender, args) => ClearAddress();
			ViewModel.CoordinatesChanged += (sender, args) => ShowCoordinates();
			ViewModel.PriceChanged += (sender, args) => ShowPrice();

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

		private void CompleteBuilding()
		{
			entryBuilding.GetCoordinates(out decimal? longitude, out decimal? latitude);
			ViewModel.CompleteBuilding(
				entryCity.CityName,
				entryStreet.StreetName,
				entryStreet.StreetTypeNameShort,
				entryBuilding.BuildingName,
				entryBuilding.FiasCompletion == true,
				latitude,
				longitude);
		}

		private void ApplyCity()
		{
			ClearBuildingEntry();
			ClearStreetEntry();

			entryStreet.CityGuid = ViewModel.CityGuid;
			entryStreet.FireStreetChange();
			entryBuilding.CityGuid = ViewModel.CityGuid;
		}

		private void ApplyStreet()
		{
			entryBuilding.StreetGuid = ViewModel.StreetGuid;
			entryBuilding.BuildingName = string.Empty;
		}

		private void ClearAddress()
		{
			ClearCityEntry();
			ClearBuildingEntry();
			ClearStreetEntry();
		}

		private void ClearCityEntry()
		{
			entryCity.FiasGuid = null;
			entryCity.CityName = string.Empty;
			entryCity.Text = string.Empty;
		}

		private void ClearStreetEntry()
		{
			entryStreet.CityGuid = null;
			entryStreet.StreetTypeName = string.Empty;
			entryStreet.StreetTypeNameShort = string.Empty;
			entryStreet.StreetName = string.Empty;
			entryStreet.StreetDistrict = string.Empty;
			entryStreet.FireStreetChange();
		}

		private void ClearBuildingEntry()
		{
			entryBuilding.StreetGuid = null;
			entryBuilding.CityGuid = null;
			entryBuilding.BuildingName = string.Empty;
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

		private void ShowCoordinates()
		{
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

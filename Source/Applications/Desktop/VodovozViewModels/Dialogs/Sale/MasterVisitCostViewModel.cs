using Fias.Client.Loaders;
using GeoCoderApi.Client;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using System;
using System.Globalization;
using Vodovoz.Tools.Logistic;
using VodovozBusiness.Domain.Service;

namespace Vodovoz.ViewModels.Dialogs.Sale
{
	public class MasterVisitCostViewModel : UowDialogViewModelBase
	{
		private const string DialogTitle = "Стоимость выезда мастера";

		private readonly IDeliveryPriceCalculator _deliveryPriceCalculator;
		private readonly IGeoCoderApiClient _geoCoderApiClient;
		private readonly IInteractiveService _interactiveService;

		public MasterVisitCostViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			IDeliveryPriceCalculator deliveryPriceCalculator,
			ICitiesDataLoader citiesDataLoader,
			IStreetsDataLoader streetsDataLoader,
			IHousesDataLoader housesDataLoader,
			IGeoCoderApiClient geoCoderApiClient,
			IInteractiveService interactiveService) : base(unitOfWorkFactory, navigation, UoWTitle: DialogTitle)
		{
			CitiesDataLoader = citiesDataLoader ?? throw new ArgumentNullException(nameof(citiesDataLoader));
			StreetsDataLoader = streetsDataLoader ?? throw new ArgumentNullException(nameof(streetsDataLoader));
			HousesDataLoader = housesDataLoader ?? throw new ArgumentNullException(nameof(housesDataLoader));
			_deliveryPriceCalculator = deliveryPriceCalculator ?? throw new ArgumentNullException(nameof(deliveryPriceCalculator));
			_geoCoderApiClient = geoCoderApiClient ?? throw new ArgumentNullException(nameof(geoCoderApiClient));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));

			Title = DialogTitle;
			UoW.Session.DefaultReadOnly = true;

			InsertFromBufferCommand = new DelegateCommand(InsertFromBuffer);
		}

		public DelegateCommand InsertFromBufferCommand { get; }

		public Func<string> ReadClipboardText { get; set; }

		public ICitiesDataLoader CitiesDataLoader { get; }

		public IStreetsDataLoader StreetsDataLoader { get; }

		public IHousesDataLoader HousesDataLoader { get; }

		public bool UpdateCity { get; private set; }

		public bool UpdateStreet { get; private set; }

		public bool UpdateBuilding { get; private set; }

		public Guid? CityFiasGuid { get; private set; }

		public string CityName { get; private set; }

		public string CityText { get; private set; }

		public Guid? StreetCityGuid { get; private set; }

		public string StreetName { get; private set; }

		public string StreetTypeName { get; private set; }

		public string StreetTypeShort { get; private set; }

		public string StreetDistrict { get; private set; }

		public Guid? BuildingCityGuid { get; private set; }

		public Guid? BuildingStreetGuid { get; private set; }

		public string BuildingName { get; private set; }

		public decimal? Latitude { get; private set; }

		public decimal? Longitude { get; private set; }

		public DeliveryPriceNode Price { get; private set; }

		public ServiceDistrict ServiceDistrict { get; private set; }

		public bool HasPrice { get; private set; }

		public void SelectCity(Guid? cityGuid)
		{
			UpdateCity = false;
			ClearStreet(cityGuid);
			ClearBuilding(cityGuid, null);
			FirePropertyChanged();
		}

		public void SelectStreet(Guid? streetGuid)
		{
			UpdateCity = false;
			UpdateStreet = false;
			UpdateBuilding = true;
			BuildingStreetGuid = streetGuid;
			BuildingName = string.Empty;
			FirePropertyChanged();
		}

		public void CompleteBuilding(
			string cityName,
			string streetName,
			string streetTypeShort,
			string buildingName,
			bool fiasCompleted,
			decimal? latitude,
			decimal? longitude)
		{
			if(!fiasCompleted)
			{
				return;
			}

			if(!string.IsNullOrWhiteSpace(buildingName) || latitude == null || longitude == null)
			{
				var address = $"{cityName}, {streetName} {streetTypeShort}, {buildingName}";
				(latitude, longitude) = FindCoordinates(address);
			}

			SetCoordinates(latitude, longitude);
			UpdatePrice();
		}

		private void InsertFromBuffer()
		{
			ClearCity();
			ClearStreet(null);
			ClearBuilding(null, null);
			FirePropertyChanged();

			var hasError = true;
			var coordinates = ReadClipboardText?.Invoke()?.Split(',');
			if(coordinates?.Length == 2)
			{
				var goodLatitude = decimal.TryParse(coordinates[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude);
				var goodLongitude = decimal.TryParse(coordinates[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude);
				SetCoordinates(latitude, longitude);

				if(goodLatitude && goodLongitude)
				{
					UpdatePrice();
					hasError = false;
				}
			}

			if(hasError)
			{
				ShowError("Буфер обмена не содержит координат или содержит неправильные координаты");
			}
		}

		private void ClearCity()
		{
			UpdateCity = true;
			CityFiasGuid = null;
			CityName = string.Empty;
			CityText = string.Empty;
		}

		private void ClearStreet(Guid? cityGuid)
		{
			UpdateStreet = true;
			StreetCityGuid = cityGuid;
			StreetName = string.Empty;
			StreetTypeName = string.Empty;
			StreetTypeShort = string.Empty;
			StreetDistrict = string.Empty;
		}

		private void ClearBuilding(Guid? cityGuid, Guid? streetGuid)
		{
			UpdateBuilding = true;
			BuildingCityGuid = cityGuid;
			BuildingStreetGuid = streetGuid;
			BuildingName = string.Empty;
		}

		private void SetCoordinates(decimal? latitude, decimal? longitude)
		{
			Latitude = latitude;
			Longitude = longitude;
			FirePropertyChanged();
		}

		private void UpdatePrice()
		{
			var price = _deliveryPriceCalculator.CalculateForService(Latitude, Longitude);
			if(price.HasError)
			{
				HasPrice = false;
				Price = null;
				ServiceDistrict = null;
				ShowError(price.ErrorMessage);
				FirePropertyChanged();
				return;
			}

			HasPrice = true;
			Price = price;
			ServiceDistrict = UoW.GetById<ServiceDistrict>(price.ServiceDistrictId);
			FirePropertyChanged();
		}

		private (decimal? Latitude, decimal? Longitude) FindCoordinates(string address)
		{
			try
			{
				var findedByGeoCoder = _geoCoderApiClient.GetCoordinateAtAddressAsync(address).GetAwaiter().GetResult();
				if(findedByGeoCoder != null)
				{
					return (findedByGeoCoder.Latitude, findedByGeoCoder.Longitude);
				}
			}
			catch(Exception)
			{
				ShowError("Произошла ошибка при запросе координат в геокодере");
			}

			return (null, null);
		}

		private void ShowError(string message)
		{
			_interactiveService.ShowMessage(ImportanceLevel.Error, message);
		}
	}
}

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
	public class MasterVisitCostViewModel : DialogViewModelBase, IDisposable
	{
		private readonly IDeliveryPriceCalculator _deliveryPriceCalculator;
		private readonly IGeoCoderApiClient _geoCoderApiClient;
		private readonly IInteractiveService _interactiveService;
		private readonly IUnitOfWork _unitOfWork;

		public MasterVisitCostViewModel(
			INavigationManager navigation,
			IUnitOfWorkFactory unitOfWorkFactory,
			IDeliveryPriceCalculator deliveryPriceCalculator,
			ICitiesDataLoader citiesDataLoader,
			IStreetsDataLoader streetsDataLoader,
			IHousesDataLoader housesDataLoader,
			IGeoCoderApiClient geoCoderApiClient,
			IInteractiveService interactiveService) : base(navigation)
		{
			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			CitiesDataLoader = citiesDataLoader ?? throw new ArgumentNullException(nameof(citiesDataLoader));
			StreetsDataLoader = streetsDataLoader ?? throw new ArgumentNullException(nameof(streetsDataLoader));
			HousesDataLoader = housesDataLoader ?? throw new ArgumentNullException(nameof(housesDataLoader));
			_deliveryPriceCalculator = deliveryPriceCalculator ?? throw new ArgumentNullException(nameof(deliveryPriceCalculator));
			_geoCoderApiClient = geoCoderApiClient ?? throw new ArgumentNullException(nameof(geoCoderApiClient));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));

			Title = "Стоимость выезда мастера";
			_unitOfWork = unitOfWorkFactory.CreateWithoutRoot(Title);
			_unitOfWork.Session.DefaultReadOnly = true;

			InsertFromBufferCommand = new DelegateCommand(InsertFromBuffer);
		}

		public event EventHandler CitySelectionChanged;
		public event EventHandler StreetSelectionChanged;
		public event EventHandler AddressCleared;
		public event EventHandler CoordinatesChanged;
		public event EventHandler PriceChanged;

		public DelegateCommand InsertFromBufferCommand { get; }

		public Func<string> ReadClipboardText { get; set; }

		public ICitiesDataLoader CitiesDataLoader { get; }

		public IStreetsDataLoader StreetsDataLoader { get; }

		public IHousesDataLoader HousesDataLoader { get; }

		public Guid? CityGuid { get; private set; }

		public Guid? StreetGuid { get; private set; }

		public decimal? Latitude { get; private set; }

		public decimal? Longitude { get; private set; }

		public DeliveryPriceNode Price { get; private set; }

		public ServiceDistrict ServiceDistrict { get; private set; }

		public bool HasPrice { get; private set; }

		public void SelectCity(Guid? cityGuid)
		{
			CityGuid = cityGuid;
			CitySelectionChanged?.Invoke(this, EventArgs.Empty);
		}

		public void SelectStreet(Guid? streetGuid)
		{
			StreetGuid = streetGuid;
			StreetSelectionChanged?.Invoke(this, EventArgs.Empty);
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

		public void Dispose()
		{
			_unitOfWork?.Dispose();
		}

		private void InsertFromBuffer()
		{
			AddressCleared?.Invoke(this, EventArgs.Empty);

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

		private void SetCoordinates(decimal? latitude, decimal? longitude)
		{
			Latitude = latitude;
			Longitude = longitude;
			CoordinatesChanged?.Invoke(this, EventArgs.Empty);
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
				PriceChanged?.Invoke(this, EventArgs.Empty);
				return;
			}

			HasPrice = true;
			Price = price;
			ServiceDistrict = _unitOfWork.GetById<ServiceDistrict>(price.ServiceDistrictId);
			PriceChanged?.Invoke(this, EventArgs.Empty);
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

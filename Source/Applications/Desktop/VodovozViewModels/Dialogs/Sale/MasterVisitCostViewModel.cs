using Fias.Client.Loaders;
using GeoCoderApi.Client;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using System;
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
		}

		public ICitiesDataLoader CitiesDataLoader { get; }

		public IStreetsDataLoader StreetsDataLoader { get; }

		public IHousesDataLoader HousesDataLoader { get; }

		public decimal? Latitude { get; private set; }

		public decimal? Longitude { get; private set; }

		public DeliveryPriceNode Price { get; private set; }

		public ServiceDistrict ServiceDistrict { get; private set; }

		public bool HasPrice { get; private set; }

		public void SetCoordinates(decimal? latitude, decimal? longitude)
		{
			Latitude = latitude;
			Longitude = longitude;
		}

		public void UpdatePrice()
		{
			var price = _deliveryPriceCalculator.CalculateForService(Latitude, Longitude);
			if(price.HasError)
			{
				HasPrice = false;
				Price = null;
				ServiceDistrict = null;
				_interactiveService.ShowMessage(ImportanceLevel.Error, price.ErrorMessage);
				return;
			}

			HasPrice = true;
			Price = price;
			ServiceDistrict = _unitOfWork.GetById<ServiceDistrict>(price.ServiceDistrictId);
		}

		public (decimal? Latitude, decimal? Longitude) FindCoordinates(string address)
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
				_interactiveService.ShowMessage(ImportanceLevel.Error, "Произошла ошибка при запросе координат в геокодере");
			}

			return (null, null);
		}

		public void ShowError(string message)
		{
			_interactiveService.ShowMessage(ImportanceLevel.Error, message);
		}

		public void Dispose()
		{
			_unitOfWork?.Dispose();
		}
	}
}

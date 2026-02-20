using Autofac;
using Fias.Client.Loaders;
using GeoCoderApi.Client;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Osrm;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Extension;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using QS.ViewModels.Dialog;
using Vodovoz.Core.Domain;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.DeliveryPoints;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Users;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Sale;
using Vodovoz.Models;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Contacts;
using Vodovoz.SidePanel;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Dialogs.Goods;
using Vodovoz.ViewModels.Infrastructure.InfoProviders;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Contacts;
using Vodovoz.ViewModels.ViewModels.Goods;
using Vodovoz.ViewModels.ViewModels.Logistic;
using VodovozInfrastructure.Services;

namespace Vodovoz.ViewModels.Dialogs.Counterparties
{
	public partial class DeliveryPointViewModel
		: EntityTabViewModelBase<DeliveryPoint>,
		IDeliveryPointInfoProvider,
		ITDICloseControlTab,
		ICustomWidthInfoProvider,
		IAskSaveOnCloseViewModel
	{
		private int _currentPage = 0;
		private User _currentUser;
		private bool _isEntityInSavingProcess;
		private bool _isBuildingsInLoadingProcess;
		private FixedPricesViewModel _fixedPricesViewModel;
		private List<DeliveryPointResponsiblePerson> _responsiblePersons;
		private readonly IGtkTabsOpener _gtkTabsOpener;
		private readonly ILogger<DeliveryPointViewModel> _logger;
		private readonly IFeatureManager _featureManager;
		private readonly IUserRepository _userRepository;
		private readonly IFixedPricesModel _fixedPricesModel;
		private readonly IDeliveryPointRepository _deliveryPointRepository;
		private readonly RoboatsJournalsFactory _roboatsJournalsFactory;
		private readonly PanelViewType[] _infoWidgets = new[] { PanelViewType.DeliveryPricePanelView };
		private readonly ICoordinatesParser _coordinatesParser;
		private readonly IDeliveryRepository _deliveryRepository;
		private readonly IOsrmSettings _globalSettings;
		private readonly IPhoneTypeSettings _phoneTypeSettings;
		private readonly INomenclatureFixedPriceRepository _fixedPriceRepository;
		private readonly IGeoCoderApiClient _geoCoderApiClient;
		private readonly IOsrmClient _osrmClient;
		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
		private bool _isInformationActive;
		private bool _isFixedPricesActive;
		private bool _isSitesAndAppsActive;
		private string _districtOnMap;
		private bool _showDistrictBorders;

		public DeliveryPointViewModel(
			ILogger<DeliveryPointViewModel> logger,
			IFeatureManager featureManager,
			IUserRepository userRepository,
			INavigationManager navigationManager,
			IGtkTabsOpener gtkTabsOpener,
			IPhoneRepository phoneRepository,
			IContactSettings contactsParameters,
			ICitiesDataLoader citiesDataLoader,
			IStreetsDataLoader streetsDataLoader,
			IHousesDataLoader housesDataLoader,
			INomenclatureFixedPriceController nomenclatureFixedPriceController,
			IDeliveryPointRepository deliveryPointRepository,
			IDeliveryScheduleJournalFactory deliveryScheduleSelectorFactory,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			RoboatsJournalsFactory roboatsJournalsFactory,
			ILifetimeScope lifetimeScope,
			ICoordinatesParser coordinatesParser,
			IScheduleRestrictionRepository scheduleRestrictionRepository,
			IDeliveryRepository deliveryRepository,
			IOsrmSettings globalSettings,
			IPhoneTypeSettings phoneTypeSettings,
			INomenclatureFixedPriceRepository fixedPriceRepository,
			IGeoCoderApiClient geoCoderApiClient,
			IOsrmClient osrmClient,
			Domain.Client.Counterparty client = null)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));

			if(client != null && uowBuilder.IsNewEntity)
			{
				Entity.Counterparty = client;
			}
			else if(client == null && uowBuilder.IsNewEntity)
			{
				throw new ArgumentNullException(nameof(client), "Нельзя создать точку доставки без указания клиента");
			}

			if(navigationManager is null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}

			if(phoneRepository == null)
			{
				throw new ArgumentNullException(nameof(phoneRepository));
			}

			if(contactsParameters == null)
			{
				throw new ArgumentNullException(nameof(contactsParameters));
			}

			if(nomenclatureFixedPriceController == null)
			{
				throw new ArgumentNullException(nameof(nomenclatureFixedPriceController));
			}

			_roboatsJournalsFactory = roboatsJournalsFactory ?? throw new ArgumentNullException(nameof(roboatsJournalsFactory));
			LifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_coordinatesParser = coordinatesParser ?? throw new ArgumentNullException(nameof(coordinatesParser));
			_deliveryRepository = deliveryRepository ?? throw new ArgumentNullException(nameof(deliveryRepository));
			_globalSettings = globalSettings ?? throw new ArgumentNullException(nameof(globalSettings));
			_phoneTypeSettings = phoneTypeSettings ?? throw new ArgumentNullException(nameof(phoneTypeSettings));
			_fixedPriceRepository = fixedPriceRepository ?? throw new ArgumentNullException(nameof(fixedPriceRepository));
			_geoCoderApiClient = geoCoderApiClient ?? throw new ArgumentNullException(nameof(geoCoderApiClient));
			_osrmClient = osrmClient ?? throw new ArgumentNullException(nameof(osrmClient));
			_deliveryPointRepository = deliveryPointRepository ?? throw new ArgumentNullException(nameof(deliveryPointRepository));

			_gtkTabsOpener = gtkTabsOpener ?? throw new ArgumentNullException(nameof(gtkTabsOpener));
			_featureManager = featureManager ?? throw new ArgumentNullException(nameof(featureManager));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));

			_fixedPricesModel = new DeliveryPointFixedPricesModel(UoW, Entity, nomenclatureFixedPriceController);
			
			PhonesViewModel = LifetimeScope.Resolve<PhonesViewModel>(new TypedParameter(typeof(IUnitOfWork), UoW));
			PhonesViewModel.Initialize(this as DialogViewModelBase, !CanEdit,  Entity.Phones, Entity, true);

			CitiesDataLoader = citiesDataLoader ?? throw new ArgumentNullException(nameof(citiesDataLoader));
			StreetsDataLoader = streetsDataLoader ?? throw new ArgumentNullException(nameof(streetsDataLoader));
			HousesDataLoader = housesDataLoader ?? throw new ArgumentNullException(nameof(housesDataLoader));

			CanArchiveDeliveryPoint =
				commonServices.CurrentPermissionService.ValidatePresetPermission("can_arc_counterparty_and_deliverypoint");
			CanSetFreeDelivery = commonServices.CurrentPermissionService.ValidatePresetPermission("can_set_free_delivery");
			CanEditOrderLimits = commonServices.CurrentPermissionService.ValidatePresetPermission("user_can_edit_orders_limits");
			CanEditNomenclatureFixedPrice =
				commonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_delivery_point_fixed_prices");

			DeliveryPointCategories =
				deliveryPointRepository?.GetActiveDeliveryPointCategories(UoW)
				?? throw new ArgumentNullException(nameof(deliveryPointRepository));

			DeliveryScheduleSelectorFactory = deliveryScheduleSelectorFactory;

			DefaultWaterNomenclatureViewModel = new CommonEEVMBuilderFactory<DeliveryPoint>(this, Entity, UoW, NavigationManager, lifetimeScope)
				.ForProperty(dp => dp.DefaultWaterNomenclature)
				.UseViewModelJournalAndAutocompleter<NomenclaturesJournalViewModel, NomenclatureFilterViewModel>(filter =>
				{
					filter.RestrictCategory = NomenclatureCategory.water;
					filter.RestrictDilers = true;
				})
				.UseViewModelDialog<NomenclatureViewModel>()
				.Finish();

			Entity.PropertyChanged += (sender, e) =>
			{
				switch(e.PropertyName)
				{ // от этого события зависит панель цен доставки, которые в свою очередь зависят от района и, возможно, фиксов
					case nameof(Entity.Latitude):
					case nameof(Entity.Longitude):
						CurrentObjectChanged?.Invoke(this, new CurrentObjectChangedArgs(Entity));
						break;
				}
			};

			if(Entity.LogisticsRequirements == null)
			{
				Entity.LogisticsRequirements = new LogisticsRequirements();
			}

			_logisticsRequirementsVM = new LogisticsRequirementsViewModel(Entity.LogisticsRequirements, commonServices);

			OpenCounterpartyCommand = new DelegateCommand(
				OpenCounterparty,
				() => Entity.Counterparty != null);

			AllActiveDistrictsWithBorders = scheduleRestrictionRepository.GetDistrictsWithBorder(UoW);
			TryAddEmployeeFixedPrices();
		}

		private void TryAddEmployeeFixedPrices()
		{
			if(Entity.Id > 0)
			{
				return;
			}

			var employeeFixedPrices =
				_fixedPriceRepository.GetEmployeesNomenclatureFixedPricesByCounterpartyId(UoW, Entity.Counterparty.Id);

			foreach(var fixedPrice in employeeFixedPrices)
			{
				_fixedPricesModel.AddFixedPrice(fixedPrice.Nomenclature, fixedPrice.Price, fixedPrice.MinCount);
			}
		}

		#region Свойства

		//permissions
		public bool CanArchiveDeliveryPoint { get; }
		public bool CanSetFreeDelivery { get; }
		public bool CanEditOrderLimits { get; }
		public bool CanEditNomenclatureFixedPrice { get; }

		//widget binds
		public int CurrentPage
		{
			get => _currentPage;
			private set => SetField(ref _currentPage, value);
		}
		
		public bool IsInformationActive
		{
			get => _isInformationActive;
			set
			{
				if(SetField(ref _isInformationActive, value) && value)
				{
					CurrentPage = 0;
					_isFixedPricesActive = false;
					_isSitesAndAppsActive = false;
				}
			}
		}
		
		public bool IsFixedPricesActive
		{
			get => _isFixedPricesActive;
			set
			{
				if(SetField(ref _isFixedPricesActive, value) && value)
				{
					CurrentPage = 1;
					_isInformationActive = false;
					_isSitesAndAppsActive = false;
				}
			}
		}

		public bool IsSitesAndAppsActive
		{
			get => _isSitesAndAppsActive;
			set
			{
				if(SetField(ref _isSitesAndAppsActive, value) && value)
				{
					CurrentPage = 2;
					_isInformationActive = false;
					_isFixedPricesActive = false;
				}
			}
		}

		public bool IsInProcess => _isEntityInSavingProcess || _isBuildingsInLoadingProcess;

		public bool CurrentUserIsAdmin => CurrentUser.IsAdmin;
		public bool CoordsWasChanged => Entity.СoordsLastChangeUser != null;
		public bool CanEdit => PermissionResult.CanUpdate || PermissionResult.CanCreate && Entity.Id == 0;
		public string CoordsLastChangeUserName => Entity.СoordsLastChangeUser.Name;

		//widget init
		public FixedPricesViewModel FixedPricesViewModel =>
			_fixedPricesViewModel ??
			(_fixedPricesViewModel = new FixedPricesViewModel(UoW, _fixedPricesModel, this, NavigationManager, LifetimeScope));

		public List<DeliveryPointResponsiblePerson> ResponsiblePersons =>
			_responsiblePersons ?? (_responsiblePersons = new List<DeliveryPointResponsiblePerson>());

		public ILifetimeScope LifetimeScope { get; }
		public PhonesViewModel PhonesViewModel { get; }
		public ICitiesDataLoader CitiesDataLoader { get; }
		public IStreetsDataLoader StreetsDataLoader { get; }
		public IHousesDataLoader HousesDataLoader { get; }
		public IOrderedEnumerable<DeliveryPointCategory> DeliveryPointCategories { get; }
		public IEntityAutocompleteSelectorFactory DeliveryScheduleSelectorFactory { get; }

		public IEntityEntryViewModel DefaultWaterNomenclatureViewModel { get; }

		public override bool HasChanges
		{
			get
			{
				PhonesViewModel.RemoveEmpty();
				return base.HasChanges;
			}
			set => base.HasChanges = value;
		}

		private LogisticsRequirementsViewModel _logisticsRequirementsVM;
		public LogisticsRequirementsViewModel LogisticsRequirementsViewModel
		{
			get => _logisticsRequirementsVM;
			set
			{
				SetField(ref _logisticsRequirementsVM, value);
				Entity.LogisticsRequirements = _logisticsRequirementsVM.Entity;
			}
		}

		#endregion

		#region IDeliveryPointInfoProvider

		public DeliveryPoint DeliveryPoint => Entity;
		public PanelViewType[] InfoWidgets => _infoWidgets;
		public event EventHandler<CurrentObjectChangedArgs> CurrentObjectChanged;
		public int? WidthRequest => 420;

		#endregion

		public void OpenFixedPrices()
		{
			IsFixedPricesActive = true;
		}

		public District GetAccurateDistrict() =>
			Entity.CoordinatesExist
				? _deliveryRepository.GetAccurateDistrict(UoW, Entity.Latitude.Value, Entity.Longitude.Value)
				: null;

		public override bool Save(bool close)
		{
			try
			{
				_isEntityInSavingProcess = true;

				if(_isBuildingsInLoadingProcess)
				{
					ShowWarningMessage("Программа загружает координаты, попробуйте повторно сохранить точку доставки.");
					return false;
				}

				if(!HasChanges)
				{
					return base.Save(close);
				}

				if(!Entity.CoordinatesExist &&
				   !CommonServices.InteractiveService.Question(
					   "Адрес точки доставки не найден на карте, вы точно хотите сохранить точку доставки?"))
				{
					return false;
				}

				if(Entity.District == null && !CommonServices.InteractiveService.Question(
					"Район доставки не найден. Это приведёт к невозможности отображения заказа на " +
					"эту точку доставки у логистов при составлении маршрутного листа. Укажите правильные координаты.\n" +
					"Продолжить сохранение точки доставки?",
					"Проверьте координаты!"))
				{
					return false;
				}

				if(Entity.Counterparty.PersonType == PersonType.natural
					&& ((Entity.RoomType == RoomType.Office) || (Entity.RoomType == RoomType.Store))
					&& !_deliveryPointRepository.CheckingAnAddressForDeliveryForNewCustomers(UoW, Entity))
				{
					var createDeliveryPoint = AskQuestion($"Уточните с клиентом: по данному адресу находится юр.лицо. Вы уверены, что хотите сохранить этот адрес для физ.лица?");
					if(!createDeliveryPoint)
					{
						return false;
					}
				}

				if(Entity.CoordinatesExist)
				{
					var accurateDistrict = _deliveryRepository.GetAccurateDistrict(UoW, Entity.Latitude.Value, Entity.Longitude.Value);

					if(accurateDistrict == null
						&& !CommonServices.InteractiveService.Question(
							"Точный район доставки по координатам не определён. Сохранить ТД без точного района?",
							"Проверьте координаты!"))
					{
						return false;
					}
				}

				if(UoW.IsNew)
				{
					ShowAddressesWithFixedPrices();
				}

				return base.Save(close);
			}
			finally
			{
				_isEntityInSavingProcess = false;
			}
		}

		public void ApplyOrderSumLimitsToAllDeliveryPointsOfClient()
		{
			foreach(var deliveryPoint in Entity.Counterparty.DeliveryPoints)
			{
				if(deliveryPoint.Id == Entity.Id)
				{
					continue;
				}

				deliveryPoint.MaximalOrderSumLimit = Entity.MaximalOrderSumLimit;
				deliveryPoint.MinimalOrderSumLimit = Entity.MinimalOrderSumLimit;
			}
		}

		public void SetCoordinatesFromBuffer(string buffer)
		{
			var result = _coordinatesParser.GetCoordinatesFromBuffer(buffer);
			
			if(!result.ParsedCoordinates.HasValue)
			{
				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Error, result.ErrorMessage);
			}
			else
			{
				var parsedCoordinates = result.ParsedCoordinates.Value;
				WriteCoordinates(parsedCoordinates.Latitude, parsedCoordinates.Longitude, true);
			}
		}

		public void WriteCoordinates(decimal? latitude, decimal? longitude, bool isManual)
		{
			Entity.ManualCoordinates = isManual;
			if(EqualCoords(Entity.Latitude, latitude) && EqualCoords(Entity.Longitude, longitude))
			{
				return;
			}

			Entity.SetСoordinates(latitude, longitude, _deliveryRepository, _globalSettings, _osrmClient, UoW);
			Entity.СoordsLastChangeUser = _currentUser ?? (_currentUser = _userRepository.GetCurrentUser(UoW));
		}

		/// <summary>
		/// Сравнивает координаты с точностью 6 знаков после запятой
		/// </summary>
		/// <returns><c>true</c> если координаты равны, <c>false</c> иначе.</returns>
		private bool EqualCoords(decimal? coord1, decimal? coord2)
		{
			if(!coord1.HasValue || !coord2.HasValue)
			{
				return false;
			}

			decimal coordDiff = Math.Abs(coord1.Value - coord2.Value);
			return Math.Round(coordDiff, 6) == decimal.Zero;
		}

		private void ShowAddressesWithFixedPrices()
		{
			var addresses = _deliveryPointRepository.GetAddressesWithFixedPrices(Entity.Counterparty.Id);
			if(addresses.Any())
			{
				ShowInfoMessage($"На других адресах имеется фиксированная стоимость:\r\n {string.Join(", \r\n", addresses)}");
			}
		}

		public IList<District> AllActiveDistrictsWithBorders { get; }
		public string CityBeforeChange { get; set; }
		public string StreetBeforeChange { get; set; }
		public string BuildingBeforeChange { get; set; }
		public string EntranceBeforeChange { get; set; }

		public bool IsAddressChanged =>
			Entity.City != CityBeforeChange
			|| Entity.Street != StreetBeforeChange
			|| Entity.Building != BuildingBeforeChange
			|| Entity.Entrance != EntranceBeforeChange;

		#region ITDICloseControlTab

		public bool CanClose()
		{
			if(IsInProcess)
			{
				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning,
					"Дождитесь завершения сохранения точки доставки и повторите", "Сохранение...");
			}

			return !IsInProcess;
		}

		#endregion

		#region IAskSaveOnCloseViewModel

		public bool AskSaveOnClose => CanEdit;

		#endregion

		#region Commands

		public DelegateCommand OpenCounterpartyCommand { get; }

		private void OpenCounterparty()
		{
			_gtkTabsOpener.OpenCounterpartyDlg(this, Entity.Counterparty.Id);
		}

		private DelegateCommand _openOnMapCommand;

		public DelegateCommand OpenOnMapCommand => _openOnMapCommand ?? (_openOnMapCommand = new DelegateCommand(
			() =>
			{
				NumberFormatInfo numberFormatInfo = new NumberFormatInfo
				{
					NumberDecimalSeparator = "."
				};

				var ln = Entity.Longitude.Value.ToString(numberFormatInfo);
				var lt = Entity.Latitude.Value.ToString(numberFormatInfo);

				System.Diagnostics.Process.Start($"https://yandex.ru/maps/?whatshere[point]={ln},{lt}&whatshere[zoom]=16");
			}
		));

		#endregion

		public void ResetAddressChanges()
		{
			CityBeforeChange = Entity.City;
			StreetBeforeChange = Entity.Street;
			BuildingBeforeChange = Entity.Building;
			EntranceBeforeChange = Entity.Entrance;
		}

		public async Task<Coordinate> UpdateCoordinatesFromGeoCoderAsync(IHousesDataLoader entryBuildingHousesDataLoader)
		{
			decimal? latitude = null;
			decimal? longitude = null;

			try
			{
				_isBuildingsInLoadingProcess = true;

				int.TryParse(Entity.Entrance, out var parsedEntrance);

				var address =
					parsedEntrance <= 0
					? $"{Entity.LocalityType} {Entity.City}, {Entity.StreetDistrict}, {Entity.Street} {Entity.StreetType}, {Entity.Building}"
					: $"{Entity.LocalityType} {Entity.City}, {Entity.StreetDistrict}, {Entity.Street} {Entity.StreetType}, {Entity.Building}" +
						$", {(Entity.EntranceType == EntranceType.Entrance ? "парадная" : "вход")} {Entity.Entrance}";

				try
				{
					var findedByGeoCoder = await _geoCoderApiClient.GetCoordinateAtAddressAsync(address, _cancellationTokenSource.Token);

					if(findedByGeoCoder != null)
					{
						latitude = findedByGeoCoder.Latitude;
						longitude = findedByGeoCoder.Longitude;
					}
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Произошла ошибка при запросе координат");
				}
			}
			finally
			{
				_isBuildingsInLoadingProcess = false;
			}

			return new Coordinate
			{
				Latitude = latitude,
				Longitude = longitude
			};
		}

		public bool IsDisposed { get; private set; }
		public string DistrictOnMapText 
		{
			get => _districtOnMap; 
			set => SetField(ref _districtOnMap, value); 
		}
		public bool ShowDistrictBorders 
		{ 
			get => _showDistrictBorders; 
			set => SetField(ref _showDistrictBorders, value);
		}

		public OrderAddressType? TypeOfAddress => null;

		public override void Dispose()
		{
			IsDisposed = true;
			_cancellationTokenSource.Cancel();
			base.Dispose();
		}
	}
}

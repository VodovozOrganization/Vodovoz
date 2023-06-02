using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using QS.ViewModels.Extension;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fias.Client.Loaders;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Models;
using Vodovoz.Services;
using Vodovoz.SidePanel;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Infrastructure.InfoProviders;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Contacts;
using Vodovoz.ViewModels.ViewModels.Goods;
using Vodovoz.ViewModels.ViewModels.Logistic;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.ViewModels.Dialogs.Counterparty
{
	public class DeliveryPointViewModel : EntityTabViewModelBase<DeliveryPoint>, IDeliveryPointInfoProvider, ITDICloseControlTab,
		IAskSaveOnCloseViewModel
	{
		private int _currentPage = 0;
		private User _currentUser;
		private bool _isEntityInSavingProcess;
		private bool _isBuildingsInLoadingProcess;
		private FixedPricesViewModel _fixedPricesViewModel;
		private List<DeliveryPointResponsiblePerson> _responsiblePersons;
		private readonly IGtkTabsOpener _gtkTabsOpener;
		private readonly IUserRepository _userRepository;
		private readonly IFixedPricesModel _fixedPricesModel;
		private readonly IDeliveryPointRepository _deliveryPointRepository;
		private readonly RoboatsJournalsFactory _roboatsJournalsFactory;
		private readonly IPromotionalSetRepository _promotionalSetRepository = new PromotionalSetRepository();
		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		public DeliveryPointViewModel(
			IUserRepository userRepository,
			IGtkTabsOpener gtkTabsOpener,
			IPhoneRepository phoneRepository,
			IContactParametersProvider contactsParameters,
			ICitiesDataLoader citiesDataLoader,
			IStreetsDataLoader streetsDataLoader,
			IHousesDataLoader housesDataLoader,
			INomenclatureJournalFactory nomenclatureSelectorFactory,
			NomenclatureFixedPriceController nomenclatureFixedPriceController,
			IDeliveryPointRepository deliveryPointRepository,
			IDeliveryScheduleJournalFactory deliveryScheduleSelectorFactory,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			RoboatsJournalsFactory roboatsJournalsFactory,
		 	Domain.Client.Counterparty client = null)
			: base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			if(client != null && uowBuilder.IsNewEntity)
			{
				Entity.Counterparty = client;
			}
			else if(client == null && uowBuilder.IsNewEntity)
			{
				throw new ArgumentNullException(nameof(client), "Нельзя создать точку доставки без указания клиента");
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
			_deliveryPointRepository = deliveryPointRepository ?? throw new ArgumentNullException(nameof(deliveryPointRepository));

			_gtkTabsOpener = gtkTabsOpener ?? throw new ArgumentNullException(nameof(gtkTabsOpener));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));

			NomenclatureSelectorFactory =
				nomenclatureSelectorFactory ?? throw new ArgumentNullException(nameof(nomenclatureSelectorFactory));

			_fixedPricesModel = new DeliveryPointFixedPricesModel(UoW, Entity, nomenclatureFixedPriceController);
			PhonesViewModel = new PhonesViewModel(phoneRepository, UoW, contactsParameters, _roboatsJournalsFactory, CommonServices)
			{
				PhonesList = Entity.ObservablePhones,
				DeliveryPoint = Entity,
				ReadOnly = !CanEdit
			};

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
			Entity.PropertyChanged += (sender, e) =>
			{
				switch(e.PropertyName)
				{ // от этого события зависит панель цен доставки, которые в свою очередь зависят от района и, возможно, фиксов
					case nameof(Entity.District):
						CurrentObjectChanged?.Invoke(this, new CurrentObjectChangedArgs(Entity));
						break;
				}
			};

			if(Entity.LogisticsRequirements == null)
			{
				Entity.LogisticsRequirements = new LogisticsRequirements();
			}

			_logisticsRequirementsVM = new LogisticsRequirementsViewModel(Entity.LogisticsRequirements, commonServices);
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

		public bool IsInProcess => _isEntityInSavingProcess || _isBuildingsInLoadingProcess;

		public bool CurrentUserIsAdmin => CurrentUser.IsAdmin;
		public bool CoordsWasChanged => Entity.СoordsLastChangeUser != null;
		public bool CanEdit => PermissionResult.CanUpdate || PermissionResult.CanCreate && Entity.Id == 0;
		public string CoordsLastChangeUserName => Entity.СoordsLastChangeUser.Name;

		//widget init
		public FixedPricesViewModel FixedPricesViewModel =>
			_fixedPricesViewModel ??
			(_fixedPricesViewModel = new FixedPricesViewModel(UoW, _fixedPricesModel, NomenclatureSelectorFactory, this));

		public List<DeliveryPointResponsiblePerson> ResponsiblePersons =>
			_responsiblePersons ?? (_responsiblePersons = new List<DeliveryPointResponsiblePerson>());

		public PhonesViewModel PhonesViewModel { get; }
		public ICitiesDataLoader CitiesDataLoader { get; }
		public IStreetsDataLoader StreetsDataLoader { get; }
		public IHousesDataLoader HousesDataLoader { get; }
		public IOrderedEnumerable<DeliveryPointCategory> DeliveryPointCategories { get; }
		public INomenclatureJournalFactory NomenclatureSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory DeliveryScheduleSelectorFactory { get; }

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
		public PanelViewType[] InfoWidgets => new[] { PanelViewType.DeliveryPricePanelView };
		public event EventHandler<CurrentObjectChangedArgs> CurrentObjectChanged;

		#endregion

		public void OpenFixedPrices()
		{
			CurrentPage = 1;
		}

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
			var error = true;
			var coordinates = buffer?.Split(',');
			if(coordinates?.Length == 2)
			{
				coordinates[0] = coordinates[0].Replace('.', ',');
				coordinates[1] = coordinates[1].Replace('.', ',');

				var goodLat = decimal.TryParse(coordinates[0].Trim(), out decimal latitude);
				var goodLon = decimal.TryParse(coordinates[1].Trim(), out decimal longitude);

				if(goodLat && goodLon)
				{
					WriteCoordinates(latitude, longitude, true);
					error = false;
				}
			}

			if(error)
			{
				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Error,
					"Буфер обмена не содержит координат или содержит неправильные координаты");
			}
		}

		public void WriteCoordinates(decimal? latitude, decimal? longitude, bool isManual)
		{
			Entity.ManualCoordinates = isManual;
			if(EqualCoords(Entity.Latitude, latitude) && EqualCoords(Entity.Longitude, longitude))
			{
				return;
			}

			Entity.SetСoordinates(latitude, longitude, UoW);
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

		public string CityBeforeChange { get; set; }
		public string StreetBeforeChange { get; set; }
		public string BuildingBeforeChange { get; set; }

		public bool IsAddressChanged =>
			Entity.City != CityBeforeChange
			|| Entity.Street != StreetBeforeChange
			|| Entity.Building != BuildingBeforeChange;


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

		private DelegateCommand _openCounterpartyCommand;

		public DelegateCommand OpenCounterpartyCommand => _openCounterpartyCommand ?? (_openCounterpartyCommand = new DelegateCommand(
			() => _gtkTabsOpener.OpenCounterpartyDlg(this, Entity.Counterparty.Id),
			() => Entity.Counterparty != null
		));

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
		}

		public async Task<Coordinate> UpdateCoordinatesFromGeoCoderAsync(IHousesDataLoader entryBuildingHousesDataLoader)
		{
			decimal? latitude = null, longitude = null;
			try
			{
				_isBuildingsInLoadingProcess = true;

				var address = $"{Entity.LocalityType} {Entity.City}, {Entity.StreetDistrict}, {Entity.Street} {Entity.StreetType}, {Entity.Building}";
				var findedByGeoCoder = await entryBuildingHousesDataLoader.GetCoordinatesByGeocoderAsync(address, _cancellationTokenSource.Token);
				if(findedByGeoCoder != null)
				{
					var culture = CultureInfo.CreateSpecificCulture("ru-RU");
					culture.NumberFormat.NumberDecimalSeparator = ".";
					latitude = decimal.Parse(findedByGeoCoder.Latitude, culture);
					longitude = decimal.Parse(findedByGeoCoder.Longitude, culture);
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

		public class Coordinate
		{
			public decimal? Latitude { get; set; }
			public decimal? Longitude { get; set; }
		}

		public bool IsDisposed { get; private set; }

		public override void Dispose()
		{
			IsDisposed = true;
			_cancellationTokenSource.Cancel();
			base.Dispose();
		}
	}
}

using Autofac;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.CachingRepositories.Common;
using Vodovoz.Core.Domain.Users.Settings;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Extensions;
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.Services;
using Vodovoz.Services.Fuel;
using Vodovoz.Settings.Organizations;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Extensions;
using Vodovoz.ViewModels.Journals.JournalViewModels.Store;
using Vodovoz.ViewModels.ViewModels.Organizations;
using Vodovoz.ViewModels.Warehouses;
using Vodovoz.ViewModels.Widgets.Print;
using Vodovoz.ViewModels.Widgets.Users;

namespace Vodovoz.ViewModels.Users
{
	public class UserSettingsViewModel : EntityTabViewModelBase<UserSettings>, ITDICloseControlTab
	{
		private readonly IEmployeeService _employeeService;
		private readonly ISubdivisionSettings _subdivisionSettings;
		private readonly ISubdivisionRepository _subdivisionRepository;
		private readonly INomenclatureFixedPriceRepository _nomenclatureFixedPriceRepository;
		private readonly IFuelApiService _fuelApiService;
		private readonly IGuiDispatcher _guiDispatcher;
		private DelegateCommand _updateFixedPricesCommand;
		private bool _sortingSettingsUpdated;
		private bool _isFixedPricesUpdating;
		private string _progressMessage;
		private double _progressFraction;
		private decimal _incrementFixedPrices = 20;
		private const double _progressStep = 0.25;
		private readonly WarehousesUserSelectionViewModel _warehousesUserSelectionViewModel;
		private bool _isWarehousesForNotificationsListChanged = false;

		private CancellationTokenSource _cancellationTokenSource;
		private Subdivision _defaultSubdivision;
		private Counterparty _defaultCounterparty;

		public UserSettingsViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigationManager,
			ICommonServices commonServices,
			ILifetimeScope lifetimeScope,
			IEmployeeService employeeService,
			ISubdivisionSettings subdivisionSettings,
			ICounterpartyJournalFactory counterpartySelectorFactory,
			ISubdivisionRepository subdivisionRepository,
			IDomainEntityNodeInMemoryCacheRepository<Subdivision> subdivisionInMemoryCacheRepository,
			INomenclatureFixedPriceRepository nomenclatureFixedPriceRepository,
			IFuelApiService fuelApiService,
			IGuiDispatcher guiDispatcher,
			DocumentsPrinterSettingsViewModel documentsPrinterSettingsViewModel,
			ViewModelEEVMBuilder<Warehouse> warehouseViewModelEEVMBuilder,
			ViewModelEEVMBuilder<Counterparty> counterpartyViewModelEEVMBuilder)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			if(navigationManager is null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}

			if(warehouseViewModelEEVMBuilder is null)
			{
				throw new ArgumentNullException(nameof(warehouseViewModelEEVMBuilder));
			}

			if(counterpartyViewModelEEVMBuilder is null)
			{
				throw new ArgumentNullException(nameof(counterpartyViewModelEEVMBuilder));
			}

			LifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_subdivisionSettings = subdivisionSettings ?? throw new ArgumentNullException(nameof(subdivisionSettings));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			SubdivisionInMemoryCacheRepository = subdivisionInMemoryCacheRepository ?? throw new ArgumentNullException(nameof(subdivisionInMemoryCacheRepository));
			_nomenclatureFixedPriceRepository =
				nomenclatureFixedPriceRepository ?? throw new ArgumentNullException(nameof(nomenclatureFixedPriceRepository));
			_fuelApiService = fuelApiService ?? throw new ArgumentNullException(nameof(fuelApiService));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
			DocumentsPrinterSettingsViewModel = documentsPrinterSettingsViewModel ?? throw new ArgumentNullException(nameof(documentsPrinterSettingsViewModel));
			InteractiveService = commonServices.InteractiveService;
			CounterpartySelectorFactory =
				(counterpartySelectorFactory ?? throw new ArgumentNullException(nameof(counterpartySelectorFactory)))
				.CreateCounterpartyAutocompleteSelectorFactory(LifetimeScope);

			SetPermissions();

			if(UserIsCashier)
			{
				ConfigureCashSorting();
			}

			_warehousesUserSelectionViewModel = new WarehousesUserSelectionViewModel(
				UoW,
				commonServices,
				navigationManager,
				Entity.MovementDocumentsNotificationUserSelectedWarehouses);

			_warehousesUserSelectionViewModel.ObservableWarehouses.ListContentChanged += OnWarehousesToNotifyListContentChanged;

			SubdivisionViewModel = BuildSubdivisionViewModel();

			FuelControlApiLoginCommand = new DelegateCommand(async () => await FuelControlApiLogin(), () => Entity.IsUserHasAuthDataForFuelControlApi);

			DocumentsPrinterSettingsViewModel.UserSettings = Entity;

			WarehouseViewModel = warehouseViewModelEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(Entity, e => e.DefaultWarehouse)
				.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
				.UseViewModelDialog<WarehouseViewModel>()
				.Finish();
		}

		private void OnWarehousesToNotifyListContentChanged(object sender, EventArgs e)
		{
			_isWarehousesForNotificationsListChanged = true;

			Entity.MovementDocumentsNotificationUserSelectedWarehouses =
				WarehousesUserSelectionViewModel.ObservableWarehouses
				.Select(w => w.WarehouseId)
				.ToList();
		}

		public IEntityEntryViewModel SubdivisionViewModel { get; }
		public IEntityEntryViewModel CounterpartyViewModel { get; set; }

		public IEntityEntryViewModel BuildSubdivisionViewModel()
		{
			return new CommonEEVMBuilderFactory<UserSettingsViewModel>(this, this, UoW, NavigationManager, LifetimeScope)
				.ForProperty(x => x.DefaultSubdivision)
				.UseViewModelJournalAndAutocompleter<SubdivisionsJournalViewModel>()
				.UseViewModelDialog<SubdivisionViewModel>()
				.Finish();
		}

		#region Свойства

		public decimal IncrementFixedPrices
		{
			get => _incrementFixedPrices;
			set => SetField(ref _incrementFixedPrices, value);
		}

		public bool IsFixedPricesUpdating
		{
			get => _isFixedPricesUpdating;
			private set => SetField(ref _isFixedPricesUpdating, value);
		}

		public string ProgressMessage
		{
			get => _progressMessage;
			private set => SetField(ref _progressMessage, value);
		}

		public double ProgressFraction
		{
			get => _progressFraction;
			private set => SetField(ref _progressFraction, value);
		}

		public Subdivision DefaultSubdivision
		{
			get => this.GetIdRefField(ref _defaultSubdivision, Entity.DefaultSubdivisionId);
			set => this.SetIdRefField(SetField, ref _defaultSubdivision, () => Entity.DefaultSubdivisionId, value);
		}

		public Counterparty DefaultCounterparty
		{
			get => this.GetIdRefField(ref _defaultCounterparty, Entity.DefaultCounterpartyId);
			set => this.SetIdRefField(SetField, ref _defaultCounterparty, () => Entity.DefaultCounterpartyId, value);
		}

		public IInteractiveService InteractiveService { get; }
		public IEntityAutocompleteSelectorFactory CounterpartySelectorFactory { get; }
		public IEntityEntryViewModel WarehouseViewModel { get; }

		public bool IsUserFromOkk => _subdivisionSettings.GetOkkId()
									 == _employeeService.GetEmployeeForUser(UoW, CommonServices.UserService.CurrentUserId)?.Subdivision?.Id;

		public bool IsUserFromRetail { get; private set; }
		public bool UserIsCashier { get; private set; }
		public bool CanUpdateFixedPrices { get; private set; }

		public IList<CashSubdivisionSortingSettings> SubdivisionSortingSettings => Entity.CashSubdivisionSortingSettings;

		public WarehousesUserSelectionViewModel WarehousesUserSelectionViewModel => _warehousesUserSelectionViewModel;

		public IDomainEntityNodeInMemoryCacheRepository<Subdivision> SubdivisionInMemoryCacheRepository { get; }
		public IDomainEntityNodeInMemoryCacheRepository<Counterparty> CounterpartyInMemoryCacheRepository { get; }
		public DocumentsPrinterSettingsViewModel DocumentsPrinterSettingsViewModel { get; }

		public DelegateCommand FuelControlApiLoginCommand { get; }

		public DelegateCommand UpdateFixedPricesCommand => _updateFixedPricesCommand ?? (_updateFixedPricesCommand = new DelegateCommand(
					() =>
					{
						try
						{
							IsFixedPricesUpdating = true;
							UpdateProgress("Получаем фиксу, которую нужно обновить...");
							using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
							{
								var fixedPrices = _nomenclatureFixedPriceRepository.GetFixedPricesFor19LWater(uow);
								UpdateProgress($"Получили данные, которые нужно обновить. Всего {fixedPrices.Count} объектов");

								for(var i = 0; i < fixedPrices.Count; i++)
								{
									fixedPrices[i].Price += IncrementFixedPrices;
									uow.Save(fixedPrices[i]);
								}

								UpdateProgress($"Получили данные, которые нужно обновить. Всего {fixedPrices.Count} объектов. " +
									"Обновляем фиксу с записью в историю изменений...");
								uow.Commit();
								UpdateProgress("Готово");
							}
						}
						finally
						{
							IsFixedPricesUpdating = false;
						}
					},
					() => !IsFixedPricesUpdating
			)
		);

		public ILifetimeScope LifetimeScope { get; set; }

		#endregion

		private void ShowNotifyIfWarehousesListChanged()
		{
			if(_isWarehousesForNotificationsListChanged)
			{
				InteractiveService.ShowMessage(
					ImportanceLevel.Info,
					"Внимание!\nСписок складов для получения уведомлений изменен.\nЧтобы изменения вступили в силу необходимо перезапустить ДВ!");
			}
		}

		private bool IsNeedToConfigurePrinterSettings()
		{
			foreach(var printerSetting in Entity.DocumentPrinterSettings)
			{
				if(string.IsNullOrWhiteSpace(printerSetting.PrinterName) || printerSetting.NumberOfCopies < 1)
				{
					InteractiveService.ShowMessage(ImportanceLevel.Error,
						$"Не заданы настройки принтера для документа типа \"{printerSetting.DocumentType.GetEnumDisplayName()}\"\n" +
						$"Либо задайте настройки принтера, либо удалите документ из списка настроек!");

					return true;
				}
			}

			return false;
		}

		public override bool Save(bool close)
		{
			ShowNotifyIfWarehousesListChanged();

			if(IsNeedToConfigurePrinterSettings())
			{
				return false;
			}

			return base.Save(close);
		}

		public override void Close(bool askSave, CloseSource source)
		{
			if(_sortingSettingsUpdated && source == CloseSource.Cancel)
			{
				if(CommonServices.InteractiveService.Question(
					"Ваши настройки сортировки касс были автоматически обновлены. Выйти без сохранения?"))
				{
					base.Close(false, source);
				}
				else
				{
					return;
				}
			}
			base.Close(askSave, source);
		}

		public bool CanClose()
		{
			if(IsFixedPricesUpdating)
			{
				ShowWarningMessage("Дождитесь завершения задачи и повторите");
			}

			return !IsFixedPricesUpdating;
		}

		public void UpdateIndices() => Entity.UpdateCashSortingIndices();

		private void SetPermissions()
		{
			CanUpdateFixedPrices = CommonServices.CurrentPermissionService.ValidatePresetPermission("can_update_fixed_prices_for_19l_water");
			IsUserFromRetail = CommonServices.CurrentPermissionService.ValidatePresetPermission("user_have_access_to_retail");
			UserIsCashier = CommonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.CashPermissions.PresetPermissionsRoles.Cashier);
		}

		private void UpdateProgress(string message)
		{
			ProgressMessage = message;
			ProgressFraction += _progressStep;
		}

		private void ConfigureCashSorting()
		{
			var availableSubdivisions = _subdivisionRepository.GetCashSubdivisionsAvailableForUser(UoW, CurrentUser).ToList();

			_sortingSettingsUpdated = Entity.UpdateCashSortingSettings(availableSubdivisions.Select(x => x.Id));
		}

		private async Task FuelControlApiLogin()
		{
			if(_cancellationTokenSource != null)
			{
				return;
			}

			Entity.FuelControlApiSessionId = string.Empty;
			Entity.FuelControlApiSessionExpirationDate = null;

			_cancellationTokenSource = new CancellationTokenSource();

			try
			{
				var session = await _fuelApiService.Login(
					Entity.FuelControlApiLogin,
					Entity.FuelControlApiPassword,
					Entity.FuelControlApiKey,
					_cancellationTokenSource.Token);

				Entity.FuelControlApiSessionId = session.SessionId;
				Entity.FuelControlApiSessionExpirationDate = session.SessionExpirationDate;

				ShowMessageInGuiThread(ImportanceLevel.Info,
					"Новое значение Id сессии получено. Старый Id больше не действует.\nЧтобы новое значение было сохранено обязательно нажмите на кнопку \"Сохранить\"");
			}
			catch(Exception ex)
			{
				ShowMessageInGuiThread(ImportanceLevel.Error, ex.Message);
			}
			finally
			{
				_cancellationTokenSource?.Dispose();
				_cancellationTokenSource = null;
			}
		}

		private void ShowMessageInGuiThread(ImportanceLevel level, string message)
		{
			_guiDispatcher.RunInGuiTread(() =>
			{
				CommonServices.InteractiveService.ShowMessage(level, message);
			});
		}

		public override void Dispose()
		{
			LifetimeScope = null;
			base.Dispose();
		}
	}
}

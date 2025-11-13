using Autofac;
using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.Dialog;
using QS.Dialog.ViewModels;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Services.FileDialog;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using QS.ViewModels.Extension;
using Vodovoz.Application.FileStorage;
using Vodovoz.Controllers;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Logistics.Cars;
using Vodovoz.Core.Domain.Permissions;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Fuel;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Fuel;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Infrastructure.Print;
using Vodovoz.JournalViewModels;
using Vodovoz.Presentation.ViewModels.AttachedFiles;
using Vodovoz.Services;
using Vodovoz.Services.Fuel;
using Vodovoz.Settings.Logistics;
using Vodovoz.ViewModels.Dialogs.Fuel;
using Vodovoz.ViewModels.Factories;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Sale;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Warehouses;
using Vodovoz.ViewModels.Widgets.Cars;
using Vodovoz.ViewModels.Widgets.Cars.CarVersions;
using Vodovoz.ViewModels.Widgets.Cars.Insurance;
using VodovozInfrastructure.StringHandlers;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class CarViewModel : EntityTabViewModelBase<Car>, IAskSaveOnCloseViewModel
	{
		private readonly IRouteListsWageController _routeListsWageController;
		private readonly IFileDialogService _fileDialogService;
		private readonly ILogger<CarViewModel> _logger;
		private readonly ICarFileStorageService _carFileStorageService;
		private readonly IFuelApiService _fuelApiService;
		private bool _canChangeBottlesFromAddress;

		private IPage<GeoGroupJournalViewModel> _gooGroupPage = null;

		private string _driverInfoText;
		private bool _isNeedToUpdateCarInfoInDriverEntity;
		private int _upcomingTechInspectKmCalculated;
		private readonly ICarEventRepository _carEventRepository;
		private readonly ICarEventSettings _carEventSettings;
		private readonly IFuelRepository _fuelRepository;
		private readonly IDocTemplateRepository _documentTemplateRepository;
		private readonly IUserRepository _userRepository;
		private readonly IUserSettingsService _userSettingsService;
		private readonly CarVersionsManagementViewModel _carVersionsManagementViewModel;
		private readonly IDocumentPrinter _documentPrinter;
		private readonly IInteractiveService _interactiveService;
		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		private byte[] _photo;
		private string _photoFilename;

		private FuelType _oldFuelType;
		private FuelCardVersion _oldLastFuelCardVersion;
		private EmployeeCategory? _oldDriverCategory;
		private CancellationTokenSource _fuelCardUpdateCancellationTokenSource;

		public CarViewModel(
			ILogger<CarViewModel> logger,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			ICarFileStorageService carFileStorageService,
			IFuelApiService fuelApiService,
			IOdometerReadingsViewModelFactory odometerReadingsViewModelFactory,
			IFuelCardVersionViewModelFactory fuelCardVersionViewModelFactory,
			IRouteListsWageController routeListsWageController,
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope,
			IFileDialogService fileDialogService,
			ICarEventRepository carEventRepository,
			ICarEventSettings carEventSettings,
			IFuelRepository fuelRepository,
			IDocTemplateRepository documentTemplateRepository,
			IUserRepository userRepository,
			IStringHandler stringHandler,
			IUserSettingsService userSettingsService,
			ViewModelEEVMBuilder<CarModel> carModelEEVMBuilder,
			ViewModelEEVMBuilder<Employee> driverEEVMBuilder,
			ViewModelEEVMBuilder<FuelType> fuelTypeEEVMBuilder,
			CarInsuranceManagementViewModel insuranceManagementViewModel,
			CarVersionsManagementViewModel carVersionsManagementViewModel,
			IDocumentPrinter documentPrinter,
			IAttachedFileInformationsViewModelFactory attachedFileInformationsViewModelFactory)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			if(navigationManager == null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}

			if(insuranceManagementViewModel is null)
			{
				throw new ArgumentNullException(nameof(insuranceManagementViewModel));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_carFileStorageService = carFileStorageService ?? throw new ArgumentNullException(nameof(carFileStorageService));
			_fuelApiService = fuelApiService ?? throw new ArgumentNullException(nameof(fuelApiService));
			_routeListsWageController = routeListsWageController ?? throw new ArgumentNullException(nameof(routeListsWageController));
			LifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_carEventRepository = carEventRepository ?? throw new ArgumentNullException(nameof(carEventRepository));
			_carEventSettings = carEventSettings ?? throw new ArgumentNullException(nameof(carEventSettings));
			_fuelRepository = fuelRepository ?? throw new ArgumentNullException(nameof(fuelRepository));
			_documentTemplateRepository = documentTemplateRepository ?? throw new ArgumentNullException(nameof(documentTemplateRepository));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			StringHandler = stringHandler ?? throw new ArgumentNullException(nameof(stringHandler));
			_userSettingsService = userSettingsService ?? throw new ArgumentNullException(nameof(userSettingsService));
			_carVersionsManagementViewModel = carVersionsManagementViewModel ?? throw new ArgumentNullException(nameof(carVersionsManagementViewModel));
			_documentPrinter = documentPrinter ?? throw new ArgumentNullException(nameof(documentPrinter));
			_interactiveService = commonServices?.InteractiveService ?? throw new ArgumentNullException(nameof(commonServices.InteractiveService));
			TabName = "Автомобиль";

			_carVersionsManagementViewModel.Initialize(Entity, this);
			CarVersionsViewModel = _carVersionsManagementViewModel.CarVersionsViewModel;
			CarVersionEditingViewModel = _carVersionsManagementViewModel.CarVersionEditingViewModel;

			OdometerReadingsViewModel = (odometerReadingsViewModelFactory ?? throw new ArgumentNullException(nameof(odometerReadingsViewModelFactory)))
				.CreateOdometerReadingsViewModel(Entity);

			insuranceManagementViewModel.Initialize(Entity, this);
			OsagoInsuranceVersionViewModel = insuranceManagementViewModel.OsagoInsuranceVersionViewModel;
			KaskoInsuranceVersionViewModel = insuranceManagementViewModel.KaskoInsuranceVersionViewModel;
			CarInsuranceVersionEditingViewModel = insuranceManagementViewModel.CarInsuranceVersionEditingViewModel;

			FuelCardVersionViewModel = (fuelCardVersionViewModelFactory ?? throw new ArgumentNullException(nameof(fuelCardVersionViewModelFactory)))
				.CreateFuelCardVersionViewModel(Entity, UoW);
			FuelCardVersionViewModel.ParentDialog = this;

			SetPermissions();

			CarModelViewModel = carModelEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(Entity, x => x.CarModel)
				.UseViewModelJournalAndAutocompleter<CarModelJournalViewModel>()
				.UseViewModelDialog<CarModelViewModel>()
				.Finish();

			CarModelViewModel.ChangedByUser += OnCarModelViewModelChangedByUser;

			DriverViewModel = driverEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(Entity, x => x.Driver)
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(filter =>
				{
					filter.Category = EmployeeCategory.driver;
					filter.Status = EmployeeStatus.IsWorking;
				})
				.UseViewModelDialog<EmployeeViewModel>()
				.Finish();

			DriverViewModel.ChangedByUser += OnDriverViewModelChangedByUser;

			FuelTypeViewModel = fuelTypeEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(Entity, x => x.FuelType)
				.UseViewModelJournalAndAutocompleter<FuelTypeJournalViewModel>()
				.UseViewModelDialog<FuelTypeViewModel>()
				.Finish();

			Entity.PropertyChanged += OnEntityPropertyChangedHandler;

			Entity.ObservableCarVersions.ElementAdded += OnObservableCarVersionsElementAdded;

			OnDriverChanged();

			ConfigureTechInspectInfo();
			ConfigureCarTechnicalCheckupInfo();

			if(Entity.Id != 0 && !string.IsNullOrWhiteSpace(Entity.PhotoFileName))
			{
				var photoResult = _carFileStorageService.GetPhotoAsync(Entity, _cancellationTokenSource.Token).GetAwaiter().GetResult();

				if(photoResult.IsSuccess)
				{
					using(var ms = new MemoryStream())
					{
						photoResult.Value.CopyTo(ms);
						Photo = ms.ToArray();
						PhotoFilename = Entity.PhotoFileName;
					}
				}
			}

			AttachedFileInformationsViewModel = attachedFileInformationsViewModelFactory
				.CreateAndInitialize<Car, CarFileInformation>(
					UoW,
					Entity,
					_carFileStorageService,
					_cancellationTokenSource.Token,
					Entity.AddFileInformation,
					Entity.RemoveFileInformation);

			AddGeoGroupCommand = new DelegateCommand(AddGeoGroup);
			CreateCarAcceptanceCertificateCommand = new DelegateCommand(CreateCarAcceptanceCertificate);
			CreateRentalContractCommand = new DelegateCommand(CreateRentalContract);

			_oldFuelType = Entity.FuelType;
			_oldLastFuelCardVersion = GetLastFuelCardVersion();
			_oldDriverCategory = Entity.Driver?.Category;

			SetIsCarUsedInDeliveryDefaultValueIfNeed();
		}
		
		public bool CanEdit { get; private set; }
		
		public bool AskSaveOnClose { get; private set; }
		
		public bool IsArchive
		{
			get => Entity.IsArchive;
			set
			{
				var oldValue = Entity.IsArchive;
				
				if(!CanChangeCompositionCompanyTransportPark)
				{
					const string message = "Невозможно изменить архивацию авто. У Вас нет права менять состав автопарка компании";
					
					if(oldValue != value)
					{
						var activeVersion = Entity.GetActiveCarVersionOnDate();

						if(activeVersion != null && (activeVersion.IsCompanyCar || activeVersion.IsRaskat))
						{
							CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, message);
							OnPropertyChanged();
							return;
						}
					}
				}
				
				Entity.IsArchive = value;
			}
		}

		private void ConfigureTechInspectInfo()
		{
			var lastTechInspectCarEvent = _carEventRepository.GetLastTechInspectCarEvent(UoW, Entity.Id, _carEventSettings.TechInspectCarEventTypeId);

			PreviousTechInspectDate = lastTechInspectCarEvent
				?.StartDate
				.ToShortDateString();

			PreviousTechInspectOdometer = lastTechInspectCarEvent?.Odometer ?? 0;

			UpcomingTechInspectKmCalculated = PreviousTechInspectOdometer + Entity.CarModel?.TeсhInspectInterval ?? 0;

			UpcomingTechInspectLeft = Entity.LeftUntilTechInspect;
		}

		private void ConfigureCarTechnicalCheckupInfo()
		{
			var lastCarTechnicalCheckupEvent =
				_carEventRepository.GetLastCarTechnicalCheckupEvent(UoW, Entity.Id, _carEventSettings.CarTechnicalCheckupEventTypeId);

			LastCarTechnicalCheckupDate =
				lastCarTechnicalCheckupEvent?.CarTechnicalCheckupEndingDate?.ToString("dd.MM.yyyy");
		}

		public string DriverInfoText
		{
			get => _driverInfoText;
			set => SetField(ref _driverInfoText, value);
		}

		public bool CanChangeBottlesFromAddress
		{
			get => _canChangeBottlesFromAddress;
			set => SetField(ref _canChangeBottlesFromAddress, value);
		}
		public AttachedFileInformationsViewModel AttachedFileInformationsViewModel { get; }

		public bool CanChangeCarModel { get; private set; }
		public bool CanEditFuelCardNumber { get; private set; }
		public bool CanViewFuelCard { get; private set; }

		public IStringHandler StringHandler { get; }

		public IEntityEntryViewModel CarModelViewModel { get; }
		public IEntityEntryViewModel DriverViewModel { get; }
		public IEntityEntryViewModel FuelTypeViewModel { get; }

		public ILifetimeScope LifetimeScope { get; }
		public CarVersionsViewModel CarVersionsViewModel { get; }
		public CarVersionEditingViewModel CarVersionEditingViewModel { get; }
		public OdometerReadingsViewModel OdometerReadingsViewModel { get; }
		public FuelCardVersionViewModel FuelCardVersionViewModel { get; }
		public CarInsuranceVersionViewModel OsagoInsuranceVersionViewModel { get; }
		public CarInsuranceVersionViewModel KaskoInsuranceVersionViewModel { get; }
		public CarInsuranceVersionEditingViewModel CarInsuranceVersionEditingViewModel { get; }

		public DelegateCommand AddGeoGroupCommand { get; }
		public DelegateCommand CreateCarAcceptanceCertificateCommand { get; }
		public DelegateCommand CreateRentalContractCommand { get; }
		
		private bool CanChangeCompositionCompanyTransportPark { get; set; }

		protected override bool BeforeSave()
		{
			if(!SetOtherCarsFuelCardVersionEndDateIfNeed())
			{
				return false;
			}

			if(IsNeedToUpdateFuelCardProductRestriction && !IsUserHasAccessToGazprom)
			{
				ShowErrorMessage(
					"Вы выполнили действия, которое требует изменения товарного ограничителя топливной карты в Газпромнефть.\n" +
					"Только пользователи, имеющие доступ в Газпромнефть могут редактировать топливные карты");

				return false;
			}

			var result = base.BeforeSave();

			UpdateArchivingDate();

			UpdateCarInfoInDriverEntity();

			return result;
		}

		private void SavePhotoIfNeeded()
		{
			if(Photo is null)
			{
				return;
			}

			if(PhotoFilename != Entity.PhotoFileName)
			{
				var result = _carFileStorageService
					.UpdatePhotoAsync(
						Entity,
						PhotoFilename,
						new MemoryStream(Photo),
						_cancellationTokenSource.Token)
					.GetAwaiter()
					.GetResult();

				if(result.IsSuccess)
				{
					Entity.PhotoFileName = PhotoFilename;
				}
				else
				{
					_interactiveService.ShowMessage(ImportanceLevel.Error, "Не удалось обновить фотографию автомобиля", "Ошибка");
				}
			}
		}

		private void AddAttachedFilesIfNeeded()
		{
			var errors = new Dictionary<string, string>();
			var repeat = false;

			if(!AttachedFileInformationsViewModel.FilesToAddOnSave.Any())
			{
				return;
			}

			do
			{
				foreach(var fileName in AttachedFileInformationsViewModel.FilesToAddOnSave)
				{
					var result = _carFileStorageService.CreateFileAsync(
						Entity,
						fileName,
						new MemoryStream(AttachedFileInformationsViewModel.AttachedFiles[fileName]), _cancellationTokenSource.Token)
						.GetAwaiter()
						.GetResult();

					if(result.IsFailure && !result.Errors.All(x => x.Code == Application.Errors.S3.FileAlreadyExists.ToString()))
					{
						errors.Add(fileName, string.Join(", ", result.Errors.Select(e => e.Message)));
					}
				}

				if(errors.Any())
				{
					repeat = _interactiveService.Question(
						"Не удалось загрузить файлы:\n" +
						string.Join("\n- ", errors.Select(fekv => $"{fekv.Key} - {fekv.Value}")) + "\n" +
						"\n" +
						"Повторить попытку?",
						"Ошибка загрузки файлов");

					errors.Clear();
				}
				else
				{
					repeat = false;
				}
			}
			while(repeat);
		}

		private void UpdateAttachedFilesIfNeeded()
		{
			if(!AttachedFileInformationsViewModel.FilesToUpdateOnSave.Any())
			{
				return;
			}

			foreach(var fileName in AttachedFileInformationsViewModel.FilesToUpdateOnSave)
			{
				_carFileStorageService.UpdateFileAsync(Entity, fileName, new MemoryStream(AttachedFileInformationsViewModel.AttachedFiles[fileName]), _cancellationTokenSource.Token)
					.GetAwaiter()
					.GetResult();
			}
		}

		private void DeleteAttachedFilesIfNeeded()
		{
			if(!AttachedFileInformationsViewModel.FilesToDeleteOnSave.Any())
			{
				return;
			}

			foreach(var fileName in AttachedFileInformationsViewModel.FilesToDeleteOnSave)
			{
				_carFileStorageService.DeleteFileAsync(Entity, fileName, _cancellationTokenSource.Token)
					.GetAwaiter()
					.GetResult();
			}
		}

		public override bool Save(bool close)
		{
			var routeLists = _carVersionsManagementViewModel.GetAllAffectedRouteLists(UoW);
			if(!routeLists.Any())
			{
				if(!base.Save(false))
				{
					return false;
				}

				SavePhotoIfNeeded();
				AddAttachedFilesIfNeeded();
				UpdateAttachedFilesIfNeeded();
				DeleteAttachedFilesIfNeeded();
				AttachedFileInformationsViewModel.ClearPersistentInformationCommand.Execute();

				return base.Save(close);
			}

			if(!CommonServices.InteractiveService.Question($"Будет пересчитана зарплата в {routeLists.Count} МЛ. Продолжить?"))
			{
				return false;
			}

			_logger.LogInformation("Запущен пересчёт зарплаты в МЛ");

			IPage<ProgressWindowViewModel> progressWindow = null;
			var cts = new CancellationTokenSource();
			bool isProgressWindowClosed = false;

			void OnProgressWindowClosed(object sender, PageClosedEventArgs args)
			{
				cts.Cancel();
				isProgressWindowClosed = true;
			}

			try
			{
				progressWindow = NavigationManager.OpenViewModel<ProgressWindowViewModel>(this);
				progressWindow.PageClosed += OnProgressWindowClosed;
				var progressBarDisplayable = progressWindow.ViewModel.Progress;

				_routeListsWageController.ProgressBarDisplayable = progressBarDisplayable;
				_routeListsWageController.RecalculateRouteListsWage(UoW, routeLists, cts.Token);
				_logger.LogInformation("Пересчёт зарплаты в МЛ завершён");

				progressBarDisplayable.Update("Сохранение...");

				foreach(var routeList in routeLists)
				{
					UoW.Save(routeList);
				}

				if(!base.Save(false))
				{
					return false;
				}

				SavePhotoIfNeeded();
				AddAttachedFilesIfNeeded();
				UpdateAttachedFilesIfNeeded();
				DeleteAttachedFilesIfNeeded();
				AttachedFileInformationsViewModel.ClearPersistentInformationCommand.Execute();

				return base.Save(close);
			}
			catch(OperationCanceledException)
			{
				_logger.LogDebug("Пересчёт зарплаты в МЛ был отменён");
				return false;
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Пересчёт зарплаты в МЛ был отменён: {ExceptionMessage}", e.Message);
				CommonServices.InteractiveService.ShowMessage(QS.Dialog.ImportanceLevel.Error, $"Пересчёт зарплаты в МЛ был отменён: {e.Message}");
				return false;
			}
			finally
			{
				if(progressWindow != null)
				{
					progressWindow.PageClosed -= OnProgressWindowClosed;
					if(!isProgressWindowClosed)
					{
						NavigationManager.ForceClosePage(progressWindow);
					}
				}
			}
		}

		protected override void AfterSave()
		{
			UpdateFuelCardProductRestrictionInGazpromIfNeed();

			base.AfterSave();
		}
		
		private void SetPermissions()
		{
			CanEdit = (Entity.Id == 0 && PermissionResult.CanCreate) || PermissionResult.CanUpdate;
			AskSaveOnClose = CanEdit;
			
			CanChangeBottlesFromAddress = CommonServices.PermissionService.ValidateUserPresetPermission(
				LogisticPermissions.Car.CanChangeCarsBottlesFromAddress, CommonServices.UserService.CurrentUserId);
			
			CanChangeCarModel =
				Entity.Id == 0 || CommonServices.CurrentPermissionService.ValidatePresetPermission(LogisticPermissions.Car.CanChangeCarModel);
			CanEditFuelCardNumber =
				CommonServices.CurrentPermissionService.ValidatePresetPermission(LogisticPermissions.Car.CanChangeFuelCardNumber);
			CanViewFuelCard =
				CommonServices.CurrentPermissionService.ValidateEntityPermission(typeof(FuelCard)).CanUpdate;
			
			CanChangeCompositionCompanyTransportPark =
				CommonServices.CurrentPermissionService.ValidatePresetPermission(CarPermissions.CanChangeCompositionCompanyTransportPark);
		}

		private void UpdateFuelCardProductRestrictionInGazpromIfNeed()
		{
			if(!IsNeedToUpdateFuelCardProductRestriction)
			{
				return;
			}

			var lastFuelCardVersion = GetLastFuelCardVersion();
			var activeFuelCardVersion = Entity.GetCurrentActiveFuelCardVersion();

			if(_fuelCardUpdateCancellationTokenSource != null)
			{
				ShowErrorMessage("В данный момент уже выполняется обновление товарного ограничителя!");

				return;
			}

			_fuelCardUpdateCancellationTokenSource = new CancellationTokenSource();

			_oldFuelType = Entity.FuelType;
			_oldLastFuelCardVersion = GetLastFuelCardVersion();
			_oldDriverCategory = Entity.Driver?.Category;

			try
			{
				if(activeFuelCardVersion != null)
				{
					ChangeFuelCardProductGroupRestriction(activeFuelCardVersion.FuelCard.CardId, _fuelCardUpdateCancellationTokenSource.Token);
				}

				if(lastFuelCardVersion != null
					&& lastFuelCardVersion.FuelCard.Id != activeFuelCardVersion?.FuelCard?.Id
					&& lastFuelCardVersion.StartDate >= DateTime.Today)
				{
					ChangeFuelCardProductGroupRestriction(lastFuelCardVersion.FuelCard.CardId, _fuelCardUpdateCancellationTokenSource.Token);
				}
			}
			catch(Exception ex)
			{
				ShowErrorMessage("При обновлении товарного ограничителя в сервисе Газпром возникла ошибка.\n" +
					"Необходимо с использованием браузера зайти в веб-интерфейс Газпромнефть и убедиться,\n" +
					"что для топливной карты установлен правильный товарный ограничитель!");

				_logger.LogCritical(
					ex,
					"Ошибка при обновлении товарного ограничителя в сервисе Газпром.");
			}
			finally
			{
				_fuelCardUpdateCancellationTokenSource.Dispose();
				_fuelCardUpdateCancellationTokenSource = null;
			}
		}

		private void ChangeFuelCardProductGroupRestriction(string fuelCardId, CancellationToken cancellationToken)
		{
			var isNeedSetProductGroupRestriction = Entity.Driver is null || Entity.Driver?.Category == EmployeeCategory.driver;

			if(isNeedSetProductGroupRestriction)
			{
				SetFuelCardProductGroupRestrictionByCardId(fuelCardId, cancellationToken);
				return;
			}

			SetFuelCardCommonFuelRestrictionByCardId(fuelCardId, cancellationToken);
		}

		private void SetFuelCardCommonFuelRestrictionByCardId(string fuelCardId, CancellationToken cancellationToken)
		{
			_fuelApiService.SetProductRestrictionsAndRemoveExistingByCardId(
				fuelCardId,
				cancellationToken)
				.GetAwaiter()
				.GetResult();
		}			

		private void SetFuelCardProductGroupRestrictionByCardId(string fuelCardId, CancellationToken cancellationToken)
		{
			var gazpromFuelProductsGroups = GazpromFuelProductsGroups.ToList();

			_fuelApiService.SetProductRestrictionsAndRemoveExistingByCardId(
				fuelCardId,
				cancellationToken,
				gazpromFuelProductsGroups)
				.GetAwaiter()
				.GetResult();
		}

		private IEnumerable<string> GazpromFuelProductsGroups =>
			Entity.FuelType is null
			? Enumerable.Empty<string>()
			: _fuelRepository
				.GetGazpromFuelProductsGroupsByFuelTypeId(UoW, Entity.FuelType.Id)
				.Select(x => x.GazpromFuelProductGroupId);

		private FuelCardVersion GetLastFuelCardVersion() =>
			Entity.FuelCardVersions
			.OrderByDescending(x => x.StartDate)
			.FirstOrDefault();

		private bool IsUserHasAccessToGazprom =>
			_userSettingsService.Settings.IsUserHasAuthDataForFuelControlApi;

		private bool IsNeedToUpdateFuelCardProductRestriction =>
			(IsFuelCardChanged() && Entity.FuelType != null)
			|| (IsFuelTypeChanged && IsFuelCardToChangeProductRestrictionAdded)
			|| (IsDriverCategoryChanged && IsFuelCardToChangeProductRestrictionAdded);

		private bool IsFuelCardChanged()
		{
			var currentLastFuelCardVersion = GetLastFuelCardVersion();

			if(_oldLastFuelCardVersion is null && currentLastFuelCardVersion is null)
			{
				return false;
			}

			return _oldLastFuelCardVersion?.Id != currentLastFuelCardVersion?.Id;
		}

		private bool IsFuelTypeChanged =>
			Entity.FuelType != null
			&& _oldFuelType?.Id != Entity.FuelType.Id;

		private bool IsDriverCategoryChanged =>
			!(_oldDriverCategory is null && Entity.Driver?.Category is null)
			&& _oldDriverCategory != Entity.Driver?.Category;

		private bool IsFuelCardToChangeProductRestrictionAdded =>
			Entity.GetCurrentActiveFuelCardVersion() != null
			|| Entity.GetActiveFuelCardVersionOnDate(DateTime.Today.AddDays(1)) != null;

		private bool SetOtherCarsFuelCardVersionEndDateIfNeed()
		{
			var newFuelCardVersion = Entity.FuelCardVersions.Where(v => v.Id == 0).FirstOrDefault();

			if(newFuelCardVersion is null)
			{
				return true;
			}

			var activeVersionsOnDateHavingFuelCard =
				_fuelRepository.GetActiveVersionsOnDateHavingFuelCard(UoW, newFuelCardVersion.StartDate, newFuelCardVersion.FuelCard.Id);

			if(!activeVersionsOnDateHavingFuelCard.Any())
			{
				return true;
			}

			var activeVersionsOnDateHavingFuelCardAndEndDate =
				activeVersionsOnDateHavingFuelCard.Where(v => v.EndDate != null || v.StartDate == newFuelCardVersion.StartDate);

			if(activeVersionsOnDateHavingFuelCardAndEndDate.Any())
			{
				CommonServices.InteractiveService.ShowMessage(
					QS.Dialog.ImportanceLevel.Error,
					$"Создать новую версию топливной карты с указанной датой начала {newFuelCardVersion.StartDate:dd.MM.yyyy} невозможно.\n" +
					$"На указанную дату карта активна у авто: {string.Join(", ", activeVersionsOnDateHavingFuelCardAndEndDate.Select(v => v.Car.RegistrationNumber))}");

				return false;
			}

			var confirmationResult = CommonServices.InteractiveService.Question(
				$"В данный момент указанная топливная карта установлена в активной версии топливных карт авто:\n" +
				$"{string.Join(", ", activeVersionsOnDateHavingFuelCard.Select(v => v.Car.RegistrationNumber))}\n" +
				$"Установить у перечисленных авто дату окончания действия версии топливной карты?");

			if(!confirmationResult)
			{
				return false;
			}

			foreach(var version in activeVersionsOnDateHavingFuelCard)
			{
				version.EndDate = newFuelCardVersion.StartDate.AddMilliseconds(-1);
				UoW.Save(version);
			}

			return true;
		}

		private void UpdateArchivingDate()
		{
			if(IsArchive && Entity.ArchivingDate == null)
			{
				Entity.ArchivingDate = DateTime.Now;
			}

			if(!IsArchive && Entity.ArchivingDate != null)
			{
				Entity.ArchivingDate = null;
			}
		}

		private void OnDriverChanged()
		{
			if(Entity.Driver == null)
			{
				return;
			}

			var docs = Entity.Driver.GetMainDocuments();
			if(docs.Any())
			{
				DriverInfoText =
					$"\tПаспорт: {docs.First().PassportSeria} № {docs.First().PassportNumber}\n" +
					$"\tАдрес регистрации: {Entity.Driver.AddressRegistration}";
			}
			else
			{
				DriverInfoText = "Главный документ отсутствует";
			}
		}

		private void OnDriverViewModelChangedByUser(object sender, EventArgs e)
		{
			SetIsNeedToUpdateCarInfoInDriverEntity();
		}

		private void OnCarModelViewModelChangedByUser(object sender, EventArgs e)
		{
			SetIsNeedToUpdateCarInfoInDriverEntity();
		}

		private void OnObservableCarVersionsElementAdded(object aList, int[] aIdx)
		{
			SetIsNeedToUpdateCarInfoInDriverEntity();
		}

		private void SetIsNeedToUpdateCarInfoInDriverEntity()
		{
			_isNeedToUpdateCarInfoInDriverEntity = !(Entity.Driver is null);
		}

		private void UpdateCarInfoInDriverEntity()
		{
			if(!_isNeedToUpdateCarInfoInDriverEntity
				|| IsArchive
				|| Entity.Driver is null
				|| Entity.Driver.Category != EmployeeCategory.driver)
			{
				return;
			}

			var changesInfo = string.Empty;

			var newCarownType = Entity.CarVersions.OrderByDescending(c => c.StartDate).First().CarOwnType;

			if(Entity.Driver.DriverOfCarOwnType is null || Entity.Driver.DriverOfCarOwnType != newCarownType)
			{
				Entity.Driver.DriverOfCarOwnType = newCarownType;
				changesInfo += "\n- принадлежность автомобиля";
			}

			if(Entity.Driver.DriverOfCarTypeOfUse is null || Entity.Driver.DriverOfCarTypeOfUse != Entity.CarModel.CarTypeOfUse)
			{
				Entity.Driver.DriverOfCarTypeOfUse = Entity.CarModel.CarTypeOfUse;
				changesInfo += "\n- тип автомобиля";
			}

			if(!string.IsNullOrEmpty(changesInfo))
			{
				CommonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Warning,
					$"Внимание! В карточке водителя будут обновлены:{changesInfo}");
			}

			_isNeedToUpdateCarInfoInDriverEntity = false;
		}

		#region Add GeoGroup

		public string PreviousTechInspectDate { get; private set; }
		public int PreviousTechInspectOdometer { get; private set; }
		public string LastCarTechnicalCheckupDate { get; private set; }

		[PropertyChangedAlso(nameof(UpcomingTechInspectKm))]
		public int UpcomingTechInspectKmCalculated
		{
			get => _upcomingTechInspectKmCalculated;
			private set => SetField(ref _upcomingTechInspectKmCalculated, value);
		}

		public int UpcomingTechInspectKm
		{
			get => Entity.TechInspectForKm ?? UpcomingTechInspectKmCalculated;
			set
			{
				if(Entity.TechInspectForKm != value)
				{
					if(UpcomingTechInspectKmCalculated != value)
					{
						Entity.TechInspectForKm = value;
					}
					else
					{
						Entity.TechInspectForKm = null;
					}
				}
			}
		}

		public int UpcomingTechInspectLeft { get; private set; }

		public byte[] Photo
		{
			get => _photo;
			set => SetField(ref _photo, value);
		}

		public string PhotoFilename
		{
			get => _photoFilename;
			set => SetField(ref _photoFilename, value);
		}

		public void ShowErrorMessage(string message)
		{
			base.ShowErrorMessage(message);
		}

		private void AddGeoGroup()
		{
			if(_gooGroupPage != null)
			{
				NavigationManager.SwitchOn(_gooGroupPage);
				return;
			}

			_gooGroupPage = NavigationManager.OpenViewModel<GeoGroupJournalViewModel>(
				this,
				OpenPageOptions.AsSlave,
				viewModel =>
				{
					viewModel.SelectionMode = JournalSelectionMode.Multiple;
					viewModel.DisableChangeEntityActions();
				});

			_gooGroupPage.ViewModel.OnSelectResult += OnJournalGeoGroupsSelectedResult;
			_gooGroupPage.PageClosed += OnGeoGroupPagePageClosed;
		}

		private void OnGeoGroupPagePageClosed(object sender, PageClosedEventArgs e)
		{
			_gooGroupPage.PageClosed -= OnGeoGroupPagePageClosed;
			_gooGroupPage.ViewModel.OnSelectResult -= OnJournalGeoGroupsSelectedResult;

			_gooGroupPage = null;
		}

		private void OnJournalGeoGroupsSelectedResult(object sender, JournalSelectedEventArgs e)
		{
			var selected = e.SelectedObjects.Cast<GeoGroupJournalNode>();
			if(!selected.Any())
			{
				return;
			}
			foreach(var item in selected)
			{
				if(!Entity.ObservableGeographicGroups.Any(x => x.Id == item.Id))
				{
					var group = UoW.GetById<GeoGroup>(item.Id);
					Entity.ObservableGeographicGroups.Add(group);
				}
			}
		}

		#endregion Add GeoGroup

		private void CreateCarAcceptanceCertificate()
		{
			NavigationManager.OpenViewModel<ShiftChangeResidueDocumentViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate(), OpenPageOptions.AsSlave, viewModel =>
			{
				viewModel.Entity.ShiftChangeResidueDocumentType = Domain.Documents.ShiftChangeResidueDocumentType.Car;
				viewModel.Entity.Car = viewModel.UoW.GetById<Car>(Entity.Id);
			});
		}

		private void CreateRentalContract()
		{
			var actualCarVersion = Entity.CarVersions.FirstOrDefault(x => x.EndDate is null);

			var contract = CarRentalContract.Create(UoW, _documentTemplateRepository, Entity, actualCarVersion?.CarOwnerOrganization, Entity.Driver);

			if(contract.IsFailure)
			{
				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Error, string.Join("\n", contract.Errors.Select(e => e.Message)));
				return;
			}

			_documentPrinter.PrintAllODTDocuments(new[] { contract.Value });
		}

		private void OnEntityPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.Driver) && Entity.Driver != null)
			{
				OnDriverChanged();
				return;
			}

			if(e.PropertyName == nameof(Entity.TechInspectForKm))
			{
				OnPropertyChanged(nameof(UpcomingTechInspectKm));
				return;
			}
		}

		private void SetIsCarUsedInDeliveryDefaultValueIfNeed()
		{
			if(Entity.Id != 0)
			{
				return;
			}

			Entity.IsUsedInDelivery = true;
		}

		public override void Dispose()
		{
			Entity.ObservableCarVersions.ElementAdded -= OnObservableCarVersionsElementAdded;
			CarModelViewModel.ChangedByUser -= OnCarModelViewModelChangedByUser;
			DriverViewModel.ChangedByUser -= OnDriverViewModelChangedByUser;
			Entity.PropertyChanged -= OnEntityPropertyChangedHandler;

			base.Dispose();
		}
	}
}

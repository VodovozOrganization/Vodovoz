using Autofac;
using Microsoft.Extensions.Logging;
using QS.Attachments.ViewModels.Widgets;
using QS.Commands;
using QS.Dialog;
using QS.Dialog.ViewModels;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using System.Linq;
using System.Threading;
using Vodovoz.Controllers;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Fuel;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Fuel;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Factories;
using Vodovoz.JournalViewModels;
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
using Vodovoz.ViewModels.Widgets.Cars.Insurance;
using Vodovoz.ViewModels.Widgets.Cars.CarVersions;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class CarViewModel : EntityTabViewModelBase<Car>
	{
		private readonly IRouteListsWageController _routeListsWageController;
		private readonly ILogger<CarViewModel> _logger;
		private bool _canChangeBottlesFromAddress;

		private IPage<GeoGroupJournalViewModel> _gooGroupPage = null;

		private AttachmentsViewModel _attachmentsViewModel;
		private string _driverInfoText;
		private bool _isNeedToUpdateCarInfoInDriverEntity;
		private readonly ICarEventRepository _carEventRepository;
		private readonly ICarEventSettings _carEventSettings;
		private readonly IFuelRepository _fuelRepository;
		private readonly CarVersionsManagementViewModel _carVersionsManagementViewModel;

		public CarViewModel(
			ILogger<CarViewModel> logger,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IAttachmentsViewModelFactory attachmentsViewModelFactory,
			IOdometerReadingsViewModelFactory odometerReadingsViewModelFactory,
			IFuelCardVersionViewModelFactory fuelCardVersionViewModelFactory,
			IRouteListsWageController routeListsWageController,
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope,
			ICarEventRepository carEventRepository,
			ICarEventSettings carEventSettings,
			IFuelRepository fuelRepository,
			ViewModelEEVMBuilder<CarModel> carModelEEVMBuilder,
			ViewModelEEVMBuilder<Employee> driverEEVMBuilder,
			ViewModelEEVMBuilder<FuelType> fuelTypeEEVMBuilder,
			CarInsuranceManagementViewModel insuranceManagementViewModel,
			CarVersionsManagementViewModel carVersionsManagementViewModel)
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
			_routeListsWageController = routeListsWageController ?? throw new ArgumentNullException(nameof(routeListsWageController));
			LifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_carEventRepository = carEventRepository ?? throw new ArgumentNullException(nameof(carEventRepository));
			_carEventSettings = carEventSettings ?? throw new ArgumentNullException(nameof(carEventSettings));
			_fuelRepository = fuelRepository ?? throw new ArgumentNullException(nameof(fuelRepository));
			_carVersionsManagementViewModel = carVersionsManagementViewModel ?? throw new ArgumentNullException(nameof(carVersionsManagementViewModel));

			TabName = "Автомобиль";

			AttachmentsViewModel = attachmentsViewModelFactory.CreateNewAttachmentsViewModel(Entity.ObservableAttachments);

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

			CanChangeBottlesFromAddress = commonServices.PermissionService.ValidateUserPresetPermission(
				Vodovoz.Permissions.Logistic.Car.CanChangeCarsBottlesFromAddress,
				commonServices.UserService.CurrentUserId);

			CanChangeCarModel =
				Entity.Id == 0 || commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.Logistic.Car.CanChangeCarModel);
			CanEditFuelCardNumber =
				commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.Logistic.Car.CanChangeFuelCardNumber);
			CanViewFuelCard =
				commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(FuelCard)).CanUpdate;

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

			Entity.PropertyChanged += (sender, args) =>
			{
				if(args.PropertyName == nameof(Entity.Driver) && Entity.Driver != null)
				{
					OnDriverChanged();
				}
			};

			Entity.ObservableCarVersions.ElementAdded += OnObservableCarVersionsElementAdded;

			OnDriverChanged();

			ConfigureTechInspectInfo();

			AddGeoGroupCommand = new DelegateCommand(AddGeoGroup);
			CreateCarAcceptanceCertificateCommand = new DelegateCommand(CreateCarAcceptanceCertificate);
		}

		private void ConfigureTechInspectInfo()
		{
			var lastTechInspectCarEvent = _carEventRepository.GetLastTechInspectCarEvent(UoW, Entity.Id, _carEventSettings.TechInspectCarEventTypeId);

			PreviousTechInspectDate = lastTechInspectCarEvent
				?.StartDate
				.ToShortDateString();

			PreviousTechInspectOdometer = lastTechInspectCarEvent?.Odometer ?? 0;

			UpcomingTechInspectKm = PreviousTechInspectOdometer + Entity.CarModel?.TeсhInspectInterval ?? 0;

			UpcomingTechInspectLeft = Entity.LeftUntilTechInspect;
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

		public AttachmentsViewModel AttachmentsViewModel
		{
			get => _attachmentsViewModel;
			set => SetField(ref _attachmentsViewModel, value);
		}

		public bool CanChangeCarModel { get; }
		public bool CanEditFuelCardNumber { get; }
		public bool CanViewFuelCard { get; }

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

		protected override bool BeforeSave()
		{
			if(!SetOtherCarsFuelCardVersionEndDateIfNeed())
			{
				return false;
			}

			var result = base.BeforeSave();

			UpdateArchivingDate();

			UpdateCarInfoInDriverEntity();

			return result;
		}

		public override bool Save(bool close)
		{
			var routeLists = _carVersionsManagementViewModel.GetAllAffectedRouteLists(UoW);
			if(!routeLists.Any())
			{
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
			if(Entity.IsArchive && Entity.ArchivingDate == null)
			{
				Entity.ArchivingDate = DateTime.Now;
			}

			if(!Entity.IsArchive && Entity.ArchivingDate != null)
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
				|| Entity.IsArchive
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

		public DelegateCommand AddGeoGroupCommand { get; }
		public DelegateCommand CreateCarAcceptanceCertificateCommand { get; }

		public string PreviousTechInspectDate { get; private set; }
		public int PreviousTechInspectOdometer { get; private set; }
		public int UpcomingTechInspectKm { get; private set; }
		public int UpcomingTechInspectLeft { get; private set; }

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

		public override void Dispose()
		{
			Entity.ObservableCarVersions.ElementAdded -= OnObservableCarVersionsElementAdded;
			CarModelViewModel.ChangedByUser -= OnCarModelViewModelChangedByUser;
			DriverViewModel.ChangedByUser -= OnDriverViewModelChangedByUser;

			base.Dispose();
		}
	}
}

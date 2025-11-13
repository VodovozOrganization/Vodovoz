using Autofac;
using NLog;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Permissions;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Extension;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Vodovoz.Application.FileStorage;
using Vodovoz.Controllers;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Users.Settings;
using Vodovoz.Domain;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.EntityRepositories.WageCalculation;
using Vodovoz.Factories;
using Vodovoz.FilterViewModels.Organization;
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.Presentation.ViewModels.AttachedFiles;
using Vodovoz.Services;
using Vodovoz.Settings.Contacts;
using Vodovoz.Settings.Delivery;
using Vodovoz.Settings.Organizations;
using Vodovoz.TempAdapters;
using Vodovoz.Tools.Logistic;
using Vodovoz.ViewModels.Infrastructure.Services;
using Vodovoz.ViewModels.Journals.FilterViewModels.Counterparties;
using Vodovoz.ViewModels.Journals.JournalViewModels.Client;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Logistic;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Contacts;
using Vodovoz.ViewModels.ViewModels.Organizations;
using VodovozInfrastructure.Endpoints;
using EmployeeSettings = Vodovoz.Settings.Employee;
using PhoneViewModel = Vodovoz.ViewModels.ViewModels.Counterparty.PhoneViewModel;

namespace Vodovoz.ViewModels.ViewModels.Employees
{
	public class EmployeeViewModel : TabViewModelBase, ITdiDialog, ISingleUoWDialog, IAskSaveOnCloseViewModel
	{
		private readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IAuthorizationService _authorizationService;
		private readonly ISubdivisionSettings _subdivisionSettings;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IWageCalculationRepository _wageCalculationRepository;
		private readonly IWarehouseRepository _warehouseRepository;
		private readonly IRouteListRepository _routeListRepository;
		private readonly ICarRepository _carRepository;
		private readonly DriverApiUserRegisterEndpoint _driverApiUserRegisterEndpoint;
		private readonly UserSettings _userSettings;
		private readonly IUserRepository _userRepository;
		private readonly IWageSettings _wageSettings;
		private readonly IOrganizationRepository _organizationRepository;
		private readonly EmployeeSettings.IEmployeeSettings _employeeSettings;
		private readonly INomenclatureFixedPriceController _nomenclatureFixedPriceController;
		private readonly IEmployeeFileStorageService _employeeFileStorageService;
		private readonly ViewModelEEVMBuilder<Phone> _phoneViewModelEEVMBuilder;
		private readonly IEmployeeRegistrationVersionController _employeeRegistrationVersionController;
		private readonly IInteractiveService _interactiveService;
		private readonly Vodovoz.Settings.Nomenclature.INomenclatureSettings _nomenclatureSettings;
		private readonly IDeliveryScheduleSettings _deliveryScheduleSettings;
		private IPermissionResult _employeeDocumentsPermissionsSet;
		private readonly IPermissionResult _employeePermissionSet;
		private bool _canActivateDriverDistrictPrioritySetPermission;
		private bool _canChangeTraineeToDriver;
		private bool _canRegisterDriverAppUser;
		private bool _canRegisterWarehouseAppUser;
		private DriverWorkScheduleSet _selectedDriverScheduleSet;
		private DriverDistrictPrioritySet _selectedDistrictPrioritySet;
		private Employee _employeeForCurrentUser;
		private IEnumerable<EmployeeDocument> _selectedEmployeeDocuments = new EmployeeDocument[0];
		private IEnumerable<EmployeeContract> _selectedEmployeeContracts = new EmployeeContract[0];
		private ValidationContext _validationContext;
		private TerminalManagementViewModel _terminalManagementViewModel;
		private DateTime? _selectedRegistrationDate;
		private EmployeeRegistrationVersion _selectedRegistrationVersion;
		private bool _counterpartyChangedByUser;
		private bool _statusChangedByUser;

		private DelegateCommand _openDistrictPrioritySetCreateWindowCommand;
		private DelegateCommand _openDistrictPrioritySetEditWindowCommand;
		private DelegateCommand _copyDistrictPrioritySetCommand;
		private DelegateCommand _activateDistrictPrioritySetCommand;
		private DelegateCommand _openDriverWorkScheduleSetCreateWindowCommand;
		private DelegateCommand _openDriverWorkScheduleSetEditWindowCommand;
		private DelegateCommand _copyDriverWorkScheduleSetCommand;
		private DelegateCommand _removeEmployeeDocumentsCommand;
		private DelegateCommand _removeEmployeeContractsCommand;
		private DelegateCommand _registerDriverAppUserOrAddRoleCommand;
		private DelegateCommand _createNewEmployeeRegistrationVersionCommand;
		private DelegateCommand _changeEmployeeRegistrationVersionStartDateCommand;

		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		public IReadOnlyList<Organization> organizations;
		private string _photoFilename;
		private byte[] _photo;

		public event EventHandler<EntitySavedEventArgs> EntitySaved;

		public EmployeeViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IEntityUoWBuilder entityUoWBuilder,
			IAuthorizationService authorizationService,
			IEmployeeWageParametersFactory employeeWageParametersFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			IEmployeePostsJournalFactory employeePostsJournalFactory,
			ISubdivisionSettings subdivisionSettings,
			IWageCalculationRepository wageCalculationRepository,
			IEmployeeRepository employeeRepository,
			ICommonServices commonServices,
			IValidationContextFactory validationContextFactory,
			IWarehouseRepository warehouseRepository,
			IRouteListRepository routeListRepository,
			ICarRepository carRepository,
			DriverApiUserRegisterEndpoint driverApiUserRegisterEndpoint,
			UserSettings userSettings,
			IUserRepository userRepository,
			IWageSettings wageSettings,
			IAttachmentsViewModelFactory attachmentsViewModelFactory,
			INavigationManager navigationManager,
			IOrganizationRepository organizationRepository,
			ILifetimeScope lifetimeScope,
			Vodovoz.Settings.Nomenclature.INomenclatureSettings nomenclatureSettings,
			IDeliveryScheduleSettings deliveryScheduleSettings,
			EmployeeSettings.IEmployeeSettings employeeSettings,
			INomenclatureFixedPriceController nomenclatureFixedPriceController,
			IEmployeeFileStorageService employeeFileStorageService,
			IAttachedFileInformationsViewModelFactory attachedFileInformationsViewModelFactory,
			ViewModelEEVMBuilder<Phone> phoneViewModelEEVMBuilder,
			bool traineeToEmployee = false) : base(commonServices?.InteractiveService, navigationManager)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
			EmployeeWageParametersFactory =
				employeeWageParametersFactory ?? throw new ArgumentNullException(nameof(employeeWageParametersFactory));
			EmployeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			EmployeePostsJournalFactory =
				employeePostsJournalFactory ?? throw new ArgumentNullException(nameof(employeePostsJournalFactory)); 
			_subdivisionSettings =
				subdivisionSettings ?? throw new ArgumentNullException(nameof(subdivisionSettings));
			_wageCalculationRepository = wageCalculationRepository ?? throw new ArgumentNullException(nameof(wageCalculationRepository));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_warehouseRepository = warehouseRepository ?? throw new ArgumentNullException(nameof(warehouseRepository));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_carRepository = carRepository ?? throw new ArgumentNullException(nameof(carRepository));
			_driverApiUserRegisterEndpoint = driverApiUserRegisterEndpoint ?? throw new ArgumentNullException(nameof(driverApiUserRegisterEndpoint));
			_userSettings = userSettings ?? throw new ArgumentNullException(nameof(userSettings));
			UoWGeneric = entityUoWBuilder.CreateUoW<Employee>(unitOfWorkFactory, TabName);
			CommonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_wageSettings = wageSettings ?? throw new ArgumentNullException(nameof(wageSettings));
			_organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
			LifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_deliveryScheduleSettings = deliveryScheduleSettings ?? throw new ArgumentNullException(nameof(deliveryScheduleSettings));
			_employeeSettings = employeeSettings ?? throw new ArgumentNullException(nameof(employeeSettings));
			_nomenclatureFixedPriceController =
				nomenclatureFixedPriceController ?? throw new ArgumentNullException(nameof(nomenclatureFixedPriceController));
			_employeeFileStorageService = employeeFileStorageService ?? throw new ArgumentNullException(nameof(employeeFileStorageService));
			_phoneViewModelEEVMBuilder = phoneViewModelEEVMBuilder ?? throw new ArgumentNullException(nameof(phoneViewModelEEVMBuilder));
			_employeeRegistrationVersionController = new EmployeeRegistrationVersionController(Entity, new EmployeeRegistrationVersionFactory());
			_interactiveService = commonServices?.InteractiveService ?? throw new ArgumentNullException(nameof(commonServices.InteractiveService));

			if(validationContextFactory == null)
			{
				throw new ArgumentNullException(nameof(validationContextFactory));
			}
			
			ConfigureValidationContext(validationContextFactory);

			PhonesViewModel = new PhonesViewModel(
				CommonServices,
				LifetimeScope.Resolve<IPhoneRepository>(),
				UoW,
				LifetimeScope.Resolve<IContactSettings>(),
				LifetimeScope.Resolve<IPhoneTypeSettings>(),
				LifetimeScope.Resolve<IExternalCounterpartyController>()
			)
			{
				PhonesList = new GenericObservableList<Phone>(Entity.Phones)
			};

			if(Entity.Id == 0)
			{
				Entity.OrganisationForSalary = _organizationRepository.GetCommonOrganisation(UoW);
				FillHiddenCategories(traineeToEmployee);

				TabName = "Новый сотрудник";
			}
			else
			{
				TabName = Entity.GetPersonNameWithInitials();
			}
			
			if(Entity.Phones == null)
			{
				Entity.Phones = new List<Phone>();
			}

			organizations = UoW.GetAll<Organization>().ToList();

			GetExternalUsers();

			_employeePermissionSet = CommonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Employee));

			if(!_employeePermissionSet.CanRead) {
				AbortOpening(PermissionsSettings.GetEntityReadValidateResult(typeof(Employee)));
			}

			SetPermissions();
			CreateCommands();
			InitializeSubdivisionEntryViewModel();

			if(Entity.Id != 0 && !string.IsNullOrWhiteSpace(Entity.PhotoFileName))
			{
				var photoResult = _employeeFileStorageService.GetPhotoAsync(Entity, _cancellationTokenSource.Token).GetAwaiter().GetResult();

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

			AttachedFileInformationsViewModel = attachedFileInformationsViewModelFactory.CreateAndInitialize<Employee, EmployeeFileInformation>(
				UoW,
				Entity,
				_employeeFileStorageService,
				_cancellationTokenSource.Token,
				Entity.AddFileInformation,
				Entity.RemoveFileInformation);

			PhoneForCounterpartyCallsViewModel = _phoneViewModelEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(Entity, e => e.PhoneForCounterpartyCalls)
				.UseViewModelJournalAndAutocompleter<PhonesJournalViewModel, PhonesJournalFilterViewModel>(filter =>
				{
					filter.RestrictEmployee = Entity;
				})
				.UseViewModelDialog<PhoneViewModel>()
				.Finish();

			PhoneForCounterpartyCallsViewModel.CanViewEntity = false;

			Entity.PropertyChanged += OnEntityPropertyChanged;
		}

		public ILifetimeScope LifetimeScope { get; private set; }
		public ExternalApplicationUser DriverAppUser { get; private set; }
		public ExternalApplicationUser WarehouseAppUser { get; private set; }

		private void InitializeSubdivisionEntryViewModel()
		{
			var subdivisionEntryViewModelBuilder =
				new CommonEEVMBuilderFactory<Employee>(this, Entity, UoW, NavigationManager, LifetimeScope);

			var canSetOnlyLogisticsSubdivision = CanManageDriversAndForwarders && !CanManageOfficeWorkers;

			SubdivisionViewModel = subdivisionEntryViewModelBuilder
				.ForProperty(x => x.Subdivision)
				.UseViewModelJournalAndAutocompleter<SubdivisionsJournalViewModel, SubdivisionFilterViewModel>(
					filter =>
					{
						if(canSetOnlyLogisticsSubdivision)
						{
							filter.SubdivisionType  = SubdivisionType.Logistic;
						}
					})
				.UseViewModelDialog<SubdivisionViewModel>()
				.Finish();

			SubdivisionViewModel.IsEditable = CanEditEmployee && (CanManageOfficeWorkers || CanManageDriversAndForwarders);
		}

		private Employee EmployeeForCurrentUser => 
			_employeeForCurrentUser ?? (_employeeForCurrentUser = _employeeRepository.GetEmployeeForCurrentUser(UoW));

		public List<EmployeeCategory> HiddenCategories { get; } = new List<EmployeeCategory>();
		
		public EmployeeDocumentType[] HiddenForRussianDocument { get; } =
		{
			EmployeeDocumentType.RefugeeId,
			EmployeeDocumentType.RefugeeCertificate,
			EmployeeDocumentType.Residence,
			EmployeeDocumentType.ForeignCitizenPassport
		};

		public EmployeeDocumentType[] HiddenForForeignCitizen { get; } =
		{
			EmployeeDocumentType.MilitaryID,
			EmployeeDocumentType.NavyPassport,
			EmployeeDocumentType.OfficerCertificate
		};
		
		public ICommonServices CommonServices { get; }
		public IUnitOfWork UoW => UoWGeneric;
		public Employee Entity => UoWGeneric.Root;
		public bool CanCopyId => Entity.Id != 0;
		public IUnitOfWorkGeneric<Employee> UoWGeneric { get; }
		public IEmployeeWageParametersFactory EmployeeWageParametersFactory { get; }
		public IEmployeeJournalFactory EmployeeJournalFactory { get; }
		public IEmployeePostsJournalFactory EmployeePostsJournalFactory { get; }
		public IEntityEntryViewModel SubdivisionViewModel { get; private set; }

		public bool HasChanges
		{
			get
			{
				PhonesViewModel.RemoveEmpty();

				return UoWGeneric.HasChanges
					|| !string.IsNullOrEmpty(Entity.LoginForNewUser)
					|| (_terminalManagementViewModel?.HasChanges ?? false);
			}
		}

		public virtual bool HasCustomCancellationConfirmationDialog => false;

		public virtual Func<int> CustomCancellationConfirmationDialogFunc => null;

		public bool AskSaveOnClose => CanEditEmployee;
		
		public IPermissionResult DriverDistrictPrioritySetPermission { get; private set; }
		public IPermissionResult DriverWorkScheduleSetPermission { get; private set; }

		public PhonesViewModel PhonesViewModel { get; }
		
		public TerminalManagementViewModel TerminalManagementViewModel =>
			_terminalManagementViewModel ?? (_terminalManagementViewModel =
				new TerminalManagementViewModel(
					_userSettings.DefaultWarehouse,
					Entity,
					this as ITdiTab,
					_employeeRepository,
					_warehouseRepository,
					_routeListRepository,
					CommonServices,
					UoW,
					_unitOfWorkFactory,
					_nomenclatureSettings));

		public bool CanReadEmployeeDocuments { get; private set; }
		public bool CanAddEmployeeDocument { get; private set; }
		public bool CanManageUsers { get; private set; }
		public bool CanManageDriversAndForwarders { get; private set; }
		public bool CanManageOfficeWorkers { get; private set; }
		public bool CanCreateNewUser => Entity.User == null && CanManageUsers;
		public bool CanEditEmployeeCategory => Entity?.Id == 0 && (CanManageOfficeWorkers || CanManageDriversAndForwarders) && CanEditEmployee;
		public bool CanEditWage { get; private set; }
		public bool CanEditOrganisationForSalary { get; private set; }
		public bool CanEditEmployee { get; private set; }
		public bool CanReadEmployee { get; private set; }
		public bool CanChangeEmployeeCounterparty { get; private set; }

		public bool CanRegisterDriverAppUser
		{
			get => _canRegisterDriverAppUser && CanEditEmployee;
			set
			{
				if(SetField(ref _canRegisterDriverAppUser, value))
				{
					OnPropertyChanged(nameof(IsValidNewDriverAppUser));
				}
			}
		}
		
		public bool CanRegisterWarehouseAppUser
		{
			get => _canRegisterWarehouseAppUser && CanEditEmployee;
			set
			{
				if(SetField(ref _canRegisterWarehouseAppUser, value))
				{
					OnPropertyChanged(nameof(IsValidNewWarehouseAppUser));
				}
			}
		}

		public bool IsValidNewDriverAppUser =>
			!string.IsNullOrWhiteSpace(DriverAppUser.Login)
				&& DriverAppUser.Password?.Length >= 3
				&& CanRegisterDriverAppUser
				&& CanEditEmployee;
		
		public bool IsValidNewWarehouseAppUser =>
			!string.IsNullOrWhiteSpace(WarehouseAppUser.Login)
			&& WarehouseAppUser.Password?.Length >= 3
			&& CanRegisterWarehouseAppUser
			&& CanEditEmployee;

		public bool CanCopyWarehouseAppUserCredentialsToDriverUser =>
			Entity.DriverAppUser is null
			&& Entity.WarehouseAppUser != null
			&& CanEditEmployee;

		public string AddDriverAppLoginInfo => CanRegisterDriverAppUser
			? "<span color=\"red\">Имя пользователя и пароль нельзя будет изменить!\n" +
			"Не забудьте нажать кнопку 'Добавить пользователя'</span>"
			: "";

		public DriverDistrictPrioritySet SelectedDistrictPrioritySet
		{
			get => _selectedDistrictPrioritySet;
			set
			{
				if(SetField(ref _selectedDistrictPrioritySet, value))
				{
					OnPropertyChanged(nameof(CanCopyDistrictPrioritySet));
					OnPropertyChanged(nameof(CanEditDistrictPrioritySet));
					OnPropertyChanged(nameof(CanActivateDistrictPrioritySet));
				}
			}
		}
		
		public bool CanActivateDistrictPrioritySet =>
			SelectedDistrictPrioritySet != null
			&& !SelectedDistrictPrioritySet.IsActive
			&& SelectedDistrictPrioritySet.DateActivated == null
			&& SelectedDistrictPrioritySet.ObservableDriverDistrictPriorities
				.All(x => x.District.DistrictsSet.Status == DistrictsSetStatus.Active)
			&& _canActivateDriverDistrictPrioritySetPermission
			&& CanEditEmployee;
		
		public bool CanCopyDistrictPrioritySet => SelectedDistrictPrioritySet != null && DriverDistrictPrioritySetPermission.CanCreate && CanEditEmployee;
		public bool CanEditDistrictPrioritySet => 
			SelectedDistrictPrioritySet != null 
			&& (DriverDistrictPrioritySetPermission.CanUpdate || DriverDistrictPrioritySetPermission.CanRead);

		public DriverWorkScheduleSet SelectedDriverScheduleSet
		{
			get => _selectedDriverScheduleSet;
			set
			{
				if(SetField(ref _selectedDriverScheduleSet, value))
				{
					OnPropertyChanged(nameof(CanCopyDriverScheduleSet));
					OnPropertyChanged(nameof(CanEditDriverScheduleSet));
				}
			}
		}

		public IEnumerable<EmployeeDocument> SelectedEmployeeDocuments
		{
			get => _selectedEmployeeDocuments;
			set
			{
				if(SetField(ref _selectedEmployeeDocuments, value))
				{
					OnPropertyChanged(nameof(CanReadEmployeeDocument));
					OnPropertyChanged(nameof(CanRemoveEmployeeDocument));
				}
			}
		}

		public bool CanReadEmployeeDocument => CanReadEmployeeDocuments && SelectedEmployeeDocuments.Any() && CanReadEmployee;
		
		public bool CanRemoveEmployeeDocument =>
			_employeeDocumentsPermissionsSet.CanDelete && SelectedEmployeeDocuments.Any() && CanEditEmployee;

		public IEnumerable<EmployeeContract> SelectedEmployeeContracts
		{
			get => _selectedEmployeeContracts;
			set
			{
				if(SetField(ref _selectedEmployeeContracts, value))
				{
					OnPropertyChanged(nameof(CanEditEmployeeContract));
					OnPropertyChanged(nameof(CanRemoveEmployeeContract));
				}
			}
		}
		
		public bool CanEditEmployeeContract => SelectedEmployeeContracts.Any() && CanEditEmployee;
		public bool CanRemoveEmployeeContract => SelectedEmployeeContracts.Any() && CanEditEmployee;
		
		public bool CanCopyDriverScheduleSet => SelectedDriverScheduleSet != null && DriverWorkScheduleSetPermission.CanCreate && CanEditEmployee;
		
		public bool CanEditDriverScheduleSet => 
			SelectedDriverScheduleSet != null
			&& (DriverWorkScheduleSetPermission.CanUpdate || DriverWorkScheduleSetPermission.CanRead)
			&& CanEditEmployee;
		
		public DateTime? SelectedRegistrationDate
		{
			get => _selectedRegistrationDate;
			set
			{
				if(!SetField(ref _selectedRegistrationDate, value))
				{
					return;
				}

				OnPropertyChanged(nameof(CanAddNewRegistrationVersion));
				OnPropertyChanged(nameof(CanChangeRegistrationVersionDate));
			}
		}
		
		public EmployeeRegistrationVersion SelectedRegistrationVersion
		{
			get => _selectedRegistrationVersion;
			set
			{
				if(SetField(ref _selectedRegistrationVersion, value))
				{
					OnPropertyChanged(nameof(CanChangeRegistrationVersionDate));
				}
			}
		}
		
		public bool CanAddNewRegistrationVersion =>
			CanEditEmployee
			&& SelectedRegistrationDate.HasValue
			&& Entity.ObservableEmployeeRegistrationVersions.All(x => x.Id != 0);

		public bool CanChangeRegistrationVersionDate =>
			CanEditEmployee
			&& SelectedRegistrationDate.HasValue
			&& SelectedRegistrationVersion != null;
		
		public DelegateCommand OpenDistrictPrioritySetCreateWindowCommand =>
			_openDistrictPrioritySetCreateWindowCommand ?? (_openDistrictPrioritySetCreateWindowCommand = new DelegateCommand(
					() =>
					{
						var newDistrictPrioritySet = new DriverDistrictPrioritySet
						{
							Driver = Entity,
							IsCreatedAutomatically = false
						};

						var driverDistrictPrioritySetViewModel = new DriverDistrictPrioritySetViewModel(
							newDistrictPrioritySet,
							UoW,
							_unitOfWorkFactory,
							CommonServices,
							_employeeRepository
						);

						driverDistrictPrioritySetViewModel.EntityAccepted += (o, eventArgs) => {
							var now = DateTime.Now;
							eventArgs.AcceptedEntity.DateCreated = now;
							eventArgs.AcceptedEntity.DateLastChanged = now;
							Entity.AddDriverDistrictPrioritySet(eventArgs.AcceptedEntity);
						};

						TabParent.AddSlaveTab(this, driverDistrictPrioritySetViewModel);
					}
				)
			);
		
		public DelegateCommand OpenDistrictPrioritySetEditWindowCommand =>
			_openDistrictPrioritySetEditWindowCommand ?? (_openDistrictPrioritySetEditWindowCommand = new DelegateCommand(
					() =>
					{
						var driverDistrictPrioritySetViewModel = new DriverDistrictPrioritySetViewModel(
							SelectedDistrictPrioritySet,
							UoW,
							_unitOfWorkFactory,
							CommonServices,
							_employeeRepository
						);

						driverDistrictPrioritySetViewModel.EntityAccepted += (o, eventArgs) => 
						{
							eventArgs.AcceptedEntity.DateLastChanged = DateTime.Now;
						};

						TabParent.AddSlaveTab(this, driverDistrictPrioritySetViewModel);
					}
				)
			);

		public DelegateCommand CopyDistrictPrioritySetCommand =>
			_copyDistrictPrioritySetCommand ?? (_copyDistrictPrioritySetCommand = new DelegateCommand(
					() =>
					{
						if(SelectedDistrictPrioritySet.Id == 0)
						{
							CommonServices.InteractiveService.ShowMessage(
								ImportanceLevel.Info,
								"Перед копированием новой версии необходимо сохранить сотрудника");
							return;
						}

						var newDistrictPrioritySet = DriverDistrictPriorityHelper.CopyPrioritySetWithActiveDistricts(
							UoW,
							SelectedDistrictPrioritySet,
							out var notCopiedPriorities
						);
						newDistrictPrioritySet.IsCreatedAutomatically = false;

						if(notCopiedPriorities.Any())
						{
							var messageBuilder = new StringBuilder(
								"Для некоторых приоритетов районов\n" +
								$"из выбранной для копирования версии (Код: {SelectedDistrictPrioritySet.Id})\n" +
								"не были найдены связанные районы из активной\n" +
								"версии районов. Список приоритетов районов,\n" +
								"которые не будут скопированы:\n"
							);

							foreach(var driverDistrictPriority in notCopiedPriorities)
							{
								messageBuilder.AppendLine(
									$"Район: ({driverDistrictPriority.District.Id}) " +
									$"{driverDistrictPriority.District.DistrictName}. " +
									$"Приоритет: {driverDistrictPriority.Priority + 1}"
								);
							}
							CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, messageBuilder.ToString());
						}

						var driverDistrictPrioritySetViewModel = new DriverDistrictPrioritySetViewModel(
							newDistrictPrioritySet,
							UoW,
							_unitOfWorkFactory,
							CommonServices,
							_employeeRepository
						);

						driverDistrictPrioritySetViewModel.EntityAccepted += (o, eventArgs) => {
							var now = DateTime.Now;
							eventArgs.AcceptedEntity.DateCreated = now;
							eventArgs.AcceptedEntity.DateLastChanged = now;
							Entity.AddDriverDistrictPrioritySet(eventArgs.AcceptedEntity);
						};

						TabParent.AddSlaveTab(this, driverDistrictPrioritySetViewModel);
					}
				)
			);
		
		public DelegateCommand ActivateDistrictPrioritySetCommand => 
			_activateDistrictPrioritySetCommand ?? (_activateDistrictPrioritySetCommand = new DelegateCommand(
					() =>
					{
						var employeeForCurrentUser = EmployeeForCurrentUser;
						var now = DateTime.Now;

						SelectedDistrictPrioritySet.DateLastChanged = now;
						SelectedDistrictPrioritySet.DateActivated = now;

						Entity.ActivateDriverDistrictPrioritySet(SelectedDistrictPrioritySet, employeeForCurrentUser);
					}
				)
			);
		
		public DelegateCommand CopyDriverWorkScheduleSetCommand =>
			_copyDriverWorkScheduleSetCommand ?? (_copyDriverWorkScheduleSetCommand = new DelegateCommand(
					() =>
					{
						if(SelectedDriverScheduleSet.Id == 0)
						{
							CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Info,
								"Перед копированием новой версии необходимо сохранить сотрудника");
							return;
						}

						if(CommonServices.InteractiveService.Question(
							$"Скопировать и активировать выбранную версию графиков работы водителя " +
							$"(Код: {SelectedDriverScheduleSet.Id})?")
						)
						{
							var employeeForCurrentUser = EmployeeForCurrentUser;

							var newScheduleSet = (DriverWorkScheduleSet)SelectedDriverScheduleSet.Clone();
							newScheduleSet.Author = employeeForCurrentUser;
							newScheduleSet.LastEditor = employeeForCurrentUser;
							newScheduleSet.IsCreatedAutomatically = false;

							Entity.AddActiveDriverWorkScheduleSet(newScheduleSet);
						}
					}
				)
			);
		
		public DelegateCommand OpenDriverWorkScheduleSetCreateWindowCommand =>
			_openDriverWorkScheduleSetCreateWindowCommand ?? (_openDriverWorkScheduleSetCreateWindowCommand = new DelegateCommand(
					() =>
					{
						var newDriverWorkScheduleSet = new DriverWorkScheduleSet
						{
							Driver = Entity,
							IsCreatedAutomatically = false
						};

						var driverWorkScheduleSetViewModel = new DriverWorkScheduleSetViewModel(
							newDriverWorkScheduleSet,
							UoW,
							CommonServices,
							_deliveryScheduleSettings,
							_employeeRepository
						);
			
						driverWorkScheduleSetViewModel.EntityAccepted += (o, eventArgs) => 
						{
							Entity.AddActiveDriverWorkScheduleSet(newDriverWorkScheduleSet);
						};

						TabParent.AddSlaveTab(this, driverWorkScheduleSetViewModel);
					}
				)
			);

		public DelegateCommand OpenDriverWorkScheduleSetEditWindowCommand =>
			_openDriverWorkScheduleSetEditWindowCommand ?? (_openDriverWorkScheduleSetEditWindowCommand = new DelegateCommand(
					() =>
					{
						var driverWorkScheduleSetViewModel = new DriverWorkScheduleSetViewModel(
							SelectedDriverScheduleSet,
							UoW,
							CommonServices,
							_deliveryScheduleSettings,
							_employeeRepository
						);
						TabParent.AddSlaveTab(this, driverWorkScheduleSetViewModel);
					}
				)
			);
		
		public DelegateCommand RemoveEmployeeDocumentsCommand =>
			_removeEmployeeDocumentsCommand ?? (_removeEmployeeDocumentsCommand = new DelegateCommand(
					() =>
					{
						foreach(var document in SelectedEmployeeDocuments)
						{
							Entity.ObservableDocuments.Remove(document);
						}	
					}
				)
			);
		
		public DelegateCommand RemoveEmployeeContractsCommand =>
			_removeEmployeeContractsCommand ?? (_removeEmployeeContractsCommand = new DelegateCommand(
					() =>
					{
						foreach(var document in SelectedEmployeeContracts)
						{
							Entity.ObservableContracts.Remove(document);
						}	
					}
				)
			);

		public DelegateCommand RegisterDriverAppUserOrAddRoleCommand =>
			_registerDriverAppUserOrAddRoleCommand ?? (_registerDriverAppUserOrAddRoleCommand = new DelegateCommand(
					() =>
					{
						try
						{
							if(CommonServices.InteractiveService.Question(
									"Сотрудник будет сохранен при регистрации пользователя",
									"Вы уверены?"))
							{
								CanRegisterDriverAppUser = false;
								
								if(Entity.WarehouseAppUser != null && Entity.DriverAppUser != null)
								{
									Save();
									UoW.Commit();
									_driverApiUserRegisterEndpoint.AddRoleToUser(
											DriverAppUser.Login, DriverAppUser.Password, ApplicationUserRole.Driver.ToString())
										.GetAwaiter()
										.GetResult();
								}
								else
								{
									if(Entity.DriverAppUser is null)
									{
										Entity.ExternalApplicationsUsers.Add(DriverAppUser);
									}
									
									Save();
									UoW.Commit();
									_driverApiUserRegisterEndpoint.RegisterUser(
											DriverAppUser.Login, DriverAppUser.Password, ApplicationUserRole.Driver.ToString())
										.GetAwaiter()
										.GetResult();
								}
							}
						}
						catch(Exception e)
						{
							RollbackApplicationUser(e, DriverAppUser);
						}
					}
				)
			);
		
		public DelegateCommand RegisterWarehouseAppUserCommand { get; private set; }
		public DelegateCommand AddRoleToWarehouseAppUserCommand { get; private set; }
		public DelegateCommand RemoveRoleFromWarehouseAppUserCommand { get; private set; }
		public DelegateCommand CopyWarehouseAppUserCredentialsToDriverAppUserCommand { get; private set; }
		
		public DelegateCommand CreateNewEmployeeRegistrationVersionCommand =>
			_createNewEmployeeRegistrationVersionCommand ?? (_createNewEmployeeRegistrationVersionCommand = new DelegateCommand(
				() =>
				{
					var journal = NavigationManager.OpenViewModel<EmployeeRegistrationsJournalViewModel>(null);
					journal.ViewModel.SelectionMode = JournalSelectionMode.Single;
					journal.ViewModel.OnSelectResult += (sender, args) =>
					{
						if(!UoW.Session.IsOpen)
						{
							return;
						}
						
						var selectedResult = args.GetSelectedObjects<EmployeeRegistrationsJournalNode>().SingleOrDefault();

						if(selectedResult is null)
						{
							return;
						}

						var employeeRegistration = UoW.GetById<EmployeeRegistration>(selectedResult.Id);

						if(!AddEmployeeRegistrationVersion(SelectedRegistrationDate, employeeRegistration))
						{
							return;
						}

						OnPropertyChanged(nameof(CanAddNewRegistrationVersion));
						OnPropertyChanged(nameof(CanChangeRegistrationVersionDate));
					};
				}));

		private bool AddEmployeeRegistrationVersion(DateTime? registrationDate, EmployeeRegistration employeeRegistration)
		{
			var error =
				_employeeRegistrationVersionController.AddNewRegistrationVersion(registrationDate, employeeRegistration);

			if(!string.IsNullOrWhiteSpace(error))
			{
				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, error);
				return false;
			}

			return true;
		}

		private bool AddDefaultEmployeeRegistrationVersion(DateTime? registrationDate)
		{
			var employeeRegistration = UoW.GetById<EmployeeRegistration>(_employeeSettings.DefaultEmployeeRegistrationVersionId);

			if(employeeRegistration == null)
			{
				CommonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Warning, 
					"Версия вида оформления по умолчанию не найдена");

				return false;
			}

			return AddEmployeeRegistrationVersion(registrationDate, employeeRegistration);
		}

		public DelegateCommand ChangeEmployeeRegistrationVersionStartDateCommand =>
			_changeEmployeeRegistrationVersionStartDateCommand ?? (_changeEmployeeRegistrationVersionStartDateCommand = new DelegateCommand(
				() =>
				{
					if(!SelectedRegistrationDate.HasValue || SelectedRegistrationVersion is null)
					{
						return;
					}
					
					_employeeRegistrationVersionController.ChangeVersionStartDate(SelectedRegistrationVersion, SelectedRegistrationDate.Value);
					OnPropertyChanged(nameof(CanAddNewRegistrationVersion));
					OnPropertyChanged(nameof(CanChangeRegistrationVersionDate));
				}));

		public AttachedFileInformationsViewModel AttachedFileInformationsViewModel { get; }
		public EntityEntryViewModel<Phone> PhoneForCounterpartyCallsViewModel { get; }
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

		public void CopyCredentialsToOtherUser(bool toWarehouseAppUser = true)
		{
			if(toWarehouseAppUser)
			{
				WarehouseAppUser.Login = DriverAppUser.Login;
				WarehouseAppUser.Password = DriverAppUser.Password;
			}
			else
			{
				DriverAppUser.Login = WarehouseAppUser.Login;
				DriverAppUser.Password = WarehouseAppUser.Password;
			}
		}

		private void OnEntityPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(Entity.Category):
					UpdateDocumentsPermissions();
					OnPropertyChanged(nameof(CanReadEmployeeDocuments));
					break;
				case nameof(Entity.Counterparty):
					CheckEmployeeCounterparty();
					break;
				case nameof(Entity.Status):
					_statusChangedByUser = true;
					break;
				case nameof(Entity.CanRecieveCounterpartyCalls):
					if(!Entity.CanRecieveCounterpartyCalls)
					{
						Entity.PhoneForCounterpartyCalls = null;
					}
					break;
				default:
					break;
			}
		}

		private void SetPermissions()
		{
			var currentPermissionService = CommonServices.CurrentPermissionService;
			
			CanManageUsers = currentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.EmployeePermissions.CanManageUsers);
			_canActivateDriverDistrictPrioritySetPermission =
				currentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.EmployeePermissions.CanActivateDriverDistrictPrioritySet);
			//Не перенес, т.к. Trainee уже нет и скорее всего надо все сносить
			_canChangeTraineeToDriver = currentPermissionService.ValidatePresetPermission("can_change_trainee_to_driver");
			CanManageDriversAndForwarders =
				currentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.EmployeePermissions.CanManageDriversAndForwarders);
			CanManageOfficeWorkers = currentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.EmployeePermissions.CanManageOfficeWorkers);
			CanEditWage = currentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.EmployeePermissions.CanEditWage);
			CanEditOrganisationForSalary =
				currentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.EmployeePermissions.CanEditOrganisationForSalary);
			DriverDistrictPrioritySetPermission = currentPermissionService.ValidateEntityPermission(typeof(DriverDistrictPrioritySet));
			DriverWorkScheduleSetPermission = currentPermissionService.ValidateEntityPermission(typeof(DriverWorkScheduleSet));

			_employeeDocumentsPermissionsSet = CommonServices.PermissionService
				.ValidateUserPermission(typeof(EmployeeDocument), CommonServices.UserService.CurrentUserId);

			UpdateDocumentsPermissions();

			CanEditEmployee = _employeePermissionSet.CanUpdate || (_employeePermissionSet.CanCreate && Entity.Id == 0);
			CanReadEmployee = _employeePermissionSet.CanRead;
			CanChangeEmployeeCounterparty =
				currentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.EmployeePermissions.CanChangeEmployeeCounterparty)
				&& CanReadEmployee;
		}

		private void UpdateDocumentsPermissions()
		{
			var isAdmin = CommonServices.UserService.GetCurrentUser().IsAdmin;
			var canWorkWithOnlyDriverDocuments =
				CommonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.EmployeePermissions.CanWorkWithOnlyDriverDocuments);
			var canWorkWithDocuments = ((Entity.Category == EmployeeCategory.driver || Entity.Category == EmployeeCategory.forwarder) && canWorkWithOnlyDriverDocuments) || !canWorkWithOnlyDriverDocuments || isAdmin;
			CanReadEmployeeDocuments = _employeeDocumentsPermissionsSet.CanRead && canWorkWithDocuments;
			CanAddEmployeeDocument = _employeeDocumentsPermissionsSet.CanCreate && canWorkWithDocuments;
		}
		
		private void CreateCommands()
		{
			CreateRegisterWarehouseAppUserCommand();
			CreateAddRoleToWarehouseAppUserCommand();
			CreateRemoveRoleFromWarehouseAppUserCommand();
			CreateCopyWarehouseAppUserCredentialsToDriverAppUserCommand();
		}

		private void CreateRegisterWarehouseAppUserCommand()
		{
			RegisterWarehouseAppUserCommand = new DelegateCommand(
				() =>
				{
					try
					{
						RegisterWarehouseAppUserOrAddRole();
						OnPropertyChanged(nameof(CanCopyWarehouseAppUserCredentialsToDriverUser));
					}
					catch(Exception e)
					{
						RollbackApplicationUser(e, WarehouseAppUser);
					}
				}
			);
		}

		private void CreateAddRoleToWarehouseAppUserCommand()
		{
			AddRoleToWarehouseAppUserCommand = new DelegateCommand(
				() =>
				{
					try
					{
						RegisterWarehouseAppUserOrAddRole(false);
					}
					catch(Exception e)
					{
						RollbackApplicationUser(e, WarehouseAppUser);
					}
				}
			);
		}
		
		private void CreateRemoveRoleFromWarehouseAppUserCommand()
		{
			RemoveRoleFromWarehouseAppUserCommand = new DelegateCommand(
				() =>
				{
					try
					{
						if(CommonServices.InteractiveService.Question(
								"Перед тем, как продолжить нужно сохранить сотрудника", "Вы уверены?"))
						{
							CanRegisterWarehouseAppUser = false;

							Save();
							UoW.Commit();

							var userRole = Entity.Category == EmployeeCategory.driver
								? ApplicationUserRole.WarehouseDriver
								: ApplicationUserRole.WarehousePicker;
							
							_driverApiUserRegisterEndpoint.RemoveRoleFromUser(WarehouseAppUser.Login, WarehouseAppUser.Password, userRole.ToString())
									.GetAwaiter()
									.GetResult();
						}
					}
					catch(Exception e)
					{
						RollbackApplicationUser(e, WarehouseAppUser);
					}
				}
			);
		}
		
		private void CreateCopyWarehouseAppUserCredentialsToDriverAppUserCommand()
		{
			CopyWarehouseAppUserCredentialsToDriverAppUserCommand = new DelegateCommand(
				() =>
				{
					if(Entity.WarehouseAppUser is null || Entity.DriverAppUser != null)
					{
						return;
					}

					DriverAppUser.Login = WarehouseAppUser.Login;
					DriverAppUser.Password = WarehouseAppUser.Password;
				});
		}
		
		private void RegisterWarehouseAppUserOrAddRole(bool register = true)
		{
			if(CommonServices.InteractiveService.Question(
					"Сотрудник будет сохранен при регистрации пользователя", "Вы уверены?"))
			{
				CanRegisterWarehouseAppUser = false;

				if(Entity.WarehouseAppUser is null)
				{
					Entity.ExternalApplicationsUsers.Add(WarehouseAppUser);
				}
								
				Save();
				UoW.Commit();

				var userRole = Entity.Category == EmployeeCategory.driver
					? ApplicationUserRole.WarehouseDriver
					: ApplicationUserRole.WarehousePicker;
							
				if(register)
				{
					_driverApiUserRegisterEndpoint.RegisterUser(WarehouseAppUser.Login, WarehouseAppUser.Password, userRole.ToString())
						.GetAwaiter()
						.GetResult();
				}
				else
				{
					_driverApiUserRegisterEndpoint.AddRoleToUser(WarehouseAppUser.Login, WarehouseAppUser.Password, userRole.ToString())
						.GetAwaiter()
						.GetResult();
				}
			}
		}

		private void RollbackApplicationUser(Exception e, ExternalApplicationUser userApp)
		{
			var login = userApp.Login;
			var password = userApp.Password;
			userApp.Login = null;
			userApp.Password = null;
			Entity.ExternalApplicationsUsers.Remove(userApp);

			if(userApp.ExternalApplicationType == ExternalApplicationType.WarehouseApp)
			{
				Entity.HasAccessToWarehouseApp = false;
			}

			Save();
			UoW.Commit();
			userApp.Login = login;
			userApp.Password = password;
			CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Error, e.Message);

			switch(userApp.ExternalApplicationType)
			{
				case ExternalApplicationType.DriverApp:
					CanRegisterDriverAppUser = true;
					break;
				case ExternalApplicationType.WarehouseApp:
					CanRegisterWarehouseAppUser = true;
					break;
			}
		}

		private void GetExternalUsers()
		{
			DriverAppUser = Entity.DriverAppUser ?? new ExternalApplicationUser
			{
				Employee = Entity,
				ExternalApplicationType = ExternalApplicationType.DriverApp
			};

			WarehouseAppUser = Entity.WarehouseAppUser ?? new ExternalApplicationUser
			{
				Employee = Entity,
				ExternalApplicationType = ExternalApplicationType.WarehouseApp
			};
			
			CanRegisterDriverAppUser =
				string.IsNullOrWhiteSpace(DriverAppUser.Login) &&
				string.IsNullOrWhiteSpace(DriverAppUser.Password);
			CanRegisterWarehouseAppUser =
				string.IsNullOrWhiteSpace(WarehouseAppUser.Login) &&
				string.IsNullOrWhiteSpace(WarehouseAppUser.Password);
			
			DriverAppUser.PropertyChanged += OnDriverAppUserPropertyChanged;
			WarehouseAppUser.PropertyChanged += OnWarehouseAppUserPropertyChanged;
		}

		private void OnWarehouseAppUserPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ExternalApplicationUser.Login) || e.PropertyName == nameof(ExternalApplicationUser.Password))
			{
				OnPropertyChanged(nameof(IsValidNewWarehouseAppUser));
			}
		}

		private void OnDriverAppUserPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ExternalApplicationUser.Login) || e.PropertyName == nameof(ExternalApplicationUser.Password))
			{
				OnPropertyChanged(nameof(IsValidNewDriverAppUser));
			}
		}
		
		private bool Validate() => CommonServices.ValidationService.Validate(Entity, _validationContext);

		private bool TrySaveNewUser()
		{
			if(!string.IsNullOrEmpty(Entity.LoginForNewUser))
			{
				if(!_authorizationService.TryToSaveUser(Entity, UoWGeneric))
				{
					return false;
				}
			}

			return true;
		}

		private void FillHiddenCategories(bool traineeToEmployee)
		{
			var allCategories = (EmployeeCategory[])Enum.GetValues(typeof(EmployeeCategory));

			if(CanManageDriversAndForwarders && !CanManageOfficeWorkers)
			{
				HiddenCategories.AddRange(allCategories.Except(new[] {EmployeeCategory.driver, EmployeeCategory.forwarder}));
			}
			else if((traineeToEmployee && !_canChangeTraineeToDriver) || (CanManageOfficeWorkers && !CanManageDriversAndForwarders))
			{
				HiddenCategories.AddRange(allCategories.Except(new[] {EmployeeCategory.office}));
			}
		}
		
		private void ConfigureValidationContext(IValidationContextFactory validationContextFactory)
		{
			_validationContext = validationContextFactory.CreateNewValidationContext(Entity);
			
			_validationContext.ServiceContainer.AddService(typeof(ISubdivisionSettings), _subdivisionSettings);
			_validationContext.ServiceContainer.AddService(typeof(IEmployeeRepository), _employeeRepository);
			_validationContext.ServiceContainer.AddService(typeof(IUserRepository), _userRepository);
		}

		public void SaveAndClose()
		{
			if(!HasChanges)
			{
				Close(false, CloseSource.Save);
				return;
			}

			if(Entity.Category == EmployeeCategory.driver
				&& Entity.Status == EmployeeStatus.IsFired 
				&& Entity.DriverOfCarOwnType == Domain.Logistic.Cars.CarOwnType.Driver)
			{
				var car = _carRepository.GetCarByDriver(UoW, Entity);
				if(car != null && _interactiveService.Question(
					$"К этому сотруднику привязан автомобиль:\n{car.FullTitle}"
					+ "\n\nАрхивировать данный автомобиль?",
					"Подтверждение"))
				{
					_carRepository.ArchiveCar(UoW, car, Domain.Logistic.Cars.ArchivingReason.Hired);
				}
			}
			
			if(Save())
			{
				EntitySaved?.Invoke(this, new EntitySavedEventArgs(UoW.RootObject));
				Close(false, CloseSource.Save);
			}
		}

		private void SavePhotoIfNeeded()
		{
			if(Photo is null)
			{
				return;
			}

			if(PhotoFilename != Entity.PhotoFileName)
			{
				var result = _employeeFileStorageService
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
					var result = _employeeFileStorageService.CreateFileAsync(Entity, fileName,
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
				_employeeFileStorageService.UpdateFileAsync(Entity, fileName, new MemoryStream(AttachedFileInformationsViewModel.AttachedFiles[fileName]), _cancellationTokenSource.Token)
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
				_employeeFileStorageService.DeleteFileAsync(Entity, fileName, _cancellationTokenSource.Token)
					.GetAwaiter()
					.GetResult();
			}
		}

		public bool Save()
		{
			if(Entity.Id == 0 && !CanManageOfficeWorkers && !CanManageDriversAndForwarders)
			{
				CommonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Info,
					"У вас недостаточно прав для создания сотрудника");
				
				return false;
			}
			
			var driverAppUser = Entity.DriverAppUser;
			
			if(CanRegisterDriverAppUser
				&& driverAppUser != null
				&& !string.IsNullOrWhiteSpace(driverAppUser.Login)
				&& !string.IsNullOrWhiteSpace(driverAppUser.Password))
			{
				if(CommonServices.InteractiveService.Question(
						"Данные пользователя водительского приложения были внесены,\n" +
						"но пользователь не был сохранен. Эти данные будут очищены,\n" +
						"а пользователь водительского приложения не будет сохранен", "Вы уверены?"))
				{
					Entity.ExternalApplicationsUsers.Remove(driverAppUser);
				}
				else
				{
					return false;
				}
			}
			
			var warehouseAppUser = Entity.WarehouseAppUser;
			
			if(CanRegisterWarehouseAppUser
				&& warehouseAppUser != null
				&& !string.IsNullOrWhiteSpace(warehouseAppUser.Login)
				&& !string.IsNullOrWhiteSpace(warehouseAppUser.Password))
			{
				if(CommonServices.InteractiveService.Question(
						"Данные пользователя складского приложения были внесены,\n" +
						"но пользователь не был сохранен. Эти данные будут очищены,\n" +
						"а пользователь складского приложения не будет сохранен", "Вы уверены?"))
				{
					Entity.ExternalApplicationsUsers.Remove(warehouseAppUser);
				}
				else
				{
					return false;
				}
			}

			if(!Validate())
			{
				return false;
			}

			if(!Entity.EmployeeRegistrationVersions.Any())
			{
				if(!AddDefaultEmployeeRegistrationVersion(Entity.FirstWorkDay))
				{
					return false;
				}
			}

			if(Entity.User != null)
			{
				Entity.User.Deactivated = Entity.Status == EmployeeStatus.IsFired;
				var associatedEmployees = _employeeRepository.GetEmployeesForUser(UoW, Entity.User.Id);
				
				if(associatedEmployees.Any(e => e.Id != Entity.Id))
				{
					var mes = string.Format("Пользователь {0} уже связан с сотрудником {1}, " +
						"при привязке этого сотрудника к пользователю, старая связь будет удалена. Продолжить?",
						Entity.User.Name,
						string.Join(", ", associatedEmployees.Select(e => e.ShortName)));
					
					if(CommonServices.InteractiveService.Question(mes))
					{
						foreach(var ae in associatedEmployees.Where(e => e.Id != Entity.Id))
						{
							ae.User = null;
							UoWGeneric.Save(ae);
						}
					}
					else
					{
						return false;
					}
				}
			}

			if(Entity.InnerPhone != null)
			{
				var associatedEmployees = UoW.Session.Query<Employee>().Where(e => e.InnerPhone == Entity.InnerPhone);
				
				if(associatedEmployees.Any(e => e.Id != Entity.Id && e.InnerPhone == Entity.InnerPhone))
				{
					string mes = string.Format("Внутренний номер {0} уже связан с сотрудником {1}. Продолжить?",
						Entity.InnerPhone,
						string.Join(", ", associatedEmployees.Select(e => e.Name))
						);
					
					if(!CommonServices.InteractiveService.Question(mes))
					{
						return false;
					}
				}
			}

			Entity.CreateDefaultWageParameter(_wageCalculationRepository, _wageSettings, CommonServices.InteractiveService);
			
			if(Entity.Counterparty != null)
			{
				if(Entity.Status == EmployeeStatus.OnCalculation || Entity.Status == EmployeeStatus.IsFired)
				{
					TryRemoveAllFixedPrices();
				}
				else
				{
					TryRemoveEmployeeFixedPricesFromOldCounterparty();
					TryAddEmployeeFixedPrices();
				}
			}
			else
			{
				TryRemoveEmployeeFixedPricesFromOldCounterparty();
			}

			UoW.Save();

			SavePhotoIfNeeded();
			AddAttachedFilesIfNeeded();
			UpdateAttachedFilesIfNeeded();
			DeleteAttachedFilesIfNeeded();
			AttachedFileInformationsViewModel.ClearPersistentInformationCommand.Execute();

			#region Попытка сохранить логин для нового юзера

			if(!TrySaveNewUser())
			{
				return false;
			}

			#endregion

			_terminalManagementViewModel?.SaveChanges();

			_logger.Info("Сохраняем сотрудника...");
			try
			{
				UoWGeneric.Save();
			}
			catch(Exception ex)
			{
				_logger.Error(ex, "Не удалось записать сотрудника.");
				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Error, ex.Message);
				return false;
			}

			_logger.Info("Ok");
			return true;
		}
		
		private void CheckEmployeeCounterparty()
		{
			if(Entity.Counterparty is null)
			{
				_counterpartyChangedByUser = true;
				return;
			}
			
			var otherEmployee = _employeeRepository.GetOtherEmployeeInfoWithSameCounterparty(
				_unitOfWorkFactory, Entity.Id, Entity.Counterparty.Id);

			if(otherEmployee != null)
			{
				ShowWarningMessage($"Выбранный клиент {Entity.Counterparty.Name} уже установлен у сотрудника с №{otherEmployee.Id} {otherEmployee.Name}");
				Entity.Counterparty = null;
			}
			
			_counterpartyChangedByUser = true;
		}

		private void TryRemoveAllFixedPrices()
		{
			if(!_statusChangedByUser)
			{
				return;
			}

			var counterparty = Entity.Counterparty;
			_nomenclatureFixedPriceController.DeleteAllFixedPricesFromCounterpartyAndDeliveryPoints(counterparty);
			UoW.Save(counterparty);
		}

		private void TryAddEmployeeFixedPrices()
		{
			if(!_counterpartyChangedByUser && !_statusChangedByUser)
			{
				return;
			}

			var fixedPrices = _nomenclatureFixedPriceController.GetEmployeesNomenclatureFixedPrices(UoW);

			if(Entity.Counterparty != null)
			{
				_nomenclatureFixedPriceController.AddEmployeeFixedPricesToCounterpartyAndDeliveryPoints(Entity.Counterparty, fixedPrices);
				UoW.Save(Entity.Counterparty);
			}
		}
		
		private void TryRemoveEmployeeFixedPricesFromOldCounterparty()
		{
			if(!_counterpartyChangedByUser)
			{
				return;
			}

			var oldCounterpartyId =
				Entity.Id > 0
					? _employeeRepository.GetEmployeeCounterpartyFromDatabase(_unitOfWorkFactory, Entity.Id)
					: null;

			if(oldCounterpartyId.HasValue && (Entity.Counterparty is null || Entity.Counterparty.Id != oldCounterpartyId))
			{
				var counterparty = UoW.GetById<Domain.Client.Counterparty>(oldCounterpartyId.Value);
				_nomenclatureFixedPriceController.DeleteAllFixedPricesFromCounterpartyAndDeliveryPoints(counterparty);
				UoW.Save(counterparty);
			}
		}

		public override bool CompareHashName(string hashName)
		{
			if(Entity == null || UoWGeneric == null || UoWGeneric.IsNew) {
				return false;
			}
			return GenerateHashName(Entity.Id) == hashName;
		}

		private string GenerateHashName(int id)
		{
			return DomainHelper.GenerateDialogHashName(typeof(Employee), id);
		}
		
		public override void Dispose()
		{
			UoW?.Dispose();
			LifetimeScope = null;
			base.Dispose();
		}
	}
}

using NLog;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Permissions;
using QS.Project.Services;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.EntityRepositories.WageCalculation;
using Vodovoz.Factories;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.Tools.Logistic;
using Vodovoz.ViewModels.Infrastructure.Services;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Logistic;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Contacts;
using VodovozInfrastructure.Endpoints;

namespace Vodovoz.ViewModels.ViewModels.Employees
{
	public class EmployeeViewModel : TabViewModelBase, ITdiDialog, ISingleUoWDialog
	{
		private readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private readonly IAuthorizationService _authorizationService;
		private readonly ISubdivisionService _subdivisionService;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IWageCalculationRepository _wageCalculationRepository;
		private readonly IEmailServiceSettingAdapter _emailServiceSettingAdapter;
		private readonly ICommonServices _commonServices;

		private readonly IWarehouseRepository _warehouseRepository;
		private readonly IRouteListRepository _routeListRepository;
		private readonly DriverApiUserRegisterEndpoint _driverApiUserRegisterEndpoint;
		private readonly UserSettings _userSettings;
		private readonly IUserRepository _userRepository;
		private readonly BaseParametersProvider _baseParametersProvider;

		private IPermissionResult _employeeDocumentsPermissionsSet;
		private bool _canActivateDriverDistrictPrioritySetPermission;
		private bool _canChangeTraineeToDriver;
		private bool _canRegisterMobileUser;
		private DriverWorkScheduleSet _selectedDriverScheduleSet;
		private DriverDistrictPrioritySet _selectedDistrictPrioritySet;
		private Employee _employeeForCurrentUser;
		private IEnumerable<EmployeeDocument> _selectedEmployeeDocuments = new EmployeeDocument[0];
		private IEnumerable<EmployeeContract> _selectedEmployeeContracts = new EmployeeContract[0];
		private ValidationContext _validationContext;
		private TerminalManagementViewModel _terminalManagementViewModel;

		private DelegateCommand _openDistrictPrioritySetCreateWindowCommand;
		private DelegateCommand _openDistrictPrioritySetEditWindowCommand;
		private DelegateCommand _copyDistrictPrioritySetCommand;
		private DelegateCommand _activateDistrictPrioritySetCommand;
		private DelegateCommand _openDriverWorkScheduleSetCreateWindowCommand;
		private DelegateCommand _openDriverWorkScheduleSetEditWindowCommand;
		private DelegateCommand _copyDriverWorkScheduleSetCommand;
		private DelegateCommand _removeEmployeeDocumentsCommand;
		private DelegateCommand _removeEmployeeContractsCommand;
		private DelegateCommand _registerDriverModileUserCommand;

		public IReadOnlyList<Organization> organizations;

		public event Action SaveAttachmentFilesChangesAction;
		public event Func<bool> HasAttachmentFilesChangesFunc;
		public event EventHandler<EntitySavedEventArgs> EntitySaved;

		public EmployeeViewModel(
			IAuthorizationService authorizationService,
			IEmployeeWageParametersFactory employeeWageParametersFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			ISubdivisionJournalFactory subdivisionJournalFactory,
			IEmployeePostsJournalFactory employeePostsJournalFactory,
			ICashDistributionCommonOrganisationProvider commonOrganisationProvider,
			ISubdivisionService subdivisionService,
			IEmailServiceSettingAdapter emailServiceSettingAdapter,
			IWageCalculationRepository wageCalculationRepository,
			IEmployeeRepository employeeRepository,
			IUnitOfWorkGeneric<Employee> uowGeneric,
			ICommonServices commonServices,
			IValidationContextFactory validationContextFactory,
			IPhonesViewModelFactory phonesViewModelFactory,
			IWarehouseRepository warehouseRepository,
			IRouteListRepository routeListRepository,
			DriverApiUserRegisterEndpoint driverApiUserRegisterEndpoint,
			UserSettings userSettings,
			IUserRepository userRepository,
			BaseParametersProvider baseParametersProvider,
			bool traineeToEmployee = false,
			INavigationManager navigationManager = null
			) : base(commonServices?.InteractiveService, navigationManager)
		{
			_authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
			EmployeeWageParametersFactory =
				employeeWageParametersFactory ?? throw new ArgumentNullException(nameof(employeeWageParametersFactory));
			EmployeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			EmployeePostsJournalFactory = employeePostsJournalFactory ?? throw new ArgumentNullException(nameof(employeePostsJournalFactory)); 
			SubdivisionJournalFactory = subdivisionJournalFactory ?? throw new ArgumentNullException(nameof(subdivisionJournalFactory)); 
			
			if(commonOrganisationProvider == null)
			{
				throw new ArgumentNullException(nameof(commonOrganisationProvider));
			}
			
			_subdivisionService = subdivisionService ?? throw new ArgumentNullException(nameof(subdivisionService));
			_emailServiceSettingAdapter = emailServiceSettingAdapter ?? throw new ArgumentNullException(nameof(emailServiceSettingAdapter));
			_wageCalculationRepository = wageCalculationRepository ?? throw new ArgumentNullException(nameof(wageCalculationRepository));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_warehouseRepository = warehouseRepository ?? throw new ArgumentNullException(nameof(warehouseRepository));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_driverApiUserRegisterEndpoint = driverApiUserRegisterEndpoint ?? throw new ArgumentNullException(nameof(driverApiUserRegisterEndpoint));
			_userSettings = userSettings ?? throw new ArgumentNullException(nameof(userSettings));
			UoWGeneric = uowGeneric ?? throw new ArgumentNullException(nameof(uowGeneric));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_baseParametersProvider = baseParametersProvider ?? throw new ArgumentNullException(nameof(baseParametersProvider));

			if(validationContextFactory == null)
			{
				throw new ArgumentNullException(nameof(validationContextFactory));
			}

			ConfigureValidationContext(validationContextFactory);

			if(phonesViewModelFactory == null)
			{
				throw new ArgumentNullException(nameof(phonesViewModelFactory));
			}

			PhonesViewModel = phonesViewModelFactory.CreateNewPhonesViewModel(UoW);
			
			if(Entity.Id == 0)
			{
				Entity.OrganisationForSalary = commonOrganisationProvider.GetCommonOrganisation(UoW);
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
			
			SetPermissions();

			Entity.PropertyChanged += OnEntityPropertyChanged;

			organizations = UoW.GetAll<Organization>().ToList();
			FillHiddenCategories(traineeToEmployee);

			CanRegisterMobileUser = string.IsNullOrWhiteSpace(Entity.AndroidLogin) && string.IsNullOrWhiteSpace(Entity.AndroidPassword);

			var permissionResult = 
				_commonServices.PermissionService.ValidateUserPermission(typeof(Employee), _commonServices.UserService.CurrentUserId);
			
			if(!permissionResult.CanRead) {
				AbortOpening(PermissionsSettings.GetEntityReadValidateResult(typeof(Employee)));
			}
		}

		private void OnEntityPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.AndroidLogin) || e.PropertyName == nameof(Entity.AndroidPassword))
			{
				OnPropertyChanged(nameof(IsValidNewMobileUser));
			}
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

		public IUnitOfWork UoW => UoWGeneric;
		public Employee Entity => UoWGeneric.Root;
		public IUnitOfWorkGeneric<Employee> UoWGeneric { get; }
		public IEmployeeWageParametersFactory EmployeeWageParametersFactory { get; }
		public IEmployeeJournalFactory EmployeeJournalFactory { get; }
		public IEmployeePostsJournalFactory EmployeePostsJournalFactory { get; }
		public ISubdivisionJournalFactory SubdivisionJournalFactory { get; }
		
		public bool HasChanges
		{
			get
			{
				PhonesViewModel.RemoveEmpty();

				var attachmentFilesHasChanges = HasAttachmentFilesChangesFunc?.Invoke() ?? false;
				
				return UoWGeneric.HasChanges
					   || attachmentFilesHasChanges
					   || !string.IsNullOrEmpty(Entity.LoginForNewUser)
					   || (_terminalManagementViewModel?.HasChanges ?? false);
			}
		}
		
		public IPermissionResult DriverDistrictPrioritySetPermission { get; private set; }
		public IPermissionResult DriverWorkScheduleSetPermission { get; private set; }

		public PhonesViewModel PhonesViewModel { get; }

		public TerminalManagementViewModel TerminalManagementViewModel => _terminalManagementViewModel ??
		                                                                  (_terminalManagementViewModel =
			                                                                  new TerminalManagementViewModel(
				                                                                  _userSettings.DefaultWarehouse,
				                                                                  Entity,
				                                                                  this as ITdiTab,
				                                                                  _employeeRepository,
				                                                                  _warehouseRepository,
				                                                                  _routeListRepository,
				                                                                  _commonServices,
				                                                                  UoW,
				                                                                  _baseParametersProvider));

		public bool CanReadEmployeeDocuments { get; private set; }
		public bool CanAddEmployeeDocument { get; private set; }
		public bool CanManageUsers { get; private set; }
		public bool CanManageDriversAndForwarders { get; private set; }
		public bool CanManageOfficeWorkers { get; private set; }
		public bool CanCreateNewUser => Entity.User == null && CanManageUsers;
		public bool CanEditEmployeeCategory => Entity?.Id == 0 && (CanManageOfficeWorkers || CanManageDriversAndForwarders);
		public bool CanEditWage { get; private set; }
		public bool CanEditOrganisationForSalary { get; private set; }

		public bool CanRegisterMobileUser
		{
			get => _canRegisterMobileUser;
			set
			{
				if(SetField(ref _canRegisterMobileUser, value))
				{
					OnPropertyChanged(nameof(IsValidNewMobileUser));
				}
			}
		}

		public bool IsValidNewMobileUser => !string.IsNullOrWhiteSpace(Entity.AndroidLogin)
										 && Entity.AndroidPassword?.Length >= 3
										 && CanRegisterMobileUser;

		public string AddMobileLoginInfo => CanRegisterMobileUser
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
			&& _canActivateDriverDistrictPrioritySetPermission;
		
		public bool CanCopyDistrictPrioritySet => SelectedDistrictPrioritySet != null && DriverDistrictPrioritySetPermission.CanCreate;
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
					OnPropertyChanged(nameof(CanEditEmployeeDocument));
					OnPropertyChanged(nameof(CanRemoveEmployeeDocument));
				}
			}
		}

		public bool CanEditEmployeeDocument => (_employeeDocumentsPermissionsSet.CanRead 
												|| _employeeDocumentsPermissionsSet.CanUpdate) 
											&& SelectedEmployeeDocuments.Any();
		public bool CanRemoveEmployeeDocument => _employeeDocumentsPermissionsSet.CanDelete && SelectedEmployeeDocuments.Any();

		public IEnumerable<EmployeeContract> SelectedEmployeeContracts
		{
			get => _selectedEmployeeContracts;
			set
			{
				if(SetField(ref _selectedEmployeeContracts, value))
				{
					OnPropertyChanged(nameof(CanEditEmployeeDocument));
					OnPropertyChanged(nameof(CanRemoveEmployeeDocument));
				}
			}
		}
		
		public bool CanEditEmployeeContract => SelectedEmployeeContracts.Any();
		public bool CanRemoveEmployeeContract => SelectedEmployeeContracts.Any();
		
		public bool CanCopyDriverScheduleSet => SelectedDriverScheduleSet != null && DriverWorkScheduleSetPermission.CanCreate;
		public bool CanEditDriverScheduleSet => 
			SelectedDriverScheduleSet != null && (DriverWorkScheduleSetPermission.CanUpdate || DriverWorkScheduleSetPermission.CanRead);

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
							UnitOfWorkFactory.GetDefaultFactory,
							_commonServices,
							_baseParametersProvider,
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
							UnitOfWorkFactory.GetDefaultFactory,
							_commonServices,
							_baseParametersProvider,
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
							ServicesConfig.CommonServices.InteractiveService
								.ShowMessage(ImportanceLevel.Info,
									"Перед копированием новой версии необходимо сохранить сотрудника");
							return;
						}

						var newDistrictPrioritySet = DriverDistrictPriorityHelper.CopyPrioritySetWithActiveDistricts(
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
							_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, messageBuilder.ToString());
						}

						var driverDistrictPrioritySetViewModel = new DriverDistrictPrioritySetViewModel(
							newDistrictPrioritySet,
							UoW,
							UnitOfWorkFactory.GetDefaultFactory,
							_commonServices,
							_baseParametersProvider,
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
							_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info,
								"Перед копированием новой версии необходимо сохранить сотрудника");
							return;
						}

						if(_commonServices.InteractiveService.Question(
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
							_commonServices,
							_baseParametersProvider,
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
							_commonServices,
							_baseParametersProvider,
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

		public DelegateCommand RegisterDriverModileUserCommand =>
			_registerDriverModileUserCommand ?? (_registerDriverModileUserCommand = new DelegateCommand(
					() =>
					{
						try
						{
							if(_commonServices.InteractiveService.Question("Сотрудник будет сохранен при регистрации пользователя", "Вы уверены?"))
							{
								CanRegisterMobileUser = false;
								Save();
								UoW.Commit();
								_driverApiUserRegisterEndpoint.Register(Entity.AndroidLogin, Entity.AndroidPassword).GetAwaiter().GetResult();
							}
						}
						catch(Exception e)
						{
							var login = Entity.AndroidLogin;
							var password = Entity.AndroidPassword;
							Entity.AndroidLogin = null;
							Entity.AndroidPassword = null;
							Save();
							UoW.Commit();
							Entity.AndroidLogin = login;
							Entity.AndroidPassword = password;
							_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Error, e.Message);
							CanRegisterMobileUser = true;
						}
					}
				)
			);

		private void SetPermissions()
		{
			CanManageUsers = _commonServices.CurrentPermissionService.ValidatePresetPermission("can_manage_users");
			_canActivateDriverDistrictPrioritySetPermission =
				_commonServices.CurrentPermissionService.ValidatePresetPermission("can_activate_driver_district_priority_set");
			_canChangeTraineeToDriver =
				_commonServices.CurrentPermissionService.ValidatePresetPermission("can_change_trainee_to_driver");
			CanManageDriversAndForwarders =
				_commonServices.CurrentPermissionService.ValidatePresetPermission("can_manage_drivers_and_forwarders");
			CanManageOfficeWorkers = _commonServices.CurrentPermissionService.ValidatePresetPermission("can_manage_office_workers");
			CanEditWage = _commonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_wage");
			CanEditOrganisationForSalary =
				_commonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_organisation_for_salary");
			DriverDistrictPrioritySetPermission =
				_commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(DriverDistrictPrioritySet));
			DriverWorkScheduleSetPermission =
				_commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(DriverWorkScheduleSet));

			_employeeDocumentsPermissionsSet = _commonServices.PermissionService
				.ValidateUserPermission(typeof(EmployeeDocument), _commonServices.UserService.CurrentUserId);

			CanReadEmployeeDocuments = _employeeDocumentsPermissionsSet.CanRead;
			CanAddEmployeeDocument = _employeeDocumentsPermissionsSet.CanCreate;
		}
		
		private bool Validate() => _commonServices.ValidationService.Validate(Entity, _validationContext);

		private bool TrySaveNewUser()
		{
			if(!string.IsNullOrEmpty(Entity.LoginForNewUser) && _emailServiceSettingAdapter.SendingAllowed)
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
			
			_validationContext.ServiceContainer.AddService(typeof(ISubdivisionService), _subdivisionService);
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

			if(Save())
			{
				EntitySaved?.Invoke(this, new EntitySavedEventArgs(UoW.RootObject));
				Close(false, CloseSource.Save);
			}
		}

		public bool Save()
		{
			if(Entity.Id == 0 && !CanManageOfficeWorkers && !CanManageDriversAndForwarders)
			{
				_commonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Info,
					"У вас недостаточно прав для создания сотрудника");
				
				return false;
			}
			
			//Проверяем, чтобы в БД не попала пустая строка
			if(string.IsNullOrWhiteSpace(Entity.AndroidLogin))
			{
				Entity.AndroidLogin = null;
			}

			if(CanRegisterMobileUser 
			&& !string.IsNullOrWhiteSpace(Entity.AndroidLogin)
			&& !string.IsNullOrWhiteSpace(Entity.AndroidPassword)
			&& _commonServices.InteractiveService.Question("Данные пользовтеля водительского приложения были внесены,\n" +
														   "но пользователь не был сохранен. Эти данные будут очищены,\n" +
														   "а пользователь водительского приложения не будет сохранен", "Вы уверены?"))
			{
				Entity.AndroidLogin = null;
				Entity.AndroidPassword = null;
			}

			if(!Validate())
			{
				return false;
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
					
					if(_commonServices.InteractiveService.Question(mes))
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
					
					if(!_commonServices.InteractiveService.Question(mes))
					{
						return false;
					}
				}
			}

			Entity.CreateDefaultWageParameter(_wageCalculationRepository, _baseParametersProvider, _commonServices.InteractiveService);

			UoWGeneric.Save(Entity);

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
				SaveAttachmentFilesChangesAction?.Invoke();
			}
			catch(Exception ex)
			{
				_logger.Error(ex, "Не удалось записать сотрудника.");
				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Error, ex.Message);
				return false;
			}

			_logger.Info("Ok");
			return true;
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
		
		public override void Close(bool askSave, CloseSource source)
		{
			base.Close(askSave, source);
			Dispose();
		}
		
		public override void Dispose()
		{
			UoW?.Dispose();
			base.Dispose();
		}
	}
}

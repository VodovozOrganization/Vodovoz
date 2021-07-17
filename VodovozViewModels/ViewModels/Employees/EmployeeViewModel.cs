using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Services;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.WageCalculation;
using Vodovoz.Factories;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.Tools.Logistic;
using Vodovoz.ViewModels.Infrastructure.Services;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Logistic;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.ViewModels.ViewModels.Employees
{
	public class EmployeeViewModel : EntityTabViewModelBase<Employee>
	{
		private readonly Logger logger = LogManager.GetCurrentClassLogger();
		private readonly IAuthorizationService _authorizationService;
		private readonly ISubdivisionService _subdivisionService;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IWageCalculationRepository _wageCalculationRepository;
		private readonly IEmailServiceSettingAdapter _emailServiceSettingAdapter;
		public IReadOnlyList<Organization> organizations;

        public bool _canManageDriversAndForwarders;
        public bool _canManageOfficeWorkers;
        private bool _canActivateDriverDistrictPrioritySetPermission;

        private object _selectedDriverScheduleSet;
        private object _selectedDistrictPrioritySet;
        private IEnumerable<EmployeeDocument> _selectedEmployeeDocuments = new EmployeeDocument[0];
        private IEnumerable<EmployeeContract> _selectedEmployeeContracts = new EmployeeContract[0];

        private DelegateCommand _saveCommand;
        private DelegateCommand _openDistrictPrioritySetCreateWindowCommand;
        private DelegateCommand _openDistrictPrioritySetEditWindowCommand;
        private DelegateCommand _copyDistrictPrioritySetCommand;
        private DelegateCommand _activateDistrictPrioritySetCommand;
        private DelegateCommand _openDriverWorkScheduleSetCreateWindowCommand;
        private DelegateCommand _openDriverWorkScheduleSetEditWindowCommand;
        private DelegateCommand _copyDriverWorkScheduleSetCommand;
        private DelegateCommand _removeEmployeeDocumentsCommand;
        private DelegateCommand _removeEmployeeContractsCommand;

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
	        IEntityUoWBuilder uoWBuilder,
	        IUnitOfWorkFactory uowFactory,
	        ICommonServices commonServices
	        ) : base(uoWBuilder, uowFactory, commonServices)
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
	        
	        if(Entity.Id == 0)
	        {
		        Entity.OrganisationForSalary = commonOrganisationProvider.GetCommonOrganisation(UoW);
	        }
	        
	        if(UoWGeneric.Root.Phones == null)
	        {
		        UoWGeneric.Root.Phones = new List<Phone>();
	        }

	        _canActivateDriverDistrictPrioritySetPermission = 
		        commonServices.CurrentPermissionService.ValidatePresetPermission("can_activate_driver_district_priority_set");
	        _canManageDriversAndForwarders = 
		        commonServices.CurrentPermissionService.ValidatePresetPermission("can_manage_drivers_and_forwarders");
	        _canManageOfficeWorkers = commonServices.CurrentPermissionService.ValidatePresetPermission("can_manage_office_workers");
	        CanEditOrganisationForSalary = 
		        commonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_organisation_for_salary");
	        DriverDistrictPrioritySetPermission = 
		        commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(DriverDistrictPrioritySet));
	        DriverWorkScheduleSetPermission =
		        commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(DriverWorkScheduleSet));

	        organizations = UoW.GetAll<Organization>().ToList();
	        FillHiddenCategories();
        }

        public IEnumerable<EmployeeCategory> HiddenCategories { get; private set; }

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

        public IEmployeeWageParametersFactory EmployeeWageParametersFactory { get; }
        public IEmployeeJournalFactory EmployeeJournalFactory { get; }
        public IEmployeePostsJournalFactory EmployeePostsJournalFactory { get; }
        public ISubdivisionJournalFactory SubdivisionJournalFactory { get; }
        /*
        public override bool HasChanges
        {
	        get
	        {
		        phonesView.RemoveEmpty();
		        return UoWGeneric.HasChanges
		               || attachmentFiles.HasChanges
		               || !string.IsNullOrEmpty(yentryUserLogin.Text);
	        }
	        set => base.HasChanges = value;
        }*/
        
        public IPermissionResult DriverDistrictPrioritySetPermission { get; }
		public IPermissionResult DriverWorkScheduleSetPermission { get; }
		
		public bool CanManageUsers => CommonServices.CurrentPermissionService.ValidatePresetPermission("can_manage_users");
		
		public bool CanCreateNewUser => 
			Entity.User == null && CommonServices.CurrentPermissionService.ValidatePresetPermission("can_manage_users");

		public bool CanEditEmployeeCategory => Entity?.Id == 0 && (_canManageOfficeWorkers || _canManageDriversAndForwarders);
		
		public bool CanEditOrganisationForSalary { get; }

		public object SelectedDistrictPrioritySet
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
			SelectedDistrictPrioritySet is DriverDistrictPrioritySet districtPrioritySet
			&& !districtPrioritySet.IsActive
			&& districtPrioritySet.DateActivated == null
			&& districtPrioritySet.ObservableDriverDistrictPriorities
				.All(x => x.District.DistrictsSet.Status == DistrictsSetStatus.Active)
			&& _canActivateDriverDistrictPrioritySetPermission;
		
		public bool CanCopyDistrictPrioritySet => SelectedDistrictPrioritySet != null && DriverDistrictPrioritySetPermission.CanCreate;
		public bool CanEditDistrictPrioritySet => 
			SelectedDistrictPrioritySet != null 
			&& (DriverDistrictPrioritySetPermission.CanUpdate || DriverDistrictPrioritySetPermission.CanRead);

		public object SelectedDriverScheduleSet
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

		public bool CanEditEmployeeDocument => SelectedEmployeeDocuments.Any();
		public bool CanRemoveEmployeeDocument => SelectedEmployeeDocuments.Any();
		
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

		public DelegateCommand SaveCommand => _saveCommand ?? (_saveCommand = new DelegateCommand(
				() =>
				{
					if(Entity.Id == 0 && !_canManageOfficeWorkers && !_canManageDriversAndForwarders)
					{
						CommonServices.InteractiveService.ShowMessage(
							ImportanceLevel.Info,
							"У вас недостаточно прав для создания сотрудника");
						
						return;
					}
					
					//Проверяем, чтобы в БД не попала пустая строка
					if(string.IsNullOrWhiteSpace(Entity.AndroidLogin))
					{
						Entity.AndroidLogin = null;
					}

					//var valid = new QSValidator<Employee>(UoWGeneric.Root, Entity.GetValidationContextItems(_subdivisionService));
					ValidationContext.Items.Add("Reason", _subdivisionService);

					if(!Validate())
					{
						return/* false*/;
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
								return /* false*/;
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
								return/* false*/;
							}
						}
					}

					Entity.CreateDefaultWageParameter(
						_wageCalculationRepository,
						new BaseParametersProvider(),
						CommonServices.InteractiveService);

					//phonesView.RemoveEmpty();
					UoWGeneric.Save(Entity);

					#region Попытка сохранить логин для нового юзера
					
					if(!TrySaveNewUser())
					{
						return/* false*/;
					}

					#endregion

					logger.Info("Сохраняем сотрудника...");
					try
					{
						UoWGeneric.Save();
						/*if(UoWGeneric.IsNew)
						{
							attachmentFiles.ItemId = UoWGeneric.Root.Id;
						}
						attachmentFiles.SaveChanges();*/
					}
					catch(Exception ex)
					{
						logger.Error(ex, "Не удалось записать сотрудника.");
						CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Error, ex.Message);
						return/* false*/;
					}
					
					logger.Info("Ok");
					return /*true*/;
				}
			)
		);

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
							UnitOfWorkFactory,
							CommonServices,
							new BaseParametersProvider(),
							_employeeRepository
						);

						driverDistrictPrioritySetViewModel.EntityAccepted += (o, eventArgs) => {
							var now = DateTime.Now;
							eventArgs.AcceptedEntity.DateCreated = now;
							eventArgs.AcceptedEntity.DateLastChanged = now;
							Entity.AddDriverDistrictPrioritySet(eventArgs.AcceptedEntity);
						};

						TabParent.AddSlaveTab(this, driverDistrictPrioritySetViewModel);
					},
					() => SelectedDistrictPrioritySet is DriverDistrictPrioritySet
				)
			);

		//TODO Подумать над частым использованием is
		public DelegateCommand OpenDistrictPrioritySetEditWindowCommand =>
			_openDistrictPrioritySetEditWindowCommand ?? (_openDistrictPrioritySetEditWindowCommand = new DelegateCommand(
					() =>
					{
						if(!(SelectedDistrictPrioritySet is DriverDistrictPrioritySet districtPrioritySet))
						{
							return;
						}
						
						var driverDistrictPrioritySetViewModel = new DriverDistrictPrioritySetViewModel(
							districtPrioritySet,
							UoW,
							UnitOfWorkFactory,
							CommonServices,
							new BaseParametersProvider(),
							_employeeRepository
						);

						driverDistrictPrioritySetViewModel.EntityAccepted += (o, eventArgs) => 
						{
							eventArgs.AcceptedEntity.DateLastChanged = DateTime.Now;
						};

						TabParent.AddSlaveTab(this, driverDistrictPrioritySetViewModel);
					},
					() => SelectedDistrictPrioritySet is DriverDistrictPrioritySet
				)
			);

		public DelegateCommand CopyDistrictPrioritySetCommand =>
			_copyDistrictPrioritySetCommand ?? (_copyDistrictPrioritySetCommand = new DelegateCommand(
					() =>
					{
						if(!(SelectedDistrictPrioritySet is DriverDistrictPrioritySet selectedDistrictPrioritySet))
						{
							return;
						}

						if(selectedDistrictPrioritySet.Id == 0)
						{
							ServicesConfig.CommonServices.InteractiveService
								.ShowMessage(ImportanceLevel.Info,
									"Перед копированием новой версии необходимо сохранить сотрудника");
							return;
						}

						var newDistrictPrioritySet = DriverDistrictPriorityHelper.CopyPrioritySetWithActiveDistricts(
							selectedDistrictPrioritySet,
							out var notCopiedPriorities
						);
						newDistrictPrioritySet.IsCreatedAutomatically = false;

						if(notCopiedPriorities.Any())
						{
							var messageBuilder = new StringBuilder(
								"Для некоторых приоритетов районов\n" +
								$"из выбранной для копирования версии (Код: {selectedDistrictPrioritySet.Id})\n" +
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
							UnitOfWorkFactory,
							CommonServices,
							new BaseParametersProvider(),
							_employeeRepository
						);

						driverDistrictPrioritySetViewModel.EntityAccepted += (o, eventArgs) => {
							var now = DateTime.Now;
							eventArgs.AcceptedEntity.DateCreated = now;
							eventArgs.AcceptedEntity.DateLastChanged = now;
							Entity.AddDriverDistrictPrioritySet(eventArgs.AcceptedEntity);
						};

						TabParent.AddSlaveTab(this, driverDistrictPrioritySetViewModel);
					},
					() => SelectedDistrictPrioritySet is DriverDistrictPrioritySet
				)
			);
		
		public DelegateCommand ActivateDistrictPrioritySetCommand => 
			_activateDistrictPrioritySetCommand ?? (_activateDistrictPrioritySetCommand = new DelegateCommand(
					() =>
					{
						if(!(SelectedDistrictPrioritySet is DriverDistrictPrioritySet districtPrioritySet))
						{
							return;
						}
			
						var employeeForCurrentUser = _employeeRepository.GetEmployeeForCurrentUser(UoW);
						var now = DateTime.Now;

						districtPrioritySet.DateLastChanged = now;
						districtPrioritySet.DateActivated = now;

						Entity.ActivateDriverDistrictPrioritySet(districtPrioritySet, employeeForCurrentUser);
					},
					() => SelectedDistrictPrioritySet is DriverDistrictPrioritySet
				)
			);
		
		public DelegateCommand CopyDriverWorkScheduleSetCommand =>
			_copyDriverWorkScheduleSetCommand ?? (_copyDriverWorkScheduleSetCommand = new DelegateCommand(
					() =>
					{
						if(!(SelectedDriverScheduleSet is DriverWorkScheduleSet selectedScheduleSet))
						{
							return;
						}

						if(selectedScheduleSet.Id == 0)
						{
							CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Info,
								"Перед копированием новой версии необходимо сохранить сотрудника");
							return;
						}

						if(CommonServices.InteractiveService.Question(
							$"Скопировать и активировать выбранную версию графиков работы водителя " +
							$"(Код: {selectedScheduleSet.Id})?")
						)
						{
							var employeeForCurrentUser = _employeeRepository.GetEmployeeForCurrentUser(UoW);

							var newScheduleSet = (DriverWorkScheduleSet)selectedScheduleSet.Clone();
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
							new BaseParametersProvider(),
							_employeeRepository
						);
			
						driverWorkScheduleSetViewModel.EntityAccepted += (o, eventArgs) => 
						{
							Entity.AddActiveDriverWorkScheduleSet(newDriverWorkScheduleSet);
						};

						TabParent.AddSlaveTab(this, driverWorkScheduleSetViewModel);
					},
					() => true
				)
			);

		public DelegateCommand OpenDriverWorkScheduleSetEditWindowCommand =>
			_openDriverWorkScheduleSetEditWindowCommand ?? (_openDriverWorkScheduleSetEditWindowCommand = new DelegateCommand(
					() =>
					{
						if(!(SelectedDriverScheduleSet is DriverWorkScheduleSet workScheduleSet))
						{
							return;
						}

						var driverWorkScheduleSetViewModel = new DriverWorkScheduleSetViewModel(
							workScheduleSet,
							UoW,
							CommonServices,
							new BaseParametersProvider(),
							_employeeRepository
						);
						TabParent.AddSlaveTab(this, driverWorkScheduleSetViewModel);
					},
					() => SelectedDriverScheduleSet is DriverDistrictPrioritySet
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
		
		public void FillHiddenCategories()
		{
			var allCategories = (EmployeeCategory[])Enum.GetValues(typeof(EmployeeCategory));

			if(_canManageDriversAndForwarders && !_canManageOfficeWorkers)
			{
				HiddenCategories = allCategories.Except(new[] {EmployeeCategory.driver, EmployeeCategory.forwarder});
			}
			else if(_canManageOfficeWorkers && !_canManageDriversAndForwarders)
			{
				HiddenCategories = allCategories.Except(new[] {EmployeeCategory.office});
			}
		}
	}
}
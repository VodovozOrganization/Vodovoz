using EmailService;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gamma.Widgets;
using NLog;
using QS.Banks.Domain;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.DB;
using QS.Project.Dialogs.GtkUI.ServiceDlg;
using QS.Project.Repositories;
using QS.Project.Services;
using QS.Project.Services.GtkUI;
using QS.Services;
using QS.Widgets.GtkUI;
using QSOrmProject;
using QSProjectsLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using QS.Tdi;
using Vodovoz.Additions;
using Vodovoz.Core.DataService;
using Vodovoz.Dialogs.Employees;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Permissions;
using Vodovoz.Domain.Service.BaseParametersServices;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Permissions;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.EntityRepositories.WageCalculation;
using Vodovoz.Infrastructure;
using Vodovoz.JournalFilters;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.Tools;
using Vodovoz.Tools.Logistic;
using Vodovoz.ViewModel;
using Vodovoz.ViewModels.Infrastructure.Services;
using Vodovoz.ViewModels.Journals.JournalSelectors;
using Vodovoz.ViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.WageCalculation;
using UserRepository = Vodovoz.EntityRepositories.UserRepository;

namespace Vodovoz
{
	[Obsolete("Используйте EmployeeViewModel")]
	public partial class EmployeeDlg : QS.Dialog.Gtk.EntityDialogBase<Employee>, INotifyPropertyChanged
	{
		private readonly IEmployeeRepository _employeeRepository = new EmployeeRepository();
		private readonly IUserRepository _userRepository = new UserRepository();
		private readonly IWageCalculationRepository _wageCalculationRepository = new WageCalculationRepository();
		private readonly BaseParametersProvider _baseParametersProvider = new BaseParametersProvider(new ParametersProvider());

		private ICashDistributionCommonOrganisationProvider commonOrganisationProvider =
			new CashDistributionCommonOrganisationProvider(
				new OrganizationParametersProvider(new ParametersProvider()));

		private TerminalManagementViewModel _terminalManagementViewModel;
		private Employee _employeeForCurrentUser;
		private ValidationContext _validationContext;

		public EmployeeDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Employee>();
			mySQLUserRepository = new MySQLUserRepository(
				new MySQLProvider(new GtkRunOperationService(), new GtkQuestionDialogsInteractive()),
				new GtkInteractiveService());
			this.authorizationService = new AuthorizationService(
				new PasswordGenerator(),
				new MySQLUserRepository(
					new MySQLProvider(new GtkRunOperationService(), new GtkQuestionDialogsInteractive()),
					new GtkInteractiveService()),
				EmailServiceSetting.GetEmailService());
			

			TabName = "Новый сотрудник";
			ConfigureDlg();
		}

		public EmployeeDlg(int id)
		{
			this.Build();
			logger.Info("Загрузка информации о сотруднике...");
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Employee>(id);
			mySQLUserRepository = new MySQLUserRepository(
				new MySQLProvider(new GtkRunOperationService(), new GtkQuestionDialogsInteractive()),
				new GtkInteractiveService());
			
			this.authorizationService = new AuthorizationService(
				new PasswordGenerator(),
				new MySQLUserRepository(
					new MySQLProvider(new GtkRunOperationService(), new GtkQuestionDialogsInteractive()),
					new GtkInteractiveService()),
				EmailServiceSetting.GetEmailService());

			ConfigureDlg();
		}

		public EmployeeDlg(Employee sub) : this(sub.Id) {}

		public EmployeeDlg(IUnitOfWorkGeneric<Employee> uow)
		{
			this.Build();
			UoWGeneric = uow;

			if(!ServicesConfig.CommonServices
				.CurrentPermissionService.ValidatePresetPermission("can_change_trainee_to_driver"))
			{
				hiddenCategory.Add(EmployeeCategory.driver);
				hiddenCategory.Add(EmployeeCategory.forwarder);
			}

			mySQLUserRepository = new MySQLUserRepository(
				new MySQLProvider(new GtkRunOperationService(), new GtkQuestionDialogsInteractive()),
				new GtkInteractiveService());
			this.authorizationService = new AuthorizationService(
				new PasswordGenerator(),
				new MySQLUserRepository(
					new MySQLProvider(new GtkRunOperationService(), new GtkQuestionDialogsInteractive()),
					new GtkInteractiveService()),
				EmailServiceSetting.GetEmailService());
			
			ConfigureDlg();
		}
		
		private ISubdivisionService subdivisionService;
		private bool canManageDriversAndForwarders;
		private bool canManageOfficeWorkers;
		private bool canEditOrganisationForSalary;
		private bool _canEditWage;
		private bool _canEditWageBySelfSubdivision;
		private IPermissionResult _employeeDocumentsPermissionsSet;

		private static readonly Logger logger = LogManager.GetCurrentClassLogger();
		private readonly MySQLUserRepository mySQLUserRepository;
		private readonly List<EmployeeCategory> hiddenCategory = new List<EmployeeCategory>();
		private readonly EmployeeDocumentType[] hiddenForRussianDocument =
			{
				EmployeeDocumentType.RefugeeId,
				EmployeeDocumentType.RefugeeCertificate,
				EmployeeDocumentType.Residence,
				EmployeeDocumentType.ForeignCitizenPassport
			};
		private readonly EmployeeDocumentType[] hiddenForForeignCitizen =
			{
				EmployeeDocumentType.MilitaryID,
				EmployeeDocumentType.NavyPassport,
				EmployeeDocumentType.OfficerCertificate
			};
		private readonly IAuthorizationService authorizationService;
		
		private void ConfigureDlg()
		{
			if (Entity.Id == 0) {
				Entity.OrganisationForSalary = commonOrganisationProvider.GetCommonOrganisation(UoW);
			}

			_employeeDocumentsPermissionsSet = ServicesConfig.CommonServices.PermissionService
				.ValidateUserPermission(typeof(EmployeeDocument), ServicesConfig.CommonServices.UserService.CurrentUserId);

			CanReadEmployeeDocuments = _employeeDocumentsPermissionsSet.CanRead;
			CanAddEmployeeDocument = _employeeDocumentsPermissionsSet.CanCreate;

			_employeeForCurrentUser = _employeeRepository.GetEmployeeForCurrentUser(UoW);

			canActivateDriverDistrictPrioritySetPermission = ServicesConfig.CommonServices
				.CurrentPermissionService.ValidatePresetPermission("can_activate_driver_district_priority_set");
			canManageDriversAndForwarders = ServicesConfig.CommonServices
				.CurrentPermissionService.ValidatePresetPermission("can_manage_drivers_and_forwarders");
			canManageOfficeWorkers = ServicesConfig.CommonServices
				.CurrentPermissionService.ValidatePresetPermission("can_manage_office_workers");
			canEditOrganisationForSalary = ServicesConfig.CommonServices
				.CurrentPermissionService.ValidatePresetPermission("can_edit_organisation_for_salary");
			_canEditWage = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_wage");
			_canEditWageBySelfSubdivision = ServicesConfig.CommonServices.UserService.GetCurrentUser(UoW).IsAdmin ||
			                                (_employeeForCurrentUser.Subdivision == Entity.Subdivision &&
			                                 ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission(
				                                 "can_edit_wage_by_self_subdivision")
			                                 );

			ConfigureCategory();
			ConfigureSubdivision();
			OnRussianCitizenToggled(null, EventArgs.Empty);
			dataentryDrivingNumber.MaxLength = 20;
			dataentryDrivingNumber.Binding
				.AddBinding(Entity, e => e.DrivingLicense, w => w.Text).InitializeFromSource();
			UoWGeneric.Root.PropertyChanged += OnPropertyChanged;
			notebookMain.Page = 0;
			notebookMain.ShowTabs = false;
			GenderComboBox.ItemsEnum = typeof(Gender);
			GenderComboBox.Binding
				.AddBinding(Entity, e => e.Gender, w => w.SelectedItemOrNull).InitializeFromSource();

			subdivisionService = SubdivisionParametersProvider.Instance;

			yenumcomboStatus.ItemsEnum = typeof(EmployeeStatus);
			yenumcomboStatus.Binding.AddBinding(Entity, e => e.Status, w => w.SelectedItem).InitializeFromSource();

			chkDriverForOneDay.Binding
				.AddBinding(Entity, e => e.IsDriverForOneDay, w => w.Active).InitializeFromSource();
			cmbDriverOf.ItemsEnum = typeof(CarTypeOfUse);
			cmbDriverOf.Binding
				.AddBinding(Entity, e => e.DriverOf, w => w.SelectedItemOrNull).InitializeFromSource();

			dataentryLastName.Binding.AddBinding(Entity, e => e.LastName, w => w.Text).InitializeFromSource();
			dataentryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			dataentryPatronymic.Binding.AddBinding(Entity, e => e.Patronymic, w => w.Text).InitializeFromSource();
			dataentryInnerPhone.Binding
				.AddBinding(
					Entity,
					e => e.InnerPhone,
					w => w.Text,
					new Gamma.Binding.Converters.NumbersToStringConverter()
				).InitializeFromSource();

			entryAddressCurrent.Binding
				.AddBinding(Entity, e => e.AddressCurrent, w => w.Text).InitializeFromSource();
			entryAddressRegistration.Binding
				.AddBinding(Entity, e => e.AddressRegistration, w => w.Text).InitializeFromSource();
            yentryEmailAddress.Binding.AddBinding(Entity, e => e.Email, w => w.Text).InitializeFromSource();

			entryInn.Binding.AddBinding(Entity, e => e.INN, w => w.Text).InitializeFromSource();
            comboSkillLevel.ItemsList = Entity.GetSkillLevels();
            comboSkillLevel.Binding
				.AddBinding(
					Entity,
					e => e.SkillLevel,
					w => w.ActiveText,
					new Gamma.Binding.Converters.NumbersToStringConverter()
				).InitializeFromSource();
            comboSkillLevel.SelectedItem = Entity.SkillLevel;

            dataentryAndroidLogin.Binding
				.AddBinding(Entity, e => e.AndroidLogin, w => w.Text).InitializeFromSource();
			dataentryAndroidPassword.Binding
				.AddBinding(Entity, e => e.AndroidPassword, w => w.Text).InitializeFromSource();

			var filterDefaultForwarder = new EmployeeRepresentationFilterViewModel();
			filterDefaultForwarder.SetAndRefilterAtOnce(
				x => x.Category = EmployeeCategory.forwarder,
				x => x.Status = EmployeeStatus.IsWorking
			);
			repEntDefaultForwarder.RepresentationModel = new EmployeesVM(filterDefaultForwarder);
			repEntDefaultForwarder.Binding
				.AddBinding(Entity, e => e.DefaultForwarder, w => w.Subject).InitializeFromSource();
			
            var employeePostsJournalFactory = new EmployeePostsJournalFactory();
            entryEmployeePost.SetEntityAutocompleteSelectorFactory(
	            employeePostsJournalFactory.CreateEmployeePostsAutocompleteSelectorFactory());
            entryEmployeePost.Binding.AddBinding(Entity, e => e.Post, w => w.Subject).InitializeFromSource();

            referenceNationality.SubjectType = typeof(Nationality);
			referenceNationality.Binding
				.AddBinding(Entity, e => e.Nationality, w => w.Subject).InitializeFromSource();
			referenceCitizenship.SubjectType = typeof(Citizenship);
			referenceCitizenship.Binding
				.AddBinding(Entity, e => e.Citizenship, w => w.Subject).InitializeFromSource();

			referenceUser.SubjectType = typeof(User);
			referenceUser.CanEditReference = false;
			referenceUser.Binding.AddBinding(Entity, e => e.User, w => w.Subject).InitializeFromSource();
			referenceUser.Sensitive = ServicesConfig.CommonServices
				.CurrentPermissionService.ValidatePresetPermission("can_manage_users");

			yenumcombobox13.ItemsEnum = typeof(RegistrationType);
			yenumcombobox13.Binding
				.AddBinding(Entity, e => e.Registration, w => w.SelectedItemOrNull).InitializeFromSource();

			comboDriverType.ItemsEnum = typeof(DriverType);
			comboDriverType.Binding
				.AddBinding(Entity, e => e.DriverType, w => w.SelectedItemOrNull).InitializeFromSource();

			ydatepicker1.Binding
				.AddBinding(Entity, e => e.BirthdayDate, w => w.DateOrNull).InitializeFromSource();
			dateFired.Binding.AddBinding(Entity, e => e.DateFired, w => w.DateOrNull).InitializeFromSource();
			dateHired.Binding.AddBinding(Entity, e => e.DateHired, w => w.DateOrNull).InitializeFromSource();
			dateCalculated.Binding
				.AddBinding(Entity, e => e.DateCalculated, w => w.DateOrNull).InitializeFromSource();

			photoviewEmployee.Binding.AddBinding(Entity, e => e.Photo, w => w.ImageFile).InitializeFromSource();
			photoviewEmployee.GetSaveFileName = () => Entity.FullName;

			attachmentFiles.AttachToTable = OrmConfig.GetDBTableName(typeof(Employee));
			if(Entity.Id != 0) {
				attachmentFiles.ItemId = UoWGeneric.Root.Id;
				attachmentFiles.UpdateFileList();
			}
			phonesView.UoW = UoWGeneric;
			if(UoWGeneric.Root.Phones == null)
				UoWGeneric.Root.Phones = new List<Phone>();
			phonesView.Phones = UoWGeneric.Root.Phones;
			accountsView.ParentReference = new ParentReferenceGeneric<Employee, Account>(
				UoWGeneric, o => o.Accounts);
			accountsView.SetTitle("Банковские счета сотрудника");

			ydateFirstWorkDay.Binding
				.AddBinding(Entity, e => e.FirstWorkDay, w => w.DateOrNull).InitializeFromSource();
			yspinTripsPriority.Binding
				.AddBinding(Entity, e => e.TripPriority, w => w.ValueAsShort).InitializeFromSource();
			yspinDriverSpeed.Binding
				.AddBinding(Entity, e => e.DriverSpeed, w => w.Value, new MultiplierToPercentConverter())
				.InitializeFromSource();
			minAddressesSpin.Binding
				.AddBinding(Entity, e => e.MinRouteAddresses, w => w.ValueAsInt).InitializeFromSource();
			maxAddressesSpin.Binding
				.AddBinding(Entity, e => e.MaxRouteAddresses, w => w.ValueAsInt).InitializeFromSource();
			checkbuttonRussianCitizen.Binding
				.AddBinding(Entity, e => e.IsRussianCitizen, w => w.Active).InitializeFromSource();
			checkVisitingMaster.Binding
				.AddBinding(Entity, e => e.VisitingMaster, w => w.Active).InitializeFromSource();
			checkChainStoreDriver.Binding
				.AddBinding(Entity, e => e.IsChainStoreDriver, w => w.Active).InitializeFromSource();

			ylblUserLogin.TooltipText =
				"При сохранении сотрудника создаёт нового пользователя с введённым логином " +
				"и отправляет сотруднику SMS с сгенерированным паролем";
			yentryUserLogin.Binding.AddBinding(Entity, e => e.LoginForNewUser, w => w.Text);
			yentryUserLogin.Sensitive = CanCreateNewUser;

			specialListCmbOrganisation.ItemsList = UoW.GetAll<Organization>();
			specialListCmbOrganisation.Binding
				.AddBinding(Entity, e => e.OrganisationForSalary, w => w.SelectedItem).InitializeFromSource();
			specialListCmbOrganisation.Sensitive = canEditOrganisationForSalary;

			ConfigureWorkSchedules();
			ConfigureDistrictPriorities();
			
			ytreeviewEmployeeDocument.ColumnsConfig = FluentColumnsConfig<EmployeeDocument>.Create()
				.AddColumn("Документ").AddTextRenderer(x => x.Document.GetEnumTitle())
				.AddColumn("Доп. название").AddTextRenderer(x => x.Name)
				.Finish();
			ytreeviewEmployeeDocument.SetItemsSource(Entity.ObservableDocuments);

			ytreeviewEmployeeContract.ColumnsConfig = FluentColumnsConfig<EmployeeContract>.Create()
				.AddColumn("Договор").AddTextRenderer(x => x.EmployeeContractTemplate.TemplateType.GetEnumTitle())
				.AddColumn("Название").AddTextRenderer(x => x.Name)
				.AddColumn("Дата начала").AddTextRenderer(x => x.FirstDay.ToString("dd/MM/yyyy"))
				.AddColumn("Дата конца").AddTextRenderer(x => x.LastDay.ToString("dd/MM/yyyy"))
				.Finish();
			ytreeviewEmployeeContract.SetItemsSource(Entity.ObservableContracts);

			wageParametersView.ViewModel = new EmployeeWageParametersViewModel
			(
				Entity, 
				this, 
				UoW, 
				new HierarchicalPresetPermissionValidator(
					_employeeRepository,
					new PermissionRepository()),
				_userRepository,
				ServicesConfig.CommonServices,
				NavigationManagerProvider.NavigationManager,
				_employeeRepository,
				_wageCalculationRepository
			);

			radioTabEmployeeDocument.Sensitive = CanReadEmployeeDocuments;
			buttonAddDocument.Sensitive = CanAddEmployeeDocument;

			ConfigureValidationContext();

			logger.Info("Ok");
		}

		public bool CanReadEmployeeDocuments { get; private set; }
		public bool CanAddEmployeeDocument { get; private set; }

		#region DriverDistrictPriorities

		private IPermissionResult driverDistrictPrioritySetPermission;
		private bool canActivateDriverDistrictPrioritySetPermission;

		private void ConfigureDistrictPriorities()
		{
			driverDistrictPrioritySetPermission =
				ServicesConfig.CommonServices.CurrentPermissionService.ValidateEntityPermission(
					typeof(DriverDistrictPrioritySet));

			ytreeDistrictPrioritySets.ColumnsConfig = FluentColumnsConfig<DriverDistrictPrioritySet>.Create()
				.AddColumn("Код")
					.HeaderAlignment(0.5f)
					.MinWidth(75)
					.AddTextRenderer(x => x.Id == 0 ? "Новый" : x.Id.ToString())
					.XAlign(0.5f)
				.AddColumn("Активен")
					.HeaderAlignment(0.5f)
					.AddToggleRenderer(x => x.IsActive)
					.XAlign(0.5f)
					.Editing(false)
				.AddColumn("Дата\nсоздания")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.DateCreated.ToString("g"))
				.AddColumn("Дата\nпоследнего изменения")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.DateLastChanged.ToString("g"))
				.AddColumn("Дата\nактивации")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.DateActivated != null ? x.DateActivated.Value.ToString("g") : "")
				.AddColumn("Дата\nдеактивации")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.DateDeactivated != null ? x.DateDeactivated.Value.ToString("g") : "")
				.AddColumn("Автор")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.Author != null ? x.Author.ShortName : "-")
					.XAlign(0.5f)
				.AddColumn("Изменил")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.LastEditor != null ? x.LastEditor.ShortName : "-")
					.XAlign(0.5f)
				.AddColumn("Создан\nавтоматически")
					.HeaderAlignment(0.5f)
					.AddToggleRenderer(x => x.IsCreatedAutomatically)
					.XAlign(0.5f)
					.Editing(false)
				.AddColumn("")
				.Finish();

			ytreeDistrictPrioritySets.RowActivated += (o, args) => {
				if(ytreeDistrictPrioritySets.GetSelectedObject() != null
					&& (driverDistrictPrioritySetPermission.CanUpdate 
						|| driverDistrictPrioritySetPermission.CanRead)
				) {
					OpenDistrictPrioritySetEditWindow();
				}
			};
			ytreeDistrictPrioritySets.ItemsDataSource = Entity.ObservableDriverDistrictPrioritySets;

			ybuttonCopyDistrictPrioritySet.Sensitive = false;
			ybuttonCopyDistrictPrioritySet.Clicked += OnButtonCopyDistrictPrioritySetClicked;

			ybuttonEditDistrictPrioritySet.Sensitive = false;
			ybuttonEditDistrictPrioritySet.Clicked += (sender, args) => OpenDistrictPrioritySetEditWindow();

			ybuttonActivateDistrictPrioritySet.Clicked += (sender, args) => OnActivateDistrictPrioritySetClicked();
			ybuttonActivateDistrictPrioritySet.Binding
				.AddBinding(this, x => x.CanActivateDistrictPrioritySet, w => w.Sensitive).InitializeFromSource();

			ytreeDistrictPrioritySets.Selection.Changed += (o, args) => {
				var selectedDistrictPrioritySet 
					= ytreeDistrictPrioritySets.GetSelectedObject() as DriverDistrictPrioritySet;
				ybuttonCopyDistrictPrioritySet.Sensitive = selectedDistrictPrioritySet != null
					&& driverDistrictPrioritySetPermission.CanCreate;
				ybuttonEditDistrictPrioritySet.Sensitive = selectedDistrictPrioritySet != null
					&& (driverDistrictPrioritySetPermission.CanUpdate 
					|| driverDistrictPrioritySetPermission.CanRead);
				CanActivateDistrictPrioritySet = selectedDistrictPrioritySet != null
					&& !selectedDistrictPrioritySet.IsActive 
					&& selectedDistrictPrioritySet.DateActivated == null
					&& selectedDistrictPrioritySet.ObservableDriverDistrictPriorities
						.All(x => x.District.DistrictsSet.Status == DistrictsSetStatus.Active)
					&& canActivateDriverDistrictPrioritySetPermission;
			};

			ybuttonCreateDistrictPrioritySet.Clicked += (sender, args) => OpenDistrictPrioritySetCreateWindow();
			ybuttonCreateDistrictPrioritySet.Sensitive = driverDistrictPrioritySetPermission.CanCreate;
		}

		private bool canActivateDistrictPrioritySet;
		public bool CanActivateDistrictPrioritySet
		{
			get => canActivateDistrictPrioritySet;
			private set {
				canActivateDistrictPrioritySet = value;
				OnPropertyChanged(nameof(CanActivateDistrictPrioritySet));
			}
		}

		private void OnButtonCopyDistrictPrioritySetClicked(object sender, EventArgs e)
		{
			if (!(ytreeDistrictPrioritySets.GetSelectedObject()
				is DriverDistrictPrioritySet selectedDistrictPrioritySet))
			{
				return;
			}

			if(selectedDistrictPrioritySet.Id == 0) {
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

			if (notCopiedPriorities.Any()) {
				var messageBuilder = new StringBuilder(
					"Для некоторых приоритетов районов\n" +
					$"из выбранной для копирования версии (Код: {selectedDistrictPrioritySet.Id})\n" +
					"не были найдены связанные районы из активной\n" +
					"версии районов. Список приоритетов районов,\n" +
					"которые не будут скопированы:\n"
				);

				foreach(var driverDistrictPriority in notCopiedPriorities) {
					messageBuilder.AppendLine(
						$"Район: ({driverDistrictPriority.District.Id}) " +
						$"{driverDistrictPriority.District.DistrictName}. " +
						$"Приоритет: {driverDistrictPriority.Priority + 1}"
					);
				}
				ServicesConfig.InteractiveService.ShowMessage(ImportanceLevel.Warning, messageBuilder.ToString());
			}

			var driverDistrictPrioritySetViewModel = new DriverDistrictPrioritySetViewModel(
				newDistrictPrioritySet,
				UoW,
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
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

		private void OpenDistrictPrioritySetEditWindow()
		{
			if(!(ytreeDistrictPrioritySets.GetSelectedObject() is DriverDistrictPrioritySet districtPrioritySet)) {
				return;
			}
				
			var driverDistrictPrioritySetViewModel = new DriverDistrictPrioritySetViewModel(
				districtPrioritySet,
				UoW,
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				_baseParametersProvider,
				_employeeRepository
			);

			driverDistrictPrioritySetViewModel.EntityAccepted += (o, eventArgs) => {
				eventArgs.AcceptedEntity.DateLastChanged = DateTime.Now;
			};

			TabParent.AddSlaveTab(this, driverDistrictPrioritySetViewModel);
		}

		private void OnActivateDistrictPrioritySetClicked()
		{
			if (!(ytreeDistrictPrioritySets.GetSelectedObject() is DriverDistrictPrioritySet districtPrioritySet))
			{
				return;
			}

			var now = DateTime.Now;

			districtPrioritySet.DateLastChanged = now;
			districtPrioritySet.DateActivated = now;

			Entity.ActivateDriverDistrictPrioritySet(districtPrioritySet, _employeeForCurrentUser);
		}

		private void OpenDistrictPrioritySetCreateWindow()
		{
			var newDistrictPrioritySet = new DriverDistrictPrioritySet {
				Driver = Entity,
				IsCreatedAutomatically = false
			};
			
			var driverDistrictPrioritySetViewModel = new DriverDistrictPrioritySetViewModel(
				newDistrictPrioritySet,
				UoW,
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
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

		#endregion

		#region DriverWorkSchedules

		private IPermissionResult driverWorkScheduleSetPermission;

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void ConfigureWorkSchedules()
		{
			driverWorkScheduleSetPermission =
				ServicesConfig.CommonServices.CurrentPermissionService.ValidateEntityPermission(
					typeof(DriverWorkScheduleSet));
			
			ytreeDriverScheduleSets.ColumnsConfig = FluentColumnsConfig<DriverWorkScheduleSet>.Create()
				.AddColumn("Код")
					.HeaderAlignment(0.5f)
					.MinWidth(75)
					.AddTextRenderer(x => x.Id == 0 ? "Новый" : x.Id.ToString())
					.XAlign(0.5f)
				.AddColumn("Активен")
					.HeaderAlignment(0.5f)
					.AddToggleRenderer(x => x.IsActive)
					.XAlign(0.5f)
					.Editing(false)
				.AddColumn("Дата\nактивации")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.DateActivated.ToString("g"))
				.AddColumn("Дата\nдеактивации")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.DateDeactivated != null ? x.DateDeactivated.Value.ToString("g") : "")
				.AddColumn("Автор")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.Author != null ? x.Author.ShortName : "-")
					.XAlign(0.5f)
				.AddColumn("Изменил")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.LastEditor != null ? x.LastEditor.ShortName : "-")
					.XAlign(0.5f)
				.AddColumn("Создан\nавтоматически")
					.HeaderAlignment(0.5f)
					.AddToggleRenderer(x => x.IsCreatedAutomatically)
					.XAlign(0.5f)
					.Editing(false)
				.AddColumn("")
				.Finish();

			ytreeDriverScheduleSets.RowActivated += (o, args) => {
				if(ytreeDriverScheduleSets.GetSelectedObject() != null 
					&& (driverWorkScheduleSetPermission.CanUpdate || driverWorkScheduleSetPermission.CanRead)) {
					OpenDriverWorkScheduleSetEditWindow();
				}
			};
			ytreeDriverScheduleSets.ItemsDataSource = Entity.ObservableDriverWorkScheduleSets;
			
			ybuttonCopyScheduleSet.Sensitive = false;
			ybuttonCopyScheduleSet.Clicked += OnButtonCopyScheduleSetClicked;

			ybuttonEditScheduleSet.Sensitive = false;
			ybuttonEditScheduleSet.Clicked += (sender, args) => OpenDriverWorkScheduleSetEditWindow();

			ytreeDriverScheduleSets.Selection.Changed += (o, args) => {
				ybuttonCopyScheduleSet.Sensitive = ytreeDriverScheduleSets.GetSelectedObject() != null
					&& driverWorkScheduleSetPermission.CanCreate;
				ybuttonEditScheduleSet.Sensitive = ytreeDriverScheduleSets.GetSelectedObject() != null
					&& (driverWorkScheduleSetPermission.CanUpdate || driverWorkScheduleSetPermission.CanRead);
			};

			ybuttonCreateScheduleSet.Clicked += (sender, args) => OpenDriverWorkScheduleSetCreateWindow();
			ybuttonCreateScheduleSet.Sensitive = driverWorkScheduleSetPermission.CanCreate;
		}

		private void OnButtonCopyScheduleSetClicked(object sender, EventArgs args)
		{
			var selectedScheduleSet = ytreeDriverScheduleSets.GetSelectedObject() as DriverWorkScheduleSet;

			if(selectedScheduleSet != null && selectedScheduleSet.Id == 0) {
				ServicesConfig.CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Info,
					"Перед копированием новой версии необходимо сохранить сотрудника");
				return;
			}

			if(selectedScheduleSet != null
				&& ServicesConfig.CommonServices.InteractiveService.Question(
					$"Скопировать и активировать выбранную версию графиков работы водителя " +
					$"(Код: {selectedScheduleSet.Id})?"
				)
			) {

				var newScheduleSet = (DriverWorkScheduleSet)selectedScheduleSet.Clone();
				newScheduleSet.Author = _employeeForCurrentUser;
				newScheduleSet.LastEditor = _employeeForCurrentUser;
				newScheduleSet.IsCreatedAutomatically = false;

				Entity.AddActiveDriverWorkScheduleSet(newScheduleSet);
			}
		}

		private void OpenDriverWorkScheduleSetEditWindow()
		{
			if(!(ytreeDriverScheduleSets.GetSelectedObject() is DriverWorkScheduleSet workScheduleSet)) {
				return;
			}
				
			var driverWorkScheduleSetViewModel = new DriverWorkScheduleSetViewModel(
				workScheduleSet,
				UoW,
				ServicesConfig.CommonServices,
				_baseParametersProvider,
				_employeeRepository
			);
			TabParent.AddSlaveTab(this, driverWorkScheduleSetViewModel);
		}

		private void OpenDriverWorkScheduleSetCreateWindow()
		{
			var newDriverWorkScheduleSet = new DriverWorkScheduleSet {
				Driver = Entity,
				IsCreatedAutomatically = false
			};
			
			var driverWorkScheduleSetViewModel = new DriverWorkScheduleSetViewModel(
				newDriverWorkScheduleSet,
				UoW,
				ServicesConfig.CommonServices,
				_baseParametersProvider,
				_employeeRepository
			);
			driverWorkScheduleSetViewModel.EntityAccepted += (o, eventArgs) => {
				Entity.AddActiveDriverWorkScheduleSet(newDriverWorkScheduleSet);
			};
			
			TabParent.AddSlaveTab(this, driverWorkScheduleSetViewModel);
		}

		#endregion

		private void ConfigureCategory() 
		{
			comboCategory.ItemsEnum = typeof(EmployeeCategory);
			comboCategory.Binding.AddBinding(Entity, e => e.Category, w => w.SelectedItem).InitializeFromSource();

			if(Entity?.Id != 0) {
				comboCategory.Sensitive = false;
				return; 
			}

			var allCategories = Enum.GetValues(typeof(EmployeeCategory)).Cast<EmployeeCategory>();

			if(!canManageDriversAndForwarders && !canManageOfficeWorkers) {
				comboCategory.Sensitive = false;
				return;
			} else if(canManageDriversAndForwarders && !canManageOfficeWorkers)
			{
				hiddenCategory.AddRange(
					allCategories.Except(
						new EmployeeCategory[] { EmployeeCategory.driver, EmployeeCategory.forwarder }
					)
				);
			}
			else if(canManageOfficeWorkers && !canManageDriversAndForwarders)
			{
				hiddenCategory.AddRange(
					allCategories.Except(new EmployeeCategory[] { EmployeeCategory.office }));
			}

			if (hiddenCategory != null && hiddenCategory.Any()) {
				comboCategory.AddEnumToHideList(hiddenCategory.Distinct().Cast<object>().ToArray());
			}
			comboCategory.ChangedByUser += (sender, e) => {
				if(Entity.Category != EmployeeCategory.driver)
					cmbDriverOf.SelectedItemOrNull = null;
			};
		}

		private void ConfigureSubdivision()
		{
			if(canManageDriversAndForwarders && !canManageOfficeWorkers) {
				var entityentrySubdivision = new EntityViewModelEntry();
				entityentrySubdivision.SetEntityAutocompleteSelectorFactory(
					new SubdivisionJournalFactory().CreateLogisticSubdivisionAutocompleteSelectorFactory(
						new EmployeeJournalFactory().CreateEmployeeAutocompleteSelectorFactory()));
				entityentrySubdivision.Binding
					.AddBinding(Entity, e => e.Subdivision, w => w.Subject).InitializeFromSource();
				hboxSubdivision.Add(entityentrySubdivision);
				hboxSubdivision.ShowAll();
				return;
			}

			var entrySubdivision = new yEntryReference();
			entrySubdivision.SubjectType = typeof(Subdivision);
			entrySubdivision.Binding.AddBinding(Entity, e => e.Subdivision, w => w.Subject).InitializeFromSource();
			hboxSubdivision.Add(entrySubdivision);
			hboxSubdivision.ShowAll();

			if(!canManageOfficeWorkers && !canManageDriversAndForwarders) {
				entrySubdivision.Sensitive = false;
			}
		}
		
		public override bool HasChanges {
			get {
				phonesView.RemoveEmpty();
				return UoWGeneric.HasChanges
					|| attachmentFiles.HasChanges
					|| !string.IsNullOrEmpty(yentryUserLogin.Text)
					|| (_terminalManagementViewModel?.HasChanges ?? false);
			}
			set => base.HasChanges = value;
		}

		bool CanCreateNewUser => Entity.User == null
			&& ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_manage_users");

		void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			logger.Debug("Property {0} changed", e.PropertyName);
		}

		public override bool Save()
		{
			if(Entity.Id == 0 && !canManageOfficeWorkers && !canManageDriversAndForwarders) {
				MessageDialogHelper.RunInfoDialog("У вас недостаточно прав для создания сотрудника");
				return false;
			}
			
			//Проверяем, чтобы в БД не попала пустая строка
			if(string.IsNullOrWhiteSpace(Entity.AndroidLogin))
			{
				Entity.AndroidLogin = null;
			}

			if(!ServicesConfig.ValidationService.Validate(Entity, _validationContext))
			{
				return false;
			}

			if (Entity.User != null) {
				Entity.User.Deactivated = Entity.Status == EmployeeStatus.IsFired;
				var associatedEmployees = _employeeRepository.GetEmployeesForUser(UoW, Entity.User.Id);
				if(associatedEmployees.Any(e => e.Id != Entity.Id)) {
					string mes = string.Format("Пользователь {0} уже связан с сотрудником {1}, " +
						"при привязке этого сотрудника к пользователю, старая связь будет удалена. Продолжить?",
									 Entity.User.Name,
									 string.Join(", ", associatedEmployees.Select(e => e.ShortName))
								 );
					if(MessageDialogHelper.RunQuestionDialog(mes)) {
						foreach(var ae in associatedEmployees.Where(e => e.Id != Entity.Id)) {
							ae.User = null;
							UoWGeneric.Save(ae);
						}
					} else
						return false;
				}
			}
			
			if(Entity.InnerPhone != null) {
				var associatedEmployees = UoW.Session.Query<Employee>().Where(e => e.InnerPhone == Entity.InnerPhone);
				if(associatedEmployees.Any(e => e.Id != Entity.Id && e.InnerPhone == Entity.InnerPhone)) {
					string mes = string.Format("Внутренний номер {0} уже связан с сотрудником {1}. Продолжить?",
						Entity.InnerPhone,
						string.Join(", ", associatedEmployees.Select(e => e.Name))
						);
					if(!MessageDialogHelper.RunQuestionDialog(mes)) {
						return false;
					}
				}
			}

			Entity.CreateDefaultWageParameter(_wageCalculationRepository, _baseParametersProvider, ServicesConfig.InteractiveService);

			phonesView.RemoveEmpty();
			UoWGeneric.Save(Entity);

			#region Попытка сохранить логин для нового юзера
			if(!String.IsNullOrEmpty(Entity.LoginForNewUser) && EmailServiceSetting.SendingAllowed)
			{
				if (!authorizationService.TryToSaveUser(Entity, UoWGeneric))
				{
					return false;
				}
			}
			#endregion

			_terminalManagementViewModel?.SaveChanges();

			logger.Info("Сохраняем сотрудника...");
			try {
				UoWGeneric.Save();
				if(UoWGeneric.IsNew) {
					attachmentFiles.ItemId = UoWGeneric.Root.Id;
				}
				attachmentFiles.SaveChanges();
			} catch(Exception ex) {
				logger.Error(ex, "Не удалось записать сотрудника.");
				QSMain.ErrorMessage((Gtk.Window)this.Toplevel, ex);
				return false;
			}
			logger.Info("Ok");
			return true;
		}

		private void ConfigureValidationContext()
		{
			_validationContext = new ValidationContext(Entity);

			_validationContext.ServiceContainer.AddService(typeof(ISubdivisionService), subdivisionService);
			_validationContext.ServiceContainer.AddService(typeof(IEmployeeRepository), _employeeRepository);
			_validationContext.ServiceContainer.AddService(typeof(IUserRepository), _userRepository);
		}

		protected void OnRussianCitizenToggled(object sender, EventArgs e)
		{
			if(Entity.IsRussianCitizen == false) {
				labelCitizenship.Visible = true;
				referenceCitizenship.Visible = true;
			} else {
				labelCitizenship.Visible = false;
				referenceCitizenship.Visible = false;
				Entity.Citizenship = null;
			}
		}

		#region RadioTabToggled
		protected void OnRadioTabInfoToggled(object sender, EventArgs e)
		{
			if(radioTabInfo.Active)
				notebookMain.CurrentPage = 0;
		}

		protected void OnRadioTabFilesToggled(object sender, EventArgs e)
		{
			if(radioTabFiles.Active)
				notebookMain.CurrentPage = 3;
		}

		protected void OnRadioTabAccountingToggled(object sender, EventArgs e)
		{
			if(radioTabAccounting.Active)
				notebookMain.CurrentPage = 2;
		}

		protected void OnRadioTabLogisticToggled(object sender, EventArgs e)
		{
			if(terminalManagementView.ViewModel == null)
			{
				terminalManagementView.ViewModel = _terminalManagementViewModel ??
				                                   (_terminalManagementViewModel =
					                                   new TerminalManagementViewModel(
						                                   CurrentUserSettings.Settings.DefaultWarehouse,
						                                   Entity,
						                                   this as ITdiTab,
						                                   _employeeRepository,
						                                   new WarehouseRepository(),
						                                   new RouteListRepository(new StockRepository(), _baseParametersProvider),
						                                   ServicesConfig.CommonServices,
						                                   UoW,
						                                   _baseParametersProvider));
			}

			if(radioTabLogistic.Active)
				notebookMain.CurrentPage = 1;
		}

		protected void OnRadioTabEmployeeDocumentToggled(object sender, EventArgs e)
		{
			if(radioTabEmployeeDocument.Active)
				notebookMain.CurrentPage = 5;
		}

		protected void OnRadioTabContractsToggled(object sender, EventArgs e)
		{
			if(radioTabContracts.Active)
				notebookMain.CurrentPage = 4;
		}
		#endregion

		#region Document
		protected void OnButtonAddDocumentClicked(object sender, EventArgs e)
		{
			EmployeeDocDlg dlg = new EmployeeDocDlg(UoW, Entity.IsRussianCitizen ? hiddenForRussianDocument : hiddenForForeignCitizen, ServicesConfig.CommonServices);
			dlg.Save += (object sender1, EventArgs e1) => Entity.ObservableDocuments.Add(dlg.Entity);
			TabParent.AddSlaveTab(this, dlg);
		}

		protected void OnButtonRemoveDocumentClicked(object sender, EventArgs e)
		{
			var toRemoveDocument = ytreeviewEmployeeDocument.GetSelectedObjects<EmployeeDocument>().ToList();
			toRemoveDocument.ForEach(x => Entity.ObservableDocuments.Remove(x));
		}

		protected void OnButtonEditDocumentClicked(object sender, EventArgs e)
		{
			if(ytreeviewEmployeeDocument.GetSelectedObject<EmployeeDocument>() != null) {
				EmployeeDocDlg dlg = new EmployeeDocDlg(((EmployeeDocument)ytreeviewEmployeeDocument.GetSelectedObjects()[0]).Id, UoW, ServicesConfig.CommonServices);
				TabParent.AddSlaveTab(this, dlg);
			}
		}

		protected void OnEmployeeDocumentRowActivated(object o, Gtk.RowActivatedArgs args)
		{
			buttonDocumentEdit.Click();
		}

		#endregion

		#region Contract
		protected void OnAddContractButtonCliked(object sender, EventArgs e)
		{
			List<EmployeeDocument> doc = Entity.GetMainDocuments();
			if(!doc.Any()) {
				MessageDialogHelper.RunInfoDialog("Отсутствует главный документ");
				return;
			} 
			if(Entity.Registration != RegistrationType.Contract) {
				MessageDialogHelper.RunInfoDialog("Должен быть указан тип регистрации: 'ГПК' ");
				return;
			}
			EmployeeContractDlg dlg = new EmployeeContractDlg(doc[0], Entity, UoW);
			dlg.Save += (object sender1, EventArgs e1) => Entity.ObservableContracts.Add(dlg.Entity);
			TabParent.AddSlaveTab(this, dlg);
		}

		protected void OnButtonRemoveContractClicked(object sender, EventArgs e)
		{
			var toRemoveContract = ytreeviewEmployeeContract.GetSelectedObjects<EmployeeContract>().ToList();
			toRemoveContract.ForEach(x => Entity.ObservableContracts.Remove(x));
		}

		protected void OnButtonEditContractClicked(object sender, EventArgs e)
		{
			if(ytreeviewEmployeeContract.GetSelectedObject<EmployeeContract>() != null) {
				EmployeeContractDlg dlg = new EmployeeContractDlg(((EmployeeContract)ytreeviewEmployeeContract.GetSelectedObjects()[0]).Id, UoW);
				TabParent.AddSlaveTab(this, dlg);
			}

		}

		protected void OnEmployeeContractRowActivated(object o, Gtk.RowActivatedArgs args)
		{
			buttonContractEdit.Click();
		}

		#endregion

		#region Driver & forwarder

		protected void OnComboCategoryEnumItemSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			radioTabLogistic.Visible
				= lblDriverOf.Visible
				= hboxDriversParameters.Visible
				= ((EmployeeCategory)e.SelectedItem == EmployeeCategory.driver);

			wageParametersView.Sensitive = _canEditWage || _canEditWageBySelfSubdivision;
		}

		protected void OnRadioWageParametersClicked(object sender, EventArgs e)
		{
			if(radioWageParameters.Active)
				notebookMain.CurrentPage = 6;
		}

		#endregion
	}
}

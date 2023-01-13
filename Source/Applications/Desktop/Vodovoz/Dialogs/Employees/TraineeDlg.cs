using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using NLog;
using QS.Banks.Domain;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QSOrmProject;
using QS.Validation;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.EntityRepositories.WageCalculation;
using Vodovoz.Factories;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Infrastructure.Services;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalSelectors;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Employees;
using VodovozInfrastructure.Endpoints;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using QS.Attachments.ViewModels.Widgets;
using ApiClientProvider;

namespace Vodovoz.Dialogs.Employees
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TraineeDlg : QS.Dialog.Gtk.EntityDialogBase<Trainee>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private static readonly BaseParametersProvider _baseParametersProvider = new BaseParametersProvider(new ParametersProvider());

		private readonly IAuthorizationService _authorizationService = new AuthorizationServiceFactory().CreateNewAuthorizationService();
		private readonly IEmployeeWageParametersFactory _employeeWageParametersFactory = new EmployeeWageParametersFactory();
		private readonly IEmployeeJournalFactory _employeeJournalFactory = new EmployeeJournalFactory();
		private readonly ISubdivisionJournalFactory _subdivisionJournalFactory = new SubdivisionJournalFactory();
		private readonly IEmployeePostsJournalFactory _employeePostsJournalFactory = new EmployeePostsJournalFactory();
		private readonly ICashDistributionCommonOrganisationProvider _cashDistributionCommonOrganisationProvider =
			new CashDistributionCommonOrganisationProvider(new OrganizationParametersProvider(new ParametersProvider()));
		private readonly ISubdivisionParametersProvider _subdivisionParametersProvider =
			new SubdivisionParametersProvider(new ParametersProvider());
		private readonly IWageCalculationRepository _wageCalculationRepository  = new WageCalculationRepository();
		private readonly IEmployeeRepository _employeeRepository = new EmployeeRepository();
		private readonly IValidationContextFactory _validationContextFactory = new ValidationContextFactory();
		private readonly IPhonesViewModelFactory _phonesViewModelFactory = new PhonesViewModelFactory(new PhoneRepository());
		private readonly IWarehouseRepository _warehouseRepository = new WarehouseRepository();
		private readonly IRouteListRepository _routeListRepository = new RouteListRepository(new StockRepository(), _baseParametersProvider);
		private readonly IUserRepository _userRepository = new UserRepository();
		private readonly IAttachmentsViewModelFactory _attachmentsViewModelFactory = new AttachmentsViewModelFactory();

		private AttachmentsViewModel _attachmentsViewModel;
		private bool canEdit;

		public TraineeDlg()
		{
			Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Trainee>();
			ConfigureDlg();
		}

		public TraineeDlg(int id)
		{
			Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Trainee>(id);
			ConfigureDlg();
		}

		public TraineeDlg(Trainee sub) : this(sub.Id)
		{
		}

		private void ConfigureDlg()
		{
			OnRussianCitizenToggled(null, EventArgs.Empty);
			notebookMain.Page = 0;
			notebookMain.ShowTabs = false;
			canEdit = permissionResult.CanUpdate || (permissionResult.CanCreate && Entity.Id == 0);

			CreateAttachmentsViewModel();
			ConfigureBindings();
		}

		private void ConfigureBindings()
		{
			logger.Info("Настройка биндинга компонентов диалога стажера");
			//Основные
			dataentryLastName.Binding.AddBinding(Entity, e => e.LastName, w => w.Text).InitializeFromSource();
			dataentryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			dataentryPatronymic.Binding.AddBinding(Entity, e => e.Patronymic, w => w.Text).InitializeFromSource();
			entryAddressCurrent.Binding.AddBinding(Entity, e => e.AddressCurrent, w => w.Text).InitializeFromSource();
			entryAddressRegistration.Binding.AddBinding(Entity, e => e.AddressRegistration, w => w.Text).InitializeFromSource();
			entryInn.Binding.AddBinding(Entity, e => e.INN, w => w.Text).InitializeFromSource();
			dataentryDrivingNumber.MaxLength = 20;
			dataentryDrivingNumber.Binding.AddBinding(Entity, e => e.DrivingLicense, w => w.Text).InitializeFromSource();
			referenceNationality.SubjectType = typeof(Nationality);
			referenceNationality.Binding.AddBinding(Entity, e => e.Nationality, w => w.Subject).InitializeFromSource();
			referenceCitizenship.SubjectType = typeof(Citizenship);
			referenceCitizenship.Binding.AddBinding(Entity, e => e.Citizenship, w => w.Subject).InitializeFromSource();
			photoviewEmployee.Binding.AddBinding(Entity, e => e.Photo, w => w.ImageFile).InitializeFromSource();
			photoviewEmployee.GetSaveFileName = () => Entity.FullName;
			phonesView.UoW = UoWGeneric;
			checkbuttonRussianCitizen.Binding.AddBinding(Entity, e => e.IsRussianCitizen, w => w.Active).InitializeFromSource();
			if(Entity.Phones == null) {
				Entity.Phones = new List<Vodovoz.Domain.Contacts.Phone>();
			}
			phonesView.Phones = Entity.Phones;

			ytreeviewEmployeeDocument.ColumnsConfig = FluentColumnsConfig<EmployeeDocument>.Create()
				.AddColumn("Документ").AddTextRenderer(x => x.Document.GetEnumTitle())
				.AddColumn("Доп. название").AddTextRenderer(x => x.Name)
				.Finish();
			ytreeviewEmployeeDocument.SetItemsSource(Entity.ObservableDocuments);

			//Реквизиты
			accountsView.SetAccountOwner(UoW, Entity);
			accountsView.SetTitle("Банковские счета стажера");

			//Файлы
			attachmentsView.ViewModel = _attachmentsViewModel;

			logger.Info("Ok");
		}

		private void CreateAttachmentsViewModel()
		{
			_attachmentsViewModel = _attachmentsViewModelFactory.CreateNewAttachmentsViewModel(Entity.ObservableAttachments);
		}

		public override bool HasChanges => UoWGeneric.HasChanges;

		public override bool Save()
		{
			var valid = new QSValidator<Trainee>(Entity);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel)) {
				return false;
			}
			phonesView.RemoveEmpty();
			logger.Info("Сохраняем стажера...");
			try
			{
				UoWGeneric.Save();
			}
			catch(Exception ex)
			{
				logger.Error(ex, "Не удалось записать стажера.");
				QSProjectsLib.QSMain.ErrorMessage((Gtk.Window)this.Toplevel, ex);
				return false;
			}
			logger.Info("Ok");
			return true;
		}

		protected void OnRadioTabInfoToggled(object sender, EventArgs e)
		{
			if(radioTabInfo.Active)
				notebookMain.CurrentPage = 0;
		}

		protected void OnRadioTabAccountingToggled(object sender, EventArgs e)
		{
			if(radioTabAccounting.Active)
				notebookMain.CurrentPage = 1;
		}

		protected void OnRadioTabFilesToggled(object sender, EventArgs e)
		{
			if(radioTabFiles.Active)
				notebookMain.CurrentPage = 2;
		}

		protected void OnRadioTabDocumentsToggled(object sender, EventArgs e)
		{
			if(radioTabDocuments.Active)
				notebookMain.CurrentPage = 3;
		}

		protected void OnButtonChangeToEmployeeClicked(object sender, EventArgs e)
		{
			if(UoW.HasChanges || Entity.Id == 0) {
				if(!MessageDialogHelper.RunQuestionDialog("Для продолжения необходимо сохранить изменения, сохранить и продолжить?")) {
					return;
				}
				if(Save()) {
					OnEntitySaved(true);
				} else {
					return;
				}
			}
			var employeeUow = UnitOfWorkFactory.CreateWithNewRoot<Employee>();
			Personnel.ChangeTraineeToEmployee(employeeUow, Entity);

			var cs = new ConfigurationSection(new ConfigurationRoot(new List<IConfigurationProvider> { new MemoryConfigurationProvider(new MemoryConfigurationSource()) }), "");

			cs["BaseUri"] = "https://driverapi.vod.qsolution.ru:7090/api/";

			var apiHelper = new ApiClientProvider.ApiClientProvider(cs);

			var driverApiRegisterEndpoint = new DriverApiUserRegisterEndpoint(apiHelper);

			var employeeViewModel = new EmployeeViewModel(
				_authorizationService,
				_employeeWageParametersFactory,
				_employeeJournalFactory,
				_subdivisionJournalFactory,
				_employeePostsJournalFactory,
				_cashDistributionCommonOrganisationProvider,
				_subdivisionParametersProvider,
				_wageCalculationRepository,
				_employeeRepository,
				employeeUow,
				ServicesConfig.CommonServices,
				_validationContextFactory,
				_phonesViewModelFactory,
				_warehouseRepository,
				_routeListRepository,
				driverApiRegisterEndpoint,
				CurrentUserSettings.Settings,
				_userRepository,
				_baseParametersProvider,
				_attachmentsViewModelFactory,
				MainClass.MainWin.NavigationManager,
				true);

			TabParent.OpenTab(DialogHelper.GenerateDialogHashName<Employee>(Entity.Id),
							  () => employeeViewModel);
			OnCloseTab(false);
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

		#region Document

		protected void OnButtonAddDocumentClicked(object sender, EventArgs e)
		{
			EmployeeDocDlg dlg = new EmployeeDocDlg(UoW, null, ServicesConfig.CommonServices, canEdit);
			dlg.Save += (object sender1, EventArgs e1) => Entity.ObservableDocuments.Add(dlg.Entity);
			TabParent.AddSlaveTab(this, dlg);
		}

		protected void OnButtonRemoveDocumentClicked(object sender, EventArgs e)
		{
			var toRemoveDistricts = ytreeviewEmployeeDocument.GetSelectedObjects<EmployeeDocument>().ToList();
			toRemoveDistricts.ForEach(x => Entity.ObservableDocuments.Remove(x));
		}

		protected void OnButtonEditDocumentClicked(object sender, EventArgs e)
		{
			if(ytreeviewEmployeeDocument.GetSelectedObject<EmployeeDocument>() != null)
			{
				EmployeeDocDlg dlg = new EmployeeDocDlg(
					((EmployeeDocument)ytreeviewEmployeeDocument.GetSelectedObjects()[0]).Id, UoW, ServicesConfig.CommonServices, canEdit);
				TabParent.AddSlaveTab(this, dlg);
			}

		}

		protected void OnEmployeeDocumentRowActivated(object o, Gtk.RowActivatedArgs args)
		{
			buttonDocumentEdit.Click();
		}
		#endregion

		public override void Destroy()
		{
			attachmentsView.Destroy();
			base.Destroy();
		}
	}
}

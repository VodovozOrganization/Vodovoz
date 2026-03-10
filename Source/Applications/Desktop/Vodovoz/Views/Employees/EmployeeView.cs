using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gamma.Widgets;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services;
using QS.Views.GtkUI;
using QSOrmProject;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gtk;
using QS.ViewModels.Control.EEVM;
using Vodovoz.Dialogs.Employees;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.Core.Domain.Employees;
using QSWidgetLib;
using Vodovoz.JournalViewModels;

namespace Vodovoz.Views.Employees
{
	public partial class EmployeeView : TabViewBase<EmployeeViewModel>, IEntityDialog
	{
		public EmployeeView(EmployeeViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureDlg();
		}

		public IUnitOfWork UoW => ViewModel.UoW;
		public object EntityObject => ViewModel.UoWGeneric.RootObject;

		private void ConfigureDlg()
		{
			notebookMain.Page = 0;
			notebookMain.ShowTabs = false;

			buttonSave.Clicked += (sender, args) => ViewModel.SaveAndClose();
			buttonSave.Sensitive = ViewModel.CanEditEmployee || ViewModel.CanChangeEmployeeCounterparty;

			buttonCancel.Clicked += (sender, args) => ViewModel.Close(false, CloseSource.Cancel);

			ConfigureRadioButtons();

			#region Вкладка Информация

			yenumcomboStatus.ItemsEnum = typeof(EmployeeStatus);
			yenumcomboStatus.Binding
				.AddBinding(ViewModel.Entity, e => e.Status, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.CanEditEmployee, w => w.Sensitive)
				.InitializeFromSource();

			dataentryLastName.Binding
				.AddBinding(ViewModel.Entity, e => e.LastName, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEditEmployee, w => w.Sensitive)
				.InitializeFromSource();

			dataentryName.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEditEmployee, w => w.Sensitive)
				.InitializeFromSource();

			dataentryPatronymic.Binding
				.AddBinding(ViewModel.Entity, e => e.Patronymic, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEditEmployee, w => w.Sensitive)
				.InitializeFromSource();

			photoviewEmployee.Binding
				.AddBinding(ViewModel.Entity, e => e.Photo, w => w.ImageFile)
				.AddBinding(ViewModel, vm => vm.CanEditEmployee, w => w.Sensitive)
				.InitializeFromSource();

			photoviewEmployee.GetSaveFileName = () => ViewModel.Entity.FullName;

			entryEmployeePost.Binding
				.AddBinding(ViewModel.Entity, e => e.Post, w => w.Subject)
				.AddBinding(ViewModel, vm => vm.CanEditEmployee, w => w.Sensitive)
				.InitializeFromSource();
			entryEmployeePost.SetEntityAutocompleteSelectorFactory(
				ViewModel.EmployeePostsJournalFactory.CreateEmployeePostsAutocompleteSelectorFactory());

			comboSkillLevel.ItemsList = ViewModel.Entity.GetSkillLevels();
			comboSkillLevel.Binding
				.AddBinding(
					ViewModel.Entity,
					e => e.SkillLevel,
					w => w.ActiveText,
					new Gamma.Binding.Converters.NumbersToStringConverter()
				)
				.AddBinding(ViewModel, vm => vm.CanEditEmployee, w => w.Sensitive)
				.InitializeFromSource();
			comboSkillLevel.SelectedItem = ViewModel.Entity.SkillLevel;

			ConfigureCategory();

			comboDriverOfCarTypeOfUse.ShowSpecialStateNot = true;
			comboDriverOfCarTypeOfUse.ItemsEnum = typeof(CarTypeOfUse);
			comboDriverOfCarTypeOfUse.AddEnumToHideList(CarTypeOfUse.Loader);
			comboDriverOfCarTypeOfUse.Binding
				.AddBinding(ViewModel.Entity, e => e.DriverOfCarTypeOfUse, w => w.SelectedItemOrNull)
				.AddBinding(ViewModel, vm => vm.CanEditEmployee, w => w.Sensitive)
				.InitializeFromSource();
			
			comboDriverOfCarOwnType.ShowSpecialStateNot = true;
			comboDriverOfCarOwnType.ItemsEnum = typeof(CarOwnType);
			comboDriverOfCarOwnType.Binding
				.AddBinding(ViewModel.Entity, e => e.DriverOfCarOwnType, w => w.SelectedItemOrNull)
				.AddBinding(ViewModel, vm => vm.CanEditEmployee, w => w.Sensitive)
				.InitializeFromSource();

			checkVisitingMaster.Binding
				.AddBinding(ViewModel.Entity, e => e.VisitingMaster, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanEditEmployee, w => w.Sensitive)
				.InitializeFromSource();
			chkDriverForOneDay.Binding
				.AddBinding(ViewModel.Entity, e => e.IsDriverForOneDay, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanEditEmployee, w => w.Sensitive)
				.InitializeFromSource();
			checkChainStoreDriver.Binding
				.AddBinding(ViewModel.Entity, e => e.IsChainStoreDriver, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanEditEmployee, w => w.Sensitive)
				.InitializeFromSource();

			referenceNationality.SubjectType = typeof(Nationality);
			referenceNationality.Binding
				.AddBinding(ViewModel.Entity, e => e.Nationality, w => w.Subject)
				.AddBinding(ViewModel, vm => vm.CanEditEmployee, w => w.Sensitive)
				.InitializeFromSource();

			GenderComboBox.ItemsEnum = typeof(Gender);
			GenderComboBox.Binding
				.AddBinding(ViewModel.Entity, e => e.Gender, w => w.SelectedItemOrNull)
				.AddBinding(ViewModel, vm => vm.CanEditEmployee, w => w.Sensitive)
				.InitializeFromSource();

			entrySubdivision.ViewModel = ViewModel.SubdivisionViewModel;

			var usersJournalFactory = new UserJournalFactory();
			entityviewmodelUser.SetEntityAutocompleteSelectorFactory(usersJournalFactory.CreateSelectUserAutocompleteSelectorFactory());
			entityviewmodelUser.Binding.AddBinding(ViewModel.Entity, e => e.User, w => w.Subject).InitializeFromSource();
			entityviewmodelUser.Sensitive = ViewModel.CanManageUsers && ViewModel.CanEditEmployee;

			ylblUserLogin.TooltipText =
				"При сохранении сотрудника создаёт нового пользователя с введённым логином " +
				"и отправляет сотруднику SMS с сгенерированным паролем";
			yentryUserLogin.Binding.AddBinding(ViewModel.Entity, e => e.LoginForNewUser, w => w.Text);
			yentryUserLogin.Sensitive = ViewModel.CanCreateNewUser && ViewModel.CanEditEmployee;

			birthdatePicker.Binding
				.AddBinding(ViewModel.Entity, e => e.BirthdayDate, w => w.DateOrNull)
				.AddBinding(ViewModel, vm => vm.CanEditEmployee, w => w.Sensitive)
				.InitializeFromSource();

			dataentryInnerPhone.Binding
				.AddBinding(
					ViewModel.Entity,
					e => e.InnerPhone,
					w => w.Text,
					new Gamma.Binding.Converters.NumbersToStringConverter()
				)
				.AddBinding(ViewModel, vm => vm.CanEditEmployee, w => w.Sensitive)
				.InitializeFromSource();

			checkbuttonRussianCitizen.Binding
				.AddBinding(ViewModel.Entity, e => e.IsRussianCitizen, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanEditEmployee, w => w.Sensitive)
				.InitializeFromSource();
			OnRussianCitizenToggled(null, EventArgs.Empty);

			referenceCitizenship.SubjectType = typeof(Citizenship);
			referenceCitizenship.Binding
				.AddBinding(ViewModel.Entity, e => e.Citizenship, w => w.Subject)
				.AddBinding(ViewModel, vm => vm.CanEditEmployee, w => w.Sensitive)
				.InitializeFromSource();

			textComment.Binding
				.AddBinding(ViewModel.Entity, e => e.Comment, w => w.Buffer.Text)
				.AddBinding(ViewModel, vm => vm.CanEditEmployee, w => w.Editable)
				.InitializeFromSource();

			dataentryDrivingNumber.MaxLength = 20;
			dataentryDrivingNumber.Binding
				.AddBinding(ViewModel.Entity, e => e.DrivingLicense, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEditEmployee, w => w.Sensitive)
				.InitializeFromSource();

			phonesView.ViewModel = ViewModel.PhonesViewModel;

			entryAddressCurrent.Binding
				.AddBinding(ViewModel.Entity, e => e.AddressCurrent, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEditEmployee, w => w.Sensitive)
				.InitializeFromSource();
			entryAddressRegistration.Binding
				.AddBinding(ViewModel.Entity, e => e.AddressRegistration, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEditEmployee, w => w.Sensitive)
				.InitializeFromSource();

			yentryEmailAddress.ValidationMode = ValidationType.email;
			yentryEmailAddress.Binding
				.AddBinding(ViewModel.Entity, e => e.Email, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEditEmployee, w => w.Sensitive)
				.InitializeFromSource();
			yentryEmailAddress.FocusOutEvent += OnEmailFocusOutEvent;

			ydateFirstWorkDay.Binding
				.AddBinding(ViewModel.Entity, e => e.FirstWorkDay, w => w.DateOrNull)
				.AddBinding(ViewModel, vm => vm.CanEditEmployee, w => w.Sensitive)
				.InitializeFromSource();
			dateFired.Binding
				.AddBinding(ViewModel.Entity, e => e.DateFired, w => w.DateOrNull)
				.AddBinding(ViewModel, vm => vm.CanEditEmployee, w => w.Sensitive)
				.InitializeFromSource();
			dateHired.Binding
				.AddBinding(ViewModel.Entity, e => e.DateHired, w => w.DateOrNull)
				.AddBinding(ViewModel, vm => vm.CanEditEmployee, w => w.Sensitive)
				.InitializeFromSource();
			dateCalculated.Binding
				.AddBinding(ViewModel.Entity, e => e.DateCalculated, w => w.DateOrNull)
				.AddBinding(ViewModel, vm => vm.CanEditEmployee, w => w.Sensitive)
				.InitializeFromSource();
			
			chkHasAccessToWarehouseApp.Binding
				.AddBinding(ViewModel.Entity, e => e.HasAccessToWarehouseApp, w => w.Active)
				.InitializeFromSource();
			chkHasAccessToWarehouseApp.Toggled += OnHasAccessToWarehouseAppToggled;
			
			tableWarehouseApiCredentials.Binding
				.AddBinding(ViewModel.Entity, e => e.HasAccessToWarehouseApp, w => w.Visible)
				.InitializeFromSource();
			
			entryWarehouseAppLogin.Binding
				.AddBinding(ViewModel.WarehouseAppUser, u => u.Login, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanRegisterWarehouseAppUser, w => w.Sensitive)
				.InitializeFromSource();
			
			entryWarehouseAppPassword.Binding
				.AddBinding(ViewModel.WarehouseAppUser, u => u.Password, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanRegisterWarehouseAppUser, w => w.Sensitive)
				.InitializeFromSource();
			
			btnRegisterWarehouseAppUser.Binding
				.AddBinding(ViewModel.Entity, e => e.HasAccessToWarehouseApp, w => w.Visible)
				.AddBinding(ViewModel, vm => vm.IsValidNewWarehouseAppUser, w => w.Sensitive)
				.InitializeFromSource();

			btnRegisterWarehouseAppUser.Clicked += (sender, args) => ViewModel.RegisterWarehouseAppUserCommand.Execute();

			entryVodovozClient.ViewModel = GetEntryVodovozClientViewModel();

			#endregion

			#region Вкладка Логистика

			dataentryAndroidLogin.Binding
				.AddBinding(ViewModel.DriverAppUser, u => u.Login, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanRegisterDriverAppUser, w => w.Sensitive)
				.InitializeFromSource();

			dataentryAndroidPassword.Binding
				.AddBinding(ViewModel.DriverAppUser, u => u.Password, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanRegisterDriverAppUser, w => w.Sensitive)
				.InitializeFromSource();

			yMobileLoginInfo.Binding
				.AddBinding(ViewModel, vm => vm.AddDriverAppLoginInfo, w => w.LabelProp)
				.InitializeFromSource();

			btnCopyWarehouseAppUserCredentials.Binding
				.AddBinding(ViewModel, vm => vm.CanCopyWarehouseAppUserCredentialsToDriverUser, w => w.Sensitive)
				.InitializeFromSource();
			btnCopyWarehouseAppUserCredentials.Clicked +=
				(sender, args) => ViewModel.CopyWarehouseAppUserCredentialsToDriverAppUserCommand.Execute(); 
			
			btnRegisterDriverAppUser.Binding
				.AddBinding(ViewModel, vm => vm.IsValidNewDriverAppUser, w => w.Sensitive)
				.InitializeFromSource();

			defaultForwarderEntry.SetEntityAutocompleteSelectorFactory(
				ViewModel.EmployeeJournalFactory.CreateWorkingForwarderEmployeeAutocompleteSelectorFactory());
			defaultForwarderEntry.Binding
				.AddBinding(ViewModel.Entity, e => e.DefaultForwarder, w => w.Subject)
				.AddBinding(ViewModel, vm => vm.CanEditEmployee, w => w.Sensitive)
				.InitializeFromSource();

			entryDistrictOfResidence.ViewModel = ViewModel.DistictsSetViewModel;

			yspinTripsPriority.Binding
				.AddBinding(ViewModel.Entity, e => e.TripPriority, w => w.ValueAsShort)
				.AddBinding(ViewModel, vm => vm.CanEditEmployee, w => w.Sensitive)
				.InitializeFromSource();
			yspinDriverSpeed.Binding
				.AddBinding(ViewModel.Entity, e => e.DriverSpeed, w => w.Value, new MultiplierToPercentConverter())
				.AddBinding(ViewModel, vm => vm.CanEditEmployee, w => w.Sensitive)
				.InitializeFromSource();

			minAddressesSpin.Binding
				.AddBinding(ViewModel.Entity, e => e.MinRouteAddresses, w => w.ValueAsInt)
				.AddBinding(ViewModel, vm => vm.CanEditEmployee, w => w.Sensitive)
				.InitializeFromSource();
			maxAddressesSpin.Binding
				.AddBinding(ViewModel.Entity, e => e.MaxRouteAddresses, w => w.ValueAsInt)
				.AddBinding(ViewModel, vm => vm.CanEditEmployee, w => w.Sensitive)
				.InitializeFromSource();

			comboDriverType.ItemsEnum = typeof(DriverType);
			comboDriverType.Binding
				.AddBinding(ViewModel.Entity, e => e.DriverType, w => w.SelectedItemOrNull)
				.AddBinding(ViewModel, vm => vm.CanEditEmployee, w => w.Sensitive)
				.InitializeFromSource();

			ycheckbuttonCarRecieveCounterpartyCalls.Binding
				.AddBinding(ViewModel.Entity, e => e.CanRecieveCounterpartyCalls, w => w.Active)
				.InitializeFromSource();

			entityentryPhoneForCounterpartyCalls.ViewModel = ViewModel.PhoneForCounterpartyCallsViewModel;
			entityentryPhoneForCounterpartyCalls.Binding
				.AddBinding(ViewModel.Entity, e => e.CanRecieveCounterpartyCalls, w => w.ViewModel.IsEditable)
				.InitializeFromSource();

			ConfigureWorkSchedules();
			ConfigureDistrictPriorities();

			#endregion

			#region Вкладка Реквизиты

			entryInn.Binding
				.AddBinding(ViewModel.Entity, e => e.INN, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEditEmployee, w => w.Sensitive)
				.InitializeFromSource();

			accountsView.SetAccountOwner(UoW, ViewModel.Entity);
			accountsView.SetTitle("Банковские счета сотрудника");
			accountsView.Sensitive = ViewModel.CanEditEmployee;

			treeRegistrationVersions.ColumnsConfig = FluentColumnsConfig<EmployeeRegistrationVersion>.Create()
				.AddColumn("Код")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.Id == 0 ? "Новая" : x.Id.ToString())
					.XAlign(0.5f)
				.AddColumn("Вид оформления")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.EmployeeRegistration.ToString())
					.XAlign(0.5f)
				.AddColumn("Начало действия")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.StartDate.ToString("g"))
					.XAlign(0.5f)
				.AddColumn("Окончание действия")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.EndDate.HasValue ? x.EndDate.Value.ToString("g") : "")
					.XAlign(0.5f)
				.AddColumn("")
				.Finish();

			treeRegistrationVersions.ItemsDataSource = ViewModel.Entity.ObservableEmployeeRegistrationVersions;
			treeRegistrationVersions.Binding
				.AddBinding(ViewModel, vm => vm.SelectedRegistrationVersion, w => w.SelectedRow)
				.InitializeFromSource();

			pickerVersionStartDate.IsEditable = ViewModel.CanEditEmployee;
			pickerVersionStartDate.Binding
				.AddBinding(ViewModel, vm => vm.SelectedRegistrationDate, w => w.DateOrNull)
				.InitializeFromSource();
			
			btnNewRegistrationVersion.Clicked += (sender, args) => ViewModel.CreateNewEmployeeRegistrationVersionCommand.Execute();
			btnNewRegistrationVersion.Binding
				.AddBinding(ViewModel, vm => vm.CanAddNewRegistrationVersion, w => w.Sensitive)
				.InitializeFromSource();
			
			btnChangeVersionStartDate.Clicked += (sender, args) => ViewModel.ChangeEmployeeRegistrationVersionStartDateCommand.Execute();
			btnChangeVersionStartDate.Binding
				.AddBinding(ViewModel, vm => vm.CanChangeRegistrationVersionDate, w => w.Sensitive)
				.InitializeFromSource();

			#endregion

			#region Вкладка Файлы

			attachedfileinformationsview.ViewModel = ViewModel.AttachedFileInformationsViewModel;
			attachedfileinformationsview.Sensitive = ViewModel.CanEditEmployee;

			#endregion

			#region Вкладка Документы

			btnAddDocument.Clicked += OnButtonAddDocumentClicked;
			btnEditDocument.Clicked += OnButtonEditDocumentClicked;
			btnRemoveDocument.Clicked += (s, e) => ViewModel.RemoveEmployeeDocumentsCommand.Execute();

			ConfigureTreeEmployeeDocuments();

			UpdateDocumentsTab();

			#endregion

			#region Вкладка Договора

			ConfigureContractsTabButtons();
			ConfigureTreeEmployeeContracts();

			#endregion

			#region Вкладка Зарплата

			specialListCmbOrganisation.ItemsList = ViewModel.organizations;
			specialListCmbOrganisation.Binding
				.AddBinding(ViewModel.Entity, e => e.OrganisationForSalary, w => w.SelectedItem)
				.InitializeFromSource();
			specialListCmbOrganisation.Sensitive = ViewModel.CanEditOrganisationForSalary && ViewModel.CanEditEmployee;

			wageParametersView.ViewModel =
				ViewModel.EmployeeWageParametersFactory.CreateEmployeeWageParametersViewModel(ViewModel.Entity, ViewModel, ViewModel.UoW);

			#endregion

			btnCopyEntityId.Binding
				.AddBinding(ViewModel, vm => vm.CanCopyId, w => w.Sensitive)
				.InitializeFromSource();

			btnCopyEntityId.Clicked += OnBtnCopyEntityIdClicked;

			ViewModel.PropertyChanged += ViewModel_PropertyChanged;
		}

		private IEntityEntryViewModel GetEntryVodovozClientViewModel()
		{
			var builder = 
				new LegacyEEVMBuilderFactory<Employee>(ViewModel, ViewModel.Entity, UoW, ViewModel.NavigationManager, ViewModel.LifetimeScope);
			
			var viewModel = builder.ForProperty(x => x.Counterparty)
				.UseTdiEntityDialog()
				.UseViewModelJournalAndAutocompleter<CounterpartyJournalViewModel>()
				.Finish();

			viewModel.IsEditable = ViewModel.CanEditEmployee || ViewModel.CanChangeEmployeeCounterparty;

			return viewModel;
		}

		private void OnHasAccessToWarehouseAppToggled(object sender, EventArgs e)
		{
			if(chkHasAccessToWarehouseApp.Active)
			{
				if(ViewModel.Entity.Category == EmployeeCategory.driver && ViewModel.Entity.DriverAppUser != null)
				{
					ViewModel.CopyCredentialsToOtherUser();

					ViewModel.CommonServices.InteractiveService.ShowMessage(
						ImportanceLevel.Info,
						"Пользователь уже зарегистрирован в водительском приложении, " +
						"производим регистрацию в складском приложении под теми же учетными данными");

					ViewModel.AddRoleToWarehouseAppUserCommand.Execute();
				}
				else if(ViewModel.Entity.WarehouseAppUser != null)
				{
					ViewModel.CommonServices.InteractiveService.ShowMessage(
						ImportanceLevel.Info,
						"Учетная запись есть в складском приложении, но отключена, восстанавливаем");
					
					ViewModel.AddRoleToWarehouseAppUserCommand.Execute();
				}
			}
			else
			{
				if(ViewModel.Entity.WarehouseAppUser == null
					|| string.IsNullOrWhiteSpace(ViewModel.Entity.WarehouseAppUser.Login)
					|| string.IsNullOrWhiteSpace(ViewModel.Entity.WarehouseAppUser.Password))
				{
					return;
				}

				if(ViewModel.CommonServices.InteractiveService.Question(
					"Пользователь уже зарегистрирован, это действие приведет к отключению доступа к складскому приложению. Продолжаем?"))
				{
					ViewModel.RemoveRoleFromWarehouseAppUserCommand.Execute();
				}
			}
		}

		private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(ViewModel.CanReadEmployeeDocuments):
					UpdateDocumentsTab();
					break;
				default:
					break;
			}
		}

		private void ConfigureRadioButtons()
		{
			radioTabInfo.Clicked += OnRadioTabInfoToggled;
			radioTabLogistic.Clicked += OnRadioTabLogisticToggled;
			radioTabContracts.Clicked += OnRadioTabContractsToggled;
			radioTabAccounting.Clicked += OnRadioTabAccountingToggled;
			radioTabFiles.Clicked += OnRadioTabFilesToggled;
			radioWageParameters.Clicked += OnRadioWageParametersClicked;
			radioTabEmployeeDocument.Clicked += OnRadioTabEmployeeDocumentToggled;
		}

		#region Вкладка Документы

		private void UpdateDocumentsTab()
		{
			var haveAccessToDocuments = ViewModel.CanReadEmployeeDocuments && ViewModel.CanReadEmployee;
			radioTabEmployeeDocument.Sensitive = haveAccessToDocuments;
			if(haveAccessToDocuments)
			{
				ConfigureDocumentsTabButtons();
			}
		}

		private void ConfigureDocumentsTabButtons()
		{
			btnAddDocument.Sensitive = ViewModel.CanAddEmployeeDocument && ViewModel.CanEditEmployee;
			btnEditDocument.Binding
				.AddBinding(ViewModel, vm => vm.CanReadEmployeeDocument, w => w.Sensitive).InitializeFromSource();
			btnRemoveDocument.Binding
				.AddBinding(ViewModel, vm => vm.CanRemoveEmployeeDocument, w => w.Sensitive).InitializeFromSource();
		}

		private void ConfigureTreeEmployeeDocuments()
		{
			ytreeviewEmployeeDocument.ColumnsConfig = FluentColumnsConfig<EmployeeDocument>.Create()
				.AddColumn("Документ").AddTextRenderer(x => x.Document.GetEnumTitle())
				.AddColumn("Доп. название").AddTextRenderer(x => x.Name)
				.Finish();

			ytreeviewEmployeeDocument.SetItemsSource(ViewModel.Entity.ObservableDocuments);
			ytreeviewEmployeeDocument.Selection.Changed += TreeEmployeeDocumentsSelectionOnChanged;
			ytreeviewEmployeeDocument.RowActivated += OnEmployeeDocumentRowActivated;
		}

		private void TreeEmployeeDocumentsSelectionOnChanged(object sender, EventArgs e)
		{
			ViewModel.SelectedEmployeeDocuments = ytreeviewEmployeeDocument.GetSelectedObjects<EmployeeDocument>();
		}

		private void OnButtonAddDocumentClicked(object sender, EventArgs e)
		{
			var dlg = new EmployeeDocDlg(
				ViewModel.UoW,
				ViewModel.Entity.IsRussianCitizen ? ViewModel.HiddenForRussianDocument : ViewModel.HiddenForForeignCitizen,
				ServicesConfig.CommonServices, ViewModel.CanEditEmployee);
			dlg.Save += (s, args) => ViewModel.Entity.ObservableDocuments.Add(dlg.Entity);
			ViewModel.TabParent.AddSlaveTab(ViewModel, dlg);
		}

		private void OnButtonEditDocumentClicked(object sender, EventArgs e)
		{
			var dlg = new EmployeeDocDlg(
				ViewModel.SelectedEmployeeDocuments.ElementAt(0).Id, ViewModel.UoW, ServicesConfig.CommonServices, ViewModel.CanEditEmployee);
			ViewModel.TabParent.AddSlaveTab(ViewModel, dlg);
		}

		private void OnEmployeeDocumentRowActivated(object o, Gtk.RowActivatedArgs args)
		{
			if(ViewModel.CanReadEmployeeDocument)
			{
				btnEditDocument.Click();
			}
		}

		#endregion

		#region Вкладка Договора

		private void ConfigureContractsTabButtons()
		{
			btnAddContract.Clicked += OnAddContractButtonClicked;
			btnEditContract.Clicked += OnButtonEditContractClicked;
			btnRemoveContract.Clicked += (s, e) => ViewModel.RemoveEmployeeContractsCommand.Execute();

			btnEditContract.Binding
				.AddBinding(ViewModel, vm => vm.CanEditEmployeeContract, w => w.Sensitive).InitializeFromSource();
			btnRemoveContract.Binding
				.AddBinding(ViewModel, vm => vm.CanRemoveEmployeeContract, w => w.Sensitive).InitializeFromSource();

			btnAddContract.Sensitive = ViewModel.CanEditEmployee;
		}

		private void ConfigureTreeEmployeeContracts()
		{
			ytreeviewEmployeeContract.ColumnsConfig = FluentColumnsConfig<EmployeeContract>.Create()
				.AddColumn("Договор").AddTextRenderer(x => x.EmployeeContractTemplate != null ? x.EmployeeContractTemplate.TemplateType.GetEnumTitle() : "")
				.AddColumn("Название").AddTextRenderer(x => x.Name)
				.AddColumn("Дата начала").AddTextRenderer(x => x.FirstDay.ToString("dd/MM/yyyy"))
				.AddColumn("Дата конца").AddTextRenderer(x => x.LastDay.ToString("dd/MM/yyyy"))
				.Finish();

			ytreeviewEmployeeContract.SetItemsSource(ViewModel.Entity.ObservableContracts);
			ytreeviewEmployeeContract.Selection.Changed += TreeEmployeeContractsSelectionOnChanged;
			ytreeviewEmployeeContract.RowActivated += OnEmployeeContractRowActivated;
			ytreeviewEmployeeContract.Binding.AddBinding(ViewModel, vm => vm.CanEditEmployee, w => w.Sensitive).InitializeFromSource();
		}

		private void TreeEmployeeContractsSelectionOnChanged(object sender, EventArgs e)
		{
			ViewModel.SelectedEmployeeContracts = ytreeviewEmployeeContract.GetSelectedObjects<EmployeeContract>();
		}

		private void OnAddContractButtonClicked(object sender, EventArgs e)
		{
			List<EmployeeDocument> doc = ViewModel.Entity.GetMainDocuments();

			if(!doc.Any())
			{
				ViewModel.CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, "Отсутствует главный документ");
				return;
			}

			var activeRegistration = ViewModel.Entity.EmployeeRegistrationVersions.SingleOrDefault(x => x.EndDate == null);
			if(activeRegistration == null || activeRegistration.EmployeeRegistration.RegistrationType != RegistrationType.Contract)
			{
				ViewModel.CommonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Warning,
					"Должна быть активная версия с видом регистрации: 'ГПХ'(вкладка Реквизиты)");
				return;
			}

			var dlg = new EmployeeContractDlg(doc[0], ViewModel.Entity, ViewModel.UoW);
			dlg.Save += (s, args) => ViewModel.Entity.ObservableContracts.Add(dlg.Entity);
			ViewModel.TabParent.AddSlaveTab(ViewModel, dlg);
		}

		private void OnButtonEditContractClicked(object sender, EventArgs e)
		{
			var dlg = new EmployeeContractDlg(ViewModel.SelectedEmployeeDocuments.ElementAt(0).Id, ViewModel.UoW);
			ViewModel.TabParent.AddSlaveTab(ViewModel, dlg);
		}

		private void OnEmployeeContractRowActivated(object o, Gtk.RowActivatedArgs args)
		{
			if(ViewModel.CanEditEmployeeContract)
			{
				btnEditContract.Click();
			}
		}

		#endregion

		#region Вкладка Логистика

		#region DriverDistrictPriorities

		private void ConfigureDistrictPriorities()
		{
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

			ytreeDistrictPrioritySets.RowActivated += (o, args) =>
			{
				if(ViewModel.CanEditDistrictPrioritySet)
				{
					ViewModel.OpenDistrictPrioritySetEditWindowCommand.Execute();
				}
			};
			ytreeDistrictPrioritySets.ItemsDataSource = ViewModel.Entity.ObservableDriverDistrictPrioritySets;
			ytreeDistrictPrioritySets.Selection.Changed += SelectionDistrictPrioritySetsOnChanged;
			ytreeDistrictPrioritySets.Sensitive = ViewModel.CanEditEmployee;

			ybuttonCopyDistrictPrioritySet.Clicked += (sender, args) => ViewModel.CopyDistrictPrioritySetCommand.Execute();
			ybuttonCopyDistrictPrioritySet.Binding
				.AddBinding(ViewModel, vm => vm.CanCopyDistrictPrioritySet, w => w.Sensitive).InitializeFromSource();

			ybuttonEditDistrictPrioritySet.Clicked += (sender, args) => ViewModel.OpenDistrictPrioritySetEditWindowCommand.Execute();
			ybuttonEditDistrictPrioritySet.Binding
				.AddBinding(ViewModel, vm => vm.CanEditDistrictPrioritySet, w => w.Sensitive).InitializeFromSource();

			ybuttonActivateDistrictPrioritySet.Clicked += (sender, args) => ViewModel.ActivateDistrictPrioritySetCommand.Execute();
			ybuttonActivateDistrictPrioritySet.Binding
				.AddBinding(ViewModel, x => x.CanActivateDistrictPrioritySet, w => w.Sensitive).InitializeFromSource();

			ybuttonCreateDistrictPrioritySet.Clicked += (sender, args) => ViewModel.OpenDistrictPrioritySetCreateWindowCommand.Execute();
			ybuttonCreateDistrictPrioritySet.Sensitive = ViewModel.DriverDistrictPrioritySetPermission.CanCreate && ViewModel.CanEditEmployee;
		}

		private void SelectionDistrictPrioritySetsOnChanged(object sender, EventArgs e)
		{
			ViewModel.SelectedDistrictPrioritySet = ytreeDistrictPrioritySets.GetSelectedObject<DriverDistrictPrioritySet>();
		}

		#endregion

		#region DriverWorkSchedules

		private void ConfigureWorkSchedules()
		{
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

			ytreeDriverScheduleSets.RowActivated += (o, args) =>
			{
				if(ViewModel.CanEditDriverScheduleSet)
				{
					ViewModel.OpenDriverWorkScheduleSetEditWindowCommand.Execute();
				}
			};
			ytreeDriverScheduleSets.ItemsDataSource = ViewModel.Entity.ObservableDriverWorkScheduleSets;
			ytreeDriverScheduleSets.Selection.Changed += SelectionDriverScheduleSetOnChanged;
			ytreeDriverScheduleSets.Binding.AddBinding(ViewModel, vm => vm.CanEditEmployee, w => w.Sensitive).InitializeFromSource();

			ybuttonCopyScheduleSet.Clicked += (sender, args) => ViewModel.CopyDriverWorkScheduleSetCommand.Execute();
			ybuttonCopyScheduleSet.Binding.AddBinding(ViewModel, vm => vm.CanCopyDriverScheduleSet, w => w.Sensitive).InitializeFromSource();

			ybuttonEditScheduleSet.Clicked += (sender, args) => ViewModel.OpenDriverWorkScheduleSetEditWindowCommand.Execute();
			ybuttonEditScheduleSet.Binding.
				AddBinding(ViewModel, vm => vm.CanEditDriverScheduleSet, w => w.Sensitive).InitializeFromSource();

			ybuttonCreateScheduleSet.Clicked += (sender, args) => ViewModel.OpenDriverWorkScheduleSetCreateWindowCommand.Execute();
			ybuttonCreateScheduleSet.Sensitive = ViewModel.DriverWorkScheduleSetPermission.CanCreate && ViewModel.CanEditEmployee;
		}

		private void SelectionDriverScheduleSetOnChanged(object sender, EventArgs e)
		{
			ViewModel.SelectedDriverScheduleSet = ytreeDriverScheduleSets.GetSelectedObject<DriverWorkScheduleSet>();
		}

		#endregion

		#endregion

		private void ConfigureCategory()
		{
			comboCategory.EnumItemSelected += OnComboCategoryEnumItemSelected;
			comboCategory.ItemsEnum = typeof(EmployeeCategory);
			comboCategory.Binding.AddSource(ViewModel.Entity)
				.AddBinding(e => e.Category, w => w.SelectedItem)
				.InitializeFromSource();
			comboCategory.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanEditEmployeeCategory, w => w.Sensitive)
				.InitializeFromSource();

			if(ViewModel.HiddenCategories != null && ViewModel.HiddenCategories.Any())
			{
				comboCategory.AddEnumToHideList(ViewModel.HiddenCategories.OfType<object>().ToArray());
			}

			comboCategory.ChangedByUser += (sender, e) =>
			{
				if(ViewModel.Entity.Category != EmployeeCategory.driver)
				{
					comboDriverOfCarOwnType.SelectedItemOrNull = null;
					comboDriverOfCarTypeOfUse.SelectedItemOrNull = null;
				}
			};
		}
		
		private void OnEmailFocusOutEvent(object o, FocusOutEventArgs args)
		{
			if(string.IsNullOrWhiteSpace(yentryEmailAddress.Text))
			{
				return;
			}

			yentryEmailAddress.Text = yentryEmailAddress.Text.TrimEnd('\r', '\n');
		}

		private void OnRussianCitizenToggled(object sender, EventArgs e)
		{
			if(ViewModel.Entity.IsRussianCitizen == false)
			{
				labelCitizenship.Visible = true;
				referenceCitizenship.Visible = true;
			}
			else
			{
				labelCitizenship.Visible = false;
				referenceCitizenship.Visible = false;
				ViewModel.Entity.Citizenship = null;
			}
		}

		#region RadioTabToggled

		private void OnRadioTabInfoToggled(object sender, EventArgs e)
		{
			if(radioTabInfo.Active)
			{
				notebookMain.CurrentPage = 0;
			}
		}

		private void OnRadioTabFilesToggled(object sender, EventArgs e)
		{
			if(radioTabFiles.Active)
			{
				notebookMain.CurrentPage = 3;
			}
		}

		private void OnRadioTabAccountingToggled(object sender, EventArgs e)
		{
			if(radioTabAccounting.Active)
			{
				notebookMain.CurrentPage = 2;
			}
		}

		private void OnRadioTabLogisticToggled(object sender, EventArgs e)
		{
			if(terminalmanagementview1.ViewModel == null)
			{
				terminalmanagementview1.ViewModel = ViewModel.TerminalManagementViewModel;
			}

			terminalmanagementview1.Sensitive = ViewModel.CanEditEmployee;

			if(radioTabLogistic.Active)
			{
				notebookMain.CurrentPage = 1;
			}
		}

		private void OnRadioTabEmployeeDocumentToggled(object sender, EventArgs e)
		{
			if(radioTabEmployeeDocument.Active)
			{
				notebookMain.CurrentPage = 5;
			}
		}

		private void OnRadioTabContractsToggled(object sender, EventArgs e)
		{
			if(radioTabContracts.Active)
			{
				notebookMain.CurrentPage = 4;
			}
		}

		#endregion

		#region Driver & forwarder

		private void OnComboCategoryEnumItemSelected(object sender, ItemSelectedEventArgs e)
		{
			radioTabLogistic.Visible
				= lblDriverOf.Visible
				= hboxDriverCheckParameters.Visible
				= (EmployeeCategory)e.SelectedItem == EmployeeCategory.driver;

			wageParametersView.Sensitive = ViewModel.CanEditWage && ViewModel.CanEditEmployee;
		}

		private void OnRadioWageParametersClicked(object sender, EventArgs e)
		{
			if(radioWageParameters.Active)
			{
				notebookMain.CurrentPage = 6;
			}
		}

		#endregion

		protected void OnBtnRegisterDriverAppUserClicked(object sender, EventArgs e)
		{
			ViewModel.RegisterDriverAppUserOrAddRoleCommand.Execute();
		}


		protected void OnBtnCopyEntityIdClicked(object sender, EventArgs e)
		{
			if(ViewModel.Entity.Id > 0)
			{
				GetClipboard(Gdk.Selection.Clipboard).Text = ViewModel.Entity.Id.ToString();
			}
		}

		public override void Destroy()
		{
			attachedfileinformationsview?.Destroy();
			base.Destroy();
		}
	}
}

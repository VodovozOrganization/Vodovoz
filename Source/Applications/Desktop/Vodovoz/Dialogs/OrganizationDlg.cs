using NLog;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Validation;
using System;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Factories;
using Vodovoz.ViewModels.ViewModels.Contacts;
using QS.Services;
using Vodovoz.Controllers;

namespace Vodovoz
{
	public partial class OrganizationDlg : QS.Dialog.Gtk.EntityDialogBase<Organization>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();

		private readonly IOrganizationVersionsViewModelFactory _organizationVersionsViewModelFactory 
			= new OrganizationVersionsViewModelFactory(ServicesConfig.CommonServices, new EmployeeJournalFactory(Startup.MainWin.NavigationManager));
		private readonly IPhoneRepository _phoneRepository = new PhoneRepository();
		private readonly ICommonServices _commonServices = ServicesConfig.CommonServices;
		private readonly IExternalCounterpartyController _externalCounterpartyController =
			new ExternalCounterpartyController(new ExternalCounterpartyRepository(), ServicesConfig.InteractiveService);
		private readonly IContactParametersProvider _contactsParameters = new ContactParametersProvider(new ParametersProvider());

		private PhonesViewModel _phonesViewModel;

		public override bool HasChanges {
			get {
				_phonesViewModel.RemoveEmpty();
				return base.HasChanges;
			}
			set => base.HasChanges = value;
		}

		public OrganizationDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Organization> ();
			ConfigureDlg ();
		}

		public OrganizationDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Organization> (id);
			ConfigureDlg ();
		}

		public OrganizationDlg (Organization sub) : this (sub.Id)
		{

		}

		private void ConfigureDlg ()
		{
			dataentryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			dataentryFullName.Binding.AddBinding(Entity, e => e.FullName, w => w.Text).InitializeFromSource();

			dataentryEmail.ValidationMode = QSWidgetLib.ValidationType.email;
			dataentryEmail.Binding.AddBinding(Entity, e => e.Email, w => w.Text).InitializeFromSource();
			dataentryINN.ValidationMode = QSWidgetLib.ValidationType.numeric;
			dataentryINN.Binding.AddBinding(Entity, e => e.INN, w => w.Text).InitializeFromSource();
			dataentryKPP.ValidationMode = QSWidgetLib.ValidationType.numeric;
			dataentryKPP.Binding.AddBinding(Entity, e => e.KPP, w => w.Text).InitializeFromSource();
			dataentryOGRN.ValidationMode = QSWidgetLib.ValidationType.numeric;
			dataentryOGRN.Binding.AddBinding(Entity, e => e.OGRN, w => w.Text).InitializeFromSource();
			dataentryOKPO.ValidationMode = QSWidgetLib.ValidationType.numeric;
			dataentryOKPO.Binding.AddBinding(Entity, e => e.OKPO, w => w.Text).InitializeFromSource();
			dataentryOKVED.Binding.AddBinding(Entity, e => e.OKVED, w => w.Text).InitializeFromSource();

			notebookMain.Page = 0;
			notebookMain.ShowTabs = false;
			accountsview1.SetAccountOwner(UoW, Entity);

			_phonesViewModel =
				new PhonesViewModel(
					_phoneRepository,
					UoW,
					_contactsParameters,
					_commonServices,
					_externalCounterpartyController)
					{
						PhonesList = UoWGeneric.Root.ObservablePhones
					};
			phonesView.ViewModel = _phonesViewModel;

			var organizationVersionsViewModel = _organizationVersionsViewModelFactory.CreateOrganizationVersionsViewModel(Entity);
			versionsView.ViewModel = organizationVersionsViewModel;
		}

		public override bool Save ()
		{
			var validator = new ObjectValidator(new GtkValidationViewFactory());
			if(!validator.Validate(Entity))
			{
				return false;
			}

			logger.Info ("Сохраняем организацию...");
			try {
				_phonesViewModel.RemoveEmpty();
				UoWGeneric.Save ();
				return true;
			} catch (Exception ex) {
				string text = "Организация не сохранилась...";
				logger.Error (ex, text);
				QSProjectsLib.QSMain.ErrorMessage ((Gtk.Window)this.Toplevel, ex, text);
				return false;
			}
		}

		protected void OnRadioTabInfoToggled (object sender, EventArgs e)
		{
			if (radioTabInfo.Active)
				notebookMain.CurrentPage = 0;
		}

		protected void OnRadioTabAccountsToggled (object sender, EventArgs e)
		{
			if (radioTabAccounts.Active)
				notebookMain.CurrentPage = 1;
		}
	}
}

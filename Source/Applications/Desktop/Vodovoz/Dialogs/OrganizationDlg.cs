using Autofac;
using NLog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Validation;
using QS.ViewModels.Extension;
using System;
using Vodovoz.Domain.Organizations;
using Vodovoz.ViewModels.Factories;
using Vodovoz.ViewModels.ViewModels.Contacts;
using Vodovoz.Controllers;
using Vodovoz.EntityRepositories;
using Vodovoz.Settings.Contacts;

namespace Vodovoz
{
	public partial class OrganizationDlg : QS.Dialog.Gtk.EntityDialogBase<Organization>, IAskSaveOnCloseViewModel
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();

		private IOrganizationVersionsViewModelFactory _organizationVersionsViewModelFactory;
		private IPhoneRepository _phoneRepository;
		private IExternalCounterpartyController _externalCounterpartyController;
		private IContactSettings _contactSettings;
		private ILifetimeScope _lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();

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
			Build ();
			UoWGeneric = _lifetimeScope.Resolve<IUnitOfWorkFactory>().CreateWithNewRoot<Organization>();
			ConfigureDlg();
		}

		public OrganizationDlg (int id)
		{
			Build ();
			UoWGeneric = _lifetimeScope.Resolve<IUnitOfWorkFactory>().CreateForRoot<Organization>(id);
			ConfigureDlg();
		}

		public OrganizationDlg (Organization sub) : this (sub.Id)
		{

		}

		public bool IsCanEditEntity => 
			permissionResult.CanUpdate
			|| (UoWGeneric.IsNew && permissionResult.CanCreate);

		public bool AskSaveOnClose => IsCanEditEntity;

		private void ConfigureDlg ()
		{
			ResolveDependencies();
			notebookMain.Visible = IsCanEditEntity || permissionResult.CanRead;

			accountsview1.CanEdit = IsCanEditEntity;
			buttonSave.Sensitive = IsCanEditEntity;
			btnCancel.Clicked += (sender, args) => OnCloseTab(IsCanEditEntity, CloseSource.Cancel);

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

			_phonesViewModel = new PhonesViewModel(
				_phoneRepository,
				UoW,
				_contactSettings,
				_externalCounterpartyController,
				_lifetimeScope)
				{
					PhonesList = UoWGeneric.Root.ObservablePhones
				};
			phonesView.ViewModel = _phonesViewModel;

			var organizationVersionsViewModel = _organizationVersionsViewModelFactory.CreateOrganizationVersionsViewModel(Entity, IsCanEditEntity);
			versionsView.ViewModel = organizationVersionsViewModel;
		}

		private void ResolveDependencies()
		{
			_organizationVersionsViewModelFactory = _lifetimeScope.Resolve<IOrganizationVersionsViewModelFactory>();
			_phoneRepository = _lifetimeScope.Resolve<IPhoneRepository>();
			_externalCounterpartyController = _lifetimeScope.Resolve<IExternalCounterpartyController>();
			_contactSettings = _lifetimeScope.Resolve<IContactSettings>();
		}

		public override bool Save ()
		{
			var validator = _lifetimeScope.Resolve<IValidator>();
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

		public override void Destroy()
		{
			if(_lifetimeScope != null)
			{
				_lifetimeScope.Dispose();
				_lifetimeScope = null;
			}
			
			base.Destroy();
		}
	}
}

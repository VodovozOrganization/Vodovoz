﻿using Autofac;
using NLog;
using QS.Navigation;
using QS.Project.Services;
using QS.ViewModels.Extension;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Organizations;
using Vodovoz.ViewModels.Factories;

namespace Vodovoz
{
	public partial class OrganizationDlg : QS.Dialog.Gtk.EntityDialogBase<Organization>, IAskSaveOnCloseViewModel
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private ILifetimeScope _lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();
		private IOrganizationVersionsViewModelFactory _organizationVersionsViewModelFactory;

		public override bool HasChanges {
			get {
				phonesview1.RemoveEmpty();
				return base.HasChanges;
			}
			set => base.HasChanges = value;
		}

		public OrganizationDlg()
		{
			Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateWithNewRoot<Organization>();
			RegisterDependencies();
			ConfigureDlg();
		}

		public OrganizationDlg(int id)
		{
			Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateForRoot<Organization>(id);
			RegisterDependencies();
			ConfigureDlg();
		}

		public OrganizationDlg(Organization sub) : this(sub.Id)
		{

		}

		public bool IsCanEditEntity => 
			permissionResult.CanUpdate
			|| (UoWGeneric.IsNew && permissionResult.CanCreate);

		public bool AskSaveOnClose => IsCanEditEntity;
		
		private void RegisterDependencies()
		{
			_organizationVersionsViewModelFactory = _lifetimeScope.Resolve<IOrganizationVersionsViewModelFactory>();
		}

		private void ConfigureDlg ()
		{
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

			phonesview1.UoW = UoWGeneric;
			if (UoWGeneric.Root.Phones == null)
				UoWGeneric.Root.Phones = new List<Phone> ();
			phonesview1.Phones = UoWGeneric.Root.Phones;

			var organizationVersionsViewModel = _organizationVersionsViewModelFactory.CreateOrganizationVersionsViewModel(Entity, IsCanEditEntity);
			versionsView.ViewModel = organizationVersionsViewModel;
		}

		public override bool Save ()
		{
			var validator = ServicesConfig.ValidationService;
			if(!validator.Validate(Entity))
			{
				return false;
			}

			_logger.Info ("Сохраняем организацию...");
			try {
				phonesview1.RemoveEmpty();
				UoWGeneric.Save ();
				return true;
			} catch (Exception ex) {
				string text = "Организация не сохранилась...";
				_logger.Error (ex, text);
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

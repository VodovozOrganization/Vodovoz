using QS.Views.GtkUI;
using QS.Widgets;
using ReactiveUI.Validation.Extensions;
using System;
using System.ComponentModel;
using Vodovoz.ViewModels.Organizations;

namespace Vodovoz.Organizations
{
	[ToolboxItem(true)]
	public partial class OrganizationView : TabViewBase<OrganizationViewModel>
	{
		public OrganizationView(OrganizationViewModel viewModel)
			: base(viewModel)
		{
			Build();
			Initialize();
		}

		private void Initialize()
		{
			notebookMain.Visible = ViewModel.CanEdit || ViewModel.CanRead;

			accountsview1.CanEdit = ViewModel.CanEdit;

			buttonSave.Binding.AddFuncBinding(
				ViewModel,
				vm => vm.CanEdit,
				w => w.Sensitive);

			buttonSave.Clicked += OnSaveButtonClicked;

			btnCancel.BindCommand(ViewModel.CancelCommand);

			dataentryName.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.InitializeFromSource();

			dataentryFullName.Binding
				.AddBinding(ViewModel.Entity, e => e.FullName, w => w.Text)
				.InitializeFromSource();

			validatedentryEmail.ValidationMode = ValidationType.Email;
			validatedentryEmail.Binding
				.AddBinding(ViewModel.Entity, e => e.Email, w => w.Text)
				.InitializeFromSource();

			validatedentryEmailForMailing.CustomRegex = ViewModel.RegexForEmailForMailing;
			validatedentryEmailForMailing.Binding
				.AddBinding(ViewModel.Entity, e => e.EmailForMailing, w => w.Text)
				.InitializeFromSource();

			validatedentryInn.ValidationMode = ValidationType.Numeric;
			validatedentryInn.Binding
				.AddBinding(ViewModel.Entity, e => e.INN, w => w.Text)
				.InitializeFromSource();

			validatedentryKpp.ValidationMode = ValidationType.Numeric;
			validatedentryKpp.Binding
				.AddBinding(ViewModel.Entity, e => e.KPP, w => w.Text)
				.InitializeFromSource();

			validatedentryOgrn.ValidationMode = ValidationType.Numeric;
			validatedentryOgrn.Binding
				.AddBinding(ViewModel.Entity, e => e.OGRN, w => w.Text)
				.InitializeFromSource();

			validatedentryOkpo.ValidationMode = ValidationType.Numeric;
			validatedentryOkpo.Binding
				.AddBinding(ViewModel.Entity, e => e.OKPO, w => w.Text)
				.InitializeFromSource();

			dataentryOKVED.Binding
				.AddBinding(ViewModel.Entity, e => e.OKVED, w => w.Text)
				.InitializeFromSource();
			
			chkIsNeedCashlessMovementControl.Binding
				.AddBinding(ViewModel.Entity, e => e.IsNeedCashlessMovementControl, w => w.Active)
				.InitializeFromSource();

			yentrySuffix.Binding
				.AddBinding(ViewModel.Entity, e => e.Suffix, w => w.Text)
				.InitializeFromSource();

			notebookMain.Page = 0;
			notebookMain.ShowTabs = false;
			accountsview1.SetAccountOwner(ViewModel.UoW, ViewModel.Entity);

			phonesview1.UoW = ViewModel.UoWGeneric;

			phonesview1.Phones = ViewModel.Entity.Phones;

			versionsView.ViewModel = ViewModel.OrganizationVersionsViewModel;
			
			radioTabInfo.Toggled += OnRadioTabInfoToggled;
			radioTabAccounts.Toggled += OnRadioTabAccountsToggled;

			vatRateVersionForOrganizationView.ViewModel = ViewModel.VatRateOrganizationVersionViewModel;
		}

		private void OnSaveButtonClicked(object sender, EventArgs e)
		{
			phonesview1.RemoveEmpty();
			ViewModel.SaveCommand.Execute();
		}

		protected void OnRadioTabInfoToggled(object sender, EventArgs e)
		{
			if(radioTabInfo.Active)
			{
				notebookMain.CurrentPage = 0;
			}
		}

		protected void OnRadioTabAccountsToggled(object sender, EventArgs e)
		{
			if(radioTabAccounts.Active)
			{
				notebookMain.CurrentPage = 1;
			}
		}
	}
}

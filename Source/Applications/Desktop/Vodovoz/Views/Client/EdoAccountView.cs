using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Gamma.GtkWidgets;
using Gamma.Widgets;
using Gtk;
using QS.ViewModels.Control.EEVM;
using QS.Views;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.ViewModels.Journals.JournalViewModels.Client;
using Vodovoz.ViewModels.ViewModels.Counterparty;
using VodovozBusiness.Domain.Client;

namespace Vodovoz.Views.Client
{
	[ToolboxItem(true)]
	public partial class EdoAccountView : ViewBase<EdoAccountViewModel>
	{
		private Menu _menuCopyFrom;
		private IDictionary<int, IList<MenuItem>> _menuItemsCopyFrom;
		
		public EdoAccountView(EdoAccountViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		public yRadioButton IsDefaultAccountRbtn => radioBtnIsDefault;

		private void Configure()
		{
			radioBtnIsDefault.Binding
				.AddBinding(ViewModel.Entity, e => e.IsDefault, w => w.Active)
				.InitializeFromSource();

			ConfigureMenuCopyFrom();
			ybuttonCheckClientInTaxcom.BindCommand(ViewModel.CheckClientInTaxcomCommand);

			InitializeEntries();

			yentryPersonalAccountCodeInEdo.Binding
				.AddBinding(ViewModel, vm => vm.CanEditPersonalAccountCodeInEdo, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.PersonalAccountIdInEdo, w => w.Text)
				.InitializeFromSource();

			yentryPersonalAccountCodeInEdo.Changed += OnEdoPersonalAccountChanged;

			ybuttonSendInviteByTaxcom.BindCommand(ViewModel.SendInviteByTaxcomCommand);
			ybuttonSendManualInvite.BindCommand(ViewModel.SendManualInviteByTaxcomCommand);

			yEnumCmbConsentForEdo.ItemsEnum = typeof(ConsentForEdoStatus);
			yEnumCmbConsentForEdo.Binding
				.AddBinding(ViewModel.Entity, e => e.ConsentForEdoStatus, w => w.SelectedItem)
				.InitializeFromSource();
			yEnumCmbConsentForEdo.Sensitive = false;

			ybuttonCheckConsentForEdo.BindCommand(ViewModel.CheckConsentForEdoCommand);
			
			ViewModel.UpdateOperators = UpdateOperators;
			specialListCmbAllOperators.Binding
				.AddBinding(ViewModel, vm => vm.CanSelectRegisteredEdoAccount, w => w.Sensitive)
				.AddBinding(ViewModel.Counterparty, e => e.CounterpartyEdoOperators, w => w.ItemsList)
				.InitializeFromSource();

			specialListCmbAllOperators.ItemSelected += OnAllOperatorsItemSelected;
			
			btnRemoveEdoAccount.BindCommand(ViewModel.RemoveEdoAccountCommand);
		}

		private void UpdateOperators()
		{
			specialListCmbAllOperators.SetRenderTextFunc<CounterpartyEdoOperator>(x => x.Title);			
		}

		private void ConfigureMenuCopyFrom()
		{
			_menuItemsCopyFrom = new Dictionary<int, IList<MenuItem>>();
			_menuCopyFrom = new Menu();
			menuBtnCopyParametersFrom.Menu = _menuCopyFrom;

			foreach(var edoAccountsByOrganizationId in ViewModel.Counterparty.CounterpartyEdoAccounts.ToLookup(x => x.OrganizationId))
			{
				if(ViewModel.Entity.OrganizationId == edoAccountsByOrganizationId.Key)
				{
					continue;
				}

				if(!_menuItemsCopyFrom.TryGetValue(edoAccountsByOrganizationId.Key.Value, out var menuItemsCopyFrom))
				{
					menuItemsCopyFrom = new List<MenuItem>();
					_menuItemsCopyFrom.Add(edoAccountsByOrganizationId.Key.Value, menuItemsCopyFrom);
				}
				
				var organization = ViewModel.UoW.GetById<Domain.Organizations.Organization>(edoAccountsByOrganizationId.Key.Value);
				var menuItem = new MenuItem(organization.Name);
				var menuItemMenu = new Menu();
				menuItem.Submenu = menuItemMenu;
				_menuCopyFrom.Add(menuItem);
				var i = 1;
				
				foreach(var edoAccount in edoAccountsByOrganizationId)
				{
					var subItem = new yMenuItem($"Аккаунт №{i}");
					subItem.BindCommand(ViewModel.CopyFromEdoOperatorWithAccountCommand, edoAccount);
					menuItemMenu.Add(subItem);
					i++;
				}
			}
			
			_menuCopyFrom.ShowAll();

			if(!_menuCopyFrom.Children.Any())
			{
				menuBtnCopyParametersFrom.Sensitive = false;
			}
		}

		private void InitializeEntries()
		{
			var builder = new LegacyEEVMBuilderFactory<CounterpartyEdoAccount>(
				ViewModel.ParentTab, ViewModel.Entity, ViewModel.UoW, ViewModel.NavigationManager, ViewModel.Scope);
			
			var operatorEdoVm = builder
				.ForProperty(x => x.EdoOperator)
				.UseViewModelJournalAndAutocompleter<EdoOperatorsJournalViewModel>()
				.UseViewModelDialog<EdoOperatorViewModel>()
				.Finish();
			operatorEdoVm.CanViewEntity = false;

			operatorEdoEntry.ViewModel = operatorEdoVm;
			operatorEdoEntry.Binding
				.AddBinding(ViewModel, e => e.CanChangeOperatorEdo, w => w.Sensitive)
				.InitializeFromSource();
			
			operatorEdoVm.ChangedByUser += OnOperatorEdoChangedByUser;
		}

		private void OnOperatorEdoChangedByUser(object sender, EventArgs e)
		{
			ViewModel.ResetConsentForEdo();
		}
		
		private void OnEdoPersonalAccountChanged(object sender, EventArgs e)
		{
			ViewModel.ResetConsentForEdo();
		}
		
		private void OnAllOperatorsItemSelected(object sender, ItemSelectedEventArgs e)
		{
			if(e.SelectedItem is CounterpartyEdoOperator counterpartyEdoOperator)
			{
				ViewModel.TryFillEdoAccount(counterpartyEdoOperator);
			}
		}

		public override void Destroy()
		{
			ViewModel.UpdateOperators = null;

			base.Destroy();
		}
	}
}

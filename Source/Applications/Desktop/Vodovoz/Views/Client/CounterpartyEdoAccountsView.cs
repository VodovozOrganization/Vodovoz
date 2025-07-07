using System;
using System.ComponentModel;
using Gtk;
using QS.Dialog;
using QS.Navigation;
using QS.Project.Journal;
using QS.Utilities;
using QS.Views;
using Vodovoz.ViewModels.Organizations;
using Vodovoz.ViewModels.ViewModels.Counterparty;

namespace Vodovoz.Views.Client
{
	[ToolboxItem(true)]
	public partial class CounterpartyEdoAccountsView : ViewBase<CounterpartyEdoAccountsViewModel>
	{
		public CounterpartyEdoAccountsView(CounterpartyEdoAccountsViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			btnAddEdoAccount.Clicked += OnAddEdoAccountClicked;
			btnAddOrganization.Clicked += OnAddOrganizationClicked;
			
			InitializeCurrentEdoAccountsViews();
		}

		//TODO: доработать создание сущности CounterpartyEdoOperator, возможно заменить ссылку на организацию ее Id
		private void OnAddEdoAccountClicked(object sender, EventArgs e)
		{
			var currentPage = (CounterpartyEdoAccountsByOrganizationView)notebookAccountsByOrganizations.CurrentPageWidget;
			ViewModel.AddEdoAccountCommand.Execute(currentPage.ViewModel);
		}
		
		private void OnAddOrganizationClicked(object sender, EventArgs e)
		{
			var organizationsPage =
				ViewModel.NavigationManager.OpenViewModelOnTdi<OrganizationJournalViewModel, Action<OrganizationJournalFilterViewModel>>(
					ViewModel.ParentTab,
					filter => filter.HasTaxcomEdoAccountId = true,
					OpenPageOptions.AsSlave,
					vm => vm.SelectionMode = JournalSelectionMode.Single
					);
			
			organizationsPage.ViewModel.OnSelectResult += OnSelectOrganization;
		}

		private void OnSelectOrganization(object sender, JournalSelectedEventArgs e)
		{
			var journalViewModel = sender as OrganizationJournalViewModel;
			journalViewModel.OnSelectResult -= OnSelectOrganization;

			var selectedNodes = e.GetSelectedObjects<OrganizationJournalNode>();

			if(selectedNodes.Length > 1)
			{
				return;
			}
			
			var firstNode = selectedNodes[0];

			if(ViewModel.EdoAccountsViewModelsByOrganizationId.ContainsKey(firstNode.Id))
			{
				ViewModel.InteractiveService.ShowMessage(ImportanceLevel.Info, "Данная организация уже добавлена!");
				return;
			}
			
			ViewModel.AddOrganizationCommand.Execute(firstNode.Id);
			AddNewOrganizationPage(firstNode.Name, ViewModel.EdoAccountsViewModelsByOrganizationId[firstNode.Id].EdoAccountsViewModel);
		}

		private void Do()
		{
			var currentPage = notebookAccountsByOrganizations.CurrentPageWidget;
			var box = (Notebook.NotebookChild)notebookAccountsByOrganizations[currentPage];
			box.TabExpand = true;
		}

		private void InitializeCurrentEdoAccountsViews()
		{
			foreach(var keyPairValue in ViewModel.EdoAccountsViewModelsByOrganizationId)
			{
				AddNewOrganizationPage(keyPairValue.Value.OrganizationName, keyPairValue.Value.EdoAccountsViewModel);
			}
		}

		private void AddNewOrganizationPage(
			string organizationName,
			CounterpartyEdoAccountsByOrganizationViewModel edoAccountsViewModel)
		{
			var accountView = new CounterpartyEdoAccountsByOrganizationView(edoAccountsViewModel);
			notebookAccountsByOrganizations.AppendPage(accountView, $"{organizationName}");
			accountView.HeightRequest = 500;
			accountView.Show();
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.ViewModels.Dialog;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Complaints;

namespace Vodovoz.ViewModels.Mango
{
	public class InternalCallViewModel : DialogViewModelBase
	{
		private readonly ITdiCompatibilityNavigation tdiCompatibilityNavigation;

		public InternalCallViewModel(ITdiCompatibilityNavigation navigation) : base(navigation)
		{
			this.tdiCompatibilityNavigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			Title = "Входящий новый номер";
		}

		#region Действия View

		public void CreateComplaint()
		{
			var parameters = new Dictionary<string, object> {
				{"uowBuilder", EntityUoWBuilder.ForCreate()},
				{"employeeSelectorFactory", new DefaultEntityAutocompleteSelectorFactory<Employee, EmployeesJournalViewModel, EmployeeFilterViewModel>(ServicesConfig.CommonServices)},
				{"counterpartySelectorFactory", new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>(ServicesConfig.CommonServices)},
				{"phone", "не реализовано"}//FIXME
			};
			tdiCompatibilityNavigation.OpenTdiTabNamedArgs<CreateComplaintViewModel>(null, parameters);
		}

		public void SelectNewConterparty()
		{
			var page = tdiCompatibilityNavigation.OpenTdiTab<CounterpartyDlg>(null);
			var tab = page.TdiTab as CounterpartyDlg;
			tab.Entity.Phones.First().Number = "+7-000-000-00-00"; //FIXME
			page.PageClosed += NewCounerpatry_PageClosed;
		}

		void NewCounerpatry_PageClosed(object sender, PageClosedEventArgs e)
		{
			if(e.CloseSource == CloseSource.Save) 
				{ }//FIXME Открыть другой диалог.
		}

		public void SelectExistConterparty()
		{
			var page = NavigationManager.OpenViewModel<CounterpartyJournalViewModel>(null);
			page.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Single;
			page.ViewModel.OnSelectResult += CounterpartyJournal_OnSelectResult;
		}

		void CounterpartyJournal_OnSelectResult(object sender, QS.Project.Journal.JournalSelectedEventArgs e)
		{
			//FIXME получить контрагента и перейти на другой диалог
		}

		#endregion

	}
}

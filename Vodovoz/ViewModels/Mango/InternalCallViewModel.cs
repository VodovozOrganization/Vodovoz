using System;
using System.Collections.Generic;
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

		#endregion
	}
}

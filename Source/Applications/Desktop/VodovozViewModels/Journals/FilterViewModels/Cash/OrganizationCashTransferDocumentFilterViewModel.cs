using System;
using System.Collections.Generic;
using QS.Project.Filter;
using QS.Project.Journal.EntitySelector;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Organizations;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.Journals.FilterViewModels
{
    public class OrganizationCashTransferDocumentFilterViewModel : FilterViewModelBase<OrganizationCashTransferDocumentFilterViewModel>
    {
        public IEntityAutocompleteSelectorFactory EmployeeSelectorFactory { get; }
        public OrganizationCashTransferDocumentFilterViewModel(IEmployeeJournalFactory employeeJournalFactory)
        {
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
            EmployeeSelectorFactory = _employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory();
			Organizations = UoW.GetAll<Organization>();
		}

        private DateTime? startDate;
        public DateTime? StartDate
        {
            get => startDate; 
            set => UpdateFilterField(ref startDate, value);
        }

        private DateTime? endDate;
        public DateTime? EndDate
        {
            get => endDate; 
            set => UpdateFilterField(ref endDate, value);
        }

        private Employee author;
        public virtual Employee Author
        {
            get => author;
            set => UpdateFilterField(ref author, value);
        }

        public IEnumerable<Organization> Organizations { get; }

        private Organization organizationFrom;
        public virtual Organization OrganizationFrom
        {
            get => organizationFrom;
            set => UpdateFilterField(ref organizationFrom, value);
        }

        private Organization organizationTo;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;

		public virtual Organization OrganizationTo
        {
            get => organizationTo;
            set => UpdateFilterField(ref organizationTo, value);
        }
    }
}

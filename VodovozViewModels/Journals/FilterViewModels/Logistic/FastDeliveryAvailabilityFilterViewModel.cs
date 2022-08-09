using QS.Project.Filter;
using QS.Project.Journal.EntitySelector;
using System;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.Journals.FilterViewModels;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Logistic
{
	public class FastDeliveryAvailabilityFilterViewModel : FilterViewModelBase<FastDeliveryAvailabilityFilterViewModel>
	{
		private DateTime? _verificationDateFrom;
		private DateTime? _verificationDateEndTo;
		private Counterparty _counterparty;
		private Employee _logistician;
		private District _district;
		private bool? _isValid;
		private bool? _isVerificationFromSite;
		private bool? _isNomenclatureNotInStock;
		private int _logisticianReactionTimeMinutes;

		public FastDeliveryAvailabilityFilterViewModel(
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			IDistrictJournalFactory districtJournalFactory)
		{
			EmployeeSelectorFactory =
				(employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory)))
				.CreateWorkingEmployeeAutocompleteSelectorFactory();

			var districtFilter = new DistrictJournalFilterViewModel()
			{
				Status = DistrictsSetStatus.Active
			};
			DistrictSelectorFactory =
				(districtJournalFactory ?? throw new ArgumentNullException(nameof(districtJournalFactory)))
				.CreateDistrictAutocompleteSelectorFactory(districtFilter);

			CounterpartySelectorFactory =
				(counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory)))
				.CreateCounterpartyAutocompleteSelectorFactory();

			IsValid = false;
		}

		public IEntityAutocompleteSelectorFactory CounterpartySelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory EmployeeSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory DistrictSelectorFactory { get; }

		public DateTime? VerificationDateFrom
		{
			get => _verificationDateFrom;
			set => UpdateFilterField(ref _verificationDateFrom, value);
		}

		public DateTime? VerificationDateTo
		{
			get => _verificationDateEndTo;
			set => UpdateFilterField(ref _verificationDateEndTo, value);
		}

		public Counterparty Counterparty
		{
			get => _counterparty;
			set => UpdateFilterField(ref _counterparty, value);
		}

		public District District
		{
			get => _district;
			set => UpdateFilterField(ref _district, value);
		}

		public bool? IsValid
		{
			get => _isValid;
			set => UpdateFilterField(ref _isValid, value);
		}

		public bool? IsVerificationFromSite
		{
			get => _isVerificationFromSite;
			set => UpdateFilterField(ref _isVerificationFromSite, value);
		}

		public bool? IsNomenclatureNotInStock
		{
			get => _isNomenclatureNotInStock; 
			set => UpdateFilterField(ref _isNomenclatureNotInStock, value);
		}

		public Employee Logistician
		{
			get => _logistician;
			set => UpdateFilterField(ref _logistician, value);
		}

		public int LogisticianReactionTimeMinutes
		{
			get => _logisticianReactionTimeMinutes;
			set => UpdateFilterField(ref _logisticianReactionTimeMinutes, value);
		}
	}
}

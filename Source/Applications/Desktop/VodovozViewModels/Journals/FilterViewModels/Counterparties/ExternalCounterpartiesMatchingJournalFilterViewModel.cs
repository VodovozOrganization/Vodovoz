using System;
using QS.Project.Filter;
using QS.Project.Journal;
using Vodovoz.Domain.Client;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Counterparties
{
	public class ExternalCounterpartiesMatchingJournalFilterViewModel
		: FilterViewModelBase<ExternalCounterpartiesMatchingJournalFilterViewModel>
	{
		private ExternalCounterpartyMatchingStatus? _matchingStatus;
		private DateTime? _startDate;
		private DateTime? _endDate;
		private string _phoneNumber;
		private int? _counterpartyId;

		public ExternalCounterpartiesMatchingJournalFilterViewModel(
			Action<ExternalCounterpartiesMatchingJournalFilterViewModel> filterParams = null)
		{
			if(filterParams != null)
			{
				SetAndRefilterAtOnce(filterParams);
			}
		}

		public ExternalCounterpartyMatchingStatus? MatchingStatus
		{
			get => _matchingStatus;
			set => UpdateFilterField(ref _matchingStatus, value);
		}
		
		public DateTime? StartDate
		{
			get => _startDate;
			set => UpdateFilterField(ref _startDate, value);
		}
		
		public DateTime? EndDate
		{
			get => _endDate;
			set => UpdateFilterField(ref _endDate, value);
		}

		public string PhoneNumber
		{
			get => _phoneNumber;
			set => SetField(ref _phoneNumber, value);
		}
		
		public int? CounterpartyId
		{
			get => _counterpartyId;
			set => SetField(ref _counterpartyId, value);
		}

		public override bool IsShow { get; set; } = true;
	}
}

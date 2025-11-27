using QS.Project.Filter;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.WageCalculation.CallCenterMotivation
{
	public class CallCenterMotivationCoefficientJournalFilterViewModel : FilterViewModelBase<CallCenterMotivationCoefficientJournalFilterViewModel>
	{
		private bool _isHideArchived = true;

		public bool IsHideArchived
		{
			get => _isHideArchived;
			set => UpdateFilterField(ref _isHideArchived, value);
		}

		public override bool IsShow { get; set; } = true;

		public string SearchString { get; internal set; }

		public string SqlSearchString => string.IsNullOrWhiteSpace(SearchString) ? string.Empty : $"%{SearchString.ToLower()}%";

		public bool IsSearchStringEmpty => string.IsNullOrWhiteSpace(SearchString);
	}
}

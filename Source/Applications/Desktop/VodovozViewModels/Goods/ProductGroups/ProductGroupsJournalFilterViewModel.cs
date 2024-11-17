using QS.Project.Filter;

namespace Vodovoz.ViewModels.Goods.ProductGroups
{
	public class ProductGroupsJournalFilterViewModel : FilterViewModelBase<ProductGroupsJournalFilterViewModel>
	{
		private bool _isHideArchived = true;
		private bool _isGroupSelectionMode;

		public bool IsHideArchived
		{
			get => _isHideArchived;
			set => UpdateFilterField(ref _isHideArchived, value);
		}

		public bool IsGroupSelectionMode
		{
			get => _isGroupSelectionMode;
			set => UpdateFilterField(ref _isGroupSelectionMode, value);
		}

		public override bool IsShow { get; set; } = true;

		public string SearchString { get; internal set; }

		public string SqlSearchString =>
			string.IsNullOrWhiteSpace(SearchString) ? string.Empty : $"%{SearchString.ToLower()}%";

		public bool IsSearchStringEmpty => string.IsNullOrWhiteSpace(SearchString);
	}
}

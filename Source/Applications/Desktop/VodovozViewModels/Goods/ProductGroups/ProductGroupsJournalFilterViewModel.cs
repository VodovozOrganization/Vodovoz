using QS.Project.Filter;

namespace Vodovoz.ViewModels.Goods.ProductGroups
{
	public class ProductGroupsJournalFilterViewModel : FilterViewModelBase<ProductGroupsJournalFilterViewModel>
	{
		private bool _isShowArchived;
		public bool IsShowArchived
		{
			get => _isShowArchived;
			set => UpdateFilterField(ref _isShowArchived, value);
		}

		public override bool IsShow { get; set; } = true;

		public string SearchString { get; internal set; }
	}
}

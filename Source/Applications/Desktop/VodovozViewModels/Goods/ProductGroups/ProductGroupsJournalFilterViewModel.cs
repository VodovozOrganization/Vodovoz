using QS.Project.Filter;

namespace Vodovoz.ViewModels.Goods.ProductGroups
{
	public class ProductGroupsJournalFilterViewModel : FilterViewModelBase<ProductGroupsJournalFilterViewModel>
	{
		private bool _isHideArchived;
		public bool IsHideArchived
		{
			get => _isHideArchived;
			set => UpdateFilterField(ref _isHideArchived, value);
		}

		public override bool IsShow { get; set; } = true;

		public string SearchString { get; internal set; }
	}
}

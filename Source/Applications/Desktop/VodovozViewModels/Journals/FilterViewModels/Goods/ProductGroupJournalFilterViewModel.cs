using QS.Project.Filter;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Goods
{
	public class ProductGroupJournalFilterViewModel : FilterViewModelBase<ProductGroupJournalFilterViewModel>
	{
		private bool _hideArchive;
		public bool HideArchive
		{
			get => _hideArchive;
			set => UpdateFilterField(ref _hideArchive, value);
		}
	}
}

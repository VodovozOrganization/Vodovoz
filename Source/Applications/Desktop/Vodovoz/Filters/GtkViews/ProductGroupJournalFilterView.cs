using QS.Views.GtkUI;
using Vodovoz.Filters.ViewModels;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;

namespace Vodovoz.Filters.GtkViews
{
	public partial class ProductGroupJournalFilterView : FilterViewBase<ProductGroupJournalFilterViewModel>
	{
		public ProductGroupJournalFilterView(ProductGroupJournalFilterViewModel filterViewModel) : base(filterViewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			ycheckArchive.Binding.AddBinding(ViewModel, vm => vm.HideArchive, w => w.Active).InitializeFromSource();
		}
	}
}

using Gtk;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Goods.ProductGroups;
using Key = Gdk.Key;

namespace Vodovoz.Filters.GtkViews
{
	public partial class ProductGroupsJournalFilterView : FilterViewBase<ProductGroupsJournalFilterViewModel>
	{
		public ProductGroupsJournalFilterView(ProductGroupsJournalFilterViewModel viewModel) : base(viewModel)
		{
			Build();
			Initialize();
		}

		private void Initialize()
		{
			yentrySearch.Binding
				.AddBinding(ViewModel, vm => vm.SearchString, w => w.Text)
				.InitializeFromSource();

			yentrySearch.KeyReleaseEvent += OnSearchKeyReleased;

			ycheckIsHideArchieved.Binding
				.AddBinding(ViewModel, vm => vm.IsHideArchived, w => w.Active)
				.InitializeFromSource();
		}

		private void OnSearchKeyReleased(object o, KeyReleaseEventArgs args)
		{
			if(args.Event.Key == Key.Return)
			{
				ViewModel.Update();
			}
		}

		public override void Destroy()
		{
			yentrySearch.KeyReleaseEvent -= OnSearchKeyReleased;

			base.Destroy();
		}
	}
}

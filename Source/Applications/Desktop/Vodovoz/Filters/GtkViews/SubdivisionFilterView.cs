using Gtk;
using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.FilterViewModels.Organization;
using Key = Gdk.Key;

namespace Vodovoz.Filters.GtkViews
{
	[ToolboxItem(true)]
	public partial class SubdivisionFilterView : FilterViewBase<SubdivisionFilterViewModel>
	{
		public SubdivisionFilterView(SubdivisionFilterViewModel filterViewModel) : base(filterViewModel)
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
			ycheckArchieve.Binding
				.AddBinding(ViewModel, vm => vm.ShowArchieved, w => w.Active)
				.InitializeFromSource();
		}

		private void OnSearchKeyReleased(object o, KeyReleaseEventArgs args)
		{
			if(args.Event.Key == Key.Return)
			{
				ViewModel.Update();
			}
		}
	}
}

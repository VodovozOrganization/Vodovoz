using Gtk;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Journals.FilterViewModels.WageCalculation.CallCenterMotivation;
using Key = Gdk.Key;

namespace Vodovoz.Filters.GtkViews
{
	public partial class СallCenterMotivationCoefficientJournalFilterView : FilterViewBase<CallCenterMotivationCoefficientJournalFilterViewModel>
	{
		public СallCenterMotivationCoefficientJournalFilterView(CallCenterMotivationCoefficientJournalFilterViewModel viewModel) : base(viewModel)
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

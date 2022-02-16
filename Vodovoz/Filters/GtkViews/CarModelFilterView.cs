using QS.Views.GtkUI;
using QS.Widgets;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;

namespace Vodovoz.Filters.GtkViews
{
	public partial class CarModelFilterView : FilterViewBase<CarModelJournalFilterViewModel>
	{
		public CarModelFilterView(CarModelJournalFilterViewModel filterViewModel) : base(filterViewModel)
		{
			Build();
			ConfigureView();
		}

		private void ConfigureView()
		{
			nullCheckArchive.RenderMode = RenderMode.Icon;
			nullCheckArchive.Binding.AddBinding(ViewModel, vm => vm.Archive, w => w.Active).InitializeFromSource();
		}
	}
}

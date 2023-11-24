using QS.Views.GtkUI;
using Vodovoz.ViewModels.Journals.FilterViewModels.GeoGroup;

namespace Vodovoz.Filters.GtkViews
{
	public partial class GeoGroupJournalFilterView : FilterViewBase<GeoGroupJournalFilterViewModel>
	{
		public GeoGroupJournalFilterView(GeoGroupJournalFilterViewModel filterViewModel) : base(filterViewModel)
		{
			Build();
			Configure();

		}

		private void Configure()
		{
			ycheckbuttonIsShowArchived.Binding
				.AddBinding(ViewModel, vm => vm.IsShowArchived, w => w.Active)
				.InitializeFromSource();
		}

	}
}

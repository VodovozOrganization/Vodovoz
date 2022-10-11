using QS.Views.GtkUI;
using Vodovoz.Journals.FilterViewModels;

namespace Vodovoz.Filters.GtkViews
{
	public partial class DistrictsSetJournalFilterView : FilterViewBase<DistrictsSetJournalFilterViewModel>
	{
		public DistrictsSetJournalFilterView(DistrictsSetJournalFilterViewModel filterViewModel) : base(filterViewModel)
		{
			this.Build();
		}
	}
}

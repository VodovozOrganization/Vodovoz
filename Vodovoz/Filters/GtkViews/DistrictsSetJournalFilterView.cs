using System;
using QS.Views.GtkUI;
using Vodovoz.FilterViewModels.Logistic;

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

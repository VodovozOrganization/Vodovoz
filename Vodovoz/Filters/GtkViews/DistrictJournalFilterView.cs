using System;
using QS.Views.GtkUI;
using Vodovoz.FilterViewModels.Logistic;

namespace Vodovoz.Filters.GtkViews
{
	public partial class DistrictJournalFilterView : FilterViewBase<DistrictJournalFilterViewModel>
	{
		public DistrictJournalFilterView(DistrictJournalFilterViewModel filterViewModel) : base(filterViewModel)
		{
			this.Build();
		}
	}
}

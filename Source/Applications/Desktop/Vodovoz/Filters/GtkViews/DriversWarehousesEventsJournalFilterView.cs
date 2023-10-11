using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DriversWarehousesEventsJournalFilterView : FilterViewBase<DriversWarehousesEventsJournalFilterViewModel>
	{
		public DriversWarehousesEventsJournalFilterView(DriversWarehousesEventsJournalFilterViewModel viewModel) : base(viewModel)
		{
			Build();
		}
	}
}

using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OnlineOrdersJournalFilterView : FilterViewBase<OnlineOrdersJournalFilterViewModel>
	{
		public OnlineOrdersJournalFilterView(OnlineOrdersJournalFilterViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{

		}
	}
}

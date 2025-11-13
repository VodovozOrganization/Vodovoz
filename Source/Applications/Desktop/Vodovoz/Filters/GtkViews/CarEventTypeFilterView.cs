using QS.Views.GtkUI;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CarEventTypeFilterView : FilterViewBase<CarEventTypeFilterViewModel>
	{
		public CarEventTypeFilterView(CarEventTypeFilterViewModel filterViewModel) : base(filterViewModel)
		{
			this.Build();
		}
	}
}

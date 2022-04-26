using QS.Views.GtkUI;
using Vodovoz.Filters.ViewModels;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RouteListTrackFilterView : FilterViewBase<RouteListTrackFilterViewModel>
	{
		public RouteListTrackFilterView(RouteListTrackFilterViewModel filterViewModel) : base(filterViewModel)
		{
			this.Build();
			ycheckbuttonIsFastDeliveryOnly.Binding.AddBinding(filterViewModel, vm => vm.IsFastDeliveryOnly, w => w.Active).InitializeFromSource();
		}

		public RouteListTrackFilterViewModel FilterViewModel => ViewModel;
	}
}

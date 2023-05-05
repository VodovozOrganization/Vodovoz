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

			Configure();
		}

		private void Configure()
		{
			ycheckbuttonShowFastDeliveryCircle.Visible = true;

			ycheckbuttonIsFastDeliveryOnly.Binding
				.AddBinding(ViewModel, vm => vm.IsFastDeliveryOnly, w => w.Active)
				.InitializeFromSource();

			ycheckbuttonShowFastDeliveryCircle.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.IsFastDeliveryOnly, w => w.Sensitive)
				.AddBinding(vm => vm.ShowFastDeliveryCircle, w => w.Active)
				.InitializeFromSource();
		}

		public RouteListTrackFilterViewModel FilterViewModel => ViewModel;
	}
}

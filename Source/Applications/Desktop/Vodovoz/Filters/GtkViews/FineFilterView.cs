using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.FilterViewModels.Employees;

namespace Vodovoz.Filters.GtkViews
{
	[ToolboxItem(true)]
	public partial class FineFilterView : FilterViewBase<FineFilterViewModel>
	{
		public FineFilterView(FineFilterViewModel filterViewModel) : base(filterViewModel)
		{
			Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			entrySubdivision.ViewModel = ViewModel.SubdivisionViewModel;

			ydateperiodpickerFineDate.Binding.AddBinding(ViewModel, vm => vm.FineDateStart, w => w.StartDateOrNull).InitializeFromSource();
			ydateperiodpickerFineDate.Binding.AddBinding(ViewModel, vm => vm.FineDateEnd, w => w.EndDateOrNull).InitializeFromSource();
			ydateperiodpickerFineDate.Binding.AddBinding(ViewModel, vm => vm.CanEditFineDate, w => w.Sensitive).InitializeFromSource();

			ydateperiodpickerRouteList.Binding.AddBinding(ViewModel, vm => vm.RouteListDateStart, w => w.StartDateOrNull).InitializeFromSource();
			ydateperiodpickerRouteList.Binding.AddBinding(ViewModel, vm => vm.RouteListDateEnd, w => w.EndDateOrNull).InitializeFromSource();
			ydateperiodpickerRouteList.Binding.AddBinding(ViewModel, vm => vm.CanEditRouteListDate, w => w.Sensitive).InitializeFromSource();
		}
	}
}

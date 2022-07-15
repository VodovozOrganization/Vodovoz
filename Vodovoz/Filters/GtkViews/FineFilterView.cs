using System;
using QS.Views.GtkUI;
using Vodovoz.FilterViewModels.Employees;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class FineFilterView : FilterViewBase<FineFilterViewModel>
	{
		public FineFilterView(FineFilterViewModel filterViewModel) : base(filterViewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			yentryreferenceSubdivisions.SubjectType = typeof(Subdivision);
			yentryreferenceSubdivisions.Binding.AddBinding(ViewModel, vm => vm.Subdivision, w => w.Subject).InitializeFromSource();
			yentryreferenceSubdivisions.Binding.AddBinding(ViewModel, vm => vm.CanEditSubdivision, w => w.Sensitive).InitializeFromSource();

			ydateperiodpickerFineDate.Binding.AddBinding(ViewModel, vm => vm.FineDateStart, w => w.StartDateOrNull).InitializeFromSource();
			ydateperiodpickerFineDate.Binding.AddBinding(ViewModel, vm => vm.FineDateEnd, w => w.EndDateOrNull).InitializeFromSource();
			ydateperiodpickerFineDate.Binding.AddBinding(ViewModel, vm => vm.CanEditFineDate, w => w.Sensitive).InitializeFromSource();

			ydateperiodpickerRouteList.Binding.AddBinding(ViewModel, vm => vm.RouteListDateStart, w => w.StartDateOrNull).InitializeFromSource();
			ydateperiodpickerRouteList.Binding.AddBinding(ViewModel, vm => vm.RouteListDateEnd, w => w.EndDateOrNull).InitializeFromSource();
			ydateperiodpickerRouteList.Binding.AddBinding(ViewModel, vm => vm.CanEditRouteListDate, w => w.Sensitive).InitializeFromSource();
		}
	}
}

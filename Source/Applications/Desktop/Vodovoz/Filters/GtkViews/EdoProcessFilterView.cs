using System;
using QS.Project.Filter;
using QS.Views.GtkUI;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EdoProcessFilterView : FilterViewBase<EdoProcessFilterViewModel>
	{
		public EdoProcessFilterView(EdoProcessFilterViewModel filterViewModel) : base(filterViewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			//entrySubdivision.ViewModel = ViewModel.SubdivisionViewModel;

			//entryAuthor.ViewModel = ViewModel.AuthorViewModel;

			//ydateperiodpickerFineDate.Binding.AddBinding(ViewModel, vm => vm.FineDateStart, w => w.StartDateOrNull).InitializeFromSource();
			//ydateperiodpickerFineDate.Binding.AddBinding(ViewModel, vm => vm.FineDateEnd, w => w.EndDateOrNull).InitializeFromSource();
			//ydateperiodpickerFineDate.Binding.AddBinding(ViewModel, vm => vm.CanEditFineDate, w => w.Sensitive).InitializeFromSource();

			//ydateperiodpickerRouteList.Binding.AddBinding(ViewModel, vm => vm.RouteListDateStart, w => w.StartDateOrNull).InitializeFromSource();
			//ydateperiodpickerRouteList.Binding.AddBinding(ViewModel, vm => vm.RouteListDateEnd, w => w.EndDateOrNull).InitializeFromSource();
			//ydateperiodpickerRouteList.Binding.AddBinding(ViewModel, vm => vm.CanEditRouteListDate, w => w.Sensitive).InitializeFromSource();
		}
	}

	public class EdoProcessFilterViewModel : FilterViewModelBase<EdoProcessFilterViewModel>
	{

	}
}

using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalNodes.Employees;
using Gamma.Utilities;

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

			entryAuthor.ViewModel = ViewModel.AuthorViewModel;

			ydateperiodpickerFineDate.Binding.AddBinding(ViewModel, vm => vm.FineDateStart, w => w.StartDateOrNull).InitializeFromSource();
			ydateperiodpickerFineDate.Binding.AddBinding(ViewModel, vm => vm.FineDateEnd, w => w.EndDateOrNull).InitializeFromSource();
			ydateperiodpickerFineDate.Binding.AddBinding(ViewModel, vm => vm.CanEditFineDate, w => w.Sensitive).InitializeFromSource();

			ydateperiodpickerRouteList.Binding.AddBinding(ViewModel, vm => vm.RouteListDateStart, w => w.StartDateOrNull).InitializeFromSource();
			ydateperiodpickerRouteList.Binding.AddBinding(ViewModel, vm => vm.RouteListDateEnd, w => w.EndDateOrNull).InitializeFromSource();
			ydateperiodpickerRouteList.Binding.AddBinding(ViewModel, vm => vm.CanEditRouteListDate, w => w.Sensitive).InitializeFromSource();

			ytreeviewFineCategories.ColumnsConfig = Gamma.ColumnConfig.FluentColumnsConfig<EmployeeFineCategoryNode>.Create()
				.AddColumn("Категория штрафа").AddTextRenderer(x => x.FineCategory.GetEnumTitle())
				.AddColumn("").AddToggleRenderer(x => x.Selected)
				.Finish();
			ytreeviewFineCategories.ItemsDataSource = ViewModel.FineCategoryNodes;

			buttonCategoryAll.Clicked += (sender, args) =>
			{
				ViewModel.SelectAllFineCategories();
				ytreeviewFineCategories.YTreeModel.EmitModelChanged();
			};

			buttonCategoryNone.Clicked += (sender, args) =>
			{
				ViewModel.DeselectAllFineCategories();
				ytreeviewFineCategories.YTreeModel.EmitModelChanged();
			};
		}
	}
}

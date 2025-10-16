using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using Vodovoz.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalNodes.Employees;

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

			ytreeviewFineCategory.ColumnsConfig = Gamma.ColumnConfig.FluentColumnsConfig<EmployeeFineCategoryNode>.Create()
				.AddColumn("Категория штрафа").AddTextRenderer(x => x.FineCategoryName)
				.AddColumn("").AddToggleRenderer(x => x.Selected)
				.Finish();
			ytreeviewFineCategory.ItemsDataSource = ViewModel.FineCategoryNodes;

			ycheckbuttonShowArchieve.Binding
				.AddBinding(ViewModel, vm => vm.ShowArchive, w => w.Active)
				.InitializeFromSource();

			buttonCategoryAll.Clicked += OnCategoryAllClicked;
			buttonCategoryNone.Clicked += OnCategoryNoneClicked;
		}

		private void OnCategoryAllClicked(object sender, EventArgs args)
		{
			ViewModel.SelectAllFineCategories();
			ytreeviewFineCategory.YTreeModel.EmitModelChanged();
		}

		private void OnCategoryNoneClicked(object sender, EventArgs args)
		{
			ViewModel.DeselectAllFineCategories();
			ytreeviewFineCategory.YTreeModel.EmitModelChanged();
		}
	}
}

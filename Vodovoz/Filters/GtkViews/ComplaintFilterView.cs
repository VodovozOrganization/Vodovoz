using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Views.GtkUI;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Complaints;
using Vodovoz.Filters.ViewModels;
using Vodovoz.FilterViewModels;
using Vodovoz.JournalFilters;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModel;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ComplaintFilterView : FilterViewBase<ComplaintFilterViewModel>
	{
		public ComplaintFilterView(ComplaintFilterViewModel filterViewModel) : base(filterViewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			entryreferencevmEmployee.RepresentationModel = new EmployeesVM(new EmployeeRepresentationFilterViewModel());
			entryreferencevmEmployee.Binding.AddBinding(ViewModel, x => x.Employee, v => v.Subject).InitializeFromSource();
			
			entryCounterparty.SetEntityAutocompleteSelectorFactory(ViewModel.CounterpartySelectorFactory);
			entryCounterparty.Binding.AddBinding(ViewModel, x => x.Counterparty, v => v.Subject).InitializeFromSource();

			yenumcomboboxType.ItemsEnum = typeof(ComplaintType);
			yenumcomboboxType.Binding.AddBinding(ViewModel, x => x.ComplaintType, v => v.SelectedItemOrNull).InitializeFromSource();

			yenumcomboboxStatus.ItemsEnum = typeof(ComplaintStatuses);
			yenumcomboboxStatus.Binding.AddBinding(ViewModel, x => x.ComplaintStatus, v => v.SelectedItemOrNull).InitializeFromSource();

			yenumcomboboxCurrentSubdivisionStatus.ItemsEnum = typeof(ComplaintStatuses);
			yenumcomboboxCurrentSubdivisionStatus.Binding.AddBinding(ViewModel, x => x.ComplaintDiscussionStatus, v => v.SelectedItemOrNull).InitializeFromSource();
			ylabelEmployeeSubdivisionStatus.Text = $"Статус в отделе {ViewModel.CurrentUserSubdivision?.ShortName}:";

			cmbComplaintKind.SetRenderTextFunc<ComplaintKind>(k => k.GetFullName);
			cmbComplaintKind.Binding.AddBinding(ViewModel, vm => vm.ComplaintKindSource, w => w.ItemsList).InitializeFromSource();
			cmbComplaintKind.Binding.AddBinding(ViewModel, vm => vm.ComplaintKind, w => w.SelectedItem).InitializeFromSource();

			yspeccomboboxComplaintObject.ShowSpecialStateAll = true;
			yspeccomboboxComplaintObject.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.ComplaintObjectSource, w => w.ItemsList)
				.AddBinding(ViewModel, vm => vm.ComplaintObject, w => w.SelectedItem).InitializeFromSource();

			yentryreferenceSubdivision.SubjectType = typeof(Subdivision);
			yentryreferenceSubdivision.Binding.AddBinding(ViewModel, x => x.Subdivision, w => w.Subject).InitializeFromSource();

			daterangepicker.Binding.AddBinding(ViewModel, x => x.StartDate, w => w.StartDateOrNull).InitializeFromSource();
			daterangepicker.Binding.AddBinding(ViewModel, x => x.EndDate, w => w.EndDateOrNull).InitializeFromSource();

			yenumcomboboxDateType.ItemsEnum = typeof(DateFilterType);
			yenumcomboboxDateType.Binding.AddBinding(ViewModel, x => x.FilterDateType, w => w.SelectedItem).InitializeFromSource();

			ybuttonMyComplaint.Clicked += (sender, e) => ViewModel.SelectMyComplaint();

			guiltyItemView.ViewModel = ViewModel.GuiltyItemVM;
		}
	}
}

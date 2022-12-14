using QS.Views.GtkUI;
using Vodovoz.Domain.Complaints;
using Vodovoz.FilterViewModels;

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
			evmeAuthor.SetEntityAutocompleteSelectorFactory(ViewModel.EmployeeSelectorFactory);
			evmeAuthor.Binding.AddBinding(ViewModel, x => x.Employee, v => v.Subject).InitializeFromSource();
			
			entryCounterparty.SetEntityAutocompleteSelectorFactory(ViewModel.CounterpartySelectorFactory);
			entryCounterparty.Binding.AddBinding(ViewModel, x => x.Counterparty, v => v.Subject).InitializeFromSource();

			yenumcomboboxType.ItemsEnum = typeof(ComplaintType);
			yenumcomboboxType.Binding.AddBinding(ViewModel, x => x.ComplaintType, v => v.SelectedItemOrNull).InitializeFromSource();

			yenumcomboboxStatus.ItemsEnum = typeof(ComplaintStatuses);
			yenumcomboboxStatus.Binding.AddBinding(ViewModel, x => x.ComplaintStatus, v => v.SelectedItemOrNull).InitializeFromSource();

			yenumcomboboxCurrentSubdivisionStatus.ItemsEnum = typeof(ComplaintDiscussionStatuses);
			yenumcomboboxCurrentSubdivisionStatus.Binding.AddBinding(ViewModel, x => x.ComplaintDiscussionStatus, v => v.SelectedItemOrNull).InitializeFromSource();

			cmbComplaintKind.SetRenderTextFunc<ComplaintKind>(k => k.GetFullName);
			cmbComplaintKind.Binding.AddBinding(ViewModel, vm => vm.ComplaintKindSource, w => w.ItemsList).InitializeFromSource();
			cmbComplaintKind.Binding.AddBinding(ViewModel, vm => vm.ComplaintKind, w => w.SelectedItem).InitializeFromSource();

			yspeccomboboxComplaintObject.ShowSpecialStateAll = true;
			yspeccomboboxComplaintObject.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.ComplaintObjectSource, w => w.ItemsList)
				.AddBinding(ViewModel, vm => vm.ComplaintObject, w => w.SelectedItem).InitializeFromSource();

			//FIXME заменить на evme когда будут новые журналы с рекурсией
			yCmbCurrentSubdivision.ItemsList = ViewModel.AllDepartments;
			yCmbCurrentSubdivision.Binding.AddBinding(ViewModel, s => s.CurrentUserSubdivision, w => w.SelectedItem).InitializeFromSource();
			yCmbCurrentSubdivision.Binding.AddBinding(ViewModel, vm => vm.CanChangeSubdivision, w => w.Sensitive).InitializeFromSource();
			yCmbCurrentSubdivision.SetSizeRequest(250, 30);

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

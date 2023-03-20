using QS.Views.GtkUI;
using Vodovoz.Domain.Complaints;
using Vodovoz.FilterViewModels;
using static Vodovoz.FilterViewModels.ComplaintFilterViewModel;

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

			//cmbComplaintKind.SetRenderTextFunc<ComplaintKind>(k => k.GetFullName);
			//cmbComplaintKind.Binding.AddBinding(ViewModel, vm => vm.ComplaintKindSource, w => w.ItemsList).InitializeFromSource();
			//cmbComplaintKind.Binding.AddBinding(ViewModel, vm => vm.ComplaintKind, w => w.SelectedItem).InitializeFromSource();

			entityentryComplaintKind.SetEntityAutocompleteSelectorFactory(ViewModel.ComplaintKindSelectorFactory);
			entityentryComplaintKind.Binding.AddBinding(ViewModel, vm => vm.ComplaintKind, w => w.Subject).InitializeFromSource();

			yspeccomboboxComplaintObject.ShowSpecialStateAll = true;
			yspeccomboboxComplaintObject.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.ComplaintObjectSource, w => w.ItemsList)
				.AddBinding(ViewModel, vm => vm.ComplaintObject, w => w.SelectedItem).InitializeFromSource();

			entryComplaintDetalization.ViewModel = ViewModel.ComplaintDetalizationEntiryEntryViewModel;
			entryComplaintDetalization.Binding.AddBinding(ViewModel, vm => vm.CanReadDetalization, w => w.ViewModel.IsEditable)
				.InitializeFromSource();

			//FIXME заменить на evme когда будут новые журналы с рекурсией
			//yCmbCurrentSubdivision.ItemsList = ViewModel.AllDepartments;
			//yCmbCurrentSubdivision.Binding.AddBinding(ViewModel, s => s.CurrentUserSubdivision, w => w.SelectedItem).InitializeFromSource();
			//yCmbCurrentSubdivision.Binding.AddBinding(ViewModel, vm => vm.CanChangeSubdivision, w => w.Sensitive).InitializeFromSource();
			//yCmbCurrentSubdivision.SetSizeRequest(250, 30);

			entityentryCurrentSubdivision.SetEntityAutocompleteSelectorFactory(ViewModel.CurrentSubdivisionSelectorFactory);
			entityentryCurrentSubdivision.Binding.AddBinding(ViewModel, vm => vm.CurrentUserSubdivision, w => w.Subject).InitializeFromSource();
			entityentryCurrentSubdivision.Binding.AddBinding(ViewModel, vm => vm.CanChangeSubdivision, w => w.Sensitive).InitializeFromSource();

			yentryreferenceSubdivision.SubjectType = typeof(Subdivision);
			yentryreferenceSubdivision.Binding.AddBinding(ViewModel, x => x.Subdivision, w => w.Subject).InitializeFromSource();

			daterangepicker.Binding
				.AddSource(ViewModel)
				.AddBinding(x => x.StartDate, w => w.StartDateOrNull)
				.AddBinding(x => x.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			yenumcomboboxDateType.ItemsEnum = typeof(DateFilterType);
			yenumcomboboxDateType.Binding.AddBinding(ViewModel, x => x.FilterDateType, w => w.SelectedItem).InitializeFromSource();

			ybuttonMyComplaint.Clicked += (sender, e) => ViewModel.SelectMyComplaint();

			guiltyItemView.ViewModel = ViewModel.GuiltyItemVM;
		}

		public override void Destroy()
		{
			yenumcomboboxType.Destroy();
			yenumcomboboxStatus.Destroy();
			yenumcomboboxCurrentSubdivisionStatus.Destroy();
			cmbComplaintKind.Destroy();
			yspeccomboboxComplaintObject.Destroy();
			yCmbCurrentSubdivision.Destroy();
			yenumcomboboxDateType.Destroy();
			guiltyItemView.Destroy();
			base.Destroy();
		}
	}
}

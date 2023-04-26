﻿using QS.Views.GtkUI;
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
			evmeAuthor.CanOpenWithoutTabParent = true;
			
			entryCounterparty.SetEntityAutocompleteSelectorFactory(ViewModel.CounterpartySelectorFactory);
			entryCounterparty.Binding.AddBinding(ViewModel, x => x.Counterparty, v => v.Subject).InitializeFromSource();
			entryCounterparty.CanOpenWithoutTabParent = true;

			yenumcomboboxType.ItemsEnum = typeof(ComplaintType);
			yenumcomboboxType.Binding.AddBinding(ViewModel, x => x.ComplaintType, v => v.SelectedItemOrNull).InitializeFromSource();

			yenumcomboboxStatus.ItemsEnum = typeof(ComplaintStatuses);
			yenumcomboboxStatus.Binding.AddBinding(ViewModel, x => x.ComplaintStatus, v => v.SelectedItemOrNull).InitializeFromSource();

			yenumcomboboxCurrentSubdivisionStatus.ItemsEnum = typeof(ComplaintDiscussionStatuses);
			yenumcomboboxCurrentSubdivisionStatus.Binding.AddBinding(ViewModel, x => x.ComplaintDiscussionStatus, v => v.SelectedItemOrNull).InitializeFromSource();

			entityentryComplaintKind.SetEntityAutocompleteSelectorFactory(ViewModel.ComplaintKindSelectorFactory);
			entityentryComplaintKind.Binding.AddBinding(ViewModel, vm => vm.ComplaintKind, w => w.Subject).InitializeFromSource();
			entityentryComplaintKind.CanOpenWithoutTabParent = true;

			yspeccomboboxComplaintObject.ShowSpecialStateAll = true;
			yspeccomboboxComplaintObject.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.ComplaintObjectSource, w => w.ItemsList)
				.AddBinding(ViewModel, vm => vm.ComplaintObject, w => w.SelectedItem).InitializeFromSource();

			entryComplaintDetalization.ViewModel = ViewModel.ComplaintDetalizationEntiryEntryViewModel;
			entryComplaintDetalization.Binding.AddBinding(ViewModel, vm => vm.CanReadDetalization, w => w.ViewModel.IsEditable)
				.InitializeFromSource();

			entityentryCurrentSubdivision.SetEntityAutocompleteSelectorFactory(ViewModel.CurrentSubdivisionSelectorFactory);
			entityentryCurrentSubdivision.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CurrentUserSubdivision, w => w.Subject)
				.AddBinding(vm => vm.CanChangeSubdivision, w => w.Sensitive)
				.InitializeFromSource();
			entityentryCurrentSubdivision.CanOpenWithoutTabParent = true;

			entityentryInWorkSubdivision.SetEntityAutocompleteSelectorFactory(ViewModel.InWorkSubdivisionSelectorFactory);
			entityentryInWorkSubdivision.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Subdivision, w => w.Subject)
				.InitializeFromSource();
			entityentryInWorkSubdivision.CanOpenWithoutTabParent = true;


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
			entityentryComplaintKind.Destroy();
			yspeccomboboxComplaintObject.Destroy();
			entityentryCurrentSubdivision.Destroy();
			yenumcomboboxDateType.Destroy();
			guiltyItemView.Destroy();
			base.Destroy();
		}
	}
}

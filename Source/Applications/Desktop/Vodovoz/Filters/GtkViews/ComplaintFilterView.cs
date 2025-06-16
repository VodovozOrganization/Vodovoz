using QS.ViewModels.Control.EEVM;
using QS.Views.GtkUI;
using Vodovoz.Core.Domain.Complaints;
using Vodovoz.Domain.Complaints;
using Vodovoz.FilterViewModels;
using Vodovoz.JournalViewModels;
using static Vodovoz.FilterViewModels.ComplaintFilterViewModel;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ComplaintFilterView : FilterViewBase<ComplaintFilterViewModel>
	{
		public ComplaintFilterView(ComplaintFilterViewModel filterViewModel) : base(filterViewModel)
		{
			Build();
			Initialize();
		}

		private void Initialize()
		{
			yenumcomboboxType.ItemsEnum = typeof(ComplaintType);
			yenumcomboboxType.Binding.AddBinding(ViewModel, x => x.ComplaintType, v => v.SelectedItemOrNull).InitializeFromSource();

			yenumcomboboxStatus.ItemsEnum = typeof(ComplaintStatuses);
			yenumcomboboxStatus.Binding.AddBinding(ViewModel, x => x.ComplaintStatus, v => v.SelectedItemOrNull).InitializeFromSource();

			yenumcomboboxCurrentSubdivisionStatus.ItemsEnum = typeof(ComplaintDiscussionStatuses);
			yenumcomboboxCurrentSubdivisionStatus.Binding.AddBinding(ViewModel, x => x.ComplaintDiscussionStatus, v => v.SelectedItemOrNull).InitializeFromSource();

			entryComplaintKind.ViewModel = ViewModel.ComplaintKindViewModel;

			yspeccomboboxComplaintObject.ShowSpecialStateAll = true;
			yspeccomboboxComplaintObject.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.ComplaintObjectSource, w => w.ItemsList)
				.AddBinding(ViewModel, vm => vm.ComplaintObject, w => w.SelectedItem).InitializeFromSource();

			entryComplaintDetalization.ViewModel = ViewModel.ComplaintDetalizationEntiryEntryViewModel;
			entryComplaintDetalization.Binding.AddBinding(ViewModel, vm => vm.CanReadDetalization, w => w.ViewModel.IsEditable)
				.InitializeFromSource();

			entryCurrentSubdivision.ViewModel = ViewModel.CurrentSubdivisionViewModel;
			entryAtWorkInSubdivision.ViewModel = ViewModel.AtWorkInSubdivisionViewModel;
			entityentryAuthor.ViewModel = ViewModel.AuthorEntiryEntryViewModel;
			ConfigureCounterpartyEntityEntry();

			daterangepicker.Binding
				.AddSource(ViewModel)
				.AddBinding(x => x.StartDate, w => w.StartDateOrNull)
				.AddBinding(x => x.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			yenumcomboboxDateType.ItemsEnum = typeof(DateFilterType);
			yenumcomboboxDateType.Binding.AddBinding(ViewModel, x => x.FilterDateType, w => w.SelectedItem).InitializeFromSource();

			ybuttonMyComplaint.Clicked += (sender, e) => ViewModel.SelectMyComplaint();

			guiltyItemView.ViewModel = ViewModel.GuiltyItemVM;

			ybtnNumberOfComplaintsAgainstDriversReport.Clicked += (s, e) => ViewModel.OpenNumberOfComplaintsAgainstDriversReportTabCommand.Execute();
		}

		private void ConfigureCounterpartyEntityEntry()
		{
			var builder = new LegacyEEVMBuilderFactory<ComplaintFilterViewModel>(
				ViewModel.JournalTab,
				ViewModel,
				ViewModel.UoW,
				ViewModel.NavigationManager,
				ViewModel.LifetimeScope);

			entityentryCounterparty.ViewModel = builder.ForProperty(x => x.Counterparty)
				.UseTdiEntityDialog()
				.UseViewModelJournalAndAutocompleter<CounterpartyJournalViewModel>()
				.Finish();
			entityentryCounterparty.ViewModel.DisposeViewModel = false;
		}

		public override void Destroy()
		{
			yenumcomboboxType.Destroy();
			yenumcomboboxStatus.Destroy();
			yenumcomboboxCurrentSubdivisionStatus.Destroy();
			yspeccomboboxComplaintObject.Destroy();
			yenumcomboboxDateType.Destroy();
			guiltyItemView.Destroy();
			base.Destroy();
		}
	}
}

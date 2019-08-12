using Gamma.ColumnConfig;
using QS.DomainModel.Config;
using QS.Project.Journal.EntitySelector;
using QS.Views.GtkUI;
using QSProjectsLib;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Complaints;

namespace Vodovoz.Views.Complaints
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ComplaintView : TabViewBase<ComplaintViewModel>
	{
		public ComplaintView(ComplaintViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			ylabelSubdivisions.Binding.AddBinding(ViewModel, vm => vm.SubdivisionsInWork, w => w.LabelProp).InitializeFromSource();
			ylabelCreatedBy.Binding.AddBinding(ViewModel, e => e.CreatedByAndDate, w => w.LabelProp).InitializeFromSource();
			ylabelChangedBy.Binding.AddBinding(ViewModel, e => e.ChangedByAndDate, w => w.LabelProp).InitializeFromSource();

			yentryName.Binding.AddBinding(ViewModel.Entity, e => e.ComplainantName, w => w.Text).InitializeFromSource();
			yentryName.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			yentryName.Binding.AddBinding(ViewModel, vm => vm.IsClientComplaint, w => w.Visible).InitializeFromSource();
			labelName.Binding.AddBinding(ViewModel, vm => vm.IsClientComplaint, w => w.Visible).InitializeFromSource();

			yenumcomboStatus.ItemsEnum = typeof(ComplaintStatuses);
			yenumcomboStatus.Binding.AddBinding(ViewModel, e => e.Status, w => w.SelectedItem).InitializeFromSource();
			yenumcomboStatus.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			ydatepickerPlannedCompletionDate.Binding.AddBinding(ViewModel.Entity, e => e.PlannedCompletionDate, w => w.Date).InitializeFromSource();
			ydatepickerPlannedCompletionDate.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			entryCounterparty.SetEntityAutocompleteSelectorFactory(ViewModel.CounterpartySelectorFactory);
			entryCounterparty.Binding.AddBinding(ViewModel.Entity, e => e.Counterparty, w => w.Subject).InitializeFromSource();
			entryCounterparty.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			entryCounterparty.Binding.AddBinding(ViewModel, vm => vm.IsClientComplaint, w => w.Visible).InitializeFromSource();
			labelCounterparty.Binding.AddBinding(ViewModel, vm => vm.IsClientComplaint, w => w.Visible).InitializeFromSource();

			var orderSelectorFactory = new EntityAutocompleteSelectorFactory<OrderJournalViewModel>(typeof(Order), () => {
				var filter = new OrderJournalFilterViewModel(ServicesConfig.InteractiveService);
				if(ViewModel.Entity.Counterparty != null) {
					filter.RestrictCounterparty = ViewModel.Entity.Counterparty;
				}
				return new OrderJournalViewModel(filter, new DefaultEntityConfigurationProvider(), ServicesConfig.CommonServices);
			});

			entryOrder.SetEntityAutocompleteSelectorFactory(orderSelectorFactory);
			entryOrder.Binding.AddBinding(ViewModel.Entity, e => e.Order, w => w.Subject).InitializeFromSource();
			entryOrder.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			entryOrder.Binding.AddBinding(ViewModel, vm => vm.IsClientComplaint, w => w.Visible).InitializeFromSource();
			labelOrder.Binding.AddBinding(ViewModel, vm => vm.IsClientComplaint, w => w.Visible).InitializeFromSource();

			yentryPhone.Binding.AddBinding(ViewModel.Entity, e => e.Phone, w => w.Text).InitializeFromSource();
			yentryPhone.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			yentryPhone.Binding.AddBinding(ViewModel, vm => vm.IsClientComplaint, w => w.Visible).InitializeFromSource();
			labelNamePhone.Binding.AddBinding(ViewModel, vm => vm.IsClientComplaint, w => w.Visible).InitializeFromSource();

			comboboxComplaintSource.SetRenderTextFunc<ComplaintSource>(x => x.Name);
			comboboxComplaintSource.Binding.AddBinding(ViewModel, vm => vm.ComplaintSources, w => w.ItemsList).InitializeFromSource();
			comboboxComplaintSource.Binding.AddBinding(ViewModel.Entity, e => e.ComplaintSource, w => w.SelectedItem).InitializeFromSource();
			comboboxComplaintSource.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			comboboxComplaintSource.Binding.AddBinding(ViewModel, vm => vm.IsClientComplaint, w => w.Visible).InitializeFromSource();
			labelSource.Binding.AddBinding(ViewModel, vm => vm.IsClientComplaint, w => w.Visible).InitializeFromSource();

			comboboxComplaintResult.SetRenderTextFunc<ComplaintResult>(x => x.Name);
			comboboxComplaintResult.Binding.AddBinding(ViewModel, vm => vm.ComplaintResults, w => w.ItemsList).InitializeFromSource();
			comboboxComplaintResult.Binding.AddBinding(ViewModel.Entity, e => e.ComplaintResult, w => w.SelectedItem).InitializeFromSource();
			comboboxComplaintResult.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			ytextviewResultText.Binding.AddBinding(ViewModel.Entity, e => e.ResultText, w => w.Buffer.Text).InitializeFromSource();
			ytextviewResultText.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			filesview.ViewModel = ViewModel.FilesViewModel;
			filesview.Sensitive = ViewModel.CanEdit;

			ytextviewComplaintText.Binding.AddBinding(ViewModel.Entity, e => e.ComplaintText, w => w.Buffer.Text).InitializeFromSource();
			ytextviewComplaintText.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			guiltyitemsview.ViewModel = ViewModel.GuiltyItemsViewModel;

			vboxDicussions.Add(new ComplaintDiscussionsView(ViewModel.DiscussionsViewModel));
			vboxDicussions.ShowAll();

			ytreeviewFines.ColumnsConfig = FluentColumnsConfig<FineItem>.Create()
				.AddColumn("№").AddTextRenderer(x => x.Fine.Id.ToString())
				.AddColumn("Сотрудник").AddTextRenderer(x => x.Employee.ShortName)
				.AddColumn("Сумма штрафа").AddTextRenderer(x => CurrencyWorks.GetShortCurrencyString(x.Money))
				.Finish();
			ytreeviewFines.Binding.AddBinding(ViewModel, vm => vm.FineItems, w => w.ItemsDataSource).InitializeFromSource();

			buttonAddFine.Clicked += (sender, e) => { ViewModel.AddFineCommand.Execute(); };
			buttonAddFine.Binding.AddBinding(ViewModel, vm => vm.CanAddFine, w => w.Sensitive).InitializeFromSource();

			buttonAttachFine.Clicked += (sender, e) => { ViewModel.AttachFineCommand.Execute(); };
			buttonAttachFine.Binding.AddBinding(ViewModel, vm => vm.CanAttachFine, w => w.Sensitive).InitializeFromSource();

			buttonSave.Clicked += (sender, e) => { ViewModel.SaveAndClose(); };
			buttonCancel.Clicked += (sender, e) => { ViewModel.Close(false); };
		}
	}
}

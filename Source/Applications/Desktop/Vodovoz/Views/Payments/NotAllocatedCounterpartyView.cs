using System.ComponentModel;
using QS.Views;
using Vodovoz.ViewModels.ViewModels.Payments;

namespace Vodovoz.Views.Payments
{
	[ToolboxItem(true)]
	public partial class NotAllocatedCounterpartyView : ViewBase<NotAllocatedCounterpartyViewModel>
	{
		public NotAllocatedCounterpartyView(NotAllocatedCounterpartyViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			btnSave.BindCommand(ViewModel.SaveCommand);
			btnCancel.BindCommand(ViewModel.CancelCommand);

			lblIdTitle.Binding
				.AddBinding(ViewModel, vm => vm.CanShowId, w => w.Visible)
				.InitializeFromSource();

			lblId.Selectable = true;
			lblId.Binding
				.AddBinding(ViewModel, vm => vm.CanShowId, w => w.Visible)
				.AddBinding(ViewModel, vm => vm.IdString, w => w.LabelProp)
				.InitializeFromSource();

			validatedINN.Binding
				.AddBinding(ViewModel.Entity, e => e.Inn, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.IsEditable)
				.InitializeFromSource();

			entryName.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.IsEditable)
				.InitializeFromSource();

			chkIsArchive.Binding
				.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active)
				.InitializeFromSource();

			ConfigureEntityEntry();
		}

		private void ConfigureEntityEntry()
		{
			profitCategoryEntry.ViewModel = ViewModel.ProfitCategoryEntryViewModel;
		}
	}
}

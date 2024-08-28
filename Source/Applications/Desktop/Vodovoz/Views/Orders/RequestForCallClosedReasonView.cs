using QS.Views.Dialog;
using Vodovoz.ViewModels.ViewModels.Orders;

namespace Vodovoz.Views.Orders
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RequestForCallClosedReasonView : DialogViewBase<RequestForCallClosedReasonViewModel>
	{
		public RequestForCallClosedReasonView(RequestForCallClosedReasonViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			btnSave.BindCommand(ViewModel.SaveAndCloseCommand);
			btnCancel.BindCommand(ViewModel.CloseCommand);

			lblIdTitle.Binding
				.AddBinding(ViewModel, vm => vm.CanShowId, w => w.Visible)
				.InitializeFromSource();

			lblId.Selectable = true;
			lblId.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanShowId, w => w.Visible)
				.AddBinding(vm => vm.IdToString, w => w.LabelProp)
				.InitializeFromSource();

			chkIsArchive.Binding
				.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			entryReason.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanChangeName, w => w.IsEditable)
				.InitializeFromSource();
		}
	}
}

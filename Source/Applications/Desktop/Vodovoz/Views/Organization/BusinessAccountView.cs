using System;
using QS.Views.Dialog;
using Vodovoz.Presentation.ViewModels.Organisations;

namespace Vodovoz.Views.Organization
{
	public partial class BusinessAccountView : DialogViewBase<BusinessAccountViewModel>
	{
		public BusinessAccountView(BusinessAccountViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			btnSave.BindCommand(ViewModel.SaveCommand);
			btnCancel.BindCommand(ViewModel.CancelCommand);

			lblIdTitle.Binding
				.AddBinding(ViewModel, e => e.CanShowId, w => w.Visible)
				.InitializeFromSource();

			lblId.Binding
				.AddBinding(ViewModel, e => e.IdString, w => w.LabelProp)
				.AddBinding(ViewModel, vm => vm.CanShowId, w => w.Visible)
				.InitializeFromSource();

			entryName.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.IsEditable)
				.InitializeFromSource();

			entryNumber.Binding
				.AddBinding(ViewModel.Entity, e => e.Number, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.IsEditable)
				.InitializeFromSource();

			entryBank.Binding
				.AddBinding(ViewModel.Entity, e => e.Bank, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.IsEditable)
				.InitializeFromSource();

			lblAccountFillTypeTitle.Binding
				.AddBinding(ViewModel, vm => vm.CanShowAccountFillType, w => w.Visible)
				.InitializeFromSource();

			enumСmbAccountFillType.Binding
				.AddBinding(ViewModel.Entity, e => e.AccountFillType, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.CanShowAccountFillType, w => w.Visible)
				.InitializeFromSource();

			entryBusinessActivity.ViewModel = ViewModel.BusinessActivityViewModel;
			entryFunds.ViewModel = ViewModel.FundsViewModel;
		}
	}
}

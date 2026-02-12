using System;
using QS.Views.GtkUI;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.ViewModels.Orders;

namespace Vodovoz.Views.Orders
{
	public partial class UndeliveryDetalizationView : TabViewBase<UndeliveryDetalizationViewModel>
	{
		public UndeliveryDetalizationView(UndeliveryDetalizationViewModel viewModel)
			: base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			yentryName.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			cmbUndeliveryObject.ShowSpecialStateNot = true;
			cmbUndeliveryObject.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.UndeliveryObjects, w => w.ItemsList)
				.AddBinding(vm => vm.SelectedUndeliveryObject, w => w.SelectedItem)
				.AddBinding(vm => vm.CanChangeUndeliveryObject, w => w.Sensitive)
				.InitializeFromSource();

			cmbUndeliveryKind.ShowSpecialStateNot = true;
			cmbUndeliveryKind.SetRenderTextFunc<UndeliveryKind>(k => k.GetFullName);
			cmbUndeliveryKind.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.VisibleUndeliveryKinds, w => w.ItemsList)
				.AddBinding(vm => vm.CanChangeUndeliveryKind, w => w.Sensitive)
				.InitializeFromSource();

			cmbUndeliveryKind.Binding
				.AddBinding(ViewModel.Entity, e => e.UndeliveryKind, w => w.SelectedItem)
				.InitializeFromSource();

			chkIsArchive.Binding
				.AddBinding(ViewModel.Entity, vm => vm.IsArchive, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			buttonSave.Binding
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			buttonSave.Clicked += OnButtonSaveClicked;
			buttonCancel.Clicked += OnButtonCancelClicked;

			ylabelClientNotificationText.Visible = false;
			yentryClientNotificationText.Visible = false;
		}

		private void OnButtonSaveClicked(object sender, EventArgs e)
		{
			ViewModel.SaveAndClose();
		}

		private void OnButtonCancelClicked(object sender, EventArgs e)
		{
			ViewModel.Close(true, QS.Navigation.CloseSource.Cancel);
		}

		public override void Dispose()
		{
			buttonSave.Clicked -= OnButtonSaveClicked;
			buttonCancel.Clicked -= OnButtonCancelClicked;
			base.Dispose();
		}
	}
}

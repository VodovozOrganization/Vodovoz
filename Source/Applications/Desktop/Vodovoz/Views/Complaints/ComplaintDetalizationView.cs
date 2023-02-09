using QS.Views.GtkUI;
using System;
using Vodovoz.ViewModels.ViewModels.Complaints;

namespace Vodovoz.Views.Complaints
{
	public partial class ComplaintDetalizationView : TabViewBase<ComplaintDetalizationViewModel>
	{
		public ComplaintDetalizationView(ComplaintDetalizationViewModel viewModel)
			: base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			yentryName.Binding.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.InitializeFromSource();

			yspeccomboboxComplaintObject.ShowSpecialStateNot = true;
			yspeccomboboxComplaintObject.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.ComplaintObjects, w => w.ItemsList)
				.AddBinding(vm => vm.SelectedComplainObject, w => w.SelectedItem)
				.AddBinding(vm => vm.CanChangeComplaintObject, w => w.Sensitive)
				.InitializeFromSource();

			yspeccomboboxComplaintKind.ShowSpecialStateNot = true;
			yspeccomboboxComplaintKind.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.VisibleComplaintKinds, w => w.ItemsList)
				.AddBinding(vm => vm.CanChangeComplaintKind, w => w.Sensitive)
				.InitializeFromSource();

			yspeccomboboxComplaintKind.Binding
				.AddBinding(ViewModel.Entity, e => e.ComplaintKind, w => w.SelectedItem)
				.InitializeFromSource();

			chkIsArchive.Binding.AddBinding(ViewModel.Entity, vm => vm.IsArchive, w => w.Active)
				.InitializeFromSource();

			buttonSave.Clicked += ButtonSaveClicked;
			buttonCancel.Clicked += ButtonCancelClicked;
		}

		private void ButtonSaveClicked(object sender, EventArgs e)
		{
			ViewModel.SaveAndClose();
		}

		private void ButtonCancelClicked(object sender, EventArgs e)
		{
			ViewModel.Close(true, QS.Navigation.CloseSource.Cancel);
		}

		public override void Dispose()
		{
			buttonSave.Clicked -= ButtonSaveClicked;
			buttonCancel.Clicked -= ButtonCancelClicked;
			base.Dispose();
		}
	}
}

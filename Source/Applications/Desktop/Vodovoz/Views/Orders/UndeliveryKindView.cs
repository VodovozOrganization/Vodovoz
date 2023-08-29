using QS.Views.GtkUI;
using System;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.ViewModels.Orders;

namespace Vodovoz.Views.Orders
{
	public partial class UndeliveryKindView : TabViewBase<UndeliveryKindViewModel>
	{
		public UndeliveryKindView(UndeliveryKindViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			yentryName.Binding.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.InitializeFromSource();

			yspeccomboboxUndeliveryObject.ShowSpecialStateNot = true;
			yspeccomboboxUndeliveryObject.Binding
				.AddBinding(ViewModel, vm => vm.UndeliveryObjects, w => w.ItemsList)
				.AddBinding(ViewModel.Entity, e => e.UndeliveryObject, w => w.SelectedItem)
				.InitializeFromSource();

			chkIsArchive.Binding.AddBinding(ViewModel.Entity, vm => vm.IsArchive, w => w.Active)
				.InitializeFromSource();

			buttonSave.Clicked += OnButtonSaveClicked;
			buttonCancel.Clicked += OnButtonCancelClicked;
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

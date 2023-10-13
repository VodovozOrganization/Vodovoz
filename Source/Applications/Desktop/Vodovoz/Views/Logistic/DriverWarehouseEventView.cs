using System;
using QS.Navigation;
using QS.Views.GtkUI;
using Vodovoz.Domain.Logistic.Drivers;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.Views.Logistic
{
	public partial class DriverWarehouseEventView : TabViewBase<DriverWarehouseEventViewModel>
	{
		public DriverWarehouseEventView(DriverWarehouseEventViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			btnSave.Clicked += OnSaveClicked;
			btnCancel.Clicked += OnCancelClicked;

			lblIdTitle.Binding
				.AddBinding(ViewModel, vm => vm.IdGtZero, w => w.Visible)
				.InitializeFromSource();

			lblId.Selectable = true;
			lblId.Binding
				.AddBinding(ViewModel, vm => vm.IdGtZero, w => w.Visible)
				.InitializeFromSource();

			entityEvent.ViewModel = ViewModel.DriverWarehouseEventNameViewModel;

			enumCmbType.ItemsEnum = typeof(DriverWarehouseEventType);
			enumCmbType.DefaultFirst = true;
			enumCmbType.Binding
				.AddBinding(ViewModel.Entity, e => e.Type, w => w.SelectedItem)
				.InitializeFromSource();
		}

		private void OnSaveClicked(object sender, EventArgs e)
		{
			ViewModel.SaveAndClose();
		}

		private void OnCancelClicked(object sender, EventArgs e)
		{
			ViewModel.Close(false, CloseSource.Cancel);
		}
	}
}

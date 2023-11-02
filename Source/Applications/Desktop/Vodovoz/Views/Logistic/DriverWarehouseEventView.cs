using System;
using QS.Navigation;
using QS.Views.GtkUI;
using Vodovoz.Domain.Logistic.Drivers;
using Vodovoz.Infrastructure.Converters;
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
			btnQrCode.Clicked += OnQrCodeClicked;
			btn.Clicked += OnCopyFromClipboard;

			lblIdTitle.Binding
				.AddBinding(ViewModel, vm => vm.IdGtZero, w => w.Visible)
				.InitializeFromSource();

			lblId.Selectable = true;
			lblId.Binding
				.AddBinding(ViewModel, vm => vm.IdGtZero, w => w.Visible)
				.AddBinding(ViewModel.Entity, e => e.Id, w => w.Text, new IntToStringConverter())
				.InitializeFromSource();

			entityEvent.ViewModel = ViewModel.DriverWarehouseEventNameViewModel;
			
			lblLatitude.Binding
				.AddBinding(ViewModel, vm => vm.IsCoordinatesVisible, w => w.Visible)
				.InitializeFromSource();
			
			spinBtnLatitude.Digits = 6;
			spinBtnLatitude.Binding
				.AddBinding(ViewModel.Entity, e => e.Latitude, w => w.ValueAsDecimal)
				.AddBinding(ViewModel, vm => vm.IsCoordinatesVisible, w => w.Visible)
				.InitializeFromSource();
			
			lblLongitude.Binding
				.AddBinding(ViewModel, vm => vm.IsCoordinatesVisible, w => w.Visible)
				.InitializeFromSource();
			
			spinBtnLongitude.Digits = 6;
			spinBtnLongitude.Binding
				.AddBinding(ViewModel.Entity, e => e.Longitude, w => w.ValueAsDecimal)
				.AddBinding(ViewModel, vm => vm.IsCoordinatesVisible, w => w.Visible)
				.InitializeFromSource();
			
			btn.Binding
				.AddBinding(ViewModel, vm => vm.IsCoordinatesVisible, w => w.Visible)
				.InitializeFromSource();

			enumCmbType.ItemsEnum = typeof(DriverWarehouseEventType);
			enumCmbType.DefaultFirst = true;
			enumCmbType.Binding
				.AddBinding(ViewModel, vm => vm.EventType, w => w.SelectedItem)
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
		
		private void OnQrCodeClicked(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}
		
		private void OnCopyFromClipboard(object sender, EventArgs e)
		{
			ViewModel.SetCoordinatesFromBufferCommand.Execute(GetClipboard(null).WaitForText());
		}
	}
}

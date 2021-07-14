using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Complaints;

namespace Vodovoz.Views.Complaints
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DriverComplaintReasonView : TabViewBase<DriverComplaintReasonViewModel>
	{
		public DriverComplaintReasonView(DriverComplaintReasonViewModel driverComplaintReasonViewModel)
			: base(driverComplaintReasonViewModel)
		{
			this.Build();

			ConfigureView();
		}

		private void ConfigureView()
		{
			yentryName.Binding.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text).InitializeFromSource();
			ycheckbuttonIsPopular.Binding.AddBinding(ViewModel.Entity, e => e.IsPopular, w => w.Active).InitializeFromSource();

			buttonSave.Clicked += OnButtonSaveClicked;
			buttonCancel.Clicked += OnButtonCancelClicked;
		}

		private void OnButtonCancelClicked(object sender, EventArgs e)
		{
			ViewModel.Close(true, QS.Navigation.CloseSource.Cancel);
		}

		private void OnButtonSaveClicked(object sender, EventArgs e)
		{
			ViewModel.SaveAndClose();
		}
	}
}

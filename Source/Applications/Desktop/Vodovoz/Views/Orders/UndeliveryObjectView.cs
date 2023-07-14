using System;
using QS.Navigation;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Orders;

namespace Vodovoz.Views.Orders
{
	public partial class UndeliveryObjectView : TabViewBase<UndeliveryObjectViewModel>
	{
		public UndeliveryObjectView(UndeliveryObjectViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			yentryName.Binding.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text).InitializeFromSource();

			ycheckbuttonArchive.Binding.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active).InitializeFromSource();

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

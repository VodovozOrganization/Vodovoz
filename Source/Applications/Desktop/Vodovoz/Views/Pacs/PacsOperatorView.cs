using QS.Views.GtkUI;
using System;
using Vodovoz.Presentation.ViewModels.Pacs;

namespace Vodovoz.Views.Pacs
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PacsOperatorView : WidgetViewBase<PacsOperatorViewModel>
	{
		public PacsOperatorView()
		{
			this.Build();
		}

		protected override void ConfigureWidget()
		{
			base.ConfigureWidget();

			comboboxPhone.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.AvailablePhones, w => w.ItemsList)
				.AddBinding(vm => vm.PhoneNumber, w => w.SelectedItem)
				.InitializeFromSource();

			buttonBreak.BindCommand(ViewModel.StartBreakCommand);
			buttonEndBreak.BindCommand(ViewModel.EndBreakCommand);
			buttonChangePhone.BindCommand(ViewModel.ChangePhoneCommand);
			buttonStartWorkshift.BindCommand(ViewModel.StartWorkShiftCommand);
			buttonEndWorkshift.BindCommand(ViewModel.EndWorkShiftCommand);
		}
	}
}

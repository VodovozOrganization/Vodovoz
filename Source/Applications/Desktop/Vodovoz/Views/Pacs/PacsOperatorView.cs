using QS.Views.GtkUI;
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
			/*
			buttonBreak.Binding
				.AddBinding(ViewModel, vm => vm.CanStartBreak, w => w.Visible)
				.InitializeFromSource();

			buttonEndBreak.Binding
				.AddBinding(ViewModel, vm => vm.CanEndBreak, w => w.Visible)
				.InitializeFromSource();

			buttonStartWorkshift.Binding
				.AddBinding(ViewModel, vm => vm.CanStartWorkShift, w => w.Visible)
				.InitializeFromSource();

			buttonEndWorkshift.Binding
				.AddBinding(ViewModel, vm => vm.CanEndWorkShift, w => w.Visible)
				.InitializeFromSource();

			buttonChangePhone.Binding
				.AddBinding(ViewModel, vm => vm.CanChangePhone, w => w.Visible)
				.InitializeFromSource();*/

			labelBreakInfo.Visible = false;
			/*labelBreakInfo.Binding
				.AddBinding(ViewModel, vm => vm.CanEndBreak, w => w.Visible)
				.InitializeFromSource();*/

			treeviewOperatorsOnBreak.Sensitive = false;

			buttonBreak.BindCommand(ViewModel.StartLongBreakCommand);
			//stop! ТУТ НАДО ИСПРАВЛЯТЬ UI 
			buttonBreak.BindCommand(ViewModel.StartShortBreakCommand);
			buttonEndBreak.BindCommand(ViewModel.EndBreakCommand);
			buttonChangePhone.BindCommand(ViewModel.ChangePhoneCommand);
			buttonStartWorkshift.BindCommand(ViewModel.StartWorkShiftCommand);
			buttonEndWorkshift.BindCommand(ViewModel.EndWorkShiftCommand);
		}
	}
}

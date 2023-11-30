using QS.Views.GtkUI;
using Vodovoz.Presentation.ViewModels.Pacs;

namespace Vodovoz.Views.Pacs
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PacsDomainSettingsView : WidgetViewBase<PacsDomainSettingsViewModel>
	{
		public PacsDomainSettingsView()
		{
			this.Build();
		}

		protected override void ConfigureWidget()
		{
			base.ConfigureWidget();
			/*
			spinButtonMaxBreakDuration.Adjustment = 
				new Gtk.Adjustment(0, ViewModel.MaxBreakTimeMinValue, ViewModel.MaxBreakTimeMaxValue, 1, 1, 4);
			spinButtonMaxBreakDuration.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.MaxBreakTime, w => w.ValueAsInt)
				.AddBinding(vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			spinButtonMaxOperatorsOnBreak.Adjustment =
				new Gtk.Adjustment(0, ViewModel.MaxOperatorsOnBreakMinValue, ViewModel.MaxOperatorsOnBreakMaxValue, 1, 1, 4);
			spinButtonMaxOperatorsOnBreak.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.MaxOperatorsOnBreak, w => w.ValueAsInt)
				.AddBinding(vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			labelInfo.LabelProp = "<span color=\"red\">Настройки были кем-то изменены! \nНажмите отмена и повторите изменения.</span>";
			labelInfo.UseMarkup = true;
			labelInfo.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.HasExternalChanges, w => w.Visible)
				.InitializeFromSource ();

			buttonSave.BindCommand(ViewModel.SaveCommand);
			buttonCancel.BindCommand(ViewModel.CancelCommand);*/
		}
	}
}

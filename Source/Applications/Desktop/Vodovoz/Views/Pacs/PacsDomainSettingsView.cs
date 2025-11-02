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
			
			spinButtonLongBreakDuration.Adjustment = 
				new Gtk.Adjustment(0, ViewModel.LongBreakDurationMinValue, ViewModel.LongBreakDurationMaxValue, 1, 1, 4);
			spinButtonLongBreakDuration.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.LongBreakDuration, w => w.ValueAsInt)
				.AddBinding(vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			spinButtonOperatorsOnLongBreak.Adjustment =
				new Gtk.Adjustment(0, ViewModel.OperatorsOnLongBreakMinValue, ViewModel.OperatorsOnLongBreakMaxValue, 1, 1, 4);
			spinButtonOperatorsOnLongBreak.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.OperatorsOnLongBreak, w => w.ValueAsInt)
				.AddBinding(vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			spinButtonMaxLongBreakCount.Adjustment =
				new Gtk.Adjustment(0, ViewModel.LongBreakCountPerDayMinValue, ViewModel.LongBreakCountPerDayMaxValue, 1, 1, 4);
			spinButtonMaxLongBreakCount.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.LongBreakCountPerDay, w => w.ValueAsInt)
				.AddBinding(vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			spinButtonShortBreakDuration.Adjustment =
				new Gtk.Adjustment(0, ViewModel.ShortBreakDurationMinValue, ViewModel.ShortBreakDurationMaxValue, 1, 1, 4);
			spinButtonShortBreakDuration.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.ShortBreakDuration, w => w.ValueAsInt)
				.AddBinding(vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			spinButtonOperatorsOnShortBreak.Adjustment =
				new Gtk.Adjustment(0, ViewModel.OperatorsOnShortBreakMinValue, ViewModel.OperatorsOnShortBreakMaxValue, 1, 1, 4);
			spinButtonOperatorsOnShortBreak.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.OperatorsOnShortBreak, w => w.ValueAsInt)
				.AddBinding(vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			spinButtonShortBreakInterval.Adjustment =
				new Gtk.Adjustment(0, ViewModel.ShortBreakIntervalMinValue, ViewModel.ShortBreakIntervalMaxValue, 1, 1, 4);
			spinButtonShortBreakInterval.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.ShortBreakInterval, w => w.ValueAsInt)
				.AddBinding(vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			labelInfo.LabelProp = "<span color=\"red\">Настройки были кем-то изменены! \nНажмите отмена и повторите изменения.</span>";
			labelInfo.UseMarkup = true;
			labelInfo.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.HasExternalChanges, w => w.Visible)
				.InitializeFromSource ();

			buttonSave.BindCommand(ViewModel.SaveCommand);
			buttonCancel.BindCommand(ViewModel.CancelCommand);
		}
	}
}

using QS.Views.Dialog;
using Vodovoz.Infrastructure;
using Vodovoz.Presentation.ViewModels.Pacs;

namespace Vodovoz.Views.Pacs
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PacsWorkShiftView : DialogViewBase<WorkShiftViewModel>
	{
		public PacsWorkShiftView(WorkShiftViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			entryName.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Name, w => w.Text)
				.InitializeFromSource();

			var eventBox = new Gtk.EventBox();
			eventBox.ModifyBg(Gtk.StateType.Normal, GdkColors.DangerBase);
			eventBox.Add(entryName);
			
			entryDuration.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Duration, w => w.Time)
				.InitializeFromSource();

			buttonSave.BindCommand(ViewModel.SaveCommand);
			buttonCancel.BindCommand(ViewModel.CancelCommand);
		}
	}
}

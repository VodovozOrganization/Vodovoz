using QS.Views.GtkUI;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.Dialogs.Organizations;

namespace Vodovoz.Views.Organization
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RoboatsEntityView : WidgetViewBase<RoboatsEntityViewModel>
	{
		public RoboatsEntityView(RoboatsEntityViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			entryRoboatsFile.Binding.AddFuncBinding(ViewModel, e => e.AudioFile ?? "", w => w.Text).InitializeFromSource();
			ylabelFileWarning.UseMarkup = true;
			ylabelFileWarning.Binding.AddSource(ViewModel)
				.AddFuncBinding(vm => $"<span color=\"red\">{vm.AudioFileWarningMessage}</span>", w => w.LabelProp)
				.AddBinding(vm => vm.AudioFileWarningMessage, w => w.Visible, new NullToBooleanConverter())
				.InitializeFromSource();

			ybuttonSelectFile.Clicked += (s, e) => ViewModel.AddAudioFileCommand.Execute();
			ViewModel.AddAudioFileCommand.CanExecuteChanged += (s, e) => ybuttonSelectFile.Sensitive = ViewModel.AddAudioFileCommand.CanExecute();
			ViewModel.AddAudioFileCommand.RaiseCanExecuteChanged();

			ybuttonDeleteFile.Clicked += (s, e) => ViewModel.DeleteAudioFileCommand.Execute();
			ViewModel.DeleteAudioFileCommand.CanExecuteChanged += (s, e) => ybuttonDeleteFile.Sensitive = ViewModel.DeleteAudioFileCommand.CanExecute();
			ViewModel.DeleteAudioFileCommand.RaiseCanExecuteChanged();

			ybuttonRollbackFile.Clicked += (s, e) => ViewModel.RollbackAudioFileCommand.Execute();
			ViewModel.RollbackAudioFileCommand.CanExecuteChanged += (s, e) => ybuttonRollbackFile.Sensitive = ViewModel.RollbackAudioFileCommand.CanExecute();
			ViewModel.RollbackAudioFileCommand.RaiseCanExecuteChanged();
		}
	}
}

using QS.Views.Dialog;
using System.ComponentModel;
using Vodovoz.ViewModels.Logistic;

namespace Vodovoz.Logistic
{
	[ToolboxItem(true)]
	public partial class DistrictSetDiffReportConfirmationView : DialogViewBase<DistrictSetDiffReportConfirmationViewModel>
	{
		public DistrictSetDiffReportConfirmationView(DistrictSetDiffReportConfirmationViewModel viewModel)
			: base(viewModel)
		{
			Build();

			Initialize();
		}

		private void Initialize()
		{
			yentrySourceVersionName.Binding
				.AddBinding(ViewModel, vm => vm.SourceDistrictSetName, w => w.Text)
				.InitializeFromSource();

			yentryTargetVersionName.Binding
				.AddBinding(ViewModel, vm => vm.TargetDistrictSetName, w => w.Text)
				.InitializeFromSource();

			ybuttonYes.BindCommand(ViewModel.GenerateDiffReportCommand);
			ybuttonNo.Clicked += (s, e) => ViewModel.CloseCommand.Execute(true);
		}
	}
}

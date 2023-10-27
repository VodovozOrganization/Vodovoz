using QS.Views.Dialog;
using Vodovoz.Extensions;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Counterparties.ClientClassification;

namespace Vodovoz.Views.Client.CounterpartyClassification
{
	[WindowSize(300, 300)]
	public partial class CounterpartyClassificationCalculationEmailSettingsView : DialogViewBase<CounterpartyClassificationCalculationEmailSettingsViewModel>
	{
		public CounterpartyClassificationCalculationEmailSettingsView(
			CounterpartyClassificationCalculationEmailSettingsViewModel viewModel
			) : base(viewModel)
		{
			Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			ylabelInfo.ForegroundColor = GdkColors.DangerText.ToHtmlColor();

			yentryMainMail.Binding
				.AddBinding(ViewModel, vm => vm.CurrentUserEmail, w => w.Text)
				.InitializeFromSource();

			yentryMainMail.Sensitive = false;

			yentryEmailForCopy.Binding
				.AddBinding(ViewModel, vm => vm.AdditionalEmail, w => w.Text)
				.InitializeFromSource();

			ybuttonStart.Clicked += (s, e) => ViewModel.StartCalculationCommand.Execute();
		}
	}
}

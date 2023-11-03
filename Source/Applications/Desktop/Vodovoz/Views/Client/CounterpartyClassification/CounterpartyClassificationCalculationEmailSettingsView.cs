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
			var primaryTextColor = GdkColors.PrimaryText.ToHtmlColor();
			var dangerTextColor = GdkColors.DangerText.ToHtmlColor();

			ylabelInfo.ForegroundColor = dangerTextColor;

			yentryMainMail.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CurrentUserEmail, w => w.Text)
				.AddFuncBinding(vm => vm.IsCurrentUserEmailValid ? primaryTextColor : dangerTextColor, w => w.TextColor)
				.InitializeFromSource();

			yentryMainMail.Sensitive = false;

			yentryEmailForCopy.Binding
				.AddBinding(ViewModel, vm => vm.AdditionalEmail, w => w.Text)
				.AddFuncBinding(vm => vm.IsAdditionalEmailValid ? primaryTextColor : dangerTextColor, w => w.TextColor)
				.InitializeFromSource();

			ybuttonStart.Clicked += (s, e) => ViewModel.StartCalculationCommand.Execute();
		}
	}
}

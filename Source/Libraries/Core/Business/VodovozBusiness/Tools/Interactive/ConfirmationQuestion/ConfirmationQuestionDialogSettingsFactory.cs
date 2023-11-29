namespace Vodovoz.Tools.Interactive.ConfirmationQuestion
{
	public class ConfirmationQuestionDialogSettingsFactory : IConfirmationQuestionDialogSettingsFactory
	{
		public ConfirmationQuestionDialogSettings GetFastDeliveryOrderTransferConfirmationDialogSettings()
		{
			return new ConfirmationQuestionDialogSettings { IsNoButtonAvailableByDefault = true };
		}
	}
}

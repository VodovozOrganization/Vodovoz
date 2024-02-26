using System;
using Vodovoz.Settings.Cash;

namespace Vodovoz.Settings.Database.Cash
{
	public class OrganizationCashTransferDocumentSettings : IOrganizationCashTransferDocumentSettings
	{
		private readonly ISettingsController _settingsController;

		public OrganizationCashTransferDocumentSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public int CashIncomeCategoryTransferId => _settingsController.GetIntValue("cash_income_category_transfer_id");

		public int CashExpenseCategoryTransferId => _settingsController.GetIntValue("cash_expense_category_transfer_id");
	}
}

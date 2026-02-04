namespace Vodovoz.Core.Domain.Permissions
{
	public static partial class PaymentPermissions
	{
		public static class BankClient
		{
			public static string CanCreateNewManualPaymentFromBankClient => "can_create_new_manual_payment_from_bank_client";
			public static string CanCancelManualPaymentFromBankClient => "can_cancel_manual_payment_from_bank_client";
		}
	}
}

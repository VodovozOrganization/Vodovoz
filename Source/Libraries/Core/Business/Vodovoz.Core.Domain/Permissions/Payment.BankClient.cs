namespace Vodovoz.Permissions
{
	public static partial class Payment
	{
		public static class BankClient
		{
			public static string CanCreateNewManualPaymentFromBankClient => "can_create_new_manual_payment_from_bank_client";
			public static string CanCancelManualPaymentFromBankClient => "can_cancel_manual_payment_from_bank_client";
		}
	}
}

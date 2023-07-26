namespace Vodovoz.Permissions
{
	public static partial class Order
	{
		/// <summary>
		/// Права чеки
		/// </summary>
		public static class CashReceiptPermissions
		{
			public static string AllReceiptStatusesAvailable => "CashReceipt.AllReceiptStatusesAvailable";
			public static string ShowOnlyCodeErrorStatusReceipts => "CashReceipt.ShowOnlyCodeErrorStatusReceipts";
			public static string ShowOnlyReceiptSendErrorStatusReceipts => "CashReceipt.ShowOnlyReceiptSendErrorStatusReceipts";
			public static string CanResendDuplicateReceipts => "CashReceipt.CanResendDuplicateReceipts";
		}
	}
}

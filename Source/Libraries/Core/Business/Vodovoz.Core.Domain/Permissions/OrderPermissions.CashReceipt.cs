namespace Vodovoz.Core.Domain.Permissions
{
	public static partial class OrderPermissions
	{
		/// <summary>
		/// Права чеки
		/// </summary>
		public static class CashReceipt
		{
			public static string AllReceiptStatusesAvailable => "CashReceipt.AllReceiptStatusesAvailable";
			public static string ShowOnlyCodeErrorStatusReceipts => "CashReceipt.ShowOnlyCodeErrorStatusReceipts";
			public static string ShowOnlyReceiptSendErrorStatusReceipts => "CashReceipt.ShowOnlyReceiptSendErrorStatusReceipts";
			public static string CanResendDuplicateReceipts => "CashReceipt.CanResendDuplicateReceipts";

			/// <summary>
			/// Право на запуск внеплановой переобработки чеков с ошибками фискализации
			/// </summary>
			public static string CanReprocessFailedReceipts => "CashReceipt.CanReprocessFailedReceipts";
		}
	}
}

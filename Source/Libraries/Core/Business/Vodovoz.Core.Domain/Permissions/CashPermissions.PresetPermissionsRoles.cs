namespace Vodovoz.Core.Domain.Permissions
{
	public static partial class CashPermissions
	{
		public static class PresetPermissionsRoles
		{
			/// <summary>
			/// Касса
			/// </summary>
			public static string Cashier => "role_сashier";

			public static string Financier => "role_financier_cash_request";

			public static string Coordinator => "role_coordinator_cash_request";

			public static string Accountant => "role_cashless_payout_accountant";

			public static string SecurityService => "role_security_service_cash_request";
		}
	}
}

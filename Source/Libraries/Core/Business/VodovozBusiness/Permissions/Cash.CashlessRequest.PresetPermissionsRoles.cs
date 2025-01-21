using System.Collections.Generic;
using System.Collections.ObjectModel;
using Vodovoz.Domain.Cash;

namespace Vodovoz.Permissions
{
	public static partial class Cash
	{
		public static partial class CashlessRequest
		{
			/// <summary>
			/// Роли основанные на предустановленных правах для заявок на выдачу денежных средств по безналу
			/// </summary>
			public static class PresetPermissionsRoles
			{
				public static string Financier => "role_financier_cash_request";

				public static string Coordinator => "role_coordinator_cash_request";

				public static string Accountant => "role_cashless_payout_accountant";

				public static string SecurityService => "role_security_service_cash_request";

				public static ReadOnlyDictionary<string, PayoutRequestUserRole> PermissionsToRoles => new ReadOnlyDictionary<string, PayoutRequestUserRole>(
					new Dictionary<string, PayoutRequestUserRole>
					{
						{ Financier, PayoutRequestUserRole.Financier },
						{ Coordinator, PayoutRequestUserRole.Coordinator },
						{ Accountant, PayoutRequestUserRole.Accountant },
						{ SecurityService, PayoutRequestUserRole.SecurityService },
					});
			}
		}
	}
}

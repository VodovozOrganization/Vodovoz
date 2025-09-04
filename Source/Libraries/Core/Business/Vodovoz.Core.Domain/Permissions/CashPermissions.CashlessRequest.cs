using System.Collections.Generic;
using System.Collections.ObjectModel;
using Vodovoz.Domain.Cash;

namespace Vodovoz.Core.Domain.Permissions
{
	public static partial class CashPermissions
	{
		/// <summary>
		/// Права заявок на выдачу денежных средств по безналу
		/// </summary>
		public static partial class CashlessRequest
		{
			/// <summary>
			/// Доступно создание календаря платежей
			/// </summary>
			public static string CanCreateGiveOutSchedule => "can_create_give_out_schedule";

			public static ReadOnlyDictionary<string, PayoutRequestUserRole> PermissionsToRoles => new ReadOnlyDictionary<string, PayoutRequestUserRole>(
				new Dictionary<string, PayoutRequestUserRole>
				{
						{ PresetPermissionsRoles.Financier, PayoutRequestUserRole.Financier },
						{ PresetPermissionsRoles.Coordinator, PayoutRequestUserRole.Coordinator },
						{ PresetPermissionsRoles.Cashier, PayoutRequestUserRole.Cashier },
						{ PresetPermissionsRoles.SecurityService, PayoutRequestUserRole.SecurityService },
				});
		}
	}
}

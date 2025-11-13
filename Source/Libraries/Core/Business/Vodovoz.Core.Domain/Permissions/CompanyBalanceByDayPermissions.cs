using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Permissions
{
	public static partial class CompanyBalanceByDayPermissions
	{
		/// <summary>
		/// Может изменять остатки ден.средств в прошлом периоде
		/// </summary>
		[Display(
			Name = "Пользователь может изменять остатки ден.средств в прошлом периоде",
			Description = "Пользователь может изменять остатки ден.средств в прошлом периоде")]
		public static string CanEditCompanyBalanceByDayInPreviousPeriods => "can_edit_company_balance_by_day_in_previous_periods";
	}
}

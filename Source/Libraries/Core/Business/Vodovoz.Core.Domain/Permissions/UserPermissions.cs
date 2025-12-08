using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Permissions
{
	public static class UserPermissions
	{
		/// <summary>
		/// Пользователь - торговый представитель
		/// </summary>
		public static string IsSalesRepresentative => "user_is_sales_representative";

		/// <summary>
		/// Пользователь может пользоваться только складом и рекламациями
		/// </summary>
		[Display(Name = "Пользователь может пользоваться только складом и рекламациями")]
		public static string UserHaveAccessOnlyToWarehouseAndComplaints => "user_have_access_only_to_warehouse_and_complaints";
	}
}

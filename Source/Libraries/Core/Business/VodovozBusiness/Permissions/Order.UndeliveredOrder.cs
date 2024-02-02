using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Permissions
{
	public static partial class Order
	{
		/// <summary>
		/// Права недовозы
		/// </summary>
		public static class UndeliveredOrder
		{
			/// <summary>
			/// Изменение недовозов - Пользователь может изменять недовозы, в т.ч. менять их статус.
			/// </summary>
			[Display(Name = "Изменение недовозов",
				Description = "Пользователь может изменять недовозы, в т.ч. менять их статус.")]
			public static string CanEditUndeliveries => "can_edit_undeliveries";
			public static string CanCloseUndeliveries => "can_close_undeliveries";
			public static string CanChangeUndeliveryProblemSource => "can_change_undelivery_problem_source";
		}
	}
}

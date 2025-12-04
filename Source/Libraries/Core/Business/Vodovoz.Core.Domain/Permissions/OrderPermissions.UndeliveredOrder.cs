using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Permissions
{
	public static partial class OrderPermissions
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

			/// <summary>
			/// Завершение обсуждения в недовозе
			/// </summary>
			[Display(
				Name = "Завершение обсуждения в недовозе",
				Description = "Дает возможность пользователю завершить обсуждение в недовозе")]
			public static string CanCompleteUndeliveryDiscussion => "can_complete_undelivery_discussion";
		}
	}
}

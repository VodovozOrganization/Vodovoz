using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Permissions
{
	public static class Counterparty
	{
		/// <summary>
		/// Пересчет классификации контрагентов
		/// </summary>
		public static string CanCalculateCounterpartyClassifications => "can_calculate_counterparty_classifications";

		/// <summary>
		/// Редактирование рефера клиента
		/// </summary>
		[Display(
			Name = "Редактирование рефера клиента",
			Description = "Дает возможность редактировать рефера клиента")]
		public static string CanEditClientRefer => nameof(CanEditClientRefer);
		
		/// <summary>
		/// Редактирование менеджера клиента
		/// </summary>
		[Display(
			Name = "Редактирование менеджера клиента",
			Description = "Дает возможность редактировать менеджера клиента")]
		public static string CanSetSalesManager => "can_set_sales_manager";
	}
}

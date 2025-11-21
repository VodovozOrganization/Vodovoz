using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Clients
{
	/// <summary>
	/// Тип аренды
	/// </summary>
	public enum RentType
	{
		[Display (Name = "Долгосрочная аренда")]
		NonfreeRent,
		[Display (Name = "Посуточная аренда")]
		DailyRent,
		[Display (Name = "Бесплатная аренда")]
		FreeRent
	}
}

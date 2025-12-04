using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Clients
{
	/// <summary>
	/// Тип соглашения
	/// </summary>
	public enum AgreementType
	{
		[Display (Name = "Долгосрочная аренда")]
		NonfreeRent,
		[Display (Name = "Посуточная аренда")]
		DailyRent,
		[Display (Name = "Бесплатная аренда")]
		FreeRent,
		[Display (Name = "Продажа воды")]
		WaterSales,
		[Display (Name = "Продажа оборудования")]
		EquipmentSales,
		[Display (Name = "Ремонт")]
		Repair
	}
}

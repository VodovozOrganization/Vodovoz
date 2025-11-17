using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Clients
{
	public enum TemplateType
	{
		[Display (Name = "Основной договор")]
		Contract,
		[Display (Name = "Доп. соглашение на воду")]
		AgWater,
		[Display (Name = "Доп. соглашение на продажу оборудования")]
		AgEquip,
		[Display (Name = "Доп. соглашение бесплатной аренды")]
		AgFreeRent,
		[Display (Name = "Доп. соглашение короткосрочной аренды")]
		AgShortRent,
		[Display (Name = "Доп. соглашение долгосрочной аренды")]
		AgLongRent,
		[Display (Name = "Доп. соглашение на обслуживание")]
		AgRepair,
		[Display (Name = "Доверенность на ТС")]
		CarProxy,
		[Display(Name = "Доверенность М-2")]
		M2Proxy,
		[Display(Name = "ГПК")]
		EmployeeContract,
		[Display(Name = "Путевой лист")]
		WayBill,
		[Display(Name = "Договор аренды автомобиля")]
		CarRentalContract
	}
}

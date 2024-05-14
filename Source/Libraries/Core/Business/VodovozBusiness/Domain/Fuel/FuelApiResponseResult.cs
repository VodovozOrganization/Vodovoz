using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Fuel
{
	public enum FuelApiResponseResult
	{
		[Display(Name = "Не указано")]
		None,
		[Display(Name = "Успех")]
		Success,
		[Display(Name = "Ошибка")]
		Error
	}
}

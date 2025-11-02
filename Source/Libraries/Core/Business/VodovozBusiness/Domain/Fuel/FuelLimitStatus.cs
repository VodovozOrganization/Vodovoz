using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Fuel
{
	public enum FuelLimitStatus
	{
		[Display(Name = "Не указано")]
		None,
		[Display(Name = "Выдан успешно")]
		Success,
		[Display(Name = "Отменен")]
		Canceled,
		[Display(Name = "Ошибка на стороне сервиса при выдаче")]
		ServiceError,
		[Display(Name = "Ошибка на стороне ДВ при выдаче")]
		LocalError
	}
}

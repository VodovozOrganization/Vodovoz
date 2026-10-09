using Vodovoz.Core.Domain.Results;

namespace VodovozBusiness.Errors.Sale
{
	public static class OnlineOrderTemplateApiErrors
	{
		/// <summary>
		/// Нельзя подключить автозаказ без данных по клиенту
		/// </summary>
		public static Error IsEmptyCounterparty => new Error("400", "Нельзя подключить автозаказ без данных по клиенту");
		/// <summary>
		/// Нельзя подключить автозаказ без данных по пользователю
		/// </summary>
		public static Error IsEmptyExternalCounterparty => new Error("400", "Нельзя подключить автозаказ без данных по пользователю");
		/// <summary>
		/// Отсутствует число месяца при указанном типе доставки
		/// </summary>
		public static Error IsEmptyScheduleMonthDay => new Error("400", "Отсутствует число месяца при указанном типе доставки");
		/// <summary>
		/// Недопустимое число месяца
		/// </summary>
		public static Error IsScheduleMonthDayInvalid => new Error("400", "Недопустимое число месяца");
		/// <summary>
		/// Отсутствует день недели при указанном типе доставки
		/// </summary>
		public static Error IsEmptyScheduleWeekDay => new Error("400", "Отсутствует день недели при указанном типе доставки");
		/// <summary>
		/// Отсутствует периодичность доставки
		/// </summary>
		public static Error IsEmptyScheduleRepeatEveryWeeks => new Error("400", "Отсутствует периодичность доставки");
		/// <summary>
		/// Недопустимая периодичность
		/// </summary>
		public static Error IsScheduleRepeatEveryWeeksInvalid => new Error("400", "Недопустимая периодичность");
		/// <summary>
		/// Отсутствует дата первой доставки
		/// </summary>
		public static Error IsEmptyScheduleFirstDeliveryDate => new Error("400", "Отсутствует дата первой доставки");
		/// <summary>
		/// Неизвестный интервал доставки
		/// </summary>
		public static Error IsDeliveryScheduleUnknown => new Error("400", "Неизвестный интервал доставки");
		/// <summary>
		/// Недопустимый интервал доставки
		/// </summary>
		public static Error IsDeliveryScheduleInvalid => new Error("400", "Недопустимый интервал доставки");
	}
}

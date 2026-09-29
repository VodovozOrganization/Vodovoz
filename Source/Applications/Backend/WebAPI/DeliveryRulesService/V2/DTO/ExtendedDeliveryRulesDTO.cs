using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using DeliveryRulesService.Constants;
using Vodovoz.Domain.Sale;

namespace DeliveryRulesService.V2.DTO
{
	/// <summary>
	/// Расширенные правила доставки
	/// </summary>
	public class ExtendedDeliveryRulesDto
	{
		/// <summary>
		/// Статус ответа
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public DeliveryRulesResponseStatus Status { get; set; }
		/// <summary>
		/// Сообщение
		/// </summary>
		public string Message { get; set; }
		/// <summary>
		/// Данные платной доставки
		/// </summary>
		public PaidDeliveryDto PaidDelivery { get; set; }
		/// <summary>
		/// Данные быстрой доставки
		/// </summary>
		public FastDeliveryDto FastDelivery { get; set; }
		/// <summary>
		/// Правила по дням
		/// </summary>
		public IList<ExtendedWeekDayDeliveryRuleDto> WeekDayDeliveryRules { get; set; }

		public void SetErrorState()
		{
			Status = DeliveryRulesResponseStatus.Error;
			WeekDayDeliveryRules = null;
			Message = ServiceConstants.InternalErrorFromGetDeliveryRule;
		}
		
		public void RuleNotFoundState(string message)
		{
			Status = DeliveryRulesResponseStatus.RuleNotFound;
			WeekDayDeliveryRules = null;
			Message = message;
		}

		/// <summary>
		/// Добавление данных по быстрой доставке
		/// </summary>
		/// <param name="fastDeliveryId">Идентификатор быстрой доставки</param>
		/// <param name="fastDeliveryPrice">Цена</param>
		/// <param name="fastDeliveryName">Наименование</param>
		/// <param name="fastDeliveryScheduleId">Идентификатор интервала быстрой доставки</param>
		/// <param name="fastDeliveryScheduleName">Название интервала быстрой доставки</param>
		public void AddFastDelivery(
			int fastDeliveryId,
			decimal fastDeliveryPrice,
			string fastDeliveryName,
			int fastDeliveryScheduleId,
			string fastDeliveryScheduleName
			)
		{
			var todayInfo = WeekDayDeliveryRules.Single(x => x.WeekDay == WeekDayName.Today);
			FastDelivery = FastDeliveryDto.Create(fastDeliveryId, fastDeliveryName, fastDeliveryPrice);

			(todayInfo.DeliveryIntervals as IList<ScheduleRestrictionDto>).Insert(
				0,
				ScheduleRestrictionDto.Create(fastDeliveryScheduleId, fastDeliveryScheduleName));
		}
	}
}

using System;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Sale;

namespace VodovozBusiness.Services.Sale
{
	/// <summary>
	/// Проверщик интервалов доставки
	/// </summary>
	public interface IDeliveryScheduleChecker
	{
		/// <summary>
		/// Доступен ли интервал доставки
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="weekDay">День недели</param>
		/// <param name="deliveryPointId">Идентификатор ТД</param>
		/// <param name="deliveryScheduleId">Идентификатор интервала</param>
		/// <returns>true - доступен, false - нет</returns>
		bool IsDeliveryIntervalAvailable(
			IUnitOfWork uow,
			DayOfWeek weekDay,
			int deliveryPointId,
			int deliveryScheduleId
		);

		/// <summary>
		/// Доступен ли интервал доставки
		/// Здесь производится расширенная проверка как дня доставки, так и дня из самого шаблона. Т.к. может быть перенос даты
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="deliveryWeekDay">День недели доставки</param>
		/// <param name="templateWeekDay">День недели доставки из шаблона</param>
		/// <param name="deliveryPointId">Идентификатор ТД</param>
		/// <param name="deliveryScheduleId">Идентификатор интервала</param>
		/// <returns>true - доступен, false - нет</returns>
		DeliveryScheduleAvailability IsDeliveryIntervalAvailable(
			IUnitOfWork uow,
			DayOfWeek deliveryWeekDay,
			DayOfWeek templateWeekDay,
			int deliveryPointId,
			int deliveryScheduleId
		);
	}
}

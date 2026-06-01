using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Core.Application.Orders.Services
{
	/// <summary>
	/// Сервис для обработки логики переноса заказов
	/// </summary>
	public interface ICustomerOrderTransferService
	{
		/// <summary>
		/// Проверяет возможность переноса заказа на новую дату и время доставки
		/// </summary>
		/// <param name="order">Заказ, который планируется перенести</param>
		/// <param name="newDeliveryDate">Новая дата доставки (если null - дата не меняется)</param>
		/// <param name="newDeliverySchedule">Новый интервал доставки</param>
		/// <returns>
		/// Результат проверки:
		///		Success - перенос возможен;
		///		Failure с соответствующей ошибкой - перенос невозможен
		/// </returns>
		Result CanTransfer(
			Order order,
			DateTime? newDeliveryDate,
			DeliverySchedule newDeliverySchedule);

		/// <summary>
		/// Проверяет возможность переноса заказа
		/// </summary>
		/// <param name="order">Заказ, который планируется перенести</param>
		/// <returns>
		/// Результат проверки:
		///		Success - перенос возможен;
		///		Failure с соответствующей ошибкой - перенос невозможен
		/// </returns>
		Result CanTransfer(Order order);

		/// <summary>
		/// Проверяет, были ли изменены параметры доставки (дата или интервал)
		/// </summary>
		/// <param name="order">Заказ для проверки</param>
		/// <param name="newDeliveryDate">Новая дата доставки (если null - изменение даты не проверяется)</param>
		/// <param name="newDeliveryScheduleId">Идентификатор нового интервала доставки (если null - изменение интервала не проверяется)</param>
		/// <returns>
		/// true - если дата или интервал доставки были изменены;
		/// false - если изменения отсутствуют или не переданы параметры для сравнения
		/// </returns>
		bool IsDeliveryParametersChanged(Order order, DateTime? newDeliveryDate, int? newDeliveryScheduleId);

		/// <summary>
		/// Применяет перенос заказа на новую дату и время доставки
		/// </summary>
		/// <param name="uow">Unit of Work для работы с базой данных</param>
		/// <param name="order">Заказ, который необходимо перенести</param>
		/// <param name="onlineOrder">Онлайн-заказ, связанный с переносимым заказом</param>
		/// <param name="newDeliveryDate">Новая дата доставки</param>
		/// <param name="newDeliverySchedule">Новый интервал доставки</param>
		/// <param name="source">Источник запроса</param>
		/// <param name="cancellationToken">Токен отмены операции</param>
		/// <returns>
		/// Результат выполнения операции:
		///		Success - перенос выполнен успешно;
		///		Failure с соответствующей ошибкой - перенос не выполнен
		/// </returns>
		/// <remarks>
		/// Метод автоматически определяет тип переноса в зависимости от статуса заказа:
		/// - Для статусов NewOrder, WaitForPayment, Accepted - выполняется простой перенос
		/// - Для статуса InTravelList - заказ удаляется из маршрутного листа и переносится
		/// - Для статусов OnLoading, OnTheWay - создается недовоз и копия заказа на новую дату
		/// </remarks>
		Task<Result> ApplyTransferAsync(
			IUnitOfWork uow,
			Order order,
			OnlineOrder onlineOrder,
			DateTime? newDeliveryDate,
			DeliverySchedule newDeliverySchedule,
			Source source,
			CancellationToken cancellationToken);
	}
}

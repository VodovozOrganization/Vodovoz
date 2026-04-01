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
	public interface IOrderTransferLogicService
	{
		/// <summary>
		/// Проверяет, можно ли перенести заказ
		/// </summary>
		Result CanTransfer(
			Order order,
			DateTime? newDeliveryDate,
			DeliverySchedule newDeliverySchedule);

		/// <summary>
		/// Проверяет, был ли изменен интервал или дата доставки
		/// </summary>
		bool IsDeliveryParametersChanged(Order order, DateTime? newDeliveryDate, int? newDeliveryScheduleId);

		/// <summary>
		/// Применяет перенос заказа
		/// </summary>
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

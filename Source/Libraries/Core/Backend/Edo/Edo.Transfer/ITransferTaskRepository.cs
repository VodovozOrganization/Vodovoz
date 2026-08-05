using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Transfer
{
	/// <summary>
	/// Предоставляет данные задач трансфера.
	/// </summary>
	public interface ITransferTaskRepository
	{
		/// <summary>
		/// Находит ожидающую задачу трансфера для направления между организациями.
		/// </summary>
		/// <param name="uow">Контекст работы с данными.</param>
		/// <param name="fromOrg">Код организации-отправителя.</param>
		/// <param name="toOrg">Код организации-получателя.</param>
		/// <param name="cancellationToken">Токен отмены.</param>
		/// <returns>Найденная задача трансфера или <see langword="null"/>.</returns>
		Task<TransferEdoTask> FindTaskAsync(
			IUnitOfWork uow,
			int fromOrg,
			int toOrg,
			CancellationToken cancellationToken);

		/// <summary>
		/// Получает задачи трансфера, превысившие время ожидания заявок.
		/// </summary>
		/// <param name="uow">Контекст работы с данными.</param>
		/// <param name="cancellationToken">Токен отмены.</param>
		/// <returns>Задачи трансфера, ожидающие обработки.</returns>
		Task<IEnumerable<TransferEdoTask>> GetStaleTasksAsync(
			IUnitOfWork uow,
			CancellationToken cancellationToken);

		/// <summary>
		/// Проверяет завершение итерации трансфера.
		/// </summary>
		/// <param name="uow">Контекст работы с данными.</param>
		/// <param name="transferIterationId">Код итерации трансфера.</param>
		/// <param name="cancellationToken">Токен отмены.</param>
		/// <returns><see langword="true"/>, если итерация завершена.</returns>
		Task<bool> IsTransferIterationCompletedAsync(
			IUnitOfWork uow,
			int transferIterationId,
			CancellationToken cancellationToken);

		/// <summary>
		/// Получает коды Честного знака, входящие в задачу трансфера.
		/// </summary>
		/// <param name="uow">Контекст работы с данными.</param>
		/// <param name="transferTask">Задача трансфера.</param>
		/// <param name="cancellationToken">Токен отмены.</param>
		/// <returns>Коды Честного знака задачи трансфера.</returns>
		Task<IEnumerable<TrueMarkWaterIdentificationCode>> GetAllCodesForTransferTaskAsync(
			IUnitOfWork uow,
			TransferEdoTask transferTask,
			CancellationToken cancellationToken);

		/// <summary>
		/// Получает минимальную дату доставки заказов, связанных с задачей трансфера.
		/// </summary>
		/// <param name="uow">Контекст работы с данными.</param>
		/// <param name="transferTaskId">Код задачи трансфера.</param>
		/// <param name="cancellationToken">Токен отмены.</param>
		/// <returns>Минимальная дата доставки или <see langword="null"/>, если дата не определена.</returns>
		Task<DateTime?> GetMinOrderDeliveryDateForTransferTaskAsync(
			IUnitOfWork uow,
			int transferTaskId,
			CancellationToken cancellationToken);
	}
}

using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Vodovoz.Core.Data.Repositories
{
	/// <summary>
	/// Репозиторий данных для мониторинга отклонений документооборота ЭДО.
	/// Первая группа методов обслуживает регистрацию отклонений и идет от окна просмотра,
	/// вторая — снятие отклонений и идет от самих зарегистрированных отклонений
	/// </summary>
	public interface IEdoDeviationRepository
	{
		/// <summary>
		/// Возвращает заявки ЭДО на отправку документов заказа, по которым не создана задача
		/// и нет незакрытого отклонения
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="createdBefore">Верхняя граница времени создания заявки</param>
		/// <param name="afterRequestId">Идентификатор заявки, после которого продолжать выборку. Ноль — с начала</param>
		/// <param name="limit">Размер страницы</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Страница заявок без задач, упорядоченная по идентификатору заявки</returns>
		Task<IList<EdoRequestMonitoringNode>> GetRequestsWithoutTaskAsync(
			IUnitOfWork uow,
			DateTime createdBefore,
			int afterRequestId,
			int limit,
			CancellationToken cancellationToken);

		/// <summary>
		/// Возвращает состояние незавершенных задач отправки документов и чеков,
		/// по которым нет ни активных зарегистрированных проблем, ни незакрытых отклонений
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="afterTaskId">Идентификатор задачи, после которого продолжать выборку. Ноль — с начала</param>
		/// <param name="limit">Размер страницы</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Страница состояний задач, упорядоченная по идентификатору задачи</returns>
		Task<IList<EdoTaskMonitoringNode>> GetMonitoredTasksAsync(
			IUnitOfWork uow,
			int afterTaskId,
			int limit,
			CancellationToken cancellationToken);

		/// <summary>
		/// Возвращает идентификаторы задач, документооборот которых завершился действием
		/// <see cref="EdoDocFlowStatus.Succeed"/>, а результата обработки кодов в ГИС МТ
		/// по нему либо нет, либо он отказной
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="gisMtTrackedFrom">
		/// Дата, с которой отслеживается результат обработки кодов в ГИС МТ:
		/// задачи заказов, доставленных раньше нее, в выборку не попадают
		/// </param>
		/// <param name="afterTaskId">Идентификатор задачи, после которого продолжать выборку. Ноль — с начала</param>
		/// <param name="limit">Размер страницы</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Страница идентификаторов задач, упорядоченная по идентификатору задачи</returns>
		Task<IList<int>> GetTaskIdsWithFinishedDocflowAsync(
			IUnitOfWork uow,
			DateTime gisMtTrackedFrom,
			int afterTaskId,
			int limit,
			CancellationToken cancellationToken);

		/// <summary>
		/// Возвращает состояние незавершенных задач трансфера, по которым нет незакрытых отклонений
		/// и нет активных проблем, кроме ожидания перемещения кодов в ГИС МТ:
		/// это ожидание мониторинг меряет сам
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="afterTaskId">Идентификатор задачи, после которого продолжать выборку. Ноль — с начала</param>
		/// <param name="limit">Размер страницы</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Страница состояний задач трансфера, упорядоченная по идентификатору задачи</returns>
		Task<IList<EdoTransferTaskMonitoringNode>> GetMonitoredTransferTasksAsync(
			IUnitOfWork uow,
			int afterTaskId,
			int limit,
			CancellationToken cancellationToken);

		/// <summary>
		/// Возвращает идентификаторы задач трансфера, документооборот которых завершился действием
		/// <see cref="EdoDocFlowStatus.Succeed"/>, а результата обработки кодов в ГИС МТ
		/// по нему либо нет, либо он отказной.
		/// Нужна тем же правилам по результату ГИС МТ, что и по задачам заказов:
		/// принятый документ трансфера завершает задачу, а результат приходит после этого
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="gisMtTrackedFrom">
		/// Дата, с которой отслеживается результат обработки кодов в ГИС МТ.
		/// У задачи трансфера своей даты доставки нет, поэтому она сравнивается
		/// с временем создания задачи
		/// </param>
		/// <param name="afterTaskId">Идентификатор задачи, после которого продолжать выборку. Ноль — с начала</param>
		/// <param name="limit">Размер страницы</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Страница идентификаторов задач трансфера, упорядоченная по идентификатору задачи</returns>
		Task<IList<int>> GetTransferTaskIdsWithFinishedDocflowAsync(
			IUnitOfWork uow,
			DateTime gisMtTrackedFrom,
			int afterTaskId,
			int limit,
			CancellationToken cancellationToken);

		/// <summary>
		/// Возвращает состояние задач трансфера по их кодам,
		/// без исключения завершенных и задач с проблемами.
		/// Решение об актуальности отклонения принимает вызывающий сервис
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="edoTaskIds">Идентификаторы задач трансфера</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Состояние задач трансфера</returns>
		Task<IList<EdoTransferTaskMonitoringNode>> GetTransferTaskNodesAsync(
			IUnitOfWork uow,
			IReadOnlyCollection<int> edoTaskIds,
			CancellationToken cancellationToken);

		/// <summary>
		/// Возвращает страницу незакрытых отклонений независимо от задач и заявок
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="afterDeviationId">Идентификатор отклонения, после которого продолжать выборку. Ноль — с начала</param>
		/// <param name="limit">Размер страницы</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Страница незакрытых отклонений, упорядоченная по коду отклонения</returns>
		Task<IList<EdoTaskDeviationNode>> GetActiveDeviationsPageAsync(
			IUnitOfWork uow,
			int afterDeviationId,
			int limit,
			CancellationToken cancellationToken);

		/// <summary>
		/// Возвращает состояние задач по их идентификаторам,
		/// без исключения задач с активными проблемами и завершенных
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="edoTaskIds">Коды задач ЭДО</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Состояние задач</returns>
		Task<IList<EdoTaskMonitoringNode>> GetTaskNodesAsync(
			IUnitOfWork uow,
			IReadOnlyCollection<int> edoTaskIds,
			CancellationToken cancellationToken);

		/// <summary>
		/// Возвращает состояние заявок по их идентификаторам, в том числе тех,
		/// по которым уже создана задача
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="edoRequestIds">Коды заявок ЭДО</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Состояние заявок</returns>
		Task<IList<EdoRequestMonitoringNode>> GetRequestNodesAsync(
			IUnitOfWork uow,
			IReadOnlyCollection<int> edoRequestIds,
			CancellationToken cancellationToken);
	}
}

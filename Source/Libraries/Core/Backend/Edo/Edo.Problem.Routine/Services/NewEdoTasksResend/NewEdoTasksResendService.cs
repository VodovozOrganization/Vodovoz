using Edo.Transport;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problem.Routine.Services.NewEdoTasksResend
{
	/// <summary>
	/// Повторно запускает просроченные новые задачи ЭДО
	/// </summary>
	public class NewEdoTasksResendService : INewEdoTasksResendService
	{
		private readonly ILogger<NewEdoTasksResendService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IEdoRepository _edoRepository;
		private readonly IOrderEdoTaskCreatedEventPublisher _taskCreatedEventPublisher;

		public NewEdoTasksResendService(
			ILogger<NewEdoTasksResendService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IEdoRepository edoRepository,
			IOrderEdoTaskCreatedEventPublisher taskCreatedEventPublisher)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
			_taskCreatedEventPublisher = taskCreatedEventPublisher ?? throw new ArgumentNullException(nameof(taskCreatedEventPublisher));
		}

		/// <summary>
		/// Повторно публикует события для просроченных задач в статусе Новая
		/// </summary>
		/// <param name="maxCreationTime">Максимальное время создания задачи</param>
		/// <param name="batchSize">Максимальное количество задач</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Количество повторно запущенных задач</returns>
		public async Task<int> ResendStaleNewTasks(
			DateTime maxCreationTime,
			int batchSize,
			CancellationToken cancellationToken = default)
		{
			if(batchSize <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(batchSize), "Размер партии должен быть больше нуля");
			}

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot("Повторный запуск просроченных новых задач ЭДО"))
			{
				var tasks = await _edoRepository.GetStaleNewEdoTasks(
					uow,
					maxCreationTime,
					batchSize,
					cancellationToken);

				var resentTasksCount = 0;
				foreach(var task in tasks)
				{
					if(!IsSupportedTask(task))
					{
						_logger.LogWarning(
							"Задача ЭДО {EdoTaskId} ({TaskType}) не относится к переотправке задач маркировки",
							task.Id,
							task.TaskType);
						continue;
					}

					await _taskCreatedEventPublisher.Publish(task, cancellationToken);
					resentTasksCount++;
				}

				_logger.LogInformation(
					"Повторно запущено просроченных новых задач ЭДО: {TasksCount}",
					resentTasksCount);

				return resentTasksCount;
			}
		}

		private static bool IsSupportedTask(OrderEdoTask task) =>
			task is DocumentEdoTask documentTask && documentTask.DocumentType == EdoDocumentType.UPD
			|| task is ReceiptEdoTask
			|| task is TenderEdoTask
			|| task is SaveCodesEdoTask;
	}
}

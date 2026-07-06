using Edo.Contracts.Messages.Events;
using Edo.Common;
using Edo.Problem.Routine.Options;
using Edo.Problems.Custom.Sources;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Settings.Edo;

namespace Edo.Problem.Routine.Services
{
	/// <summary>
	/// Сервис возобновления отправки чеков, отложенных из-за ночного времени
	/// </summary>
	public class ReceiptNightSendProblemService
	{
		private readonly ILogger<ReceiptNightSendProblemService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOptionsMonitor<ReceiptNightSendProblemWorkerOptions> _options;
		private readonly IEdoRepository _edoRepository;
		private readonly IBus _messageBus;
		private readonly IEdoReceiptSettings _edoReceiptSettings;

		public ReceiptNightSendProblemService(
			ILogger<ReceiptNightSendProblemService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOptionsMonitor<ReceiptNightSendProblemWorkerOptions> options,
			IEdoRepository edoRepository,
			IBus messageBus,
			IEdoReceiptSettings edoReceiptSettings)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
			_edoReceiptSettings = edoReceiptSettings ?? throw new ArgumentNullException(nameof(edoReceiptSettings));
		}

		private DateTime MinEdoTaskCreationTime => DateTime.Today - _options.CurrentValue.ProblemTimeout;

		/// <summary>
		/// Обработчик задач с отложенной ночной отправкой чеков
		/// </summary>
		public async Task ProcessNightSendProblems(CancellationToken cancellationToken)
		{
			if(ReceiptSendPauseTimeHelper.IsNightPauseTime(
				DateTime.Now.TimeOfDay,
				_edoReceiptSettings.ReceiptSendPauseStartTime,
				_edoReceiptSettings.ReceiptSendPauseEndTime))
			{
				_logger.LogInformation(
					"Отложенные ночные чеки не возобновляются, текущее время попадает в ночное окно {Start}-{End}",
					_edoReceiptSettings.ReceiptSendPauseStartTime,
					_edoReceiptSettings.ReceiptSendPauseEndTime);

				return;
			}

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(nameof(ReceiptNightSendProblemService)))
			{
				var tasks = await _edoRepository.GetProblemEdoTasks<ReceiptEdoTask>(
					uow,
					ReceiptSendPausedByNightTime.SourceName,
					MinEdoTaskCreationTime,
					cancellationToken);

				await TryResumeReceiptTasks(uow, tasks, cancellationToken);
			}
		}

		private async Task TryResumeReceiptTasks(
			IUnitOfWork uow,
			IList<ReceiptEdoTask> receiptTasks,
			CancellationToken cancellationToken)
		{
			var successCount = 0;
			var errorCount = 0;

			foreach(var receiptTask in receiptTasks)
			{
				try
				{
					var resumed = await TryResumeReceiptTask(uow, receiptTask, cancellationToken);

					if(resumed)
					{
						successCount++;
					}
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Ошибка при возобновлении отправки чека по задаче ЭДО {EdoTaskId}", receiptTask.Id);
					errorCount++;
				}
			}

			_logger.LogInformation(
				"Обработка отложенных ночных чеков завершена. Всего задач: {Total}. Возобновлено: {Success}. Ошибок: {Errors}",
				receiptTasks.Count,
				successCount,
				errorCount);
		}

		private async Task<bool> TryResumeReceiptTask(
			IUnitOfWork uow,
			ReceiptEdoTask receiptTask,
			CancellationToken cancellationToken)
		{
			if(receiptTask.ReceiptStatus != EdoReceiptStatus.Sending)
			{
				_logger.LogWarning(
					"Задача ЭДО {EdoTaskId} находится в статусе чека {ReceiptStatus}. Возобновление возможно только в статусе {ExpectedStatus}",
					receiptTask.Id,
					receiptTask.ReceiptStatus,
					EdoReceiptStatus.Sending);

				return false;
			}

			var nightProblem = receiptTask.Problems
				.FirstOrDefault(x =>
					x.SourceName == ReceiptSendPausedByNightTime.SourceName
					&& x.State == TaskProblemState.Active);

			if(nightProblem == null)
			{
				_logger.LogWarning(
					"У задачи ЭДО {EdoTaskId} не найдена активная проблема {ProblemSourceName}",
					receiptTask.Id,
					ReceiptSendPausedByNightTime.SourceName);

				return false;
			}

			nightProblem.State = TaskProblemState.Solved;

			if(receiptTask.Problems.Any(x => x.State == TaskProblemState.Active))
			{
				await uow.SaveAsync(nightProblem, cancellationToken: cancellationToken);
				await uow.CommitAsync(cancellationToken);

				_logger.LogInformation(
					"Задача ЭДО {EdoTaskId} имеет другие активные проблемы. Ночная проблема закрыта, отправка чека не возобновлена",
					receiptTask.Id);

				return false;
			}

			if(receiptTask.Status == EdoTaskStatus.Problem
				|| receiptTask.Status == EdoTaskStatus.Waiting)
			{
				receiptTask.Status = EdoTaskStatus.InProgress;
			}

			await uow.SaveAsync(nightProblem, cancellationToken: cancellationToken);
			await uow.SaveAsync(receiptTask, cancellationToken: cancellationToken);
			await uow.CommitAsync(cancellationToken);

			await _messageBus.Publish(
				new ReceiptReadyToSendEvent { ReceiptEdoTaskId = receiptTask.Id },
				cancellationToken);

			_logger.LogInformation(
				"Возобновлена отправка чека по задаче ЭДО {EdoTaskId}",
				receiptTask.Id);

			return true;
		}
	}
}

using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;
using Edo.Common;
using Vodovoz.Core.Domain.Edo;

namespace Edo.CodesSaver
{
	public class SaveCodesEventHandler : IDisposable
	{
		private readonly ILogger<SaveCodesEventHandler> _logger;
		private readonly IUnitOfWork _uow;
		private readonly ISaveCodesService _saveCodesService;

		public SaveCodesEventHandler(
			ILogger<SaveCodesEventHandler> logger,
			IUnitOfWork uow,
			ISaveCodesService saveCodesService
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_saveCodesService = saveCodesService ?? throw new ArgumentNullException(nameof(saveCodesService));
		}

		public async Task Handle(int edoTaskId, CancellationToken cancellationToken)
		{
			var edoTask = await _uow.Session.GetAsync<SaveCodesEdoTask>(edoTaskId);
			if(edoTask == null)
			{
				_logger.LogWarning("ЭДО задача с id {EdoTaskId} не найдена", edoTaskId);
				return;
			}

			if(edoTask.Status != EdoTaskStatus.New)
			{
				_logger.LogWarning("ЭДО задача с id {EdoTaskId} уже обработана", edoTaskId);
				return;
			}

			await _saveCodesService.SaveCodesToPool(edoTask, cancellationToken);
			edoTask.Status = EdoTaskStatus.Completed;
			edoTask.StartTime = DateTime.Now;
			edoTask.EndTime = DateTime.Now;

			await _uow.SaveAsync(edoTask, cancellationToken: cancellationToken);
			await _uow.CommitAsync(cancellationToken);
		}

		public void Dispose()
		{
			_uow.Dispose();
		}
	}
}

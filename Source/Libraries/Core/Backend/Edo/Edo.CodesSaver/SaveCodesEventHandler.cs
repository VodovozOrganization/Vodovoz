using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Codes.Pool;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Edo.CodesSaver
{
	public class SaveCodesEventHandler : IDisposable
	{
		private readonly ILogger<SaveCodesEventHandler> _logger;
		private readonly IUnitOfWork _uow;
		private readonly TrueMarkCodesPool _trueMarkCodesPool;

		public SaveCodesEventHandler(
			ILogger<SaveCodesEventHandler> logger,
			IUnitOfWork uow,
			TrueMarkCodesPool trueMarkCodesPool
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_trueMarkCodesPool = trueMarkCodesPool ?? throw new ArgumentNullException(nameof(trueMarkCodesPool));
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

			await SaveCodes(edoTask, cancellationToken);
			edoTask.Status = EdoTaskStatus.Completed;
			edoTask.StartTime = DateTime.Now;
			edoTask.EndTime = DateTime.Now;

			await _uow.SaveAsync(edoTask, cancellationToken: cancellationToken);
			await _uow.CommitAsync(cancellationToken);
		}

		private async Task SaveCodes(SaveCodesEdoTask edoTask, CancellationToken cancellationToken)
		{
			foreach(var taskItem in edoTask.Items)
			{
				var productCode = taskItem.ProductCode;
				if(productCode.Problem != ProductCodeProblem.None)
				{
					continue;
				}
				await _trueMarkCodesPool.PutCodeAsync(productCode.SourceCode.Id, cancellationToken);
			}
		}

		public void Dispose()
		{
			_uow.Dispose();
		}
	}
}

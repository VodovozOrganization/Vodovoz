using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.CodesPool;
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
			_uow.OpenTransaction();

			var edoTask = await _uow.Session.GetAsync<SaveCodesEdoTask>(edoTaskId);
			if(edoTask == null)
			{
				// задача не найдена
				_logger.LogWarning("ЭДО задача с id {EdoTaskId} не найдена", edoTaskId);
				return;
			}

			if(edoTask.Status != EdoTaskStatus.New)
			{
				// задача уже обработана
				_logger.LogWarning("ЭДО задача с id {EdoTaskId} уже обработана", edoTaskId);
				return;
			}

			try
			{
				await SaveCodes(edoTask, cancellationToken);
			}
			catch(Exception ex)
			{
				// зарегистрировать проблему по исключению
				return;
			}

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

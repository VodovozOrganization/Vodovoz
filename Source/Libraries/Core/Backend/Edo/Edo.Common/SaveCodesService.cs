using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TrueMark.Codes.Pool;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Edo.Common
{
	public class SaveCodesService : ISaveCodesService
	{
		private readonly ILogger<SaveCodesService> _logger;
		private readonly ITrueMarkCodesPool _trueMarkCodesPool;

		public SaveCodesService(
			ILogger<SaveCodesService> logger,
			ITrueMarkCodesPool trueMarkCodesPool)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_trueMarkCodesPool = trueMarkCodesPool ?? throw new ArgumentNullException(nameof(trueMarkCodesPool));
		}
		
		public async Task SaveCodesToPool(SaveCodesEdoTask edoTask, CancellationToken cancellationToken)
		{
			await SaveCodesToPool(edoTask.Items, cancellationToken);
		}
		
		public async Task SaveCodesToPool(ReceiptEdoTask receiptEdoTask, CancellationToken cancellationToken)
		{
			await SaveCodesToPool(receiptEdoTask.Items, cancellationToken);
		}

		private async Task SaveCodesToPool(IEnumerable<EdoTaskItem> taskItems, CancellationToken cancellationToken)
		{
			foreach(var taskItem in taskItems)
			{
				var productCode = taskItem.ProductCode;

				if(productCode.SourceCode is null)
				{
					continue;
				}
				
				if(productCode.Problem != ProductCodeProblem.None)
				{
					continue;
				}

				productCode.ResultCode = null;
				await _trueMarkCodesPool.PutCodeAsync(productCode.SourceCode.Id, cancellationToken);
			}
		}
	}
}

using Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrueMarkApi.Client;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Common
{
	public class EdoTaskItemTrueMarkStatusProvider
	{
		private readonly ITrueMarkApiClient _trueMarkApiClient;
		private readonly EdoTask _edoTask;
		private Dictionary<TrueMarkWaterIdentificationCode, EdoTaskItemTrueMarkStatus> _codesStatuses;
		private IEnumerable<EdoTaskItem> _codeItems;
		private bool _codesChecked;

		public EdoTaskItemTrueMarkStatusProvider(EdoTask edoTask, ITrueMarkApiClient trueMarkApiClient)
		{
			_edoTask = edoTask ?? throw new ArgumentNullException(nameof(edoTask));
			_trueMarkApiClient = trueMarkApiClient ?? throw new ArgumentNullException(nameof(trueMarkApiClient));

			ClearCache();
		}

		public void ClearCache()
		{
			_codeItems = GetCodeItems();

			_codesStatuses = _codeItems.SelectMany(AggregateAllTaskItemCodes)
				.GroupBy(x => x.Key)
				.ToDictionary(x => x.Key, x => x.First().Value);

			_codesChecked = false;
		}

		private IEnumerable<KeyValuePair<TrueMarkWaterIdentificationCode, EdoTaskItemTrueMarkStatus>> AggregateAllTaskItemCodes(
			EdoTaskItem edoTaskItem
			)
		{
			if(edoTaskItem.ProductCode.ResultCode != null)
			{
				yield return new KeyValuePair<TrueMarkWaterIdentificationCode, EdoTaskItemTrueMarkStatus>(
					edoTaskItem.ProductCode.ResultCode,
					new EdoTaskItemTrueMarkStatus { EdoTaskItem = edoTaskItem, ItemCodeType = EdoTaskItemCodeType.Result }
				);
			}

			if(edoTaskItem.ProductCode.SourceCode != null)
			{
				yield return new KeyValuePair<TrueMarkWaterIdentificationCode, EdoTaskItemTrueMarkStatus>(
					edoTaskItem.ProductCode.SourceCode,
					new EdoTaskItemTrueMarkStatus { EdoTaskItem = edoTaskItem, ItemCodeType = EdoTaskItemCodeType.Source }
				);
			}
		}

		public async Task<IDictionary<string, EdoTaskItemTrueMarkStatus>> GetItemsStatusesAsync(CancellationToken cancellationToken)
		{
			if(!_codeItems.Any())
			{
				return new Dictionary<string, EdoTaskItemTrueMarkStatus>();
			}

			if(!_codesChecked)
			{
				await Check(cancellationToken);
			}

			return _codesStatuses.ToDictionary(x => x.Key.IdentificationCode, x => x.Value);
		}

		public async Task<IDictionary<TrueMarkWaterIdentificationCode, EdoTaskItemTrueMarkStatus>> GetCodeItemsStatusesAsync(CancellationToken cancellationToken)
		{
			if(!_codeItems.Any())
			{
				return new Dictionary<TrueMarkWaterIdentificationCode, EdoTaskItemTrueMarkStatus>();
			}

			if(!_codesChecked)
			{
				await Check(cancellationToken);
			}

			return _codesStatuses;
		}

		private async Task Check(CancellationToken cancellationToken)
		{
			var identificationCodesDic = _codesStatuses.Keys
				.ToDictionary(x => x.IdentificationCode);

			var response = await _trueMarkApiClient.GetProductInstanceInfoAsync(identificationCodesDic.Keys, cancellationToken);
			if(!response.ErrorMessage.IsNullOrWhiteSpace())
			{
				throw new EdoException($"Не удалось получить данные о кодах из честного знака для ЭДО задачи №{_edoTask.Id}. " +
					$"Подробности: {response.ErrorMessage}");
			}

			foreach(var instanceStatus in response.InstanceStatuses)
			{
				var domainCode = identificationCodesDic[instanceStatus.IdentificationCode];
				_codesStatuses[domainCode].ProductInstanceStatus = instanceStatus;
			}

			_codesChecked = true;
		}

		private IEnumerable<EdoTaskItem> GetCodeItems()
		{
			switch(_edoTask.TaskType)
			{
				case EdoTaskType.Transfer:
					return ((TransferEdoTask)_edoTask).TransferEdoRequests.SelectMany(x => x.TransferedItems);
				case EdoTaskType.Receipt:
					return ((ReceiptEdoTask)_edoTask).Items;
				case EdoTaskType.Document:
					return ((DocumentEdoTask)_edoTask).Items;
				case EdoTaskType.Tender:
					return ((TenderEdoTask)_edoTask).Items;
				default:
					throw new NotSupportedException($"Проверка кодов для задачи {_edoTask.TaskType} не поддерживается.");
			}
		}
	}
}

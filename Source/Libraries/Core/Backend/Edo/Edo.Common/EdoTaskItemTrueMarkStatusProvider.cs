﻿using Core.Infrastructure;
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
		private readonly Dictionary<string, EdoTaskItemTrueMarkStatus> _codesStatuses = new Dictionary<string, EdoTaskItemTrueMarkStatus>();
		private readonly EdoTask _edoTask;
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
			_codesStatuses.Clear();
			foreach(var item in _codeItems)
			{
				_codesStatuses.Add(
					item.ProductCode.ResultCode.IdentificationCode,
					new EdoTaskItemTrueMarkStatus { EdoTaskItem = item }
				);
			}
			_codesChecked = false;
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

			return _codesStatuses;
		}

		private async Task Check(CancellationToken cancellationToken)
		{
			var response = await _trueMarkApiClient.GetProductInstanceInfoAsync(_codesStatuses.Keys, cancellationToken);
			if(!response.ErrorMessage.IsNullOrWhiteSpace())
			{
				throw new EdoException($"Не удалось получить данные о кодах из честного знака для ЭДО задачи №{_edoTask.Id}. " +
					$"Подробности: {response.ErrorMessage}");
			}

			foreach(var instanceStatus in response.InstanceStatuses)
			{
				_codesStatuses[instanceStatus.IdentificationCode].ProductInstanceStatus = instanceStatus;
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
				case EdoTaskType.Document:
					return ((OrderEdoTask)_edoTask).Items;
				default:
					throw new NotSupportedException($"Проверка кодов для задачи {_edoTask.TaskType} не поддерживается.");
			}
		}
	}
}

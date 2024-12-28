using Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Contracts;
using TrueMarkApi.Client;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Documents
{
	public class EdoTaskItemTrueMarkStatusProvider
	{
		private readonly TrueMarkApiClient _trueMarkApiClient;
		private readonly Dictionary<string, EdoTaskItemTrueMarkStatus> _codesStatuses = new Dictionary<string, EdoTaskItemTrueMarkStatus>();
		private readonly DocumentEdoTask _edoTask;
		private bool _codesChecked;

		public EdoTaskItemTrueMarkStatusProvider(DocumentEdoTask edoTask, TrueMarkApiClient trueMarkApiClient)
		{
			_edoTask = edoTask ?? throw new ArgumentNullException(nameof(edoTask));
			_trueMarkApiClient = trueMarkApiClient ?? throw new ArgumentNullException(nameof(trueMarkApiClient));

			foreach(var item in _edoTask.Items)
			{
				_codesStatuses.Add(
					item.ProductCode.ResultCode.IdentificationCode,
					new EdoTaskItemTrueMarkStatus { EdoTaskItem = item }
				);
			}
		}

		public async Task<IDictionary<string, EdoTaskItemTrueMarkStatus>> GetItemsStatusesAsync(CancellationToken cancellationToken)
		{
			if(!_edoTask.Items.Any())
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
				_codesStatuses[instanceStatus.IdentificationCode].Status = instanceStatus;
			}

			_codesChecked = true;
		}
	}

	public class EdoTaskItemTrueMarkStatus
	{
		public EdoTaskItem EdoTaskItem { get; set; }
		public ProductInstanceStatus Status { get; set; }
	}
}

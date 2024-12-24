using Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Documents
{
	public class TransferRequestCreator
	{
		private readonly IEdoRepository _edoRepository;

		public TransferRequestCreator(IEdoRepository edoRepository)
		{
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
		}

		public async Task CreateTransferRequests(DocumentEdoTask edoTask, EdoTaskItemTrueMarkStatusProvider taskItemStatusProvider, CancellationToken cancellationToken)
		{
			var orderEdoRequest = edoTask.CustomerEdoRequest as OrderEdoRequest;
			if(orderEdoRequest == null)
			{
				throw new EdoException($"Трансфер кодов недоступен для заявки типа {edoTask.TaskType}. " +
					$"Эта проблема должна обрабатываться валидацией, необходимо проверить работу валидатора.");
			}

			if(edoTask.TransferEdoRequests.Any())
			{
				throw new EdoException($"Задача №{edoTask.Id} уже содержит заявки на перенос кодов. " +
					$"Повторный перенос кодов не предусмотрен.");
			}

			var itemStatuses = await taskItemStatusProvider.GetItemsStatusesAsync(cancellationToken);

			var edoOrganizations = _edoRepository.GetEdoOrganizations();
			var organizationTo = orderEdoRequest.Order.Contract.Organization;

			var transferRequests = new Dictionary<string, TransferEdoRequest>();
			foreach(var itemStatus in itemStatuses.Values)
			{
				if(cancellationToken.IsCancellationRequested)
				{
					return;
				}

				if(itemStatus.Status == null)
				{
					throw new EdoException($"Строка №{itemStatus.EdoTaskItem.Id} в задаче №{itemStatus.EdoTaskItem.EdoTask.Id} " +
						$"не была проверена в честном знаке." +
						$"Эта проблема должна обрабатываться валидацией, необходимо проверить работу валидатора.");
				}

				var edoOrganizationFrom = edoOrganizations.FirstOrDefault(x => x.INN == itemStatus.Status.OwnerInn);
				if(edoOrganizationFrom == null)
				{
					throw new EdoException($"ЭДО организация с ИНН {itemStatus.Status.OwnerInn} ({itemStatus.Status.OwnerName}) не найдена." +
						$"Эта проблема должна обрабатываться валидацией, необходимо проверить работу валидатора.");
				}

				var transferRequest = transferRequests.GetOrAdd(itemStatus.Status.OwnerInn, (inn) => new TransferEdoRequest
				{
					FromOrganizationId = edoOrganizationFrom.Id,
					ToOrganizationId = organizationTo.Id,
					DocumentEdoTask = edoTask,
				});

				if(!transferRequest.TransferedItems.Contains(itemStatus.EdoTaskItem))
				{
					transferRequest.TransferedItems.Add(itemStatus.EdoTaskItem);
				}
			}
		}
	}
}

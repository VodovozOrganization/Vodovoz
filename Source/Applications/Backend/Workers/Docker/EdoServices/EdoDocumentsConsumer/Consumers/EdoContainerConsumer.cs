using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using TaxcomEdo.Contracts.Documents;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.EntityRepositories.Orders;

namespace EdoDocumentsConsumer.Consumers
{
	public class EdoContainerConsumer : IConsumer<EdoContainerInfo>
	{
		private readonly ILogger<EdoContainerConsumer> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOrderRepository _orderRepository;
		private readonly IEdoContainerFileStorageService _edoContainerFileStorageService;

		public EdoContainerConsumer(
			ILogger<EdoContainerConsumer> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOrderRepository orderRepository,
			IEdoContainerFileStorageService edoContainerFileStorageService
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_edoContainerFileStorageService =
				edoContainerFileStorageService ?? throw new ArgumentNullException(nameof(edoContainerFileStorageService));
		}
		
		public async Task Consume(ConsumeContext<EdoContainerInfo> context)
		{
			var containerInfo = context.Message;

			try
			{
				await Consume(containerInfo, new CancellationToken());
			}
			catch(Exception e)
			{
				_logger.LogError(e,
					"Ошибка при обновлении данных контейнера с документом {MainDocumentId}", containerInfo.MainDocumentId);
			}
		}
		
		private async Task Consume(EdoContainerInfo containerInfo, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Обновляем данные по контейнеру с документом {MainDocumentId}", containerInfo.MainDocumentId);

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot($"Обновление данных контейнера"))
			{
				EdoContainer container = null;
				
				container = _orderRepository.GetEdoContainerByMainDocumentId(uow, containerInfo.MainDocumentId);

				if(container != null)
				{
					container.DocFlowId = containerInfo.DocFlowId;
					container.Received = containerInfo.Received;
					container.InternalId = containerInfo.InternalId;
					container.ErrorDescription = containerInfo.ErrorDescription;
					container.EdoDocFlowStatus = Enum.Parse<EdoDocFlowStatus>(containerInfo.EdoDocFlowStatus);

					if(container.EdoDocFlowStatus == EdoDocFlowStatus.Succeed)
					{
						using var ms = new MemoryStream(containerInfo.Documents);

						var result = await _edoContainerFileStorageService.UpdateContainerAsync(container, ms, cancellationToken);

						if(result.IsFailure)
						{
							var errors = string.Join(", ", result.Errors.Select(e => e.Message));
							_logger.LogError("Не удалось обновить контейнер, ошибка: {Errors}", errors);
						}
					}

					_logger.LogInformation("Сохраняем изменения контейнера №{ContainerId}", container.Id);
					await uow.SaveAsync(container);
					await uow.CommitAsync();
				}
				else
				{
					_logger.LogError("Не найден контейнер с документом {MainDocumentId}", containerInfo.MainDocumentId);
				}
			}
		}
	}
}

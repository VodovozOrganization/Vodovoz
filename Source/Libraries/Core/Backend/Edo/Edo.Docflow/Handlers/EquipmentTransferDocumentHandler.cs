using Edo.Docflow.Factories;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;
using TaxcomEdo.Contracts.Documents;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Orders.Documents;

namespace Edo.Docflow.Handlers
{
	/// <summary>
	/// Обработчик документов типа акт приёма-передачи оборудования
	/// </summary>
	public class EquipmentTransferDocumentHandler : IInformalOrderDocumentHandler, IDisposable
	{
		private readonly IUnitOfWork _uow;
		private readonly InfoForCreatingEdoEquipmentTransferFactory _informalOrderDocumentnfoFactory;
		private readonly ILogger<EquipmentTransferDocumentHandler> _logger;

		public EquipmentTransferDocumentHandler(
			IUnitOfWork uow,
			InfoForCreatingEdoEquipmentTransferFactory equipmentTransferInfoFactory,
			ILogger<EquipmentTransferDocumentHandler> logger
			)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_informalOrderDocumentnfoFactory = equipmentTransferInfoFactory ?? throw new ArgumentNullException(nameof(equipmentTransferInfoFactory));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public OrderDocumentType DocumentType => OrderDocumentType.EquipmentTransfer;

		public async Task<InfoForCreatingEdoInformalOrderDocument> ProcessDocument(
			OrderEntity order,
			OrderDocumentFileData fileData,
			int documentId,
			CancellationToken cancellationToken
			)
		{
			var document = await _uow.Session.GetAsync<EquipmentTransferDocumentEntity>(documentId, cancellationToken);

			if(document == null)
			{
				_logger.LogWarning("Документ акта приёма-передачи оборудования с ID {DocumentId} не найден", documentId);
			}

			if(document.Order?.Id != order.Id)
			{
				_logger.LogWarning("Документ {DocumentId} не принадлежит заказу {OrderId}", documentId, order.Id);
			}

			var documentInfo = _informalOrderDocumentnfoFactory.CreateInfoForCreatingEdoInformalOrderDocument(order, fileData);

			return documentInfo;
		}

		public void Dispose()
		{
			_uow.Dispose();
		}
	}
}

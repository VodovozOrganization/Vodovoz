using Edo.InformalOrderDocuments.Factories;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;
using TaxcomEdo.Contracts.Documents;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Orders.Documents;

namespace Edo.InformalOrderDocuments.Handlers
{
	/// <summary>
	/// Обработчик документов заказа типа акт приёма-передачи оборудования
	/// </summary>
	public class EquipmentTransferDocumentHandler : IInformalOrderDocumentHandler, IDisposable
	{
		private readonly IUnitOfWork _uow;
		private readonly IPrintableDocumentSaver _printableDocumentSaver;
		private readonly IInformalOrderDocumentFileDataFactory _equipmentTransferFileDataFactory;
		private readonly ILogger<EquipmentTransferDocumentHandler> _logger;

		public EquipmentTransferDocumentHandler(
			IUnitOfWork uow,
			IPrintableDocumentSaver printableDocumentSaver,
			IInformalOrderDocumentFileDataFactory equipmentTransferFileDataFactory,
			ILogger<EquipmentTransferDocumentHandler> logger)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_printableDocumentSaver = printableDocumentSaver ?? throw new ArgumentNullException(nameof(printableDocumentSaver));
			_equipmentTransferFileDataFactory = equipmentTransferFileDataFactory ?? throw new ArgumentNullException(nameof(equipmentTransferFileDataFactory));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public OrderDocumentType DocumentType => OrderDocumentType.EquipmentTransfer;

		public async Task<OrderDocumentFileData> ProcessDocumentAsync(
			OrderEntity order,
			int documentId,
			CancellationToken cancellationToken)
		{
			var equipmentTransferDocument = await _uow.Session.GetAsync<EquipmentTransferDocumentEntity>(documentId, cancellationToken);

			if(equipmentTransferDocument == null)
			{
				_logger.LogWarning("Документ акта приёма-передачи оборудования с ID {DocumentId} не найден", documentId);
				throw new ArgumentException($"Документ с ID {documentId} не найден");
			}

			var pdfBytes = _printableDocumentSaver.SaveToPdf(equipmentTransferDocument);

			var documentDate = equipmentTransferDocument.DocumentDate
				?? order.DeliveryDate
				?? order.CreateDate
				?? DateTime.Now;

			var fileData = _equipmentTransferFileDataFactory.CreateInformalOrderDocumentFileData(
				order.Id,
				documentDate,
				pdfBytes);

			return fileData;
		}

		public void Dispose()
		{
			_uow.Dispose();
		}
	}
}

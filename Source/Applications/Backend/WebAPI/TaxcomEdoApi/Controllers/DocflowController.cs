using System;
using Core.Infrastructure;
using Edo.Contracts.Messages.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Taxcom.Client.Api;
using Taxcom.Client.Api.Entity;
using TaxcomEdo.Contracts.Documents;
using TaxcomEdoApi.Library.Services;

namespace TaxcomEdoApi.Controllers
{
	public class DocflowController : ControllerBase
	{
		private readonly ILogger<DocflowController> _logger;
		private readonly ITaxcomEdoService _taxcomEdoService;
		private readonly IEdoDocflowService _docflowService;

		public DocflowController(
			ILogger<DocflowController> logger,
			ITaxcomEdoService taxcomEdoService,
			IEdoDocflowService docflowService)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_taxcomEdoService = taxcomEdoService ?? throw new ArgumentNullException(nameof(taxcomEdoService));
			_docflowService = docflowService ?? throw new ArgumentNullException(nameof(docflowService));
		}
		
		[HttpPost]
		public IActionResult CreateAndSendIndividualAccountingUpd(UniversalTransferDocumentInfo updInfo)
		{
			var documentId = updInfo.DocumentId;
			_logger.LogInformation(
				"Поступил запрос отправки УПД {UpdNumber} {DocumentId}",
				updInfo.Number,
				documentId);
			
			try
			{
				var container = _taxcomEdoService.CreateContainerWithUpd(updInfo);
				
				_logger.LogInformation(
					"Отправляем контейнер с УПД {UpdNumber} {DocumentId}",
					updInfo.Number,
					documentId);
				
				_docflowService.SendMessageAsync(container);
				return Ok();
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка в процессе формирования УПД №{UpdNumber} {DocumentId} и ее отправки",
					updInfo.Number,
					documentId);
				return Problem();
			}
		}
		
		[HttpPost]
		public IActionResult CreateAndSendBill(InfoForCreatingEdoBill data)
		{
			var orderId = data.OrderInfoForEdo.Id;
			_logger.LogInformation("Создаем счёт по заказу №{OrderId}", orderId);
			
			try
			{
				var container = _taxcomEdoService.CreateContainerWithBill(data);
				
				_logger.LogInformation("Отправляем контейнер со счетом по заказу №{OrderId}", orderId);
				_taxcomApi.Send(container);
				return Ok();
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка в процессе формирования контейнера по заказу №{OrderId} для отправки счета",
					orderId);
				return Problem();
			}
		}
		
		[HttpPost]
		public IActionResult CreateAndSendBillWithoutShipmentForDebt(InfoForCreatingBillWithoutShipmentForDebtEdo data)
		{
			return CreateAndSendBillWithoutShipment(data);
		}
		
		[HttpPost]
		public IActionResult CreateAndSendBillWithoutShipmentForPayment(InfoForCreatingBillWithoutShipmentForPaymentEdo data)
		{
			return CreateAndSendBillWithoutShipment(data);
		}
		
		[HttpPost]
		public IActionResult CreateAndSendBillWithoutShipmentForAdvancePayment(InfoForCreatingBillWithoutShipmentForAdvancePaymentEdo data)
		{
			return CreateAndSendBillWithoutShipment(data);
		}
		
		[HttpGet]
		public IActionResult StartAutoSendReceive()
		{
			_logger.LogInformation("Запуск необходимых транзакций по ЭДО");
			
			try
			{
				_taxcomApi.AutoSendReceive();
				return Ok();
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при запуске необходимых транзакций по ЭДО");
				return Problem();
			}
		}
		
		[HttpGet]
		public IActionResult GetDocFlowRawData(string docFlowId)
		{
			_logger.LogInformation("Получение документов контейнера документооборота {DocFlowId}", docFlowId);
			
			try
			{
				var documents = _taxcomApi.GetDocflowRawData(docFlowId);
				return Ok(documents);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при получении документов контейнера документооборота {DocFlowId}", docFlowId);
				return Problem();
			}
		}
		
		[HttpGet]
		public IActionResult GetDocFlowsUpdates([FromBody] GetDocFlowsUpdatesParameters docFlowsUpdatesParams)
		{
			_logger.LogInformation("Получаем исходящие документы");
			
			try
			{
				var docFlowUpdates =
					_taxcomApi.GetDocflowsUpdates(
						docFlowsUpdatesParams.DocFlowStatus.TryParseAsEnum<DocFlowStatus>(),
						docFlowsUpdatesParams.LastEventTimeStamp,
						docFlowsUpdatesParams.DocFlowDirection.TryParseAsEnum<DocFlowDirection>(),
						docFlowsUpdatesParams.DepartmentId,
						docFlowsUpdatesParams.IncludeTransportInfo);

				return Ok(docFlowUpdates);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при получении исходящих документов");
				return Problem();
			}
		}
		
		[HttpGet]
		public IActionResult GetMessageList()
		{
			_logger.LogInformation("Получаем исходящие документы");
			
			try
			{
				var docFlowUpdates =
					_taxcomApi.GetDocflowsUpdates(
						docFlowsUpdatesParams.DocFlowStatus.TryParseAsEnum<DocFlowStatus>(),
						docFlowsUpdatesParams.LastEventTimeStamp,
						docFlowsUpdatesParams.DocFlowDirection.TryParseAsEnum<DocFlowDirection>(),
						docFlowsUpdatesParams.DepartmentId,
						docFlowsUpdatesParams.IncludeTransportInfo);

				return Ok(docFlowUpdates);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при получении исходящих документов");
				return Problem();
			}
		}
		
		[HttpGet]
		public IActionResult AcceptIngoingDocflow(string docflowId, string organization)
		{
			_logger.LogInformation(
				"Поступил запрос принятия входящего документооборота {DocFlowId}", docflowId);
			
			try
			{
				var taxcomContainer = _taxcomApi.GetMainDocumentContainerFromDocflow(docflowId);

				if(taxcomContainer?.Documents == null || !taxcomContainer.Documents.Any())
				{
					const string emptyContainer = "Поступил запрос на принятие пустого контейнера или без документов";
					_logger.LogWarning(emptyContainer);
					return Problem(emptyContainer);
				}

				var mainDocument = taxcomContainer.Documents.First();

				if(mainDocument is not UniversalInvoiceDocument upd)
				{
					const string notUpd = "Поступил запрос на принятие контейнера с документом не УПД";
					_logger.LogWarning(notUpd);
					return Problem(notUpd);
				}

				var xmlString = _taxcomEdoService.GetSendCustomerInformationEvent(docflowId, organization, upd.Version);
				
				_logger.LogInformation(
					"Сформировали файл действие для отправки титула покупателя по {DocFlowId} версия {UpdVersion}",
					docflowId,
					upd.Version);

				_taxcomApi.SendCustomerInformationWithRawData(xmlString);
				return Ok();
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при принятии входящего документооборота {DocFlowId}", docflowId);
				return Problem();
			}
		}

		[HttpGet]
		public IActionResult SendOfferCancellation(string docflowId, string comment)
		{
			_logger.LogInformation("Аннулирование документооборота {DocFlowId} по причине {Reason}",
				docflowId, 
				comment
			);

			try
			{
				var document = _taxcomEdoService.CreateOfferCancellation(docflowId, comment);

				_logger.LogInformation("Сформировали файл действие для отправки предложения об " +
					"аннулировании для документооборота {DocFlowId}", docflowId);

				_taxcomApi.OfferCancellationWithRawData(document.ToXmlString());
				return Ok();
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при отправке предложения об аннулировании документооборота " +
					"{DocFlowId}", docflowId);
				return Problem();
			}
		}

		[HttpGet]
		public IActionResult AcceptOfferCancellation(string docflowId)
		{
			_logger.LogInformation("Принятие аннулирования документооборот {DocFlowId}",
				docflowId
			);

			try
			{
				var document = _taxcomEdoService.AcceptOfferCancellation(docflowId);

				_logger.LogInformation("Сформировали файл действие для отправки принятия предложения об " +
					"аннулировании документооборота {DocFlowId}", docflowId);

				_taxcomApi.AcceptCancellationOfferWithRawData(document.ToXmlString());
				return Ok();
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при отправке принятия предложения об аннулировании документооборота " +
					"{DocFlowId}", docflowId);
				return Problem();
			}
		}

		[HttpGet]
		public IActionResult RejectOfferCancellation(string docflowId, string comment)
		{
			_logger.LogInformation("Отказ в аннулировании документооборота {DocFlowId} по причине {Reason}",
				docflowId,
				comment
			);

			try
			{
				var document = _taxcomEdoService.RejectOfferCancellation(docflowId, comment);

				_logger.LogInformation("Сформировали файл действие для отправки отказа в " +
					"аннулировании документооборота {DocFlowId}", docflowId);

				_taxcomApi.RejectCancellationOfferWithRawData(document.ToXmlString());
				return Ok();
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при отправке отказа в аннулировании документооборота " +
					"{DocFlowId}", docflowId);
				return Problem();
			}
		}
		
		[HttpGet]
		public IActionResult GetDocFlowStatus(string docFlowId)
		{
			_logger.LogInformation("Получение текущего статуса документооборота {DocFlowId}", docFlowId);

			try
			{
				var docflowDescription = _edoDocflowService.GetStatus(docFlowId);
				return Ok(docflowDescription);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при получении текущего статуса документооборота {DocFlowId}", docFlowId);
				return Problem();
			}
		}

		[HttpGet]
		public IActionResult GetStatus()
		{
			return Ok("It's working!!!");
		}
		
		private IActionResult CreateAndSendBillWithoutShipment(InfoForCreatingBillWithoutShipmentEdo data)
		{
			var documentType = data.GetBillWithoutShipmentInfoTitle();
			var orderWithoutShipmentId = data.OrderWithoutShipmentInfo.Id;
			
			_logger.LogInformation("Создаем {OrderWithoutShipmentType} №{OrderWithoutShipmentForPaymentId}",
				documentType,
				orderWithoutShipmentId
			);
			
			try
			{
				var container = _taxcomEdoService.CreateContainerWithBillWithoutShipment(data);
				
				_logger.LogInformation("Отправляем контейнер по {OrderWithoutShipmentType} №{OrderWithoutShipmentId}",
					documentType,
					orderWithoutShipmentId);
				
				_taxcomApi.Send(container);
				return Ok();
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка в процессе формирования контейнера по {OrderWithoutShipmentType} №{OrderWithoutShipmentId} и его отправки",
					documentType,
					orderWithoutShipmentId);
				return Problem();
			}
		}
	}
}

using System;
using Core.Infrastructure;
using Edo.Contracts.Messages.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Taxcom.Client.Api;
using TaxcomEdo.Contracts.Counterparties;
using TaxcomEdo.Contracts.Documents;
using TaxcomEdoApi.Library.Services;
using TISystems.TTC.CRM.BE.Serialization;

namespace TaxcomEdoApi.Controllers
{
	[ApiController]
	[Route("/api/[action]")]
	public class TaxcomEdoController : ControllerBase
	{
		private readonly ILogger<TaxcomEdoController> _logger;
		private readonly TaxcomApi _taxcomApi;
		private readonly ITaxcomEdoService _taxcomEdoService;
		
		public TaxcomEdoController(
			ILogger<TaxcomEdoController> logger,
			TaxcomApi taxcomApi,
			ITaxcomEdoService taxcomEdoService)
		{
			_logger = logger;
			_taxcomApi = taxcomApi ?? throw new ArgumentNullException(nameof(taxcomApi));
			_taxcomEdoService = taxcomEdoService ?? throw new ArgumentNullException(nameof(taxcomEdoService));
		}

		[HttpPost]
		public IActionResult CreateAndSendUpd(InfoForCreatingEdoUpd data)
		{
			var orderId = data.OrderInfoForEdo.Id;
			_logger.LogInformation(
				"Поступил запрос отправки УПД по заказу {OrderId}",
				orderId);
			
			try
			{
				var container = _taxcomEdoService.CreateContainerWithUpd(data);
				
				_logger.LogInformation("Отправляем контейнер с УПД по заказу №{OrderId}", orderId);
				_taxcomApi.Send(container);
				return Ok();
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка в процессе формирования УПД №{OrderId} и ее отправки", orderId);
				return Problem();
			}
		}
		
		[HttpPost]
		public IActionResult NewCreateAndSendUpd(UniversalTransferDocumentInfo updInfo)
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
				
				System.IO.File.WriteAllBytes(@"D:\test.zip", container.ExportToZip());
				
				//_taxcomApi.Send(container);
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
		public IActionResult GetContactListUpdates(DateTime? lastCheckContactsUpdates, EdoContactStateCode? contactState)
		{
			_logger.LogInformation("Получаем обновленный список контактов...");

			ContactStatus? contactStatus = null;

			if(contactState.HasValue)
			{
				if(Enum.TryParse(contactState.ToString(), out ContactStatus parsedContactStatus))
				{
					contactStatus = parsedContactStatus;
				}
			}
			
			try
			{
				var response = _taxcomApi.GetContactListUpdates(lastCheckContactsUpdates, contactStatus);
				var contactUpdates = ContactListSerializer.DeserializeContactList(response);
				
				return Ok(contactUpdates);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при получении обновлений для списка контактов");
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

		[HttpPost]
		public IActionResult AcceptContact([FromBody] string edxClientId)
		{
			_logger.LogInformation("Принимаем приглашение другой стороны {EdxClientId}", edxClientId);
			
			try
			{
				_taxcomApi.AcceptContact(edxClientId);
				return Ok();
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при связке контактной пары {EdxClientId}", edxClientId);
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
		public IActionResult OfferCancellation(string docFlowId, string reason)
		{
			_logger.LogInformation(
				"Пришел запрос на аннулирование документооборота {DocFlowId} по причине {Reason}",
				docFlowId,
				reason);
			
			try
			{
				_taxcomApi.OfferCancellation(docFlowId, reason);
				return Ok();
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при аннулировании документооборота {DocFlowId} с причиной {Reason}",
					docFlowId,
					reason);
				return Problem();
			}
		}
		
		[HttpGet]
		public IActionResult AcceptIngoingDocflow(string docFlowId, string organization)
		{
			_logger.LogInformation("Поступил запрос принятия входящего документооборота {DocFlowId}", docFlowId);
			
			try
			{
				var sendCustomerInfoEvent = _taxcomEdoService.GetSendCustomerInformationEvent(docFlowId, organization);
				var xmlString = sendCustomerInfoEvent.ToXmlString();
				
				_logger.LogInformation("Сформировали файл действие для отправки титула покупателя по {DocFlowId}", docFlowId);
				
				_taxcomApi.SendCustomerInformationWithRawData(xmlString);
				return Ok();
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при принятии входящего документооборота {DocFlowId}", docFlowId);
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

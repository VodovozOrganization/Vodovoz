using System;
using System.Linq;
using Core.Infrastructure;
using Edo.Contracts.Messages.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Taxcom.Client.Api;
using Taxcom.Client.Api.Entity;
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
		public IActionResult CreateAndSendBulkAccountingUpd(InfoForCreatingEdoUpd data)
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
		public IActionResult CreateAndSendIndividualAccountingUpd(UniversalTransferDocumentInfo updInfo)
		{
			var documentId = updInfo.DocumentId;
			_logger.LogInformation(
				"Поступил запрос отправки УПД {UpdNumber} {DocumentId}",
				updInfo.StringNumber,
				documentId);
			
			try
			{
				var container = _taxcomEdoService.CreateContainerWithUpd(updInfo);
				
				_logger.LogInformation(
					"Отправляем контейнер с УПД {UpdNumber} {DocumentId}",
					updInfo.StringNumber,
					documentId);
				
				_taxcomApi.Send(container);
				return Ok();
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка в процессе формирования УПД №{UpdNumber} {DocumentId} и ее отправки",
					updInfo.StringNumber,
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

		[HttpPost]
		public IActionResult CreateAndSendInformalOrderDocument(InfoForCreatingEdoInformalOrderDocument data)
		{
			var orderId = data.FileData.OrderId;
			_logger.LogInformation("Поступил запрос на отправку неформализованного документа по заказу №{OrderId}", orderId);

			try
			{
				var container = _taxcomEdoService.CreateContainerWithInformalOrderDocument(data);

				_logger.LogInformation("Отправляем контейнер с неформализованным документом по заказу №{OrderId}", orderId);
				_taxcomApi.Send(container);
				return Ok();
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка в процессе формирования контейнера по заказу №{OrderId} для отправки документа заказа",
					orderId);
				return Problem();
			}
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
				var docflowDescription = _taxcomApi.GetStatus(docFlowId);
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

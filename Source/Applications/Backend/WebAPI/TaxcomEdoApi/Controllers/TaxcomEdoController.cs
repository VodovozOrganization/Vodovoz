using System;
using Core.Infrastructure;
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
		public void CreateAndSendUpd(InfoForCreatingEdoUpd data)
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
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка в процессе формирования УПД №{OrderId} и ее отправки", orderId);
			}
		}
		
		[HttpPost]
		public void CreateAndSendBill(InfoForCreatingEdoBill data)
		{
			var orderId = data.OrderInfoForEdo.Id;
			_logger.LogInformation("Создаем счёт по заказу №{OrderId}", orderId);
			
			try
			{
				var container = _taxcomEdoService.CreateContainerWithBill(data);
				
				_logger.LogInformation("Отправляем контейнер со счетом по заказу №{OrderId}", orderId);
				_taxcomApi.Send(container);
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка в процессе формирования контейнера по заказу №{OrderId} для отправки счета",
					orderId);
			}
		}
		
		[HttpPost]
		public void CreateAndSendBillWithoutShipment(InfoForCreatingBillWithoutShipmentEdo data)
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
			}
			catch(Exception e)
			{
				_logger.LogError(e,
					"Ошибка в процессе формирования контейнера по {OrderWithoutShipmentType} №{OrderWithoutShipmentId} и его отправки",
					documentType,
					orderWithoutShipmentId);
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
		public IActionResult AcceptContact(string edxClientId)
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
				if(_taxcomApi.AutoSendReceive())
				{
					return Ok();
				}

				return Problem("Не удалось запустить необходимые транзакции");
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при запуске необходимых транзакций по ЭДО");
				return Problem();
			}
		}
	}
}

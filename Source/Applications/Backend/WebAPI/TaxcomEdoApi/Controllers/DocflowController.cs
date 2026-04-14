using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TaxcomEdo.Contracts.Documents;
using TaxcomEdo.Contracts.Responses;
using TaxcomEdo.Contracts.Xml.Container;
using TaxcomEdoApi.Library.Services;
using TaxcomEdoApi.Library.Services.Interfaces;
using GetMessageListParameters = TaxcomEdoApi.Library.Services.GetMessageListParameters;

namespace TaxcomEdoApi.Controllers
{
	[ApiController]
	[Route("/api/[action]")]
	public class DocflowController : ControllerBase
	{
		private readonly ILogger<DocflowController> _logger;
		private readonly ITaxcomEdoService _taxcomEdoService;
		private readonly ContainerService _containerService;
		private readonly IEdoDocflowService _docflowService;
		private readonly X509Certificate2 _certificate;

		public DocflowController(
			ILogger<DocflowController> logger,
			ITaxcomEdoService taxcomEdoService,
			ContainerService containerService,
			IEdoDocflowService docflowService,
			X509Certificate2 certificate)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_taxcomEdoService = taxcomEdoService ?? throw new ArgumentNullException(nameof(taxcomEdoService));
			_containerService = containerService ?? throw new ArgumentNullException(nameof(containerService));
			_docflowService = docflowService ?? throw new ArgumentNullException(nameof(docflowService));
			_certificate = certificate ?? throw new ArgumentNullException(nameof(certificate));
		}
		
		/// <summary>
		/// Отправка оператору ЭДО УПД по экземплярному учету
		/// </summary>
		/// <param name="updInfo">Данные для УПД</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		[HttpPost]
		public async Task<TaxcomResponse> CreateAndSendIndividualAccountingUpd(
			UniversalTransferDocumentInfo updInfo, CancellationToken cancellationToken)
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

				var bytes = _containerService.ExportNewToZip(container);

				await System.IO.File.WriteAllBytesAsync(@"D:\testRawContainer.zip", bytes, cancellationToken);
				//await _docflowService.SendMessageAsync(_containerService.ExportNewToZip(container), _certificate.RawData, cancellationToken);
				return TaxcomResponse.Success();
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка в процессе формирования УПД №{UpdNumber} {DocumentId} и ее отправки",
					updInfo.Number,
					documentId);
				
				return TaxcomResponse.Error(
					$"Произошла ошибка в процессе формирования УПД №{updInfo.Number} {documentId} и ее отправки." +
					" Попробуйте позднее или обратитесь в отдел разработки");
			}
		}
		
		[HttpPost]
		public async Task<TaxcomResponse> CreateAndSendBill(InfoForCreatingEdoBill data, CancellationToken cancellationToken)
		{
			var orderId = data.OrderInfoForEdo.Id;
			_logger.LogInformation("Создаем счёт по заказу №{OrderId}", orderId);
			
			try
			{
				var container = _taxcomEdoService.CreateContainerWithBill(data);
				
				_logger.LogInformation("Отправляем контейнер со счетом по заказу №{OrderId}", orderId);
				var result =
					await _docflowService.SendMessageAsync(_containerService.ExportNewToZip(container), _certificate.RawData, cancellationToken);
				
				return result;
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка в процессе формирования контейнера по заказу №{OrderId} для отправки счета",
					orderId);
				
				return TaxcomResponse.Error(
					$"Произошла ошибка в процессе формирования контейнера по заказу №{orderId} для отправки счета. " +
					"Попробуйте позднее или обратитесь в отдел разработки");
			}
		}
		
		[HttpPost]
		public async Task<TaxcomResponse> CreateAndSendBillWithoutShipmentForDebt(
			InfoForCreatingBillWithoutShipmentForDebtEdo data, CancellationToken cancellationToken)
		{
			return await CreateAndSendBillWithoutShipment(data, cancellationToken);
		}
		
		[HttpPost]
		public async Task<TaxcomResponse> CreateAndSendBillWithoutShipmentForPayment(
			InfoForCreatingBillWithoutShipmentForPaymentEdo data, CancellationToken cancellationToken)
		{
			return await CreateAndSendBillWithoutShipment(data, cancellationToken);
		}
		
		[HttpPost]
		public async Task<TaxcomResponse> CreateAndSendBillWithoutShipmentForAdvancePayment(
			InfoForCreatingBillWithoutShipmentForAdvancePaymentEdo data, CancellationToken cancellationToken)
		{
			return await CreateAndSendBillWithoutShipment(data, cancellationToken);
		}
		
		[HttpGet]
		public async Task<TaxcomResponse<EdoDocFlowUpdates>> GetDocFlowRawData(string docFlowId, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Получение документов контейнера документооборота {DocFlowId}", docFlowId);
			
			try
			{
				var response = await _docflowService.GetMessageAsync(docFlowId, _certificate.RawData, cancellationToken);
				return response;
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при получении документов контейнера документооборота {DocFlowId}", docFlowId);
				
				return TaxcomResponse<EdoDocFlowUpdates>.Error(
					$"Произошла ошибка при получении документов контейнера документооборота {docFlowId}. " +
					"Попробуйте позднее или обратитесь в отдел разработки");
			}
		}
		
		[HttpPost]
		public async Task<TaxcomResponse<ContainerDescription>> GetDocFlowsUpdates(
			GetDocFlowsUpdatesParameters docFlowsUpdatesParams, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Получаем изменения {Direction} ДО", docFlowsUpdatesParams.DocFlowDirection);
			
			try
			{
				var response = await _docflowService.GetListAsync(docFlowsUpdatesParams, _certificate.RawData, cancellationToken);
				return response;
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при получении изменений ДО");
				return TaxcomResponse<ContainerDescription>.Error(
					"Произошла ошибка при получении изменений ДО. Попробуйте позднее или обратитесь в отдел разработки");
			}
		}
		
		[HttpPost]
		public async Task<TaxcomResponse<ContainerDescription>> GetMessageListAsync(
			GetMessageListParameters docFlowsUpdatesParams, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Получаем документообороты {Direction} ДО", docFlowsUpdatesParams.DocflowDirection);
			
			try
			{
				var response = await _docflowService.GetMessageListAsync(docFlowsUpdatesParams, _certificate.RawData, cancellationToken);
				return response;
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при получении ДО");
				return TaxcomResponse<ContainerDescription>.Error(
					"Произошла ошибка при получении ДО. Попробуйте позднее или обратитесь в отдел разработки");
			}
		}
		
		[HttpGet]
		public async Task<IActionResult> AcceptIngoingDocflow(string docflowId, string organization, CancellationToken cancellationToken)
		{
			_logger.LogInformation(
				"Поступил запрос принятия входящего документооборота {DocFlowId}", docflowId);

			return Ok();

			/*try
			{
				var taxcomContainer = await _docflowService.GetMessageAsync(docflowId, _certificate.RawData, cancellationToken);

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

				await _docflowService.SendMessageAsync(xmlString, _certificate.RawData, cancellationToken);
				return Ok();
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при принятии входящего документооборота {DocFlowId}", docflowId);
				return Problem();
			}*/
		}

		[HttpGet]
		public async Task<IActionResult> SendOfferCancellation(string docflowId, string comment, CancellationToken cancellationToken)
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

				await _docflowService.SendMessageAsync(Array.Empty<byte>(), _certificate.RawData, cancellationToken);
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
		public async Task<IActionResult> AcceptOfferCancellation(string docflowId, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Принятие аннулирования документооборот {DocFlowId}",
				docflowId
			);

			try
			{
				var document = _taxcomEdoService.AcceptOfferCancellation(docflowId);

				_logger.LogInformation("Сформировали файл действие для отправки принятия предложения об " +
					"аннулировании документооборота {DocFlowId}", docflowId);

				await _docflowService.SendMessageAsync(Array.Empty<byte>(),_certificate.RawData, cancellationToken);
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
		public async Task<IActionResult> RejectOfferCancellation(string docflowId, string comment, CancellationToken cancellationToken)
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

				await _docflowService.SendMessageAsync(Array.Empty<byte>(), _certificate.RawData, cancellationToken);
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
		public async Task<IActionResult> GetDocFlowStatus(string docFlowId, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Получение текущего статуса документооборота {DocFlowId}", docFlowId);

			try
			{
				var response = await _docflowService.GetMessageAsync(docFlowId, _certificate.RawData, cancellationToken);
				return Ok(response);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при получении текущего статуса документооборота {DocFlowId}", docFlowId);
				return Problem();
			}
		}
		
		private async Task<TaxcomResponse> CreateAndSendBillWithoutShipment(
			InfoForCreatingBillWithoutShipmentEdo data, CancellationToken cancellationToken)
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
				
				var result =
					await _docflowService.SendMessageAsync(_containerService.ExportNewToZip(container), _certificate.RawData, cancellationToken);
				
				return result;
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка в процессе формирования контейнера по {OrderWithoutShipmentType} №{OrderWithoutShipmentId} и его отправки",
					documentType,
					orderWithoutShipmentId);
				
				return TaxcomResponse.Error(
					$"Произошла ошибка в процессе формирования контейнера по {documentType} №{orderWithoutShipmentId} и его отправки. " +
					"Попробуйте позднее или обратитесь в отдел разработки");
			}
		}
	}
}

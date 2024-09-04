using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Taxcom.Client.Api;
using TaxcomEdo.Contracts;
using TaxcomEdoApi.Library.Services;

namespace TaxcomEdoApi.Controllers
{
	[ApiController]
	[Route("/api/[action]")]
	public class TaxcomEdoController : ControllerBase
	{
		private readonly ILogger<TaxcomEdoController> _logger;
		private readonly TaxcomApi _taxcomApi;
		private readonly TaxcomEdoService _taxcomEdoService;
		
		public TaxcomEdoController(
			ILogger<TaxcomEdoController> logger,
			TaxcomApi taxcomApi,
			TaxcomEdoService taxcomEdoService)
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
	}
}

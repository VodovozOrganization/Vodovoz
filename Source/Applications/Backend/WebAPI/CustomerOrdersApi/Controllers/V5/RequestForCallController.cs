using System;
using CustomerOrdersApi.Library.V5.Dto.Orders;
using CustomerOrdersApi.Library.V5.Services;
using Gamma.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CustomerOrdersApi.Controllers.V5
{
	public class RequestForCallController : SignatureControllerBase
	{
		private readonly ICustomerOrdersService _customerOrdersService;

		public RequestForCallController(
			ILogger<RequestForCallController> logger,
			ICustomerOrdersService customerOrdersService) : base(logger)
		{
			_customerOrdersService = customerOrdersService ?? throw new ArgumentNullException(nameof(customerOrdersService));
		}
		
		[HttpPost]
		public IActionResult CreateRequestForCall(CreatingRequestForCallDto creatingInfoDto)
		{
			var sourceName = creatingInfoDto.Source.GetEnumTitle();
			
			try
			{
				_logger.LogInformation(
					"Поступил запрос от {Source} на создание заявки на звонок c подписью {Signature}, проверяем...",
					sourceName,
					creatingInfoDto.Signature);
				
				if(!_customerOrdersService.ValidateRequestForCallSignature(creatingInfoDto, out var generatedSignature))
				{
					return InvalidSignature(creatingInfoDto.Signature, generatedSignature);
				}

				_logger.LogInformation("Подпись валидна, сохраняем");
				_customerOrdersService.CreateRequestForCall(creatingInfoDto);
				
				return Ok();
			}
			catch(Exception e)
			{
				_logger.LogError(e,
					"Ошибка при сохранении заявки на звонок контакта {Phone} от {Source}",
					creatingInfoDto.PhoneNumber,
					sourceName);

				return Problem();
			}
		}
	}
}

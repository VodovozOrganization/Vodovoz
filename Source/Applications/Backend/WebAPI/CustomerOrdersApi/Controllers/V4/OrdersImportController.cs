using CustomerOrdersApi.Library.SiteOrdersImport.Dto;
using CustomerOrdersApi.Library.SiteOrdersImport.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CustomerOrdersApi.Controllers.V4
{
	/// <summary>
	/// Приём выгрузки заказов и брошенных корзин с сайта.
	/// Сайт сам инициирует POST в ночном окне на этот endpoint.
	/// </summary>
	[ApiController]
	[ApiVersion("4.0")]
	[Route("api/v{version:apiVersion}/orders/[action]")]
	public class OrdersImportController : SignatureControllerBase
	{
		private readonly ISiteOrdersImportRequestValidator _requestValidator;
		private readonly ISiteOrdersImportService _siteOrdersImportService;

		public OrdersImportController(
			ILogger<OrdersImportController> logger,
			ISiteOrdersImportRequestValidator requestValidator,
			ISiteOrdersImportService siteOrdersImportService)
			: base(logger)
		{
			_requestValidator = requestValidator ?? throw new ArgumentNullException(nameof(requestValidator));
			_siteOrdersImportService = siteOrdersImportService ?? throw new ArgumentNullException(nameof(siteOrdersImportService));
		}

		[HttpPost]
		public async Task<IActionResult> ImportAsync(OrdersImportRequest request, CancellationToken cancellationToken)
		{
			if(request is null)
			{
				return ValidationProblem("Пустое тело запроса");
			}

			if(!_requestValidator.ValidateSignature(request, DateTime.Now, out var generatedSignature))
			{
				return InvalidSignature(request.Token, generatedSignature);
			}

			var validationResult = _requestValidator.Validate(request);

			if(validationResult.IsFailure)
			{
				var firstError = validationResult.Errors.First();
				var statusCode = int.TryParse(firstError.Code, out var parsedCode) ? parsedCode : 400;

				return Problem(firstError.Message, statusCode: statusCode);
			}

			try
			{
				_logger.LogInformation(
					"Поступил пакет выгрузки с сайта batch_id={BatchId} ({ContractVersion})",
					request.BatchId,
					request.ContractVersion);

				var response = await _siteOrdersImportService.ImportAsync(request, cancellationToken);

				_logger.LogInformation(
					"Пакет {BatchId} обработан: принято {ImportedCount}, с ошибкой {ErrorCount}",
					response.BatchId,
					response.ImportedOrderIds.Count,
					response.ErrorOrderIds.Count);

				return Ok(response);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при обработке пакета выгрузки {BatchId}", request.BatchId);

				return Problem();
			}
		}
	}
}

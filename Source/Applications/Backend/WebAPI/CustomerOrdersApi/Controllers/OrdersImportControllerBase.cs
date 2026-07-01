using CustomerOrdersApi.Library.SiteOrdersImport.Dto;
using CustomerOrdersApi.Library.SiteOrdersImport.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CustomerOrdersApi.Controllers
{
	/// <summary>
	/// Общая логика приёма пакета выгрузки с сайта для разных версий API.
	/// </summary>
	public abstract class OrdersImportControllerBase : ControllerBase
	{
		private readonly ILogger _logger;
		private readonly ISiteOrdersImportRequestValidator _requestValidator;
		private readonly ISiteOrdersImportService _siteOrdersImportService;

		protected OrdersImportControllerBase(
			ILogger logger,
			ISiteOrdersImportRequestValidator requestValidator,
			ISiteOrdersImportService siteOrdersImportService)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_requestValidator = requestValidator ?? throw new ArgumentNullException(nameof(requestValidator));
			_siteOrdersImportService = siteOrdersImportService ?? throw new ArgumentNullException(nameof(siteOrdersImportService));
		}

		/// <summary>
		/// Проверяет пакет, выполняет импорт и возвращает ответ по контракту v1.
		/// </summary>
		protected async Task<IActionResult> ImportInternalAsync(
			OrdersImportRequest request,
			CancellationToken cancellationToken)
		{
			if(request is null)
			{
				return ValidationProblem("Пустое тело запроса");
			}

			var validationResult = _requestValidator.Validate(request, DateTime.Now);

			if(!validationResult.IsValid)
			{
				if(!validationResult.IsUnauthorized)
				{
					return ValidationProblem(validationResult.Message);
				}

				_logger.LogWarning("{ValidationError} в пакете {BatchId}", validationResult.Message, request.BatchId);

				return Unauthorized(validationResult.Message);

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

using CustomerOrdersApi.Library.SiteOrdersImport.Dto;
using CustomerOrdersApi.Library.SiteOrdersImport.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CustomerOrdersApi.Controllers.Default
{
	/// <summary>
	/// Приём выгрузки заказов и брошенных корзин с сайта (I-5840, контракт v1).
	/// Сайт сам инициирует POST в ночном окне на этот endpoint.
	/// </summary>
	[ApiController]
	[Route("api/orders/import")]
	public class OrdersImportController : ControllerBase
	{
		private const string _invalidToken = "Некорректный токен";

		private readonly ILogger<OrdersImportController> _logger;
		private readonly ISiteImportTokenValidator _tokenValidator;
		private readonly ISiteOrdersImportService _siteOrdersImportService;

		public OrdersImportController(
			ILogger<OrdersImportController> logger,
			ISiteImportTokenValidator tokenValidator,
			ISiteOrdersImportService siteOrdersImportService)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_tokenValidator = tokenValidator ?? throw new ArgumentNullException(nameof(tokenValidator));
			_siteOrdersImportService = siteOrdersImportService ?? throw new ArgumentNullException(nameof(siteOrdersImportService));
		}

		[HttpPost]
		public async Task<IActionResult> ImportAsync(OrdersImportRequest request, CancellationToken cancellationToken)
		{
			if(request is null)
			{
				return ValidationProblem("Пустое тело запроса");
			}

			var validationError = ValidateRequest(request);

			if(validationError != null)
			{
				return ValidationProblem(validationError);
			}

			try
			{
				_logger.LogInformation(
					"Поступил пакет выгрузки с сайта batch_id={BatchId} ({ContractVersion}), проверяем токен...",
					request.BatchId,
					request.ContractVersion);

				if(!_tokenValidator.Validate(request.Token, DateTime.Now, out _))
				{
					_logger.LogWarning("{InvalidToken} в пакете {BatchId}", _invalidToken, request.BatchId);

					return Unauthorized(_invalidToken);
				}

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

		private static string ValidateRequest(OrdersImportRequest request)
		{
			if(string.IsNullOrWhiteSpace(request.BatchId))
			{
				return "Не заполнен batch_id";
			}

			if(string.IsNullOrWhiteSpace(request.ContractVersion))
			{
				return "Не заполнен contract_version";
			}

			if(request.Items is null || request.Items.Count == 0)
			{
				return "Не заполнен items";
			}

			return null;
		}
	}
}

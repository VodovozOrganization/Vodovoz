using CustomerOrdersApi.Library.SiteOrdersImport.Dto;
using CustomerOrdersApi.Library.SiteOrdersImport.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace CustomerOrdersApi.Controllers.V5
{
	/// <summary>
	/// Приём выгрузки заказов и брошенных корзин с сайта.
	/// Сайт сам инициирует POST в ночном окне на этот endpoint.
	/// </summary>
	[ApiController]
	[ApiVersion("5.0")]
	[Route("api/v{version:apiVersion}/orders/import")]
	public class OrdersImportController : OrdersImportControllerBase
	{
		public OrdersImportController(
			ILogger<OrdersImportController> logger,
			ISiteOrdersImportRequestValidator requestValidator,
			ISiteOrdersImportService siteOrdersImportService)
			: base(logger, requestValidator, siteOrdersImportService)
		{
		}

		[HttpPost]
		public Task<IActionResult> ImportAsync(OrdersImportRequest request, CancellationToken cancellationToken)
		{
			return ImportInternalAsync(request, cancellationToken);
		}
	}
}

using BitrixApi.Contracts.Dto.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Mime;
using System.Threading;

namespace BitrixApi.Controllers.V1
{
	public class CounterpartyDocumentsController : VersionedController
	{
		public CounterpartyDocumentsController(
			ILogger<CounterpartyDocumentsController> logger) : base(logger)
		{
		}

		/// <summary>
		/// Отправка документов контрагента на почту
		/// </summary>
		/// <param name="request">Dto запроса на отправку отчета контрагенту</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		[HttpPost]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public IActionResult SendDocument(SendReportRequest request, CancellationToken cancellationToken)
		{
			return NoContent();
		}
	}
}

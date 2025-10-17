using BitrixApi.Contracts.Dto.Requests;
using BitrixApi.Library.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

namespace BitrixApi.Controllers.V1
{
	public class CounterpartyDocumentsController : VersionedController
	{
		private readonly EmalSendService _emalSendService;

		public CounterpartyDocumentsController(
			ILogger<CounterpartyDocumentsController> logger,
			EmalSendService emalSendService)
			: base(logger)
		{
			_emalSendService = emalSendService ?? throw new System.ArgumentNullException(nameof(emalSendService));
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
		public async Task<IActionResult> SendDocument(SendReportRequest request, CancellationToken cancellationToken)
		{
			try
			{
				await _emalSendService.SendDocumentByEmail(request, cancellationToken);
				return Ok();
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "При обработке запроса отправки документа возникла ошибка");
				return Problem(ex.Message, statusCode: 500, title: "При обработке запроса отправки документа возникла ошибка");
			}
		}
	}
}

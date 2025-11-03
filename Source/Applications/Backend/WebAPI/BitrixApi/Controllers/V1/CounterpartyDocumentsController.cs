using BitrixApi.Contracts.Dto.Requests;
using BitrixApi.Library.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

namespace BitrixApi.Controllers.V1
{
	public class CounterpartyDocumentsController : VersionedController
	{
		private readonly IEmalSendService _emalSendService;

		public CounterpartyDocumentsController(
			ILogger<CounterpartyDocumentsController> logger,
			IEmalSendService emalSendService)
			: base(logger)
		{
			_emalSendService = emalSendService ?? throw new ArgumentNullException(nameof(emalSendService));
		}

		/// <summary>
		/// Отправка документов контрагента на почту
		/// </summary>
		/// <param name="request">Dto запроса на отправку отчета контрагенту</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		[HttpPost("SendDocumentByEmail")]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> SendDocumentByEmail(SendReportRequest request, CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation("Поступил запрос на отправку документа. " +
					"INN: {CounterpartyInn}. " +
					"OrganizationId: {OrganizationId}. " +
					"Email: {Email}. " +
					"ReportType: {}ReportType",
					request.CounterpartyInn,
					request.OrganizationId,
					request.EmailAdress,
					request.ReportType);

				await _emalSendService.SendDocumentByEmail(request, cancellationToken);
				return NoContent();
			}
			catch(KeyNotFoundException ex)
			{
				_logger.LogCritical(ex, "Запрашиваемый объект не найден: {ExceptionMessage}", ex.Message);
				return Problem(
					ex.Message,
					statusCode: StatusCodes.Status404NotFound,
					title: "Запрашиваемый объект не найден",
					instance: Request.Path);
			}
		}
	}
}

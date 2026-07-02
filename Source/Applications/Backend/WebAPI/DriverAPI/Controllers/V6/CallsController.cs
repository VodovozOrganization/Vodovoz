using DriverApi.Contracts.V6.Requests;
using DriverAPI.Library.V6.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Presentation.WebApi.Common;

namespace DriverAPI.Controllers.V6
{
	/// <summary>
	/// Контроллер оплат СБП
	/// </summary>
	[ApiVersion("6.0")]
	[Authorize(Roles = nameof(ApplicationUserRole.Driver))]
	public class CallsController : VersionedController
	{
		private readonly ICallsService _callsService;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger">Логгер</param>
		/// <param name="callsService">Сервис для работы со звонками</param>
		public CallsController(
			ILogger<ApiControllerBase> logger,
			ICallsService callsService
			) : base(logger)
		{
			_callsService = callsService ?? throw new System.ArgumentNullException(nameof(callsService));
		}

		/// <summary>
		/// Смены типа оплаты заказа
		/// </summary>
		/// <param name="getCallRequest">Модель данных входящего запроса</param>
		[HttpPost]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> GetCall(GetCallRequest getCallRequest, CancellationToken cancellationToken)
		{
		}
	}
}

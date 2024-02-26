using DriverApi.Contracts.V5.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Mime;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Services;

namespace DriverAPI.Controllers.V5
{
	/// <summary>
	/// Контроллер значений
	/// </summary>
	[ApiVersion("5.0")]
	[Authorize(Roles = nameof(ApplicationUserRole.Driver))]
	public class ValuesController : VersionedController
	{
		private readonly IDriverApiParametersProvider _webApiParametersProvider;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="webApiParametersProvider"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public ValuesController(
			ILogger<ValuesController> logger,
			IDriverApiParametersProvider webApiParametersProvider)
			: base(logger)
		{
			_webApiParametersProvider = webApiParametersProvider ?? throw new ArgumentNullException(nameof(webApiParametersProvider));
		}

		/// <summary>
		/// Получение телефонного номера компании
		/// </summary>
		/// <returns><see cref="CompanyNumberResponse"/></returns>
		[HttpGet]
		[AllowAnonymous]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CompanyNumberResponse))]
		public IActionResult GetCompanyPhoneNumber()
		{
			return Ok(new CompanyNumberResponse()
			{
				Number = _webApiParametersProvider.CompanyPhoneNumber
			});
		}
	}
}

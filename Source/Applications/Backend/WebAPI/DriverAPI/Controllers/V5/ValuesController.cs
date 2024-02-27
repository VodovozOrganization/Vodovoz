using DriverApi.Contracts.V5.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Mime;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Settings.Logistics;

namespace DriverAPI.Controllers.V5
{
	/// <summary>
	/// Контроллер значений
	/// </summary>
	[ApiVersion("5.0")]
	[Authorize(Roles = nameof(ApplicationUserRole.Driver))]
	public class ValuesController : VersionedController
	{
		private readonly IDriverApiSettings _driverApiSettings;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="driverApiSettings"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public ValuesController(
			ILogger<ValuesController> logger,
			IDriverApiSettings driverApiSettings)
			: base(logger)
		{
			_driverApiSettings = driverApiSettings ?? throw new ArgumentNullException(nameof(driverApiSettings));
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
				Number = _driverApiSettings.CompanyPhoneNumber
			});
		}
	}
}

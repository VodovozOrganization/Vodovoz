using DriverApi.Contracts.V4;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Settings.Logistics;

namespace DriverAPI.Controllers.V4
{
	/// <summary>
	/// Контроллер значений
	/// </summary>
	[ApiVersion("4.0", Deprecated = true)]
	[Authorize(Roles = nameof(ApplicationUserRole.Driver))]
	public class ValuesController : VersionedController
	{
		private readonly IDriverApiSettings _webApiSettings;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="webApiSettings"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public ValuesController(IDriverApiSettings webApiSettings)
		{
			_webApiSettings = webApiSettings ?? throw new ArgumentNullException(nameof(webApiSettings));
		}

		/// <summary>
		/// Получение телефонного номера компании
		/// </summary>
		/// <returns><see cref="CompanyNumberResponseDto"/></returns>
		[HttpGet]
		[AllowAnonymous]
		[Produces("application/json")]
		[Route("GetCompanyPhoneNumber")]
		public CompanyNumberResponseDto GetCompanyPhoneNumber()
		{
			return new CompanyNumberResponseDto()
			{
				Number = _webApiSettings.CompanyPhoneNumber
			};
		}
	}
}

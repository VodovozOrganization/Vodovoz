﻿using DriverApi.Contracts.V4;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Services;

namespace DriverAPI.Controllers.V4
{
	/// <summary>
	/// Контроллер значений
	/// </summary>
	[Authorize(Roles = nameof(ApplicationUserRole.Driver))]
	public class ValuesController : VersionedController
	{
		private readonly IDriverApiParametersProvider _webApiParametersProvider;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="webApiParametersProvider"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public ValuesController(IDriverApiParametersProvider webApiParametersProvider)
		{
			_webApiParametersProvider = webApiParametersProvider ?? throw new ArgumentNullException(nameof(webApiParametersProvider));
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
				Number = _webApiParametersProvider.CompanyPhoneNumber
			};
		}
	}
}

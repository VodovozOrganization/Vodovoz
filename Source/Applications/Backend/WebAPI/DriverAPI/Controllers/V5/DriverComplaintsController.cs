﻿using DriverApi.Contracts.V5;
using DriverAPI.Library.V5.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using Vodovoz.Core.Domain.Employees;

namespace DriverAPI.Controllers.V5
{
	/// <summary>
	/// Контроллер рекламаций водителей
	/// </summary>
	[ApiVersion("5.0")]
	[Authorize(Roles = nameof(ApplicationUserRole.Driver))]
	public class DriverComplaintsController : VersionedController
	{
		private readonly IDriverComplaintService _driverComplaintService;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="driverComplaintService"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public DriverComplaintsController(
			ILogger<DriverComplaintsController> logger,
			IDriverComplaintService driverComplaintService) : base(logger)
		{
			_driverComplaintService = driverComplaintService
				?? throw new ArgumentNullException(nameof(driverComplaintService));
		}

		/// <summary>
		/// Получение популярных причин низкого рейтинга адреса
		/// </summary>
		/// <returns>Список популярных причин</returns>
		[HttpGet]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<DriverComplaintReasonDto>))]
		public IActionResult GetDriverComplaintReasons()
		{
			return MapResult(_driverComplaintService.TryGetPinnedComplaintReasons());
		}
	}
}

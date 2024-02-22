using DriverApi.Contracts.V5;
using DriverAPI.Library.V5.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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
		/// <param name="driverComplaintService"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public DriverComplaintsController(IDriverComplaintService driverComplaintService)
		{
			_driverComplaintService = driverComplaintService ?? throw new ArgumentNullException(nameof(driverComplaintService));
		}

		/// <summary>
		/// Получение популярных причин низкого рейтинга адреса
		/// </summary>
		/// <returns>Список популярных причин</returns>
		[HttpGet]
		[Produces("application/json")]
		[Route("GetDriverComplaintReasons")]
		public IEnumerable<DriverComplaintReasonDto> GetDriverComplaintReasons()
		{
			return _driverComplaintService.GetPinnedComplaintReasons();
		}
	}
}

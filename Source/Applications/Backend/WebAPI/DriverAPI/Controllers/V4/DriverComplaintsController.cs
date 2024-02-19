using DriverAPI.Library.DTOs;
using DriverAPI.Library.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Employees;

namespace DriverAPI.Controllers.V4
{
	/// <summary>
	/// Контроллер рекламаций водителей
	/// </summary>
	[Authorize(Roles = nameof(ApplicationUserRole.Driver))]
	public class DriverComplaintsController : VersionedController
	{
		private readonly IDriverComplaintModel _iAPIDriverComplaintData;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="iAPIDriverComplaintData"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public DriverComplaintsController(IDriverComplaintModel iAPIDriverComplaintData)
		{
			_iAPIDriverComplaintData = iAPIDriverComplaintData ?? throw new ArgumentNullException(nameof(iAPIDriverComplaintData));
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
			return _iAPIDriverComplaintData.GetPinnedComplaintReasons();
		}
	}
}

using DriverAPI.Library.DTOs;
using DriverAPI.Library.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace DriverAPI.Controllers.V2
{
	[ApiVersion("2.0")]
	[Route("api/v{version:apiVersion}")]
	[ApiController]
	[Authorize]
	public class DriverComplaintsController : ControllerBase
	{
		private readonly IDriverComplaintModel _iAPIDriverComplaintData;

		public DriverComplaintsController(IDriverComplaintModel iAPIDriverComplaintData)
		{
			_iAPIDriverComplaintData = iAPIDriverComplaintData ?? throw new ArgumentNullException(nameof(iAPIDriverComplaintData));
		}

		/// <summary>
		/// Эндпоинт получения популярных причин
		/// </summary>
		/// <returns>Список популярных причин</returns>
		[HttpGet]
		[Route("GetDriverComplaintReasons")]
		public IEnumerable<DriverComplaintReasonDto> GetDriverComplaintReasons()
		{
			return _iAPIDriverComplaintData.GetPinnedComplaintReasons();
		}
	}
}
